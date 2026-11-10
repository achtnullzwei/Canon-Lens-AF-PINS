# Canon LensAF plugin for PINS

***THIS IS A TESTING VERSION. USE AT YOUR OWN RISK.***

### Info
This plugin lets you use your Canon EFS lens as a focuser in PINS. The original plugin is called LensAF. This is a fork that has been refactored to run headless on linux together with PINS. Since EDSDK is not available it has been refactored to use GPSDK, as this is what PINS uses for Canon cameras

### How to test
- Create a new folder: /home/pi/.local/share/NINA/Plugins/3.0.0/Canon-LensAF-PINS
- Copy the LensAF.dll into this folder
- Reboot the Pi
- Connect the Canon camera
- Once the camera is connected in Equipment, refresh the Focuser list and you should see a Canon focuser with the lens name available. Select it and connect.
- In a lot of cases on the lens, the focus mode switch needs to be on "MF"
- Check that the current focus point is somewhere in the upper 3 quarters of the focus range before calibrating
- Calibrate via sequencer item. It should move all the way to infinity and then a small step back. If it doesn't reach infinity calibrate again.
- Autofocus and manual focus should work now

### Additional info & what does not work
- Calibration of the Lens is needed. This can only be done via Sequencer Item at the moment. Search for "calibrate" in the sequencer item list.
- The "Aperture" Sequencer item is broken and does not work
- The original plugin has settings to set a custom initial focus point per lens. This is not working.
- In contrast to the original NINA plugin, no separate plugin settings are available.

**Still work in progress ;-)**

### Credits
- Original Plugin for NINA. Copyright (c) by christian-photo: https://github.com/christian-photo/Camera-Lens-AF
- Some code was incorporated and adapted from PINS (GPCamera, GPSDK). Copyright © 2025 Nico Trost <nico.trost57@gmail.com> and the PI.N.S. contributors: https://github.com/nitr57/pins
