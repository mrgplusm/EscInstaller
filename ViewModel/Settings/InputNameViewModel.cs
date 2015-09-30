using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using EscFileHandler.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using McuCommunication;
using McuCommunication.Commodules;

namespace EscInstaller.ViewModel.Settings
{
    public class InputNameViewModel : SettingsBaseViewModel
    {
        private readonly Timer _vuTimer = new Timer
                                              {
                                                  Interval = 300,
                                                  AutoReset = true,
                                                  Enabled = false
                                              };
        public Action VuValueChanged;
        private double _vuMeterValue;


        // textbox        
        public InputNameViewModel()
        {
            VuMeterValue = 0;
            Messenger.Default.Register<int>(this, "ActivateVu", s =>
                {
                    if (s == CurrenttMainUnit.Id)
                        RaisePropertyChanged(() => ActivateVu);
                });

            Messenger.Default.Register<int>(this, "MonitorSliderUpdate",
                                            s =>
                                            {
                                                if (s == CurrenttMainUnit.Id)
                                                    RaisePropertyChanged(() => MonitorSlider);
                                            });


            _vuTimer.Elapsed += TimerElapsedEvent;
        }

        public string SensAndGainBlockHeader
        {
            get
            {
                return (Id < ConnStatMethods.StartCountFrom)
                           ? InputName._inputGroupSensitivity
                           : InputName._inputGroupGain;
            }
        }

