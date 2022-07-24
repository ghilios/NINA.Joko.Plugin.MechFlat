using NINA.Core.Utility;
using NINA.Joko.Plugin.MechFlat.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using System;

namespace NINA.Joko.Plugin.MechFlat {
    public class MechFlatOptions : BaseINPC, IMechFlatOptions {
        private readonly PluginOptionsAccessor optionsAccessor;

        public MechFlatOptions(IProfileService profileService) {
            var guid = PluginOptionsAccessor.GetAssemblyGuid(typeof(MechFlatOptions));
            if (guid == null) {
                throw new Exception($"Guid not found in assembly metadata");
            }

            this.optionsAccessor = new PluginOptionsAccessor(profileService, guid.Value);
            InitializeOptions();
        }

        private void InitializeOptions() {
            shutterTime_sec = optionsAccessor.GetValueDouble(nameof(ShutterTime_sec), 1d);
        }

        public void ResetDefaults() {
            ShutterTime_sec = 1;
        }

        private double shutterTime_sec;

        public double ShutterTime_sec {
            get => shutterTime_sec;
            set {
                if (shutterTime_sec != value) {
                    shutterTime_sec = value;
                    optionsAccessor.SetValueDouble(nameof(ShutterTime_sec), shutterTime_sec);
                    RaisePropertyChanged();
                }
            }
        }
    }
}
