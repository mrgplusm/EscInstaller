#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Common.Commodules;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class VuMeter
    {
        private readonly MainUnitViewModel _main;

        private readonly Timer _vuTimer = new Timer
        {
            Interval = 500,
            AutoReset = false,
            Enabled = false
        };

        private readonly List<double> _vuValues = new List<double>();
        private bool _isActive;

        public VuMeter(MainUnitViewModel main)
        {
            _main = main;
            _vuTimer.Elapsed += VuTimerEvent;
            
            _vuValues.Add(-80);
        }

        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                _isActive = value;
                OnVuMeterActivated();
            }
        }

        public int ChannelId { get; private set; }
        public event EventHandler VuMeterActivated;

        protected virtual void OnVuMeterActivated()
        {
            var handler = VuMeterActivated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void StopVuMeter()
        {
            _vuValues.Clear();
            _vuValues.Add(-80);
            _vuTimer.Stop();
            IsActive = false;
        }

        /// <summary>
        ///     mute all the muteblocks except for this channel
        /// </summary>
        /// <param name="channelId">
        ///     0-11 = Flow1-12,
        ///     12 = Alarm1,
        ///     13 = Alarm2,
        ///     14 = Mic1,
        ///     15 = Mic2,
        ///     16 = ExtAudio,
        ///     17 = Pilote,
        ///     18 = Spdif1,
        ///     19 = Spdif2,
        /// </param>
        public void SetVuChannel(int channelId)
        {
            ChannelId = channelId;
            for (var i = 0; i < 20; i++)
            {
                CommunicationViewModel.AddData((i == ChannelId)
                    ? new MuteBlock(i, false, _main.Id)
                    : new MuteBlock(i, true, _main.Id));
            }
        }

        public void StartVu()
        {
            _vuTimer.Start();
            IsActive = true;
        }

        private async void VuTimerEvent(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _vuTimer.Stop();
            //VuMeterValue = _r.Next(0,100);
            var z = new GetVu(_main.Id);
            CommunicationViewModel.AddData(z);
            await z.WaitAsync();

            _vuValues.Add(z.VuMeterValue);
            OnVuDataReceived(new VuDataReceivedEventArgs
            {
                Last = _vuValues.Last(),
                Max = _vuValues.Max(),
                Avarage = _vuValues.Average(),
                LastVuMeasure = DateTime.Now,
                Channel = ChannelId
            });
            if (IsActive)
                _vuTimer.Start();
        }

        public event EventHandler<VuDataReceivedEventArgs> VuDataReceived;

        protected virtual void OnVuDataReceived(VuDataReceivedEventArgs e)
        {
            var handler = VuDataReceived;
            if (handler != null) handler(this, e);
        }
    }

    public class VuDataReceivedEventArgs : EventArgs
    {
        public int Channel { get; set; }
        public double Last { get; set; }
        public double Avarage { get; set; }
        public double Max { get; set; }
        public DateTime LastVuMeasure { get; set; }
    }
}