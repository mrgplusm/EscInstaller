using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace EscInstaller.ViewModel.OverView
{
    public abstract class VuMeterControl : SnapDiagramData
    {
        private readonly MainUnitViewModel _main;

        protected VuMeterControl(MainUnitViewModel main)
        {
            _main = main;
            _vuMeterAvarage = -80;
            _vuMeterCurrent = -80;
            _vuMeterMax = -80;
            main.VuMeter.VuMeterActivated += VuMeter_VuMeterActivated;
            main.VuMeter.VuDataReceived += VuMeter_VuDataReceived;
        }

        void VuMeter_VuDataReceived(object sender, VuDataReceivedEventArgs e)
        {
            if (!VuActivated) return;
            VuMeterCurrent = e.Last;
            VuMeterAvarage = e.Avarage;
            VuMeterMax = e.Max;
            LastVuMeasure = e.LastVuMeasure;
        }

        void VuMeter_VuMeterActivated(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => VuActivated);
        }

        private double _vuMeterCurrent;
        private double _vuMeterAvarage;
        private double _vuMeterMax;
        private DateTime _lastVuMeasure;

        public double VuMeterCurrent
        {
            get { return _vuMeterCurrent; }
            set
            {
                _vuMeterCurrent = value;
                RaisePropertyChanged(() => VuMeterCurrent);
            }
        }

        public double VuMeterAvarage
        {
            get { return _vuMeterAvarage; }
            set
            {
                _vuMeterAvarage = value;
                RaisePropertyChanged(() => VuMeterAvarage);
            }
        }

        public double VuMeterMax
        {
            get { return _vuMeterMax; }
            set
            {
                _vuMeterMax = value;
                RaisePropertyChanged(() => VuMeterMax);
            }
        }

        public DateTime LastVuMeasure
        {
            get { return _lastVuMeasure; }
            set
            {
                _lastVuMeasure = value;
                RaisePropertyChanged(() => LastVuMeasure);
            }
        }

        public ICommand StopVu
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _main.VuMeter.StopVuMeter();

                });
            }
        }

        public abstract ICommand StartVu { get; }
        public abstract bool VuActivated { get; }
    }
}