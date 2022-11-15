using Newtonsoft.Json;
using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Joko.Plugin.MechFlat.SequenceItems {
    [ExportMetadata("Name", "Flash Flat Panel")]
    [ExportMetadata("Description", "Flashes a flat panel on for only a set period of time")]
    [ExportMetadata("Icon", "LightBulbSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_FlatDevice")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class FlashFlatPanel : SequenceItem, IValidatable {
        private readonly IFlatDeviceMediator flatDeviceMediator;


        [ImportingConstructor]
        public FlashFlatPanel(IFlatDeviceMediator flatDeviceMediator) {
            this.flatDeviceMediator = flatDeviceMediator;
            Time = 1;
            Brightness = 0;
        }

        private FlashFlatPanel(FlashFlatPanel cloneMe) : this(cloneMe.flatDeviceMediator) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new FlashFlatPanel(this) {
                Time = Time,
                Brightness = Brightness
            };
        }

        private double time;

        [JsonProperty]
        public double Time {
            get => time;
            set {
                time = value;
                RaisePropertyChanged();
            }
        }

        private int brightness;

        [JsonProperty]
        public int Brightness {
            get {
                return brightness;
            }
            set {
                brightness = value;
                RaisePropertyChanged();
            }
        }

        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (flatDeviceMediator.GetInfo().LightOn) {
                Logger.Warning("Flat panel light is already on before the flash operation");
            }

            Logger.Info($"Setting brightness to {Brightness}");
            await flatDeviceMediator.SetBrightness(Brightness, token);
            Logger.Info($"Turning flat panel on");
            var waitTask = CoreUtil.Delay(GetEstimatedDuration(), token);
            await flatDeviceMediator.ToggleLight(true, token);
            await waitTask;
            Logger.Info($"Turning flat panel off");
            await flatDeviceMediator.ToggleLight(false, token);
        }

        public override TimeSpan GetEstimatedDuration() {
            return TimeSpan.FromSeconds(Time);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(FlashFlatPanel)}, Time: {Time}s, Brightness: {Brightness}";
        }

        public bool Validate() {
            var i = new List<string>();
            var info = flatDeviceMediator.GetInfo();
            if (!info.Connected) {
                i.Add(Loc.Instance["LblFlatDeviceNotConnected"]);
            } else {
                if (!info.SupportsOnOff) {
                    i.Add(Loc.Instance["LblFlatDeviceCannotControlBrightness"]);
                }
            }

            Issues = i;
            return i.Count == 0;
        }

        public override void AfterParentChanged() {
            Validate();
        }
    }
}
