## TODOs
- [x] Fix Aperture Sequencer Item
- [ ] ~~Check with PINS Devs how custom settings can potentially be implemented. (Calibrate Button, Max Calibration Steps, Initial Focus Point, etc)~~ This only makes sense once TnS has a plugin system that can be leveraged across platforms and allows deveolpers outside the project to create plugins for TnS.
- [x] deb package for PINS

## Completed
- [x] Headless Linux for PINS
- [x] Remove Nikon functionality (cannot test with Nikon cameras and calibration requires a test for a specific ERROR message when the focuser hits infinity to avoid issues.)
- [x] Refactor from EDCamera/EDSDK to GPCamera/GPSDK
- [x] Set Aperture available as a generic sequenser item in Touch'n'Stars
- [ ] Tested on RPi 4 with PINS unstable (as of 10.11.2026)
    - [x] Calibration via SequencerItem works
    - [x] Moving the focuser in/out via Focuser control and "move to" in sequencer works
    - [ ] Test on real target  
