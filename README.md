# Canon LensAF plugin for PINS

***THIS IS STILL EXPERIMENTAL. USE AT YOUR OWN RISK.***

### Info
This plugin lets you use your Canon EFS lens as a focuser in PINS. The original plugin is called LensAF. This is a fork that has been refactored to run headless on linux together with PINS. Since EDSDK is not available it has been refactored to use GPSDK, as this is what PINS uses for Canon cameras.

This is only works on Raspberry Pi with PINS!

### How to install/update
- Download the latest release .deb
- Open a Terminal or use SSH and cd into the folder where the downloaded .deb file resides.
- Only install when PINS is idle.

```
sudo apt install ./pins-plugin-canon-lensaf_*.deb
sudo systemctl restart pins.service
```

- Connect the Canon camera
- Once the camera is connected in Equipment, refresh the Focuser list and you should see a Canon focuser with the lens name available. Select it and connect.
- In a lot of cases on the lens, the focus mode switch needs to be on "MF"
- Check that the current focus point is somewhere in the upper 4/5 of the focus range before calibrating
- Calibrate via sequencer item. It should move all the way to infinity and then a small step back. If it doesn't reach infinity calibrate again.
- Autofocus and manual focus including focus sequencer items should work now
- Apterture can also be set. Add the Set Aperture sequencer item and edit the field. Format example: 5.6
- Invalid/out of range Apertures will throw an error.
- Quick tip: On the next autofocus run, note the final steps. Next time, move to this step number as a rough initial focuspoint in the sequencer.

### Additional info & what does not work
- Calibration of the Lens is needed. This can only be done via Sequencer Item at the moment. Search for "calibrate" in the sequencer item list.
- The original plugin has a full settings page. This is not implemented as it requires changes in TnS/ninaAPI.

**Still work in progress ;-)**

### Credits
- Original Plugin for NINA. Copyright (c) by christian-photo: https://github.com/christian-photo/Camera-Lens-AF
- Some code was incorporated and adapted from PINS (GPCamera, GPSDK). Copyright © 2025 Nico Trost <nico.trost57@gmail.com> and the PI.N.S. contributors: https://github.com/nitr57/pins
- Workflows & CI Scripts from https://github.com/acocalypso/pins
