#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.SDCard;
using GalaSoft.MvvmLight.CommandWpf;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlMonitor : SnapDiagramData
    {
        public const int Width = 85;
        public const int XLocation = BlSpeakerPeq.Width + Distance + BlSpeakerPeq.XLocation;
        private readonly CardModel _card;

        private readonly Dictionary<int, int> _flowMux = new Dictionary<int, int>
        {
            {0, 3},
            {1, 4},
            {2, 5},
            {3, 5},
            {4, 3},
            {5, 3},
            {6, 3},
            {7, 3},
            {8, 3},
            {9, 3},
            {10, 3},
            {11, 3}
        };

        private readonly MainUnitViewModel _main;
        private byte[] _ks;
        private double[] _l;
        private bool _playTestTone;
        public Action CalibrateFinished;
        public Action CalibrateStarted;
        public Action CalibrateUserCancel;
        public Action MeasureLoadFinished;
        public Action MeasureLoadStarted;

        public BlMonitor(FlowModel flow, MainUnitViewModel main, CardModel card)
        {
            _card = card;
            _main = main;
            Location.X = XLocation;
            Flow = flow;
            _main.KreisUpdated += ReceiverOnEepromReceived;
        }

        public override int Id
        {
            get { return Flow.Id; }
        }

        public string DisplaySetting
        {
            get
            {
                return Flow.HasAmplifier
                    ? Flow.AmplifierOperationMode.ToString()
                    : string.Empty;
            }
        }

        public FlowModel Flow { get; }

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
            get { return Flow.HasAmplifier; }
        }

        public override Point Size
        {
            get
            {
                return new Point(Width,
                    UnitHeight);
            }
        }

        public SdMessageViewModel SelectedMessage { get; set; }
        public string ReferenceValue { get; set; } = "4";

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

        public bool TestToneEnabled { get; set; } = true;

        public AmplifierCalibrationMode AmplifierCalibrationMode
        {
            get { return Flow.AmplifierCalibrationMode; }
            set
            {
                Flow.AmplifierCalibrationMode = value;
                RaisePropertyChanged(() => AmplifierCalibrationMode);
            }
        }

        public ICommand CalibrateCommand
        {
            get
            {
                return new RelayCommand(Calibrate,
                    () => CommunicationViewModel.OpenConnections.Any(s => s.ConnectMode == ConnectMode.Install));
            }
        }

        public bool EighOhms
        {
            get { return _card.EightOhms; }
            set
            {
                _card.EightOhms = value;
                RaisePropertyChanged(() => EighOhms);
            }
        }

        public int DeviationHp
        {
            get { return Flow.DeviationHp; }
            set
            {
                Flow.DeviationHp = value;
                RaisePropertyChanged(() => DeviationHp);
                CommunicationViewModel.AddData(new SetDeviation(Flow.Id, value + 1, false));
            }
        }

        public int DeviationLp
        {
            get { return Flow.DeviationLp; }
            set
            {
                Flow.DeviationLp = value;
                RaisePropertyChanged(() => DeviationLp);
                CommunicationViewModel.AddData(new SetDeviation(Flow.Id, value + 1, true));
            }
        }

        public string PilotfrequencyInt
        {
            get { return (PilotFrequency*0.001) + " KHz"; }
        }

        public double PilotFrequency
        {
            get
            {
                if (Flow.PilotFrequency < 1)
                    Flow.PilotFrequency = 20000;
                return Flow.PilotFrequency;
            }
            set
            {
                Flow.PilotFrequency = value;
                RaisePropertyChanged(() => PilotFrequency);
            }
        }

        /// <summary>
        ///     LpWatt  LpOhm   HpWatt  HpOhm
        ///     A       A       A       A
        ///     B       B       B       B
        ///     AB      AB      AB      AB
        /// </summary>
        public double[] MeasuredValues
        {
            get
            {
                if (Flow.MeasuredValues == null || Flow.MeasuredValues.Length < 12)
                    Flow.MeasuredValues = new double[12];
                return Flow.MeasuredValues;
            }
            set
            {
                Flow.MeasuredValues = value;
                RaisePropertyChanged(() => MeasuredValues);
            }
        }

        public ICommand MeasureLoad
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var t = new MeasureLoad(Flow.Id, AmplifierCalibrationMode, PilotFrequency);
                    CommunicationViewModel.AddData(t);


                    await t.WaitAsync();
                    //variable to store values in
                    var mv = new double[12];
                    var i = 0;
                    foreach (var loudSpeakerLine in (LoudspeakerLine[]) Enum.GetValues(typeof (LoudspeakerLine)))
                    {
                        foreach (var lineProperty in (LineProperty[]) Enum.GetValues(typeof (LineProperty)))
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

        /// <summary>
        ///     Current Kreis Struct (for this zone)
        /// </summary>
        /// <returns></returns>
        private byte[] KreisStruct
        {
            get
            {
                if (_main.DspCopy != null)
                    return _ks ?? (_ks = _main.DspCopy.Skip(McuDat.KreisInstall).Take(72).ToArray());
                _ks = Enumerable.Range(0, 131072).Select(n => (byte) 0).ToArray();
                return _ks;
            }
        }

        public double[] Loads
        {
            get { return _l ?? (_l = GetLoads(Flow, _main.DataModel)); }
        }

        public bool KhzModeActivated
        {
            get { return Flow.KhzModeActivated; }
            set
            {
                Flow.KhzModeActivated = value;
                RaisePropertyChanged(() => KhzModeActivated);
                CommunicationViewModel.AddData(new Measurement1KEnable(new[] {value}, new[] {Flow.Id}));
            }
        }

        public bool SelectForCalibrare
        {
            get { return Flow.SelectForCalibrare; }
            set
            {
                Flow.SelectForCalibrare = value;
                RaisePropertyChanged(() => SelectForCalibrare);
            }
        }

        public bool TestMode
        {
            get { return Flow.TestModeActivated; }
            set
            {
                Flow.TestModeActivated = value;
                RaisePropertyChanged(() => TestMode);
                CommunicationViewModel.AddData(new TestMode(value, Flow.Id));
            }
        }

        public override string SettingName
        {
            get { return string.Empty; }
        }

        private void ReceiverOnEepromReceived(object sender, EventArgs eventArgs)
        {
            _l = GetLoads(Flow, _main.DataModel);
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        private void ReceivedDemuxByte(GetDspValue d)
        {
            //set demuxer for this channel
            var data =
                (DspCoefficients.DemuxBytes(McuDat.DemuxAlarm[Flow.Id%12], _flowMux[Flow.Id%12] + 2,
                    _flowMux[Flow.Id%12])).ToArray();
            var q = new DspData(data, _main.Id);

            CommunicationViewModel.AddData(q);
        }

        /// <summary>
        ///     Low Pilot
        ///     0 1 2
        ///     Watts   A B AB
        ///     3 4 5
        ///     Ohm     A B AB
        ///     High Pilot
        ///     6 7 8
        ///     Watts   A B AB
        ///     9 1011
        ///     Ohm     A B AB
        /// </summary>
        public static double[] GetLoads(FlowModel flow, MainUnitModel main)
        {
            var ret = new double[12];
            if (main.DspCopy.Count < 131070)
                return Enumerable.Range(0, 12).Select(n => 0.0).ToArray();
            var ks = main.DspCopy.Skip(McuDat.KreisInstall).Skip(flow.Id*72).Take(72).ToArray();
            for (var g = 0; g < 2; g++)
            {
                var start = 12 + g*18;

                //stored load high pilot a, b, ab
                for (var i = 0; i < 3; i++)
                {
                    ret[i + g*6] = (DspCoefficients.Nfb(new[]
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
                            var q = ret[i + g*6];
                            ret[i + 3 + g*6] = q < .01 ? 0 : Math.Round(10000/ret[i + g*6], 0);

                            break;
                        case AmplifierOperationMode.Single50V:
                            q = ret[i + g*6];
                            ret[i + 3 + g*6] = q < 0.01 ? 0 : Math.Round(2500/ret[i + g*6], 0);
                            break;
                        default:
                            ret[i + 3 + g*6] = 0;
                            break;
                    }
                }
            }
            for (var i = 0; i < ret.Length; i++) if (ret[i] < 10) ret[i] = 0;

            return ret;
        }

        private async void Calibrate()
        {
            var userinput1 = MessageBox.Show(Monitoring.mboxcalibrateZone,
                Monitoring.mboxcalibrateZoneTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (userinput1 == MessageBoxResult.Yes)
            {
                var ml = new CalibrateZone(Flow.Id, AmplifierCalibrationMode, PilotFrequency);
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

        public override void SetYLocation()
        {
            var row = Id%12;
            var yspace = row > 3 ? (InnerSpace + RowHeight)*(row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight*row + yspace;
        }
    }
}