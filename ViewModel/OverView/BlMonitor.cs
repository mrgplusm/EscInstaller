using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.EscCommunication;
using EscInstaller.ViewModel.SDCard;
using GalaSoft.MvvmLight.CommandWpf;
using Common;
using Common.Commodules;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;

using System.Windows;
using CommunicationViewModel = EscInstaller.ViewModel.Connection.CommunicationViewModel;

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlMonitor : SnapDiagramData
    {

        private readonly FlowModel _flow;
        private readonly MainUnitViewModel _main;
        private readonly CardModel _card;

        public const int Width = 85;
        public const int XLocation = BlSpeakerPeq.Width + Distance + BlSpeakerPeq.XLocation;

        public BlMonitor(FlowModel flow, MainUnitViewModel main, CardModel card)
        {
            _card = card;
            _main = main;
            Location.X = XLocation;
            _flow = flow;
            _main.KreisUpdated += ReceiverOnEepromReceived;
        }

        private void ReceiverOnEepromReceived(object sender, EventArgs eventArgs)
        {
            _l = GetLoads(_flow, _main.DataModel);
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public override int Id
        {
            get { return _flow.Id; }
        }

        public string DisplaySetting
        {
            get
            {
                return _flow.HasAmplifier
                           ? _flow.AmplifierOperationMode.ToString()
                           : string.Empty;
            }
        }

        public FlowModel Flow
        {
            get { return _flow; }
        }

        public override bool IsEnabled
        {
            get
            {
                bool r;
                bool.TryParse(LibraryData.Settings["MonitoringEnabled"], out r);
                return r;
            }
        }

        public bool HasAmplifier
        {
            get { return _flow.HasAmplifier; }
        }

        public override Point Size
        {
            get
            {
                return new Point(Width,
                    UnitHeight);
            }
        }


        private bool _playTestTone;
        private string _referenceValue = "4";





        public SdMessageViewModel SelectedMessage { get; set; }

        public string ReferenceValue
        {
            get { return _referenceValue; }
            set { _referenceValue = value; }
        }


        private readonly Dictionary<int, int> _flowMux = new Dictionary<int, int> {
            {0,3},
            {1,4},
            {2,5},
            {3,5},
            {4,3},
            {5,3},
            {6,3},
            {7,3},
            {8,3},
            {9,3},
            {10,3},
            {11,3},
        };

        public bool PlayTestTone
        {
            get { return _playTestTone; }
            set
            {
                //todo: implement this
                //if (SelectedMessage == null) return;
                //_playTestTone = value;
                //TestToneEnabled = false;
                //var z = new PlayTestMessage(_flow.Id, SelectedMessage.SdCardPosition, value);
                //CommunicationViewModel.AddData(z);

                ////get value for multiplexer
                //var q = new GetDspValue(_main.Id, McuDat.DemuxAlarm[_flow.Id % 12]);
                //q.ReceivedModule += ReceivedDemuxByte;
                //q.Failure += (s) => { TestToneEnabled = true; };
                //CommunicationViewModel.AddData(q);
            }
        }
        private bool _testmodeEnabled = true;

        public bool TestToneEnabled
        {
            get { return _testmodeEnabled; }
            set { _testmodeEnabled = value; }
        }

        private void ReceivedDemuxByte(GetDspValue d)
        {
            //set demuxer for this channel
            var data = (DspCoefficients.DemuxBytes(McuDat.DemuxAlarm[_flow.Id % 12], _flowMux[_flow.Id % 12] + 2, _flowMux[_flow.Id % 12])).ToArray();
            var q = new DspData(data, _main.Id);

            CommunicationViewModel.AddData(q);
        }

        public AmplifierCalibrationMode AmplifierCalibrationMode
        {
            get { return _flow.AmplifierCalibrationMode; }
            set
            {
                _flow.AmplifierCalibrationMode = value;
                RaisePropertyChanged(() => AmplifierCalibrationMode);
            }
        }

        public ICommand CalibrateCommand
        {
            get { return new RelayCommand(Calibrate, () => CommunicationViewModel.OpenConnections.Any(s => s.ConnectMode == ConnectMode.Install)); }
        }

        public bool EighOhms
        {
            get
            {
                return _card.EightOhms;
            }
            set
            {
                _card.EightOhms = value;
                RaisePropertyChanged(() => EighOhms);
            }
        }


        public int DeviationHp
        {
            get
            {
                return _flow.DeviationHp;
            }
            set
            {
                _flow.DeviationHp = value;
                RaisePropertyChanged(() => DeviationHp);
                CommunicationViewModel.AddData(new SetDeviation(_flow.Id, value + 1, false));
            }
        }

        public int DeviationLp
        {
            get
            {
                return _flow.DeviationLp;
            }
            set
            {
                _flow.DeviationLp = value;
                RaisePropertyChanged(() => DeviationLp);
                CommunicationViewModel.AddData(new SetDeviation(_flow.Id, value + 1, true));
            }
        }

        public string PilotfrequencyInt
        {
            get
            {
                return (PilotFrequency * 0.001) + " KHz";
            }
        }

        public double PilotFrequency
        {
            get
            {
                if (_flow.PilotFrequency < 1)
                    _flow.PilotFrequency = 20000;
                return _flow.PilotFrequency;
            }
            set
            {
                _flow.PilotFrequency = value;
                RaisePropertyChanged(() => PilotFrequency);
            }
        }

        /// <summary>
        /// LpWatt  LpOhm   HpWatt  HpOhm
        /// A       A       A       A
        /// B       B       B       B
        /// AB      AB      AB      AB
        /// </summary>
        public double[] MeasuredValues
        {
            get
            {
                if (_flow.MeasuredValues == null || _flow.MeasuredValues.Length < 12)
                    _flow.MeasuredValues = new double[12];
                return _flow.MeasuredValues;
            }
            set
            {
                _flow.MeasuredValues = value;
                RaisePropertyChanged(() => MeasuredValues);
            }
        }

        public Action MeasureLoadFinished;
        public Action MeasureLoadStarted;

        public ICommand MeasureLoad
        {
            get
            {
                return new RelayCommand(async () =>
                    {
                        var t = new MeasureLoad(_flow.Id, AmplifierCalibrationMode, PilotFrequency);
                        CommunicationViewModel.AddData(t);


                        await t.WaitAsync();
                        //variable to store values in
                        var mv = new double[12];
                        var i = 0;
                        foreach (var loudSpeakerLine in (LoudspeakerLine[])Enum.GetValues(typeof(LoudspeakerLine)))
                        {
                            foreach (var lineProperty in (LineProperty[])Enum.GetValues(typeof(LineProperty)))
                            {
                                mv[i++] = t.Properties(loudSpeakerLine, lineProperty);
                            }

                        }
                        Application.Current.Dispatcher.Invoke(() => MeasuredValues = mv);

                        if (MeasureLoadFinished != null)
                            MeasureLoadFinished.Invoke();



                    }, () => CommunicationViewModel.OpenConnections.Any(s => s.ConnectMode == ConnectMode.Install));
            }
        }

        private byte[] _ks;
        /// <summary>
        /// Current Kreis Struct (for this zone)
        /// </summary>
        /// <returns></returns>
        private byte[] KreisStruct
        {
            get
            {
                if (_main.DspCopy != null)
                    return _ks ?? (_ks = _main.DspCopy.Skip(McuDat.KreisInstall).Take(72).ToArray());
                _ks = Enumerable.Range(0, 131072).Select(n => (byte)0).ToArray();
                return _ks;
            }
        }

        /// <summary>       
        /// Low Pilot 
        ///         0 1 2 
        /// Watts   A B AB
        ///         3 4 5 
        /// Ohm     A B AB
        /// High Pilot
        ///         6 7 8 
        /// Watts   A B AB
        ///         9 1011
        /// Ohm     A B AB
        /// </summary>
        public static double[] GetLoads(FlowModel flow, MainUnitModel main)
        {
            var ret = new double[12];
            if (main.DspCopy.Count < 131070)
                return Enumerable.Range(0, 12).Select(n => 0.0).ToArray();
            var ks = main.DspCopy.Skip(McuDat.KreisInstall).Skip(flow.Id * 72).Take(72).ToArray();
            for (var g = 0; g < 2; g++)
            {
                var start = 12 + g * 18;

                //stored load high pilot a, b, ab
                for (var i = 0; i < 3; i++)
                {
                    ret[i + g * 6] = (DspCoefficients.Nfb(new[]
                            {
                                ks[start + i*2],
                                ks[start + i*2 + 1]
                            }));
                }
            }
            for (var g = 0; g < 2; g++)
            {
                //add resistance to array
                for (var i = 0; i < 3; i++)
                {
                    switch (flow.AmplifierOperationMode)
                    {
                        case AmplifierOperationMode.Bridged100V:
                        case AmplifierOperationMode.Trafo:
                            var q = ret[i + g * 6];
                            ret[i + 3 + g * 6] = q < .01 ? 0 : Math.Round(10000 / ret[i + g * 6], 0);

                            break;
                        case AmplifierOperationMode.Single50V:
                            q = ret[i + g * 6];
                            ret[i + 3 + g * 6] = q < 0.01 ? 0 : Math.Round(2500 / ret[i + g * 6], 0);
                            break;
                        default:
                            ret[i + 3 + g * 6] = 0;
                            break;
                    }
                }
            }
            for (int i = 0; i < ret.Length; i++) if (ret[i] < 10) ret[i] = 0;

            return ret;
        }

        private double[] _l;

        public double[] Loads
        {
            get { return _l ?? (_l = GetLoads(_flow, _main.DataModel)); }
        }


        public bool KhzModeActivated
        {
            get
            {
                return _flow.KhzModeActivated;
            }
            set
            {
                _flow.KhzModeActivated = value;
                RaisePropertyChanged(() => KhzModeActivated);
                CommunicationViewModel.AddData(new Measurement1KEnable(new[] { value }, new[] { _flow.Id }));
            }
        }

        public bool SelectForCalibrare
        {
            get { return _flow.SelectForCalibrare; }
            set
            {
                _flow.SelectForCalibrare = value;
                RaisePropertyChanged(() => SelectForCalibrare);
            }
        }

        public bool TestMode
        {
            get { return _flow.TestModeActivated; }
            set
            {
                _flow.TestModeActivated = value;
                RaisePropertyChanged(() => TestMode);
                CommunicationViewModel.AddData(new TestMode(value, _flow.Id));
            }
        }

        public Action CalibrateFinished;
        public Action CalibrateStarted;
        public Action CalibrateUserCancel;

        private async void Calibrate()
        {

            var userinput1 = MessageBox.Show(Monitoring.mboxcalibrateZone,
                                             Monitoring.mboxcalibrateZoneTitle,
                                             MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (userinput1 == MessageBoxResult.Yes)
            {
                var ml = new CalibrateZone(_flow.Id, AmplifierCalibrationMode, PilotFrequency);
                CommunicationViewModel.AddData(ml);
                await ml.WaitAsync();

                if (CalibrateFinished != null)
                    CalibrateFinished.Invoke();



            }
            else
            {
                if (CalibrateUserCancel != null)
                    CalibrateUserCancel.Invoke();
            }
        }

        public override string SettingName
        {
            get { return string.Empty; }
        }

        public override void SetYLocation()
        {
            var row = Id % 12;
            var yspace = row > 3 ? (InnerSpace + RowHeight) * (row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight * row + yspace;
        }
    }
}