#region

using System.Windows;
using Common.Commodules;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlAuxiliary : SnapDiagramData
    {
        public const int Width = BlOutput.Width;
        public const int XLocation = BlMonitor.Width + Distance + BlMonitor.XLocation;
        private readonly CardModel _card;
        private readonly MainUnitViewModel _main;

        public BlAuxiliary(MainUnitViewModel main, CardModel card)
        {
            _card = card;
            Location.X = XLocation;

            _main = main;
        }

        public override string SettingName
        {
            get { return Auxiliary._auxBlockTitle; }
        }

        public string DisplaySetting
        {
            get
            {
                var t = _main.DataModel.SpeakerDataModels[_card.Id + 12];
                return SpeakerDataViewModel.DisplayValue(t, true);
            }
        }

        public string DisplayId
        {
            get { return (_card.Id + 1).ToString("N0"); }
        }

        public override int Id
        {
            get { return _card.Id; }
        }

        public double AuxGainSlider
        {
            get { return _card.AuxGainSlider; }
            set
            {
                _card.AuxGainSlider = value;
                CommunicationViewModel.AddData(new SetGainSlider(_card.Id, (int) value, SliderType.Auxiliary));
            }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        public override void SetYLocation()
        {
            Location.Y = (InnerSpace + RowHeight*5)*_card.Id + RowHeight*4;
        }
    }
}