        public string NameOfInput
        {
            get
            {
                return CurrentFlow.NameOfInput;
            }
            set
            {
                CurrentFlow.NameOfInput = value;

                RaisePropertyChanged(() => NameOfInput);
                Messenger.Default.Send(Id, "InpName");

                try
                {
                    var nameU = new NameUpdate(Id, CurrentFlow.NameOfInput, NameType.Input);
                    AddData(nameU);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (CurrentFlow.InputNameNotEqualsOutput) return;
                CurrentFlow.NameOfOutput = CurrentFlow.NameOfInput;
                AddData(new NameUpdate(Id, value, NameType.Output));
                Messenger.Default.Send(Id, "InpName");
            }
        }

        public bool InputsensitivityIsEnabled
        {
            get { return bool.Parse(LibraryData.Settings["InputsensitivityIsEnabled"]); }
        }

        //checkbox
        public bool CopyNameOutput
        {
            get { return !CurrentFlow.InputNameNotEqualsOutput; }
            set
            {
                CurrentFlow.InputNameNotEqualsOutput = !value;
                if (!value) return;
                CurrentFlow.NameOfOutput = CurrentFlow.NameOfInput;
                Messenger.Default.Send(Id, "InpName");
                try
                {
                    var nameU = new NameUpdate(Id, CurrentFlow.NameOfOutput, NameType.Output);
                    AddData(nameU);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        //VU meter                 
        public bool ActivateVu
        {
            get { return LibraryData.VuListening(CurrenttMainUnit.Id) == Id; }
            set
            {
                RouteAux = false;
                if (!value)
                {
                    //_timer.Stop();
                    VuMeterValue = 0;
                    LibraryData.SetVuListening(CurrenttMainUnit.Id, -1);
                    _vuTimer.Stop();
                }
                else
                {
                    // _timer.Start();
                    LibraryData.SetVuListening(CurrenttMainUnit.Id, Id);

                    VuMutePos unmute;
                    if (Id >= ConnStatMethods.StartCountFrom)
                    {
                        unmute = (VuMutePos)12 + (Id - ConnStatMethods.StartCountFrom) % 5;
                    }
                    else
                    {
                        unmute = (VuMutePos)(Id % 12);
                    }

                    //mute all the muteblocks except for this channel
                    foreach (VuMutePos vuMutePo in Enum.GetValues(typeof(VuMutePos)))
                    {
                        AddData(unmute.Equals(vuMutePo)
                                    ? new MuteBlock(vuMutePo, false, Id / 12)
                                    : new MuteBlock(vuMutePo, true, Id / 12));
                    }

                    _vuTimer.Start();
                }
                Messenger.Default.Send(CurrenttMainUnit.Id, "ActivateVu");
            }
        }

        //readonly Random _r = new Random(DateTime.Now.Millisecond);


        //route signal to aux.        
        public bool RouteAux
        {
            get { return LibraryData.SelectedAux(CurrenttMainUnit.Id) == Id; }
            set
            {
                if (!value) LibraryData.SetSelectedAux(CurrenttMainUnit.Id, -1);
                else
                    LibraryData.SetSelectedAux(CurrenttMainUnit.Id, Id);
                AddData(new AuxDemux(value, Id));
            }
        }

        public double MonitorSlider
        {
            get { return CurrenttMainUnit.MonitorSlider; }
            set
            {
                CurrenttMainUnit.MonitorSlider = value;
                AddData(new SetGainSlider(Id, value, SliderType.Monitoring));
                Messenger.Default.Send(CurrenttMainUnit.Id, "MonitorSliderUpdate");
            }
        }

        public InputSens InputSens
        {
            get
            {
                if (CurrenttMainUnit.InputSensitivity == null
                    || CurrenttMainUnit.InputSensitivity.Count < 12 || Id > ConnStatMethods.StartCountFrom)
                    return InputSens.None;
                return CurrenttMainUnit.InputSensitivity[Id % 12];
            }
            set
            {
                if (CurrenttMainUnit.InputSensitivity == null
                    || CurrenttMainUnit.InputSensitivity.Count < 12 || Id > ConnStatMethods.StartCountFrom)
                    return;
                CurrenttMainUnit.InputSensitivity[Id % 12] = value;
                RaisePropertyChanged(() => InputSens);
                AddData(new SetInputSensitivity(Id, value));
            }
        }

        //Slider
        public double InputSlider
        {
            get { return CurrentFlow.InputSlider; }
            set
            {
                AddData(new SetGainSlider(Id, (int)value, SliderType.Input));
                CurrentFlow.InputSlider = (int)value;
            }
        }


        /// <summary>
        ///     Value between 0 and 30
        /// </summary>
        public double VuMeterValue
        {
            get { return _vuMeterValue; }
            private set
            {
                _vuMeterValue = value;
                RaisePropertyChanged(() => VuMeterValue);

                if (VuValueChanged != null)
                    VuValueChanged.Invoke();
            }
        }


        public string Titlebox
        {
            get
            {
                if (IsExtension)
                    return string.Format(Names[(Id - ConnStatMethods.StartCountFrom) % 5], DisplayId);
                return string.Format(Names[5], DisplayId);
            }
        }

        public static Dictionary<int, string> Names = new Dictionary<int, string>()
        {                        
            {0, InputName.TitleAlarm},
            {1, InputName.TitleAlarm},
            {2, InputName.TitleMic},
            {3, InputName.TitleMic},
            {4, InputName.TitleAux},
            {5, InputName.Title},
        };



        public ICommand GetSystemValues
        {
            get { return new RelayCommand(GetValues); }
        }

        public bool IsAlert
        {
            get { return (Id >= ConnStatMethods.StartCountFrom && (Id - ConnStatMethods.StartCountFrom) % 5 == 1); }
        }

        /// <summary>
        /// Alarm, Mic1, Mic2, Aux, Pilot 
        /// </summary>
        public bool IsExtension
        {
            get { return Id >= ConnStatMethods.StartCountFrom; }
        }

        public override void Cleanup()
        {
            _vuTimer.Dispose();
            CurrentFlow.ActivateVu = false;
        }

        private void TimerElapsedEvent(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _vuTimer.Stop();
            //VuMeterValue = _r.Next(0,100);
            var z = new GetVu(Id);
            z.ReceivedModule += VuMeterReceived;
            AddData(z);
        }

        private void VuMeterReceived(GetVu getVu)
        {
            VuMeterValue = getVu.VuMeterValue;
            _vuTimer.Start();
        }

        private void GetValues()
        {
        }
    }
}