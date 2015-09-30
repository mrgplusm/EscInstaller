using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Futurama.Model;
using UpdateControls.Fields;

namespace Futurama.ViewModel
{
    public class BroadcastButtonViewModel
    {
        private readonly BroadcastButtonModel _data;
        private readonly Independent<bool> _alarm = new Independent<bool>();
        private readonly Independent<bool> _alert = new Independent<bool>();        

        public BroadcastButtonViewModel(BroadcastButtonModel data)
        {
            _data = data;
        }

        public bool Alert
        {
            get
            {
                return (_data.Message == Broadcast.Alert) ? _alert.Value = true : _alert.Value = false;                
            }
            set {
                _data.Message = value ? Broadcast.Alert : Broadcast.None;
                _alert.Value = true;
            }
        }

        public bool Alarm
        {
            get { return (_data.Message == Broadcast.Alarm) ? _alarm.Value = true : _alarm.Value = false; }
            set { _data.Message = value ? Broadcast.Alarm : Broadcast.None; }
        }
    }
}
