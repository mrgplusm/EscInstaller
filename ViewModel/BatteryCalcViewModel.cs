#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

#endregion

namespace EscInstaller.ViewModel
{
    public class BatteryCalcViewModel : ViewModelBase
    {
        private static readonly List<AmplifierModel> GetAmplifiers = new List<AmplifierModel>
        {
            new AmplifierModel {Name = "Entero 8125", Id = 0, Efficiency = 0.75},
            new AmplifierModel {Name = "Entero 4125", Id = 1, Efficiency = 0.85},
            new AmplifierModel {Name = "Entero 8250", Id = 2, Efficiency = 0.65},
            new AmplifierModel {Name = "Entero 4250", Id = 3, Efficiency = 0.80}
        };

        private double _batteryPowerNeeded;

        public BatteryCalcViewModel()
        {
            ComboChoose = new ObservableCollection<AmplifierModel>(GetAmplifiers);
            if (LibraryData.FuturamaSys.Amplifiers == null)
                LibraryData.FuturamaSys.Amplifiers = new List<AmplifierModel>();

            Amplifiers =
                new ObservableCollection<AmplifierViewModel>(
                    LibraryData.FuturamaSys.Amplifiers.Select(s => new AmplifierViewModel(s)));
        }

        public double AgingFactor { get; set; }
        public double EscUnits { get; set; }
        public double OperatingTime { get; set; }

        public double BatteryPowerNeeded
        {
            get { return _batteryPowerNeeded; }
            private set
            {
                _batteryPowerNeeded = value;
                RaisePropertyChanged(() => BatteryPowerNeeded);
            }
        }

        public ICommand Recalculate
        {
            get
            {
                return new RelayCommand(() => BatteryPowerNeeded =
                    LibraryData.FuturamaSys.Amplifiers.Aggregate(0.0,
                        (current, item) =>
                            current +
                            item.Loads.Sum(
                                load => load.Load)*1/
                            item.Efficiency));
            }
        }

        public ObservableCollection<AmplifierModel> ComboChoose { get; private set; }
        public ObservableCollection<AmplifierViewModel> Amplifiers { get; }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand<AmplifierViewModel>(s =>
                {
                    LibraryData.FuturamaSys.Amplifiers.Remove(s.DataModel);
                    Amplifiers.Remove(s);
                }, s => Amplifiers.Count > 0);
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand<AmplifierModel>(s =>
                {
                    var amp = s;
                    LibraryData.FuturamaSys.Amplifiers.Add(amp);
                    Amplifiers.Add(new AmplifierViewModel(amp));
                });
            }
        }
    }


    public class AmplifierViewModel
    {
        private readonly List<AmplifierLoadModel> _loads;

        public AmplifierViewModel(AmplifierModel model)
        {
            if (model.Loads == null)
            {
                model.Loads = new List<AmplifierLoadModel>();
            }
            _loads = model.Loads;
            Name = model.Name;
            Efficiency = model.Efficiency;
            DataModel = model;
            Loads = new ObservableCollection<AmplifierLoadModel>(_loads);
        }

        public AmplifierModel DataModel { get; }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand<AmplifierLoadModel>(s =>
                {
                    _loads.Remove(s);
                    Loads.Remove(s);
                });
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand<AmplifierLoadModel>(s =>
                {
                    var l = new AmplifierLoadModel();
                    Loads.Add(l);
                    DataModel.Loads.Add(l);
                }, s => Loads.Count < 9);
            }
        }

        public ObservableCollection<AmplifierLoadModel> Loads { get; }
        public string Name { get; }
        public double Efficiency { get; }
    }
}