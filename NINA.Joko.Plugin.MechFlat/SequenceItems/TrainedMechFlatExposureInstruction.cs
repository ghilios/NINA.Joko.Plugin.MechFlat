using Newtonsoft.Json;
using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Model;
using NINA.Joko.Plugin.MechFlat.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.SequenceItem.FilterWheel;
using NINA.Sequencer.SequenceItem.FlatDevice;
using NINA.Sequencer.SequenceItem.Imaging;
using NINA.Sequencer.SequenceItem.Utility;
using NINA.Sequencer.Utility;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Joko.Plugin.MechFlat.SequenceItems {
    [ExportMetadata("Name", "Trained Mechanical Shutter Flat Exposure")]
    [ExportMetadata("Description", "Looks up the trained exposure value for the specified parameters and takes flat exposures, while introducing delays to accomodate mechanical shutters")]
    [ExportMetadata("Icon", "BrainBulbSVG")]
    [ExportMetadata("Category", "Mechanical Shutter Flats")]
    [Export(typeof(ISequenceItem))]
    [Export(typeof(ISequenceContainer))]
    [JsonObject(MemberSerialization.OptIn)]
    public class TrainedMechFlatExposureInstruction : SequentialContainer, IImmutableContainer {

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            this.Items.Clear();
            this.Conditions.Clear();
            this.Triggers.Clear();
        }

        private readonly IProfileService profileService;
        private readonly IMechFlatOptions options;
        private bool keepPanelClosed;

        [ImportingConstructor]
        public TrainedMechFlatExposureInstruction(IProfileService profileService, ICameraMediator cameraMediator, IImagingMediator imagingMediator, IImageSaveMediator imageSaveMediator, IImageHistoryVM imageHistoryVM, IFilterWheelMediator filterWheelMediator, IFlatDeviceMediator flatDeviceMediator) :
            this(
                null,
                profileService,
                MechFlatPlugin.MechFlatOptions,
                new WaitForPreciseTimeSpan(),
                new CloseCover(flatDeviceMediator),
                new ToggleLight(flatDeviceMediator) { OnOff = false },
                new FlashFlatPanel(flatDeviceMediator),
                new SwitchFilter(profileService, filterWheelMediator),
                new TakeExposure(profileService, cameraMediator, imagingMediator, imageSaveMediator, imageHistoryVM) { ImageType = CaptureSequence.ImageTypes.FLAT },
                new LoopCondition() { Iterations = 1 },
                new OpenCover(flatDeviceMediator),
                new WaitForPreciseTimeSpan()
            ) {
        }

        public TrainedMechFlatExposureInstruction(
            TrainedMechFlatExposureInstruction cloneMe,
            IProfileService profileService,
            IMechFlatOptions options,
            WaitForPreciseTimeSpan beforeWait,
            CloseCover closeCover,
            ToggleLight toggleLightOff,
            FlashFlatPanel flashFlatPanel,
            SwitchFilter switchFilter,
            TakeExposure takeExposure,
            LoopCondition loopCondition,
            OpenCover openCover,
            WaitForPreciseTimeSpan afterWait
            ) {
            this.profileService = profileService;
            this.options = options;

            this.Add(closeCover);
            this.Add(switchFilter);
            this.Add(toggleLightOff);

            var container = new SequentialContainer();
            if (loopCondition.Iterations <= 0) {
                loopCondition.Iterations = 1;
            }
            ShutterTime_sec = options.ShutterTime_sec;

            container.Add(loopCondition);

            var exposureContainer = new ParallelContainer();
            exposureContainer.Add(takeExposure);

            var flatPanelContainer = new SequentialContainer();
            flatPanelContainer.Add(beforeWait);
            flatPanelContainer.Add(flashFlatPanel);
            flatPanelContainer.Add(afterWait);

            exposureContainer.Add(flatPanelContainer);
            container.Add(exposureContainer);

            this.Add(container);
            this.Add(openCover);

            IsExpanded = false;

            if (cloneMe != null) {
                CopyMetaData(cloneMe);
            }
        }

        private InstructionErrorBehavior errorBehavior = InstructionErrorBehavior.ContinueOnError;

        [JsonProperty]
        public override InstructionErrorBehavior ErrorBehavior {
            get => errorBehavior;
            set {
                errorBehavior = value;
                foreach (var item in Items) {
                    item.ErrorBehavior = errorBehavior;
                }
                RaisePropertyChanged();
            }
        }

        private int attempts = 1;

        [JsonProperty]
        public override int Attempts {
            get => attempts;
            set {
                if (value > 0) {
                    attempts = value;
                    foreach (var item in Items) {
                        item.Attempts = attempts;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        [JsonProperty]
        public bool KeepPanelClosed {
            get => keepPanelClosed;
            set {
                keepPanelClosed = value;

                RaisePropertyChanged();
            }
        }
        
        private double shutterTime_sec = 1.0d;

        [JsonProperty]

        public double ShutterTime_sec {
            get => shutterTime_sec;
            set {
                shutterTime_sec = value;
                RaisePropertyChanged();
            }
        }

        public CloseCover GetCloseCoverItem() {
            return (Items[0] as CloseCover);
        }

        public SwitchFilter GetSwitchFilterItem() {
            return (Items[1] as SwitchFilter);
        }

        public ToggleLight GetToggleLightOff() {
            return (Items[2] as ToggleLight);
        }

        public SequentialContainer GetImagingContainer() {
            return (Items[3] as SequentialContainer);
        }

        public LoopCondition GetIterations() {
            return GetImagingContainer().Conditions[0] as LoopCondition;
        }

        public ParallelContainer GetImagingInnerContainer() {
            return GetImagingContainer().Items[0] as ParallelContainer;
        }

        public TakeExposure GetExposureItem() {
            return GetImagingInnerContainer().Items[0] as TakeExposure;
        }

        public SequentialContainer GetFlatControlContainer() {
            return GetImagingInnerContainer().Items[1] as SequentialContainer;
        }

        public WaitForPreciseTimeSpan GetBeforeWaitItem() {
            return GetFlatControlContainer().Items[0] as WaitForPreciseTimeSpan;
        }

        public FlashFlatPanel GetFlashFlatPanelItem() {
            return GetFlatControlContainer().Items[1] as FlashFlatPanel;
        }

        public WaitForPreciseTimeSpan GetAfterWaitItem() {
            return GetFlatControlContainer().Items[2] as WaitForPreciseTimeSpan;
        }

        public OpenCover GetOpenCoverItem() {
            return (Items[4] as OpenCover);
        }

        public override object Clone() {
            var clone = new TrainedMechFlatExposureInstruction(
                this,
                this.profileService,
                this.options,
                (WaitForPreciseTimeSpan)GetBeforeWaitItem().Clone(),
                (CloseCover)this.GetCloseCoverItem().Clone(),
                (ToggleLight)this.GetToggleLightOff().Clone(),
                (FlashFlatPanel)this.GetFlashFlatPanelItem().Clone(),
                (SwitchFilter)this.GetSwitchFilterItem().Clone(),
                (TakeExposure)this.GetExposureItem().Clone(),
                (LoopCondition)this.GetIterations().Clone(),
                (OpenCover)this.GetOpenCoverItem().Clone(),
                (WaitForPreciseTimeSpan)GetAfterWaitItem().Clone()
            ) {
                KeepPanelClosed = KeepPanelClosed
            };
            return clone;
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            var loop = GetIterations();
            if (loop.CompletedIterations >= loop.Iterations) {
                Logger.Warning($"The Trained Flat Exposure progress is already complete ({loop.CompletedIterations}/{loop.Iterations}). The instruction will be skipped");
                throw new SequenceItemSkippedException();
            }

            /* Lookup trained values and set brightness and exposure time accordingly */
            var filter = GetSwitchFilterItem()?.Filter;
            var beforeWait = GetBeforeWaitItem();
            var afterWait = GetAfterWaitItem();
            var flashFlat = GetFlashFlatPanelItem();
            var takeExposure = GetExposureItem();
            var binning = takeExposure.Binning;
            var gain = takeExposure.Gain == -1 ? profileService.ActiveProfile.CameraSettings.Gain ?? -1 : takeExposure.Gain;
            var info = profileService.ActiveProfile.FlatDeviceSettings.GetBrightnessInfo(new FlatDeviceFilterSettingsKey(filter?.Position, binning, gain));

            flashFlat.Brightness = info.AbsoluteBrightness;
            flashFlat.Time = info.Time;
            takeExposure.ExposureTime = info.Time + 2.0d * ShutterTime_sec;
            beforeWait.Time = ShutterTime_sec;
            afterWait.Time = ShutterTime_sec;
            Logger.Info($"Taking Mechanical Shutter Flat Exposure. Mechanical Shutter wait {ShutterTime_sec} secs, Exposure time {info.Time} secs");

            if (KeepPanelClosed) {
                GetOpenCoverItem().Skip();
            } else {
                GetOpenCoverItem().ResetProgress();
            }

            /* Panel most likely cannot open/close so it should just be skipped */
            var closeItem = GetCloseCoverItem();
            if (!closeItem.Validate()) {
                closeItem.Skip();
            }
            var openItem = GetOpenCoverItem();
            if (!openItem.Validate()) {
                openItem.Skip();
            }

            return base.Execute(progress, token);
        }

        public override bool Validate() {
            var switchFilter = GetSwitchFilterItem();
            var takeExposure = GetExposureItem();
            var flashFlat = GetFlashFlatPanelItem();

            var valid = takeExposure.Validate() && switchFilter.Validate() && flashFlat.Validate();

            var issues = new List<string>();

            if (valid) {
                var filter = switchFilter?.Filter;
                var binning = takeExposure.Binning;
                var gain = takeExposure.Gain == -1 ? profileService.ActiveProfile.CameraSettings.Gain ?? -1 : takeExposure.Gain;
                if (profileService.ActiveProfile.FlatDeviceSettings.GetBrightnessInfo(new FlatDeviceFilterSettingsKey(filter?.Position, binning, gain)) == null) {
                    issues.Add(string.Format(Loc.Instance["Lbl_SequenceItem_Validation_FlatDeviceTrainedExposureNotFound"], filter?.Name, gain, binning?.Name));
                    valid = false;
                }
            }

            Issues = issues.Concat(takeExposure.Issues).Concat(switchFilter.Issues).Concat(flashFlat.Issues).Distinct().ToList();
            RaisePropertyChanged(nameof(Issues));

            return valid;
        }

        /// <summary>
        /// When an inner instruction interrupts this set, it should reroute the interrupt to the real parent set
        /// </summary>
        /// <returns></returns>
        public override Task Interrupt() {
            return this.Parent?.Interrupt();
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(TrainedMechFlatExposureInstruction)}, ShutterTime: {ShutterTime_sec}";
        }
    }
}