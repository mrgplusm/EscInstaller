#region

using System.Windows;
using Common;
using Common.Model;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlInputPeq : PeqBaseViewModel
    {
        public const int Width = BlToneControl.Width;
        public const int XLocation = BlExtInput.Width + Distance + BlExtInput.XLocation;
        private readonly FlowModel _flow;
        private SpeakerDataViewModel _currentSpeaker;
#if DEBUG
        public BlInputPeq()
            : base(new MainUnitViewModel(LibraryData.EmptyEsc(1), new MainViewModel()))
        {
        }
#endif

        public BlInputPeq(FlowModel flow, MainUnitViewModel main)
            : base(main)
        {
            Location.X = XLocation;

            _flow = flow;
            SetYLocation();
        }

        public override string SettingName
        {
            get
            {
                return string.Format(EscInstaller.Main.BlockTitleInputPeq,
                    _flow.Id - 1 - (GenericMethods.StartCountFrom%5));
            }
        }

        public override int Id
        {
            get { return _flow.Id; }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        protected override int PresetId
        {
            get { return _flow.Id; }
        }

        public override string DisplaySetting
        {
            get { return string.Empty; }
        }

        public override SpeakerDataViewModel CurrentSpeaker
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new SpeakerDataViewModel(new SpeakerDataModel());
                }
                //extflowId 2 & 3, position 15&16 => +13

                if (_currentSpeaker != null) return _currentSpeaker;
                _currentSpeaker =
                    new SpeakerDataViewModel(
                        Main.SpeakerDataModels[(_flow.Id - GenericMethods.StartCountFrom)%5 + 13], Id);
                _currentSpeaker.SpeakerNameChanged += (sender, args) => RaisePropertyChanged(() => DisplaySetting);
                return _currentSpeaker;
            }
        }

        public int InputPeqId
        {
            get { return (_flow.Id - GenericMethods.StartCountFrom)%5; }
        }

        public override void SetYLocation()
        {
            Location.Y =
                Main.DataModel.ExpansionCards*5*RowHeight + 5*RowHeight
                + ((_flow.Id - GenericMethods.StartCountFrom)%5*RowHeight)
                + (Main.DataModel.ExpansionCards + 1)*InnerSpace;

            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }
        }
    }
}