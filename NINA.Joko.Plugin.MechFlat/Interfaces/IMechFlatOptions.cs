using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Joko.Plugin.MechFlat.Interfaces {
    public interface IMechFlatOptions : INotifyPropertyChanged {
        double ShutterTime_sec { get; set; }

        void ResetDefaults();
    }
}
