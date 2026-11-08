#region "copyright"

/*
    Copyright © 2024 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using CommunityToolkit.Mvvm.Input;
using GPSDK;
using static GPSDK.GPSDK;
using LensAF.Properties;
using LensAF.Util;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Interfaces;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;
using System.Globalization;

namespace LensAF
{
    public class CanonFocuser : BaseINPC, IFocuser
    {
        private bool _isMoving = false;
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                _isMoving = value;
                RaisePropertyChanged();
            }
        }

        private int _position = Settings.Default.FocusStopPosition;
        public int Position
        {
            get => _position;
            set
            {
                _position = value;
                RaisePropertyChanged();
            }
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                RaisePropertyChanged();
            }
        }

        public double StepSize { get; } = 1;

        private string _name = "Canon Lens Driver";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public int MaxIncrement { get; set; } = 10000;

        public int MaxStep { get; set; } = 10000;
    
        public bool CanSetMaxStep { get; set; } = false;

        public bool CanReverse { get; set; } = false;

        public bool Reverse { get; set; } = false;

        public bool TempCompAvailable { get; set; } = false;

        public bool TempComp { get; set; } = false;

        public double Temperature { get; set; } = double.NaN;

        public bool HasSetupDialog { get; set; } = false;

        public string Id { get; set; }

        public string Category { get; set; } = "Canon";

        public string Description { get; set; } = "A lens driver for lenses attached to Canon bodies";

        public string DriverInfo { get; set; } = string.Empty;

        public string DriverVersion { get; set; } = "1.0.0.0";

        public IList<string> SupportedActions { get; set; }

        public AsyncRelayCommand CalibrateLens { get; set; }
        public RelayCommand CancelCalibrate { get; set; }

        private CancellationTokenSource calibrationToken;

        public string DisplayName { get; set; } = "Canon Lens Driver";

        public CanonFocuser(string id)
        {
            Id = id;

            DisplayName = $"Canon Lens Driver ({id})";

            CalibrateLens = new AsyncRelayCommand(async () =>
            {
                if (!ValidateCamera())
                {
                    return;
                }

                calibrationToken = new CancellationTokenSource();

                try
                {
                    await CalibrateCamera(calibrationToken.Token);
                }
                catch (OperationCanceledException e)
                {
                    Notification.ShowInformation("Calibration canceled");
                    Logger.Info($"Calibration canceled: {e.Message}");
                    return;
                }

                Notification.ShowSuccess("Calibration finished");
            });

            CancelCalibrate = new RelayCommand(() =>
            {
                calibrationToken?.Cancel();
            });
        }

        private bool ValidateCamera()
        {
            List<string> validation = Utility.Validate(LensAF.Camera);
            if (validation.Count > 0)
            {
                foreach (string issue in validation)
                {
                    Notification.ShowError(issue);
                    Logger.Error($"Canon camera validation failed for focus movement: {issue}");
                }
                return false;
            }
            return true;
        }

        public string Action(string actionName, string actionParameters)
        {
            return string.Empty;
        }

        public Task<bool> Connect(CancellationToken token)
        {
            return Task.Run(() =>
            {
                if (!ValidateCamera())
                {
                    return false;
                }

                LensAF.AddLensConfigIfNecessary(DisplayName);

                LensAF.Camera.Disconnected += OnCameraDisconnect;

                Connected = true;
                return Connected;
            });
        }

        private Task OnCameraDisconnect(object arg1, EventArgs arg2)
        {
            if (Connected) this.Disconnect();
            return Task.CompletedTask;
        }

        public async Task CalibrateCamera(CancellationToken ct)
        {
            try
            {
                LensAF.AddLensConfigIfNecessary(DisplayName);

                // camera.StartLiveView(new NINA.Equipment.Model.CaptureSequence());
                Utility.SetProperty(LensAF.Camera, "output", "TFT + PC");
                IsMoving = true;

                for (int i = 0; i < Settings.Default.CalibrationLargeSteps; i++)
                {
                    await DriveManualFocus("Far 3", ct);
                    ct.ThrowIfCancellationRequested();
                }

                // Waiting for a bit to give enough time to settle the focuser
                await Task.Delay(300, ct);
                await DriveManualFocus("Near 2", ct);
                ct.ThrowIfCancellationRequested();

                Position = Settings.Default.FocusStopPosition;

                int focusPosition = LensAF.GetFocusPosition(Name);
                if (focusPosition > 0)
                {
                    await Move(focusPosition, ct);
                    ct.ThrowIfCancellationRequested();
                }
            }
            catch
            {
                // camera.StopLiveView();
                Utility.SetProperty(LensAF.Camera, "output", "TFT");
                IsMoving = false;
                throw;
            }

            // camera.StopLiveView();
            Utility.SetProperty(LensAF.Camera, "output", "TFT");
            IsMoving = false;

        }

        public void Disconnect()
        {
            Connected = false;
            LensAF.Camera.Disconnected -= OnCameraDisconnect;
        }

        public void Halt()
        {
        }

        private async Task<bool> DriveManualFocus(string direction, CancellationToken? ct = null)
        {
            if (!Connected)
            {
                throw new Exception("Lens driver not connected");
            }
            // Start driving the manual focus motor
            GP_ERROR_CODE error = Utility.SetProperty(LensAF.Camera, "manualfocusdrive", direction);

            if (error != GP_ERROR_CODE.GP_OK)
            {
                Logger.Debug(Utility.ErrorCodeToString(error));
                return false;
            }

            await Task.Delay(100, ct ?? CancellationToken.None);

            return true;
        }

        public async Task Move(int position, CancellationToken ct, int waitInMs = 1000)
        {
            if (!ValidateCamera())
            {
                return;
            }

            double diff = Position - position;
            GPCamera camera = Utility.GetCanonCamera(LensAF.Camera);

            Utility.GetProperty(LensAF.Camera, "output", out var value);

            bool wasLiveviewEnabled = (value == "TFT + PC");
            if (!wasLiveviewEnabled)
                Utility.SetProperty(LensAF.Camera, "output", "TFT + PC");
                wasLiveviewEnabled = true;

            string direction = "Near 1";
            if (diff < 0)
            {
                direction = "Far 1";
                diff = -diff;
            }

            bool ok = true;

            while (diff > 0 && ok)
            {
                ok = await DriveManualFocus(direction, ct);
                diff -= StepSize;
                Position -= (int)StepSize * (direction == "Far 1" ? -1 : 1);
                ct.ThrowIfCancellationRequested();
            }

            Position = position;

            if (wasLiveviewEnabled)
            {
                Utility.SetProperty(LensAF.Camera, "output", "TFT");
            }
            return;
        }

        public void SendCommandBlind(string command, bool raw = true)
        {
            return;
        }

        public bool SendCommandBool(string command, bool raw = true)
        {
            return false;
        }

        public string SendCommandString(string command, bool raw = true)
        {
            return string.Empty;
        }

        public void SetupDialog()
        {
            Logger.Warning("Canon lens driver SetupDialog not implemented");
            return;
        }
    }
}
