#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight.Command;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class BlExtInput : VuMeterControl
    {
        public const int Width = BlInputName.Width;
        public const int XLocation = 0;

        public static readonly Dictionary<int, string> Names = new Dictionary<int, string>()
        {
            {0, InputName.BlockAlarm1},
            {1, InputName.BlockAlarm2},
            {2, InputName.BlockMic1},
            {3, InputName.BlockMic2},
            {4, InputName.BlockAux}
        };

        private readonly FlowModel _flow;

        private readonly Dictionary<int, int> _vuBlock = new Dictionary<int, int>
        {
            {0, 12},
            {1, 13},
            {2, 14},
            {3, 15},
            {4, 16}
        };

        private int _channelId;
        private List<SnapShot> _snapShots;

        public BlExtInput(FlowModel flow, MainUnitViewModel unit, MainViewModel main)
            : base(unit)
        {
            _flow = flow;
            Unit = unit;
            MainViewModel = main;
            _channelId = _vuBlock[0];
            if (_flow.Id < GenericMethods.StartCountFrom)
                throw new ArgumentException("External input on flow " + _flow.Id);
            Unit.ExtinputUpdate += () => RaisePropertyChanged(() => DisplaySetting);
            Unit.DspMirrorUpdated += Receiver_DspMirrorUpdated;


            InitSliders();
            Unit.VuMeter.VuMeterActivated += (sender, args) => RaisePropertyChanged(() => VuActivated);
        }

        /// <summary>
        ///     design data only
        /// </summary>
#if DEBUG
        public BlExtInput() : base(new MainUnitViewModel())
        {
            Sliders =
                new ObservableCollection<SliderValue>(
                    Enumerable.Range(0, 5).Select(n => new SliderValue(new FlowModel() {Id = n + 500}, this)));
        }
#endif
        public MainUnitViewModel Unit { get; }
        public MainViewModel MainViewModel { get; }

        public override int Id => _flow.Id;

        public ObservableCollection<SliderValue> Sliders { get; private set; }

        public override List<SnapShot> Snapshots => _snapShots ?? (_snapShots = new List<SnapShot>()
        {
            new SnapShot(this) {Offset = {X = Size.X, Y = 17}}
        });

        public MainUnitViewModel MainUnitView => Unit;

        public string DisplaySetting => Sliders[(_flow.Id - GenericMethods.StartCountFrom)%5].Value.ToString("N2") + " dB";

        public override string SettingName => ExternalInput.Title;

        public string BlockName => Names[(_flow.Id - GenericMethods.StartCountFrom)%5];

        public override Point Size => new Point(Width, UnitHeight);

        public override ICommand StartVu
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Unit.VuMeter.SetVuChannel(_channelId);
                    Unit.VuMeter.StartVu();
                });
            }
        }

        public override bool VuActivated
        {
            get { return Unit.VuMeter.IsActive && _vuBlock.Values.Contains(Unit.VuMeter.ChannelId); }
        }

        private void InitSliders()
        {
            Sliders = new ObservableCollection<SliderValue>(Unit.DataModel.Cards.OfType<ExtensionCardModel>()
                .First()
                .Flows.Select(n => new SliderValue(n, this)));

            Sliders[0].UpdateUseVu(true);

            foreach (var sliderValue in Sliders)
            {
                sliderValue.UseVuUpdated += sliderValue_UseVuUpdated;
            }
        }

        private void Receiver_DspMirrorUpdated(object sender, EventArgs e)
        {
            foreach (var sliderValue in Sliders)
            {
                sliderValue.UpdateValue();
            }
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => DisplaySetting);
        }

        private void sliderValue_UseVuUpdated(object sender, EventArgs e)
        {
            var t = sender as SliderValue;
            if (t == null) return;
            foreach (var sliderValue in Sliders.Where(sliderValue => !sliderValue.Equals(t)))
            {
                sliderValue.UpdateUseVu(false);
            }
            _channelId = _vuBlock[(t.Id - GenericMethods.StartCountFrom)%5];

            Unit.VuMeter.SetVuChannel(_channelId);
        }

        public override void SetYLocation()
        {
            Location.Y = Unit.DataModel.ExpansionCards*5*RowHeight + 5*RowHeight
                         + ((_flow.Id - GenericMethods.StartCountFrom)%5*RowHeight)
                         + (Unit.DataModel.ExpansionCards + 1)*InnerSpace;


            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }
        }

        public void OnValueUpdate()
        {
            Unit.ExtinputUpdate?.Invoke();
        }
    }
}