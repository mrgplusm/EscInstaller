#region

using System;
using System.Windows;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlSpeakerPeq : PeqBaseViewModel
    {
        public const int Width = 107;
        public const int XLocation = BlDelay.Width + Distance + BlDelay.XLocation;
        private SpeakerDataViewModel _currentSpeaker;
#if DEBUG
        public BlSpeakerPeq()
            : base(new MainUnitViewModel())
        {
            DataModel = new FlowModel();
        }
#endif

        public BlSpeakerPeq(FlowModel flow, MainUnitViewModel main)
            : base(main)
        {
            Location.X = XLocation;

            DataModel = flow;
            main.PresetNamesUpdated += Receiver_PresetNamesUpdated;
        }

        public FlowModel DataModel { get; }

        public override int Id
        {
            get { return DataModel.Id; }
        }

        public override string SettingName
        {
            get { return string.Format(SpeakerLibrary.Title, DataModel.Id + 1); }
        }

        public override string DisplaySetting
        {
            get
            {
                var t = Main.DataModel.SpeakerDataModels[DataModel.Id%12];
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
                if (_currentSpeaker == null)
                {
                    _currentSpeaker = new SpeakerDataViewModel(
                        Main.SpeakerDataModels[DataModel.Id%12], Id);
                }
                _currentSpeaker.SpeakerNameChanged += (sender, args) => RaisePropertyChanged(() => DisplaySetting);
                return _currentSpeaker;

                //return (SpeakerDataViewModel)SpeakerSelect.CurrentItem; 
            }
        }

        protected override int PresetId
        {
            get { return DataModel.Id; }
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
            var row = Id%12;
            var yspace = row > 3 ? (InnerSpace + RowHeight)*(row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight*row + yspace;
        }
    }
}