using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin. Generate a fresh one for your plugin!
[assembly: Guid("7a03ff55-5640-4a29-abc8-5a3d3a594044")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("0.0.0.3")]
[assembly: AssemblyFileVersion("0.0.0.3")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("Mechanical Shutter Flats")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Produces flat exposures using trained data and delays to accomodate mechanical camera shutters")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("George Hilios (jokogeo)")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("Mechanical Shutter Flats")]
[assembly: AssemblyCopyright("Copyright © 2022")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.3.3002")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/ghilios/NINA.Joko.Plugin.MechFlat")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage URL - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "Flat,Mechanical,Lacerta,FCB")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/ghilios/NINA.Joko.Plugin.MechFlat/commits/develop")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"This plugin enables taking trained flat exposures during a sequence when you have a camera with a mechanical shutter.

The mechanical shutter opening and closing while the planel is on can negatively affect flat frames, so this plugin instead does:
1. Start exposure
2. Wait a configured delay
3. Turn on the panel
4. Wait the duration from the flat training data
5. Turn off the panel
6. Exposure ends
")]

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