#region

using System;
using System.Collections;
using System.Collections.Generic;
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
        private bool _isVisible;
        private BroadCastMessage _broadCastMessage;
        private bool _alarm2Enabled;
        private bool _isEnabled;

        public MatrixCellViewModel(MatrixCell cell)
        {
            Cell = cell;
            SetBroadCastMessage();
            UpdateEnabled();
        }

        public BroadCastMessage BroadCastMessage
        {
            get { return _broadCastMessage; }
            set
            {
                if (_broadCastMessage == value) return;
                var old = _broadCastMessage;                
                _broadCastMessage = value;
                StoreChange();
                if (old == BroadCastMessage.Alarm1 && Alarm1Count() < 1)
                {
                    SetNoMessageForColumn();
                }
                RaisePropertyChanged(() => BroadCastMessage);
            }
        }

        public int FlowId => Cell.FlowId;

        public MatrixCell Cell { get; private set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        private bool IsLinked()
        {
            var flow = GenericMethods.GetFlowForId(Cell.FlowId);
            return flow.Path != LinkTo.No;
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            private set
            {
                if(_isEnabled == value) return;
                _isEnabled = value;
                RaisePropertyChanged(()=> IsEnabled);
            }
        }


        private bool CellIsEnabled()
        {
            if (IsLinked()) return false;

            if (Cell.ButtonId < 192 || Cell.ButtonId > 203) return true;

            return LibraryData.FuturamaSys.Messages == null || ABCDUsed();
        }

        private bool CellAlarm2Enabled()
        {
            if (!IsEnabled) return false;

            var count = Alarm1Count();
            if (count < 1) return false;
            return (count != 1) || (_broadCastMessage != BroadCastMessage.Alarm1);
        }
        

        public void UpdateEnabled()
        {
            IsEnabled = CellIsEnabled();
            Alarm2Enabled = CellAlarm2Enabled();            
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

            return bitArray[Cell.ButtonId - 192];
        }

        public bool Alarm2Enabled
        {
            get { return _alarm2Enabled; }
            private set
            {
                if (_alarm2Enabled == value) return;
                _alarm2Enabled = value;
                RaisePropertyChanged(()=> Alarm2Enabled);
            }
        }

        private IEnumerable<MatrixCell> ColumnKeys()
        {
            var mainUnitKey = GenericMethods.GetMainunitIdForFlowId(Cell.FlowId);
            return ColumnHeaderViewModel.CellsForUnit(mainUnitKey, Cell.ButtonId);
        }

        private int Alarm1Count()
        {            
            var valuesToSelect = ColumnKeys().Select(x => LibraryData.FuturamaSys.Selection[x]).ToList();

            return (valuesToSelect.Count(d => d == BroadCastMessage.Alarm1));                        
        }

        private void SetNoMessageForColumn()
        {
            foreach (var matrixCell in ColumnKeys().ToArray())
            {
                LibraryData.FuturamaSys.Selection[matrixCell] = BroadCastMessage.None;
            }
            OnChanged(new SelectionEventArgs() { ButtonId = Cell.ButtonId, ColumnSelection = true, NewValue = BroadCastMessage.None, Alarm2Removed = true });            
        }

        public event EventHandler<SelectionEventArgs> Changed;

        private void StoreChange()
        {
            LibraryData.FuturamaSys.Selection[Cell] = _broadCastMessage;

            OnChanged(new SelectionEventArgs() { ButtonId = Cell.ButtonId, FlowId = Cell.FlowId, NewValue = _broadCastMessage });
        }

        public void MessageSelectionChanged(SelectionEventArgs e)
        {
            if (e.Alarm2Removed)
            {
                _broadCastMessage = BroadCastMessage.None;
                RaisePropertyChanged(() => BroadCastMessage);
                return;
            }

            if (e.ColumnSelection || (_broadCastMessage != e.NewValue && e.FlowId == Cell.FlowId && e.ButtonId == Cell.ButtonId))
            {
                _broadCastMessage = e.NewValue;
                RaisePropertyChanged(() => BroadCastMessage);
            }

            Alarm2Enabled = CellAlarm2Enabled();
        }

        

        public void UpdatePosition(MatrixCell key)
        {
            Cell = key;

            SetBroadCastMessage();
            RaisePropertyChanged(() => BroadCastMessage);
            UpdateEnabled();
            RaisePropertyChanged(() => Cell);            
        }

        private void SetBroadCastMessage()
        {

            if (LibraryData.FuturamaSys.Selection.TryGetValue(Cell, out _broadCastMessage)) return;

            LibraryData.FuturamaSys.Selection.Add(Cell, BroadCastMessage.None);
            BroadCastMessage = BroadCastMessage.None;
        }

        protected virtual void OnChanged(SelectionEventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}