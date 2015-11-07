#region

using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    public class MatrixCellViewModel : ViewModelBase
    {
        private readonly int _relativeFlowId;
        private MatrixCell _data;
        private bool _isVisible;
        private bool _isLinked;
        private bool _alarm2Enabled;

        public MatrixCellViewModel(int relativeFlowId, int mainUnitId, int buttonId)
        {
            _relativeFlowId = relativeFlowId;

            _data = Data(mainUnitId * 12 + _relativeFlowId, buttonId);
        }

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

                if (ButtonId < 192 || ButtonId > 203) return true;

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

            return bitArray[ButtonId - 192];
        }

        public int ButtonId => _data.ButtonId;

        public int FlowId => _data.FlowId;

        public bool Alarm2Enabled
        {
            get
            {
                return _alarm2Enabled && IsEnabled;
            }
            set
            {
                _alarm2Enabled = value;
                RaisePropertyChanged(()=> Alarm2Enabled);
            }
        }

        public event EventHandler<MessageSelectionEventArgs> Changed;

        public bool Alert
        {
            get { return _data.BroadcastMessage == BroadCastMessage.Alarm2; }
            set
            {
                if (value && _data.BroadcastMessage == BroadCastMessage.Alarm2) return;
                _data.BroadcastMessage = (value) ? BroadCastMessage.Alarm2 : BroadCastMessage.None;
                TriggerChange();
            }
        }

        private void TriggerChange()
        {
            OnChanged(new MessageSelectionEventArgs() { ButtonId = ButtonId, FlowId = FlowId, NewValue = _data.BroadcastMessage });
        }

        public bool Alarm
        {
            get { return _data.BroadcastMessage == BroadCastMessage.Alarm1; }
            set
            {
                if (value && _data.BroadcastMessage == BroadCastMessage.Alarm1) return;
                _data.BroadcastMessage = (value) ? BroadCastMessage.Alarm1 : BroadCastMessage.None;
                TriggerChange();
            }
        }

        public void MessageSelectionChanged(object sender, MessageSelectionEventArgs e)
        {
           if(!e.ColumnSelection) return;
           if(_data.BroadcastMessage == e.NewValue) return;
            _data.BroadcastMessage = e.NewValue;

            RaisePropertyChanged(() => Alarm);
            RaisePropertyChanged(() => Alert);
        }

        public void column_DataSourceChanged(DataSourceChangedEventArgs e)
        {
            if (IsInDesignMode)
            {
                _data.BroadcastMessage = BroadCastMessage.Alarm1;
                return;
            }
            if (LibraryData.FuturamaSys == null)
                throw new Exception("Currently no projectfile is opened");

            _data = Data(e.MainUnitViewModel.Id * 12 + _relativeFlowId, e.BaseButtonId);


            RaisePropertyChanged(() => Alarm);
            RaisePropertyChanged(() => Alert);
            RaisePropertyChanged(() => IsEnabled);
            RaisePropertyChanged(() => Alarm2Enabled);
            RaisePropertyChanged(() => ButtonId);
        }

        private static MatrixCell Data(int flowId, int buttonId)
        {
            var q =
                LibraryData.FuturamaSys.MatrixSelection.FirstOrDefault(b => b.ButtonId == buttonId && b.FlowId == flowId);
            if (q != null) return q;
            q = new MatrixCell { FlowId = flowId, ButtonId = buttonId };
            LibraryData.FuturamaSys.MatrixSelection.Add(q);

            return q;
        }


        protected virtual void OnChanged(MessageSelectionEventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}