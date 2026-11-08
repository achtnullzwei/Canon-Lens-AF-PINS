#region "copyright"

/*
    Copyright © 2024 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;

namespace LensAF.Util
{
    public class CameraInfo
    {
        public static List<string> GetAVs(ICameraMediator cam) 
        {
            Utility.GetPropertyList(cam, "aperture", out var rawList);
            return (List<string>)rawList;          
        }

        public string LensName;
        public string CameraName;
        public string CameraFirmware;
        public CameraInfo(ICameraMediator cam) 
        {
            Utility.GetProperty(cam, "lensname", out LensName);
            Utility.GetProperty(cam, "cameramodel", out CameraName);
            Utility.GetProperty(cam, "deviceversion", out CameraFirmware);

            isCanon = true;
        }

        private bool isCanon = false;

        public void SetAperture(ICameraMediator cam, string aperture)
        {
            if (isCanon)
            {
                List<string> avs = GetAVs(cam);
                if (avs.Contains(aperture))
                {
                    Utility.SetProperty(cam, "aperture", aperture);
                }
                
            }
        }
    }
}
