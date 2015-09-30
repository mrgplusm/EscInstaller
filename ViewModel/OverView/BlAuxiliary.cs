using System.Linq;
using System.Windows;
using Common;
using EscInstaller.View;
using Common.Model;
using System.Collections.Generic;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings;
using Common.Commodules;

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlAuxiliary : SnapDiagramData
    {        
        readonly MainUnitViewModel _main;
        readonly CardModel _card;

        public const int Width = BlOutput.Width;
        public const int XLocation = BlMonitor.Width + Distance + BlMonitor.XLocation;

        public BlAuxiliary(MainUnitViewModel main, CardModel card)
        {
            _card = card;
            Location.X = XLocation;

            _main = main;
        }

        

        public override void SetYLocation()
        {
            Location.Y = (InnerSpace + RowHeight * 5) * _card.Id + RowHeight * 4;
        }

        public override string SettingName
        {
            get
            {
                return Auxiliary._auxBlockTitle;
            }
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
            get
            {
                return (_card.Id +1).ToString("N0");
            }
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
                CommunicationViewModel.AddData(new SetGainSlider(_card.Id, (int)value, SliderType.Auxiliary));
            }
        }

        

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }
    }
}