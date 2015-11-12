#region

using System;
using System.Collections;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    public class MatrixCellViewModel : ViewModelBase
    {        
        private MatrixCell _key;        
        private bool _isVisible;
        private bool _isLinked;
        private bool _alarm2Enabled;
        private BroadCastMessage _broadCastMessage;

        public MatrixCellViewModel(int buttonId, int flowId)
        {     
            _key = new MatrixCell(flowId, buttonId);
            SetBroadCastMessage();
        }

        public int FlowId => _key.FlowId;
        

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        

        public bool IsLinked
        {
            get { return _isLinked; }
            set
            {
                _isLinked = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public bool IsEnabled
        {
            get
            {
                if (_isLinked) return false;

                if (_key.ButtonId < 192 || _key.ButtonId > 203) return true;

                return LibraryData.FuturamaSys.Messages == null || ABCDUsed();
            }
        }

        public void UpdateEnabled()
        {
            RaisePropertyChanged(() => IsEnabled);
        }

        private bool ABCDUsed()
        {
            var bitArray = new BitArray(12);
            var i = 0;
            if (LibraryData.FuturamaSys.Messages == null)
                return false;
            foreach (var message in LibraryData.FuturamaSys.Messages)
            {
                bitArray[i++] = message.ButtonA1 == 0xff;
                bitArray[i++] = message.ButtonB1 == 0xff;
                bitArray[i++] = message.ButtonC1 == 0xff;
                bitArray[i++] = message.ButtonD1 == 0xff;
            }

            return bitArray[_key.ButtonId - 192];
        }        

        public bool Alarm2Enabled
        {
            get
            {
                return _alarm2Enabled && IsEnabled;
            }
            set
            {
                _alarm2Enabled = value;
                RaisePropertyChanged(() => Alarm2Enabled);
            }
        }

        public event EventHandler<MessageSelectionEventArgs> Changed;

        public bool Alert
        {
            get { return _broadCastMessage == BroadCastMessage.Alarm2; }
            set
            {
                if (value && _broadCastMessage == BroadCastMessage.Alarm2) return;
                _broadCastMessage = (value) ? BroadCastMessage.Alarm2 : BroadCastMessage.None;
                TriggerChange();
            }
        }

        private void TriggerChange()
        {
            OnChanged(new MessageSelectionEventArgs() { ButtonId = _key.ButtonId, FlowId = _key.FlowId, NewValue = _broadCastMessage });
        }

        public bool Alarm
        {
            get { return _broadCastMessage == BroadCastMessage.Alarm1; }
            set
            {
                if (value && _broadCastMessage == BroadCastMessage.Alarm1) return;
                _broadCastMessage = (value) ? BroadCastMessage.Alarm1 : BroadCastMessage.None;
                TriggerChange();
            }
        }

        public void MessageSelectionChanged(MessageSelectionEventArgs e)
        {
            if (!e.ColumnSelection) return;
            if (_broadCastMessage == e.NewValue) return;
            _broadCastMessage = e.NewValue;

            RaisePropertyChanged(() => Alarm);
            RaisePropertyChanged(() => Alert);
        }
        
        public void UpdatePosition(int buttonId, int flowId)
        {
            _key = new MatrixCell(flowId, buttonId);
            SetBroadCastMessage();                        

            RaisePropertyChanged(() => Alarm);
            RaisePropertyChanged(() => Alert);
            RaisePropertyChanged(() => IsEnabled);
            RaisePropertyChanged(() => Alarm2Enabled);            
        }

        private void SetBroadCastMessage()
        {
            if (LibraryData.FuturamaSys.Selection.TryGetValue(_key, out _broadCastMessage)) return;
            LibraryData.FuturamaSys.Selection.Add(_key, BroadCastMessage.None);
            _broadCastMessage = BroadCastMessage.None;
        }

        protected virtual void OnChanged(MessageSelectionEventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}