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
    public sealed class BlDelay : SnapDiagramData
    {
        public const int Width = 45;
        public const int XLocation = BlLink.Width + Distance + BlLink.XLocation;
        private readonly MainUnitViewModel _main;

        public BlDelay(MainUnitViewModel main, FlowModel flow)
        {
            _main = main;
            DataModel = flow;
            Location.X = XLocation;
            Location.Y = RowHeight;
            _main.DelayChanged += () => RaisePropertyChanged(() => DisplaySetting);
        }

        public string DisplaySetting
        {
            get
            {
                switch (DataModel.Id%12)
                {
                    case 1:
                        return _main.DataModel.DelayMilliseconds1.ToString("N0") + " ms";
                    case 2:
                        return _main.DataModel.DelayMilliseconds2.ToString("N0") + " ms";
                    default:
                        return string.Empty;
                }
            }
        }

        public FlowModel DataModel { get; }

        public override int Id
        {
            get { return DataModel.Id; }
        }

        public override string SettingName
        {
            get { return Delay._delayBlockTitle; }
        }

        public double Delayms1
        {
            get { return _main.DataModel.DelayMilliseconds1; }
            set
            {
                if (Math.Abs(_main.DataModel.DelayMilliseconds1 - value) < .7) return;
                _main.DataModel.DelayMilliseconds1 = value;
                RaisePropertyChanged(() => Delayms1);
                UpdateDisplayValue();
                CommunicationViewModel.AddData(new SetDelay(1, Delayms1, _main.Id));
            }
        }

        public double Delayms2
        {
            get { return _main.DataModel.DelayMilliseconds2; }
            set
            {
                if (Math.Abs(_main.DataModel.DelayMilliseconds2 - value) < .7) return;
                _main.DataModel.DelayMilliseconds2 = value;
                RaisePropertyChanged(() => Delayms2);
                UpdateDisplayValue();
                CommunicationViewModel.AddData(new SetDelay(2, Delayms2, _main.Id));
            }
        }

        public double MaxDelay
        {
            get { return SetDelay.MaxSingleDelay; }
        }

        public double MaxMeter
        {
            get { return SetDelay.Meter*MaxDelay; }
        }

        public double MaxFeet
        {
            get { return SetDelay.Feet*MaxDelay; }
        }

        //todo: implement mcu
        public bool ChainDelays
        {
            get { return _main.DataModel.ChainDelays; }
            set { _main.DataModel.ChainDelays = value; }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        public override void SetYLocation()
        {
            var row = Id%12;
            var yspace = row > 3 ? (InnerSpace + RowHeight)*(row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight*row + yspace;
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        private void UpdateDisplayValue()
        {
            if (_main.DelayChanged != null)
                _main.DelayChanged();
        }
    }
}