#region

using System;
using System.Windows;
using Common.Commodules;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlOutput : SnapDiagramData
    {
        public const int Width = 100;
        public const int XLocation = BlMonitor.Width + Distance + BlMonitor.XLocation;
        private readonly FlowModel _flow;

        public BlOutput(FlowModel flow, MainUnitViewModel main)
        {
            _flow = flow;
            Location.X = XLocation;
            main.PresetNamesUpdated += Receiver_PresetNamesUpdated;
            main.DspMirrorUpdated += ReceiverOnDspMirrorUpdated;
        }

        public override int Id
        {
            get { return _flow.Id; }
        }

        public string DisplayId
        {
            get { return (_flow.Id + 1).ToString("N0"); }
        }

        public bool HasAmplifier
        {
            get { return _flow.HasAmplifier; }
            set { _flow.HasAmplifier = value; }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        public string NameOfOutput
        {
            get { return _flow.NameOfOutput; }
            set
            {
                _flow.NameOfOutput = value;

                if (string.IsNullOrWhiteSpace(value)) return;

                RaisePropertyChanged(() => NameOfOutput);
                CommunicationViewModel.AddData(new NameUpdate(_flow.Id, value, NameType.Output));
            }
        }

        public double Gain
        {
            get { return _flow.PageGain; }
            set
            {
                _flow.PageGain = value;
                CommunicationViewModel.AddData(new SetGainSlider(_flow.Id, value, SliderType.Page));
            }
        }

        public double OutputGain
        {
            get { return _flow.OutputGain; }
            set
            {
                _flow.OutputGain = value;
                CommunicationViewModel.AddData(new SetGainSlider(_flow.Id, value, SliderType.Output));
            }
        }

        public override string SettingName
        {
            get { return string.Format(PageGain._outputBlockTitle, _flow.Id + 1); }
        }

        private void ReceiverOnDspMirrorUpdated(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(() => Gain);
            RaisePropertyChanged(() => OutputGain);
        }

        private void Receiver_PresetNamesUpdated(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => NameOfOutput);
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => NameOfOutput);
        }

        public override void SetYLocation()
        {
            var row = Id%12;
            var yspace = row > 3 ? (InnerSpace + RowHeight)*(row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight*row + yspace;
        }
    }
}