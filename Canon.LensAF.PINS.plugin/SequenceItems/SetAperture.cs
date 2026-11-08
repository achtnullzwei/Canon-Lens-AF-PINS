#region "copyright"

/*
    Copyright © 2025 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
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

        private static List<string> apertures = new List<string>();
        public List<string> Apertures
        {
            get => apertures;
            set
            {
                apertures = value;
                RaisePropertyChanged();
            }
        }

        private string selectedAperture;

        [JsonProperty]
        public string SelectedAperture
        {
            get => selectedAperture;
            set
            {
                selectedAperture = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand<object> RefreshCommand { get; set; }

        private readonly ICameraMediator camera;

        [ImportingConstructor]
        public SetAperture(ICameraMediator camera)
        {
            RefreshCommand = new RelayCommand<object>((o) => SetApertureList());
            this.camera = camera;

            camera.Connected += Camera_Connected;
            camera.Disconnected += CameraDisconnected;

            if (camera.GetInfo().Connected)
            {
                SetApertureList();
            }
        }

        private bool isCanon(ICameraMediator camera)
        {
            var cam = Utility.GetCanonCamera(camera, false);
            if (cam != null)
            {
                return true;
            }
            return false;
        }

        private void SetApertureList()
        {
            if (!camera.GetInfo().Connected)
            {
                return;
            }
            List<string> a = new List<string>();
            var cam = Utility.GetCanonCamera(camera, false);
            if (cam != null)
            {
                Logger.Info("Getting apertures for canon");
                a = CameraInfo.GetAVs(camera);
            }
            
            Apertures = a;
        }

        private Task Camera_Connected(object arg1, EventArgs args)
        {
            SetApertureList();
            return Task.CompletedTask;
        }

        private Task CameraDisconnected(object arg1, EventArgs args)
        {
            Apertures = new List<string>();
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
                Apertures = Apertures,
            };
        }

        public bool Validate()
        {
            List<string> i = new List<string>();
            if (!camera.GetInfo().Connected)
            {
                i.Add("Camera is not connected");
            }
            Issues = i;
            return i.Count == 0;
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token)
        {
            if (isCanon(camera))
            {
                new CameraInfo(camera).SetAperture(camera, SelectedAperture);
            }
            return Task.CompletedTask;
        }

        ~SetAperture()
        {
            camera.Connected -= Camera_Connected;
            camera.Disconnected -= CameraDisconnected;
        }
    }
}