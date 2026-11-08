#region "copyright"

/*
    Copyright © 2025 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using GPSDK;
using static GPSDK.GPSDK;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Globalization;

namespace LensAF.Util
{
    public static class Utility
    {
        public static object GetInstanceField<T>(T instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField;
            FieldInfo field = typeof(T).GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        public static IntPtr GetCamera(ICameraMediator camera, bool showError = true)
        {
            try
            {
                List<string> errors = Validate(camera);
                if (errors.Count > 0)
                {
                    if (showError)
                    {
                        foreach (string error in errors)
                        {
                            Notification.ShowError(error);
                        }
                    }

                    return IntPtr.Zero;
                }
                IDevice cam = camera.GetDevice() is PersistSettingsCameraDecorator decorator ? decorator.Camera : camera.GetDevice();
                return (IntPtr)GetInstanceField((GPCamera)cam, "_camera");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Notification.ShowError(e.Message);
                return IntPtr.Zero;
            }
        }

        public static IntPtr GetCameraContext(ICameraMediator camera, bool showError = true)
        {
            try
            {
                List<string> errors = Validate(camera);
                if (errors.Count > 0)
                {
                    if (showError)
                    {
                        foreach (string error in errors)
                        {
                            Notification.ShowError(error);
                        }
                    }

                    return IntPtr.Zero;
                }
                IDevice cam = camera.GetDevice() is PersistSettingsCameraDecorator decorator ? decorator.Camera : camera.GetDevice();
                return (IntPtr)GetInstanceField((GPCamera)cam, "_context");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Notification.ShowError(e.Message);
                return IntPtr.Zero;
            }
        }

        public static object GetCameraLock(ICameraMediator camera, bool showError = true)
        {
            try
            {
                List<string> errors = Validate(camera);
                if (errors.Count > 0)
                {
                    if (showError)
                    {
                        foreach (string error in errors)
                        {
                            Notification.ShowError(error);
                        }
                    }

                    return null;
                }
                IDevice cam = camera.GetDevice() is PersistSettingsCameraDecorator decorator ? decorator.Camera : camera.GetDevice();
                return GetInstanceField((GPCamera)cam, "_gpLock");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Notification.ShowError(e.Message);
                return null;
            }
        }

        public static GPCamera GetCanonCamera(ICameraMediator camera, bool showError = true)
        {
            try
            {
                List<string> errors = Validate(camera);
                if (errors.Count > 0)
                {
                    if (showError)
                    {
                        foreach (string error in errors)
                        {
                            Notification.ShowError(error);
                        }
                    }
                    return null;
                }
                IDevice cam = camera.GetDevice() is PersistSettingsCameraDecorator decorator ? decorator.Camera : camera.GetDevice();
                return (GPCamera)cam;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Notification.ShowError(e.Message);
                return null;
            }
        }

        public static List<string> Validate(ICameraMediator Camera)
        {
            List<string> error = new List<string>();
            bool cameraConnected = Camera.GetInfo().Connected;

            if (!cameraConnected)
            {
                error.Add("No camera connected");
                return error;
            }

            if (!(Camera.GetDevice().Category == "libgphoto2" && NameContains(Camera.GetDevice().Name, "Canon") && cameraConnected))
            {
                error.Add("No Canon camera connected");
            }

            return error;
        }

        private static bool NameContains(string name, string value)
        {
            return name?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }



// The following functions were copied and adapted from pins/GPCamera.cs by Nico Trost <nico.trost57@gmail.com> and the PI.N.S. contributors
// Source: https://github.com/nitr57/pins/blob/unstable/NINA.Equipment/SDK/CameraSDKs/GPSDK/GPSDK.cs
// Licensed under the Mozilla Public License 2.0 (MPL-2.0)

        public static string ErrorCodeToString(GP_ERROR_CODE error)
        {
            return $"libgphoto2 camera error occurred: {error}";
        }

        public static GP_ERROR_CODE SetProperty(ICameraMediator cam, string property, string value) {
                IntPtr camera = GetCamera(cam);
                IntPtr context = GetCameraContext(cam);
                object gpLock = GetCameraLock(cam);
                gpLock ??= new object();
                IntPtr widget = IntPtr.Zero;
                lock (gpLock) {
                    GP_ERROR_CODE err = GpCameraGetSingleConfig(camera, property, out widget, context);
                    if (err != GP_ERROR_CODE.GP_OK) {
                        return err;
                    }

                    try {
                        err = SetWidgetValue(widget, value);
                        if (err != GP_ERROR_CODE.GP_OK) {
                            return err;
                        }
                        return GpCameraSetSingleConfig(camera, property, widget, context);
                    } finally {
                        GpWidgetFree(widget);
                    }
                }
        }

        private static GP_ERROR_CODE SetWidgetValue(IntPtr widget, string value) {
            // gp_widget_set_value takes a void*; Nikon bulb is commonly a toggle,
            // not a string.
            var typeResult = GpWidgetGetType(widget, out var type);
            if (typeResult != GP_ERROR_CODE.GP_OK) {
                return typeResult;
            }

            switch (type) {
                case CameraWidgetType.GP_WIDGET_TOGGLE:
                    if (TryParseBooleanPropertyValue(value, out int toggleValue)) {
                        return GpWidgetSetValue(widget, toggleValue);
                    }
                    return GP_ERROR_CODE.GP_ERROR_BAD_PARAMETERS;
                case CameraWidgetType.GP_WIDGET_RANGE:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float rangeValue)
                        || float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out rangeValue)) {
                        return GpWidgetSetValue(widget, rangeValue);
                    }
                    return GP_ERROR_CODE.GP_ERROR_BAD_PARAMETERS;
                case CameraWidgetType.GP_WIDGET_DATE:
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int dateValue)) {
                        return GpWidgetSetValue(widget, dateValue);
                    }
                    return GP_ERROR_CODE.GP_ERROR_BAD_PARAMETERS;
                default:
                    return GpWidgetSetValue(widget, value);
            }
        }

        public static GP_ERROR_CODE GetProperty(ICameraMediator cam, string property, out string data) {
                IntPtr camera = GetCamera(cam);
                IntPtr context = GetCameraContext(cam);
                object gpLock = GetCameraLock(cam);
                gpLock ??= new object();
                IntPtr widget = IntPtr.Zero;
                GP_ERROR_CODE err;
                data = string.Empty;
                lock (gpLock) {

                    err = GpCameraGetSingleConfig(camera, property, out widget, context);
                    if (err != GP_ERROR_CODE.GP_OK) {
                        return err;
                    }

                    try {
                        err = GetWidgetValue(widget, out data);
                        if (err != GP_ERROR_CODE.GP_OK) {
                            return err;
                        }
                        return GP_ERROR_CODE.GP_OK;
                    } finally {
                        GpWidgetFree(widget);
                    }
                }
        }

        private static GP_ERROR_CODE GetWidgetValue(IntPtr widget, out string value) {
            value = string.Empty;
            var typeResult = GpWidgetGetType(widget, out var type);
            if (typeResult != GP_ERROR_CODE.GP_OK) {
                return typeResult;
            }

            switch (type) {
                case CameraWidgetType.GP_WIDGET_TOGGLE:
                    var toggleResult = GpWidgetGetValue(widget, out int toggleValue);
                    value = toggleValue == 0 ? "0" : "1";
                    return toggleResult;
                case CameraWidgetType.GP_WIDGET_RANGE:
                    var rangeResult = GpWidgetGetValue(widget, out float rangeValue);
                    value = rangeValue.ToString(CultureInfo.InvariantCulture);
                    return rangeResult;
                case CameraWidgetType.GP_WIDGET_DATE:
                    var dateResult = GpWidgetGetValue(widget, out int dateValue);
                    value = dateValue.ToString(CultureInfo.InvariantCulture);
                    return dateResult;
                default:
                    return GpWidgetGetValue(widget, out value);
            }
        }

        private static bool TryParseBooleanPropertyValue(string value, out int toggleValue) {
            toggleValue = 0;
            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            string normalized = value.Trim();
            if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out toggleValue)) {
                toggleValue = toggleValue == 0 ? 0 : 1;
                return true;
            }

            if (normalized.Equals("true", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("on", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("yes", StringComparison.OrdinalIgnoreCase)) {
                toggleValue = 1;
                return true;
            }

            if (normalized.Equals("false", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("off", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("no", StringComparison.OrdinalIgnoreCase)) {
                toggleValue = 0;
                return true;
            }

            return false;
        }

        public static GP_ERROR_CODE GetPropertyList(ICameraMediator cam, string property, out IList<string> list) {
                IntPtr camera = GetCamera(cam);
                IntPtr context = GetCameraContext(cam);
                object gpLock = GetCameraLock(cam);
                gpLock ??= new object();
                IntPtr widget = IntPtr.Zero;
                GP_ERROR_CODE err;
                list = [];
                lock (gpLock) {
                    err = GpCameraGetSingleConfig(camera, property, out widget, context);
                    if (err != GP_ERROR_CODE.GP_OK) {
                        return err;
                    }

                    try {
                        // Get number of available choices
                        int count = GpWidgetCountChoices(widget);
                        if (count <= 0) {
                            Logger.Warning($"No choices available for {property}");
                            return GP_ERROR_CODE.GP_ERROR_NOT_SUPPORTED;
                        }

                        // Iterate through all choices
                        for (int i = 0; i < count; ++i) {
                            err = GpWidgetGetChoice(widget, i, out var choice);
                            if (err == GP_ERROR_CODE.GP_OK && !string.IsNullOrEmpty(choice)) {
                                list.Add(choice);
                            }
                        }

                        return err;
                    } finally {
                        GpWidgetFree(widget);
                    }
                }
        }




    }
}