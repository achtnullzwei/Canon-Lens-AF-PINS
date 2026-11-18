#region "copyright"

/*
    Copyright © 2025 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org.
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LensAF.Util;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;

namespace LensAF.SequenceItems
{
    [ExportMetadata("Name", "Set Aperture")]
    [ExportMetadata("Description", "Sets the aperture of the lens (canon)")]
    [ExportMetadata("Icon", "CameraSVG")]
    [ExportMetadata("Category", "Lens")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SetAperture : SequenceItem, IValidatable
    {
        private IList<string> issues = new List<string>();
        public IList<string> Issues
        {
            get => issues;
            set
            {
                issues = value;
                RaisePropertyChanged();
            }
        }

        // Kept private since the UI no longer renders a dropdown list
        private List<string> apertures = new List<string>();

        private string selectedAperture = string.Empty;

        [JsonProperty]
        public string SelectedAperture
        {
            get => selectedAperture;
            set
            {
                selectedAperture = value?.Trim() ?? string.Empty;
                RaisePropertyChanged();
                Validate();
            }
        }

        private readonly ICameraMediator camera;

        [ImportingConstructor]
        public SetAperture(ICameraMediator camera)
        {
            this.camera = camera;

            camera.Connected += Camera_Connected;
            camera.Disconnected += CameraDisconnected;

            if (camera.GetInfo().Connected)
            {
                SetApertureList();
            }
        }

        private bool IsCanon()
        {
            return Utility.GetCanonCamera(camera, false) != null;
        }

        private void SetApertureList()
        {
            if (!camera.GetInfo().Connected)
            {
                apertures = new List<string>();
                return;
            }

            if (IsCanon())
            {
                Logger.Info("Getting apertures for canon");
                apertures = CameraInfo.GetAVs(camera) ?? new List<string>();
            }
            else
            {
                apertures = new List<string>();
            }
        }

        private Task Camera_Connected(object arg1, EventArgs args)
        {
            SetApertureList();
            Validate();
            return Task.CompletedTask;
        }

        private Task CameraDisconnected(object arg1, EventArgs args)
        {
            apertures = new List<string>();
            Validate();
            return Task.CompletedTask;
        }

        public override object Clone()
        {
            return new SetAperture(camera)
            {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                SelectedAperture = SelectedAperture // Preserves user input when duplicating step
            };
        }

        public bool Validate()
        {
            var i = new List<string>();

            if (!camera.GetInfo().Connected)
            {
                i.Add("Camera is not connected.");
            }
            else if (!IsCanon())
            {
                i.Add("Connected camera is not a supported Canon camera.");
            }
            else
            {
                // Ensure the list is populated if the lens was attached after connection
                if (apertures == null || apertures.Count == 0)
                {
                    SetApertureList();
                }

                if (string.IsNullOrWhiteSpace(SelectedAperture))
                {
                    i.Add("No aperture value entered.");
                }
                else if (!apertures.Any(a => string.Equals(a, SelectedAperture, StringComparison.OrdinalIgnoreCase)))
                {
                    string available = apertures.Count > 0 ? string.Join(", ", apertures) : "none";
                    i.Add($"Aperture '{SelectedAperture}' is not supported by this lens. Available: {available}");
                }
            }

            Issues = i;
            return i.Count == 0;
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token)
        {
            // Re-run validation at execution time in case the lens changed or sequence was forced to run
            if (!Validate())
            {
                throw new SequenceEntityFailedException(string.Join(" ", Issues));
            }

            // Normalize casing/formatting to match the exact string returned by CameraInfo.GetAVs
            string matchedAperture = apertures.First(a => string.Equals(a, SelectedAperture, StringComparison.OrdinalIgnoreCase));
            new CameraInfo(camera).SetAperture(camera, matchedAperture);

            return Task.CompletedTask;
        }

        ~SetAperture()
        {
            camera.Connected -= Camera_Connected;
            camera.Disconnected -= CameraDisconnected;
        }
    }
}
