using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System.ComponentModel.Composition;
using Settings = NINA.Joko.Plugin.MechFlat.Properties.Settings;
using System.Threading;
using System.Globalization;
using System.Windows.Input;

namespace NINA.Joko.Plugin.MechFlat {
    [Export(typeof(IPluginManifest))]
    public class MechFlatPlugin : PluginBase {

        [ImportingConstructor]
        public MechFlatPlugin(IProfileService profileService, IOptionsVM options, IImageSaveMediator imageSaveMediator) {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            if (MechFlatOptions == null) {
                MechFlatOptions = new MechFlatOptions(profileService);
            }

            if (SystemCultureInfo == null) {
                SystemCultureInfo = new Thread(delegate () { }).CurrentCulture;
            }

            ResetOptionDefaultsCommand = new RelayCommand((object o) => MechFlatOptions.ResetDefaults());
        }
        public static MechFlatOptions MechFlatOptions { get; private set; }

        public static CultureInfo SystemCultureInfo { get; private set; }

        public ICommand ResetOptionDefaultsCommand { get; private set; }
    }
}
