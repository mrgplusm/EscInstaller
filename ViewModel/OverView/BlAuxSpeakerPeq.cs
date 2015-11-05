#region

using System;
using System.Linq;
using System.Windows;
using Common;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlAuxSpeakerPeq : PeqBaseViewModel
    {
        public const int Width = BlSpeakerPeq.Width;
        public const int XLocation = BlDelay.Width + Distance + BlDelay.XLocation;
        private readonly CardModel _card;
        private SpeakerDataViewModel _currentSpeaker;
        //readonly FlowModel _flow;

        /// <summary>
        ///     design time creation
        /// </summary>
#if DEBUG
        public BlAuxSpeakerPeq()
            : base(new MainUnitViewModel())
        {
            LibraryData.CreateEmptySystem();
            _card = LibraryData.FuturamaSys.MainUnits.First().Cards.OfType<CardModel>().First();
        }
#endif

        public BlAuxSpeakerPeq(MainUnitViewModel main, CardModel card)
            : base(main)
        {
            Location.X = XLocation;

            _card = card;
            main.PresetNamesUpdated += Receiver_PresetNamesUpdated;
        }

        public override int Id
        {
            get { return _card.Id; }
        }

        public override string SettingName
        {
            get { return string.Format(SpeakerLibrary.Title, _card.Id + 1); }
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

        public override SpeakerDataViewModel CurrentSpeaker
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new SpeakerDataViewModel(new SpeakerDataModel());
                }
                if (_currentSpeaker != null) return _currentSpeaker;
                _currentSpeaker = new SpeakerDataViewModel(Main.SpeakerDataModels[_card.Id + 12], Id);

                _currentSpeaker.SpeakerNameChanged += ((sender, args) => RaisePropertyChanged(() => DisplaySetting));
                return _currentSpeaker;
            }
        }

        protected override int PresetId
        {
            get { return _card.Id + 12; }
        }

        private void Receiver_PresetNamesUpdated(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public override void SetYLocation()
        {
            Location.Y = (InnerSpace + RowHeight*5)*_card.Id + RowHeight*4;
        }
    }
}