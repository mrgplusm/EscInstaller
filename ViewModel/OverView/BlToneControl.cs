#region

using System.Windows;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlToneControl : SnapDiagramData
    {
        public const int Width = 106;
        public const int XLocation = BlInputName.Width + Distance + BlInputName.XLocation;
        private readonly FlowModel _flow;

        public BlToneControl(FlowModel flow)
        {
            _flow = flow;
            Location.X = XLocation;
        }

        public override int Id
        {
            get { return _flow.Id; }
        }

        public override string SettingName
        {
            get { return string.Format(ToneControl._toneBlockTitle, _flow.Id + 1); }
        }

        public string DisplaySetting
        {
            get { return string.Format("B:{0}dB T:{1}dB", _flow.Bass.ToString("N0"), _flow.Treble.ToString("N0")); }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        public double Bass
        {
            get { return _flow.Bass; }
            set
            {
                _flow.Bass = (int) value;
                Update();
            }
        }

        public double Treble
        {
            get { return _flow.Treble; }
            set
            {
                _flow.Treble = (int) value;
                Update();
            }
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public override void SetYLocation()
        {
            var row = Id%12;
            Location.Y = RowHeight*row;
        }

        private void Update()
        {
            var sos = DspCoefficients.GetToneControl(_flow.Bass,
                _flow.Treble);
            bool b;
            CommunicationViewModel.AddData(bool.TryParse(LibraryData.Settings["SafeloadEnabled"], out b) && b
                ? new SafeToneControl(sos, _flow.Id)
                : new SetToneControl(sos, _flow.Id));
            RaisePropertyChanged(() => DisplaySetting);
        }
    }
}