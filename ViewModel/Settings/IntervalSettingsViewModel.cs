using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Commodules;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace EscInstaller.ViewModel.Settings
{
    public class IntervalSettingsViewModel : DiagramData
    {
        private readonly MainUnitViewModel _main;
        private ObservableCollection<AmountComboItem> _amounts220;
        private ObservableCollection<AmountComboItem> _amounts48;

#if DEBUG
        public IntervalSettingsViewModel()
        {
            _main = new MainUnitViewModel();
        }
#endif

        public IntervalSettingsViewModel(MainUnitViewModel main)
        {
            _main = main;
            UserDefinedVerion = main.DataModel.BoseVersion;
        }

        public string SettingName
        {
            get
            {
                return "IntervalSettings";
            }
        }

        public ObservableCollection<AmountComboItem> ErrorAmounts220
        {
            get
            {
                return _amounts220 ?? (_amounts220 = new ObservableCollection<AmountComboItem>(Enumerable.Range(1, 5)
                    .Select(s => new AmountComboItem(s))));
            }

        }

        public ObservableCollection<AmountComboItem> ErrorAmounts48
        {
            get
            {
                return _amounts48 ?? (_amounts48 = new ObservableCollection<AmountComboItem>(Enumerable.Range(1, 3)
                    .Select(s => new AmountComboItem(s))));
            }
        }

        public class AmountComboItem
        {
            private readonly int _value;


            public AmountComboItem(int value)
            {
                _value = value;

            }

            public int Value
            {
                get { return _value; }
            }

            public string Display
            {
                get { return (_value).ToString("N0"); }
            }
        }

        private static readonly Dictionary<int, int> V220IntervalTable = new Dictionary<int, int>()
        {
            {1,80},
            {2,40},
            {3,25},
            {4,20},
            {5,15},
        };

        private static readonly Dictionary<int, int> V48IntervalTable = new Dictionary<int, int>()
        {
            {1,83},
            {2,41},
            {3,28},            
        };


        public ICommand UpdateE2Prom
        {
            get
            {
                return new RelayCommand(() =>
                {                    
                    var dlg = new OpenFileDialog();
                    dlg.ShowDialog();
                    if (string.IsNullOrEmpty(dlg.FileName))
                        return;


                    const int expSize = 131072;

                    byte[] bytes = File.ReadAllBytes(dlg.FileName);
                    if (bytes.Count() != expSize)
                    {
                        MessageBox.Show(expSize + " bytes were expected while this file counts only " + bytes.Count() + " bytes",
                                        "Wrong file size", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (MessageBox.Show("The eeprom data of the selected file will be added to the send queue, " +
                                    "once or if connected this is send to the MCU",
                                    "Confirm update", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
                        return;

                    //131072 == 2048*64
                    const int start = 10000;
                    const int chuncksize = 112;
                    const int packets = 1081;
                    for (var i = 0; i < packets; i++)
                    {
                        var chunk = bytes.Skip(chuncksize * i + start).Take(chuncksize).ToArray();
                        CommunicationViewModel.AddData(new SetE2PromExt(_main.Id, chunk, (ushort)(chuncksize * i + start), true));
                    }
                },

                    () =>
                    {
                        bool b;
                        return bool.TryParse(LibraryData.Settings["EepromUpdateEnabled"], out b) && b;
                    });
            }
        }

        public AmountComboItem Error220VAmount
        {
            get { return ErrorAmounts220.FirstOrDefault(v => v.Value == _main.DataModel.Error220VAmount); }
            set
            {
                _main.DataModel.Error220VAmount = value.Value;
                if (V220IntervalTable.ContainsKey(value.Value))
                {
                    _main.DataModel.Int220VInterval = V220IntervalTable[value.Value];
                    RaisePropertyChanged(() => Error220VInterval);
                }
                RaisePropertyChanged(() => Error220VAmount);

            }
        }

        public AmountComboItem Error48VAmount
        {
            get { return ErrorAmounts48.FirstOrDefault(v => v.Value == _main.DataModel.Error48VAmount); }
            set
            {
                _main.DataModel.Error48VAmount = value.Value;
                if (V48IntervalTable.ContainsKey(value.Value))
                {
                    _main.DataModel.Int48VInterval = V48IntervalTable[value.Value];
                    RaisePropertyChanged(() => Error48VInterval);
                }
                RaisePropertyChanged(() => Error48VAmount);

            }
        }

        public int Error220VInterval
        {
            get { return _main.DataModel.Int220VInterval; }
            set
            {
                _main.DataModel.Int220VInterval = value;
                RaisePropertyChanged(() => Error220VInterval);

            }
        }

        public int Error48VInterval
        {
            get { return _main.DataModel.Int48VInterval; }
            set
            {
                _main.DataModel.Int48VInterval = value;
                RaisePropertyChanged(() => Error48VInterval);

            }
        }

        public bool CanUserManipulateInterval
        {
            get
            {
                bool r;
                return bool.TryParse(ConfigurationManager.AppSettings["CanUserManipulateInterval"], out r) && r;
            }
        }

        public ICommand SendParameters
        {
            get
            {
                return new RelayCommand(UpdateSettings1, () => Error48VAmount != null && Error220VAmount != null);
            }

        }

        private void UpdateSettings1()
        {

            CommunicationViewModel.AddData(new SetMeasurement(_main.Id, Error48VInterval, Error220VInterval, Error48VAmount.Value, Error220VAmount.Value));
        }

        public override Point Size
        {
            get { return new Point(); }
        }

        public string BoseVersion
        {
            get
            {
                return _main.DataModel.BoseVersion;
            }
        }

        public ICommand SetVersion
        {
            get
            {
                return new RelayCommand(() => CommunicationViewModel.AddData(new SetBoseVersion(UserDefinedVerion, _main.Id)));
            }
        }

        public ICommand GetVersion
        {
            get
            {

                return new RelayCommand(async () =>
                {
                    var q = new GetBoseVersion(_main.Id);
                    CommunicationViewModel.AddData(q);

                    await q.WaitAsync();
                    _main.DataModel.BoseVersion = q.BoseVersion;
                    UserDefinedVerion = q.BoseVersion.Trim();
                    RaisePropertyChanged(() => BoseVersion);
                    RaisePropertyChanged(() => UserDefinedVerion);


                });
            }
        }

        public string UserDefinedVerion { get; set; }
    }
}
