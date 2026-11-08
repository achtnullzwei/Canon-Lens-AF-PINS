using System.Reflection;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin
[assembly: Guid("7c8b2a91-8d6f-4a92-bc35-f0e84d7c19c2")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("3.1.2.4")]
[assembly: AssemblyFileVersion("3.1.2.4")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("Lens AF")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Run AF with your Camera Lens or change the aperture! (Canon)")]

// Your name
[assembly: AssemblyCompany("Achtnullzwei")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("Canon Camera Lens AF for PINS")]
[assembly: AssemblyCopyright("Copyright ©  2026 Christian Palm, Achtnullzwei")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.2.0.9001")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
[assembly: AssemblyMetadata("Repository", "https://github.com/achtnullzwei/Canon-Lens-AF-PINS")]
[assembly: AssemblyMetadata("Homepage", "https://github.com/achtnullzwei/Canon-Lens-AF-PINS")]


// The following attributes are optional for the official manifest meta data

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "AF,Sequencer,Canon,Focuser")]

//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"This plugin provides a focuser driver for lenses attachted to Canon cameras

To be able to use the driver, a Canon camera has to be connected.

## Important Note!
**Test the plugin before you use it in your imaging runs, because this plugin may not work for everyone! This plugin doesn't work with ASCOM.DSLR unfortuantely!**  


**Requirements**:  
- A Canon or Camera,  
- A Camera Lens that supports AF

The plugin was tested using the following camera and lenses (Note that there are often issues with third-party lenses, so be aware of that):
- Canon EOS RA
- Canon EF-S 200mm F/4 IS USM

The plugin didn't work with the following cameras/lenses:
- Tamron XR Di II 18-200mm (on Canon)

## Manual Focus Control controls (The Manual Focus Control does not affect the focuser position!)
- Mousewheel/Trackpad: Zoom in/out
- Left mousebutton: Pan around
- Right mousebutton: reset


Known issues:
- Some lenses may try to refocus, when taking images if the lens is set to AF")]


// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]