using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Joko.Plugin.MechFlat.SequenceItems {
    [ExportMetadata("Name", "Wait for Precise Time Span")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Utility_WaitForTimeSpan_Description")]
    [ExportMetadata("Icon", "HourglassSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Utility")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitForPreciseTimeSpan : SequenceItem {

        [ImportingConstructor]
        public WaitForPreciseTimeSpan() {
            Time = 1;
        }

        private WaitForPreciseTimeSpan(WaitForPreciseTimeSpan cloneMe) : base(cloneMe) {
        }

        public override object Clone() {
            return new WaitForPreciseTimeSpan(this) {
                Time = Time
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

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return NINA.Core.Utility.CoreUtil.Wait(GetEstimatedDuration(), token, progress);
        }

        public override TimeSpan GetEstimatedDuration() {
            return TimeSpan.FromSeconds(Time);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(WaitForPreciseTimeSpan)}, Time: {Time}s";
        }
    }
}
