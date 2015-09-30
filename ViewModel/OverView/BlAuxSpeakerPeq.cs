using System.Linq;
using System.Windows;
using Common;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Settings;

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlAuxSpeakerPeq : PeqBaseViewModel
    {
        //readonly FlowModel _flow;

        /// <summary>
        /// design time creation
        /// </summary>
#if DEBUG
        public BlAuxSpeakerPeq()
            : base(new MainUnitViewModel())
        {
            LibraryData.CreateEmptySystem();
            _card = LibraryData.FuturamaSys.MainUnits.First().Cards.OfType<CardModel>().First();
        }
#endif

        private readonly CardModel _card;

        public const int Width = BlSpeakerPeq.Width;
        public const int XLocation = BlDelay.Width + Distance + BlDelay.XLocation;
        public BlAuxSpeakerPeq(MainUnitViewModel main, CardModel card)
            : base(main)
        {
            Location.X = XLocation;

            _card = card;
            main.EepromHandler.PresetNamesUpdated += Receiver_PresetNamesUpdated;
        }

        void Receiver_PresetNamesUpdated(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public override int Id
        {
            get { return _card.Id; }
        }

        public override string SettingName
        {
            get { return string.Format(SpeakerLibrary.Title, _card.Id + 1); }
        }

        public override void SetYLocation()
        {
            Location.Y = (InnerSpace + RowHeight * 5) * _card.Id + RowHeight * 4;
        }

        public override string DisplaySetting
        {
            get
            {
                var t = Main.DataModel.SpeakerDataModels[12 + _card.Id];
                return SpeakerDataViewModel.DisplayValue(t, true);
            }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }



        private SpeakerDataViewModel _currentSpeaker;
        public override SpeakerDataViewModel CurrentSpeaker
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new SpeakerDataViewModel(new SpeakerDataModel());
                }
                return _currentSpeaker ?? (_currentSpeaker = new SpeakerDataViewModel(Main.SpeakerDataModels[_card.Id + 12], Id) { SpeakerNameChanged = () => RaisePropertyChanged(() => DisplaySetting) });

            }
        }



        protected override int PresetId
        {
            get { return _card.Id + 12; }
        }
    }
}