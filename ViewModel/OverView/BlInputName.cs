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
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class BlInputName : VuMeterControl
    {
        public const int Width = 110;
        public const int XLocation = 0;
        private readonly FlowModel _flow;
        private readonly MainUnitViewModel _main;
        private List<SnapShot> _snapShots;

        public BlInputName(FlowModel flow, MainUnitViewModel main)
            : base(main)
        {
            _flow = flow;
            _main = main;


            //main.SensitivityDownloaded += ReceiverOnSensitivityDownloaded;
            main.PresetNamesUpdated += Receiver_PresetNamesUpdated;
            main.DspMirrorUpdated += Receiver_DspMirrorUpdated;
            main.MonitorSliderUpdated += (a, b) => RaisePropertyChanged(() => MonitorSlider);
        }

        public override ICommand StartVu
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _main.VuMeter.SetVuChannel(_flow.Id%12);
                    _main.VuMeter.StartVu();
                });
            }
        }

        public override bool VuActivated
        {
            get { return _main.VuMeter.ChannelId == _flow.Id%12 && _main.VuMeter.IsActive; }
        }

        public override int Id
        {
            get { return _flow.Id; }
        }

        public int DisplayId
        {
            get { return _flow.Id + 1; }
        }

        public override string SettingName
        {
            get { return string.Format(InputName.BlockInput, _flow.Id + 1); }
        }

        public override sealed List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>()
                {
                    new SnapShot(this) {Offset = {X = Size.X, Y = 17}}
                });
            }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        public virtual string NameOfInput
        {
            get { return _flow.NameOfInput; }
            set
            {
                _flow.NameOfInput = value;

                RaisePropertyChanged(() => NameOfInput);

                var nameU = new NameUpdate(_flow.Id, _flow.NameOfInput, NameType.Input);
                CommunicationViewModel.AddData(nameU);

                if (!_flow.InputNameNotEqualsOutput) return;
                _flow.NameOfOutput = _flow.NameOfInput;
                CommunicationViewModel.AddData(new NameUpdate(_flow.Id, value, NameType.Output));
                Messenger.Default.Send(_flow.Id, "InpName");
            }
        }

        public bool InputsensitivityIsEnabled
        {
            get
            {
                bool b;
                return bool.TryParse(LibraryData.Settings["InputsensitivityIsEnabled"], out b) && b;
            }
        }

        //checkbox
        public bool CopyNameOutput
        {
            get { return _flow.InputNameNotEqualsOutput; }
            set
            {
                _flow.InputNameNotEqualsOutput = value;
                if (!value) return;
                _flow.NameOfOutput = _flow.NameOfInput;
                UpdateOutputName();
                try
                {
                    var nameU = new NameUpdate(_flow.Id, _flow.NameOfOutput, NameType.Output);
                    CommunicationViewModel.AddData(nameU);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        //route signal to aux.        
        public bool RouteAux
        {
            get { return LibraryData.SelectedAux(_main.Id) == _flow.Id; }
            set
            {
                if (!value) LibraryData.SetSelectedAux(_main.Id, -1);
                else
                    LibraryData.SetSelectedAux(_main.Id, _flow.Id);
                CommunicationViewModel.AddData(new AuxDemux(value, _flow.Id));
                RaisePropertyChanged(() => RouteAux);
            }
        }

        public double MonitorSlider
        {
            get { return _main.MonitorSlider; }
            set
            {
                _main.MonitorSlider = value;
                CommunicationViewModel.AddData(new SetGainSlider(_flow.Id, value, SliderType.Monitoring));
            }
        }

        public InputSens InputSens
        {
            get
            {
                if (_main.DataModel.InputSensitivity == null
                    || _main.DataModel.InputSensitivity.Count < 12 || _flow.Id > GenericMethods.StartCountFrom)
                    return InputSens.None;
                return _main.DataModel.InputSensitivity[_flow.Id%12];
            }
            set
            {
                if (_main.DataModel.InputSensitivity == null
                    || _main.DataModel.InputSensitivity.Count < 12 || _flow.Id > GenericMethods.StartCountFrom)
                    return;
                _main.DataModel.InputSensitivity[_flow.Id%12] = value;
                RaisePropertyChanged(() => InputSens);
                CommunicationViewModel.AddData(new SetInputSensitivity(_flow.Id, value));
            }
        }

        //Slider
        public double InputSlider
        {
            get { return _flow.InputSlider; }
            set
            {
                CommunicationViewModel.AddData(new SetGainSlider(_flow.Id, (int) value, SliderType.Input));
                _flow.InputSlider = (int) value;
            }
        }

        public bool IsAlert
        {
            get
            {
                return (_flow.Id >= GenericMethods.StartCountFrom && (_flow.Id - GenericMethods.StartCountFrom)%5 == 1);
            }
        }

        private void Receiver_DspMirrorUpdated(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => InputSlider);
        }

        private void Receiver_PresetNamesUpdated(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => NameOfInput);
        }

        private void ReceiverOnSensitivityDownloaded(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(() => InputSens);
        }

        public override void SetYLocation()
        {
            var row = Id%12;
            var yspace = row > 3 ? (InnerSpace + RowHeight)*(row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight*row + yspace;
        }

        private void UpdateOutputName()
        {
            var t = _main.DiagramObjects.OfType<BlOutput>().FirstOrDefault(i => i.Id == Id);
            if (t != null)
                t.UpdateName();
        }
    }
}