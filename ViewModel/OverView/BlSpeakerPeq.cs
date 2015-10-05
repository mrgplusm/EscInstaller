using EscInstaller.View;
using Common.Model;
using EscInstaller.ViewModel.Settings;
using System.Windows;
using EscInstaller.ViewModel.Settings.Peq;

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlSpeakerPeq : PeqBaseViewModel
    {

        private readonly FlowModel _flow;


#if DEBUG
        public BlSpeakerPeq()
            : base(new MainUnitViewModel())
        {
            _flow = new FlowModel();
        }
#endif

        public const int Width = 107;
        public const int XLocation = BlDelay.Width + Distance + BlDelay.XLocation;
        public BlSpeakerPeq(FlowModel flow, MainUnitViewModel main)
            : base(main)
        {
            Location.X = XLocation;

            _flow = flow;
            main.EepromHandler.PresetNamesUpdated += Receiver_PresetNamesUpdated;
        }

        void Receiver_PresetNamesUpdated(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(()=> DisplaySetting);
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        public FlowModel DataModel
        {
            get { return _flow; }
        }

        public override int Id
        {
            get { return _flow.Id; }
        }

        public override string SettingName
        {
            get { return string.Format(SpeakerLibrary.Title, _flow.Id + 1); }
        }

        public override void SetYLocation()
        {
            var row = Id % 12;
            var yspace = row > 3 ? (InnerSpace + RowHeight) * (row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight * row + yspace;
        }

        public override string DisplaySetting
        {
            get
            {
                var t = Main.DataModel.SpeakerDataModels[_flow.Id % 12];
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
                if (_currentSpeaker == null)
                {
                    _currentSpeaker = new SpeakerDataViewModel(
                        Main.SpeakerDataModels[_flow.Id%12], Id);
                }
                _currentSpeaker.SpeakerNameChanged = ()=> RaisePropertyChanged(()=>DisplaySetting);
                return _currentSpeaker;
                
                //return (SpeakerDataViewModel)SpeakerSelect.CurrentItem; 
            }
        }

        protected override int PresetId
        {
            get { return _flow.Id; }
        }

    }
}