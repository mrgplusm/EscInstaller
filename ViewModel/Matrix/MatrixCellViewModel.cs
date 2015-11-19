#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
        private bool _isVisible;
        private bool _isLinked;        
        private BroadCastMessage _broadCastMessage;

        public MatrixCellViewModel(MatrixCell cell)
        {
            Cell = cell;
            SetBroadCastMessage();
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

        

        public bool IsLinked
        {
            get
            {

                var flow = GenericMethods.GetFlowForId(Cell.FlowId);
                return flow.Path != LinkTo.No;
            }
            
        }

        public bool IsEnabled
        {
            get
            {
                if (IsLinked) return false;

                if (Cell.ButtonId < 192 || Cell.ButtonId > 203) return true;

                return LibraryData.FuturamaSys.Messages == null || ABCDUsed();
            }
        }

        public void UpdateEnabled()
        {            
            RaisePropertyChanged(() => IsEnabled);
            RaisePropertyChanged(() => Alarm2Enabled);            
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
            get
            {
                if (!IsEnabled) return false;
                
                if (_alarm1Count < 1) return false;
                if ((_alarm1Count == 1) && (_broadCastMessage == BroadCastMessage.Alarm1)) return false;
                return true;
            }
            
        }

        private int _alarm1Count;
        private void UpdateAlarm1Count()
        {
            var mainUnitKey = GenericMethods.GetMainunitIdForFlowId(Cell.FlowId);
            var cellSelection = CellsForUnit(mainUnitKey, Cell.ButtonId).ToArray();
            var valuesToSelect = cellSelection.Select(x => LibraryData.FuturamaSys.Selection[x]).ToList();            

            _alarm1Count = (valuesToSelect.Count(d => d == BroadCastMessage.Alarm1));
            if (_alarm1Count < 1)
            {
                foreach (var matrixCell in cellSelection)
                {
                    LibraryData.FuturamaSys.Selection[matrixCell] = BroadCastMessage.None;
                }
                OnChanged(new SelectionEventArgs() { ButtonId = Cell.ButtonId, ColumnSelection = true, NewValue = BroadCastMessage.None, Alarm2Removed = true});
            }
        }

        public static IEnumerable<MatrixCell> CellsForUnit(int mainUnitId, int buttonId)
        {            
            Func<int, bool> keys = (id) => id >= mainUnitId * 12 && id < mainUnitId * 12 + 12;
            return LibraryData.FuturamaSys.Selection.Keys.Where(d => d.ButtonId == buttonId && keys(d.FlowId));
        }

        public event EventHandler<SelectionEventArgs> Changed;

        public bool Alert
        {
            get { return _broadCastMessage == BroadCastMessage.Alarm2; }
            set
            {
                if (value && _broadCastMessage == BroadCastMessage.Alarm2) return;
                _broadCastMessage = (value) ? BroadCastMessage.Alarm2 : BroadCastMessage.None;
                
                TriggerChange();
                RaisePropertyChanged(() => Alarm);
            }
        }

        private void TriggerChange()
        {
            LibraryData.FuturamaSys.Selection[Cell] = _broadCastMessage;

            OnChanged(new SelectionEventArgs() { ButtonId = Cell.ButtonId, FlowId = Cell.FlowId, NewValue = _broadCastMessage });
        }

        public bool Alarm
        {
            get { return _broadCastMessage == BroadCastMessage.Alarm1; }
            set
            {
                if (value && _broadCastMessage == BroadCastMessage.Alarm1) return;
                _broadCastMessage = (value) ? BroadCastMessage.Alarm1 : BroadCastMessage.None;
                
                TriggerChange();
                RaisePropertyChanged(()=> Alert);
            }
        }

        public void MessageSelectionChanged(SelectionEventArgs e)
        {
            if (e.Alarm2Removed)
            {
                _broadCastMessage = BroadCastMessage.None;
                RaisePropertyChanged(()=> Alert);
                return;
            }
                     
            if (e.ColumnSelection || (_broadCastMessage != e.NewValue && e.FlowId == Cell.FlowId && e.ButtonId == Cell.ButtonId))
            {                                             
                _broadCastMessage = e.NewValue;
                RaisePropertyChanged(() => Alarm);
                RaisePropertyChanged(() => Alert);         
            }
            

            Alarm2Handler();

        }

        private void Alarm2Handler()
        {
            UpdateAlarm1Count();            
            RaisePropertyChanged(() => Alarm2Enabled);
        }
        
        public void UpdatePosition(MatrixCell key)
        {
            Cell = key;

            SetBroadCastMessage();                        

            RaisePropertyChanged(() => Alarm);
            RaisePropertyChanged(() => Alert);
            RaisePropertyChanged(() => IsEnabled);
            RaisePropertyChanged(()=> Cell);
            Alarm2Handler();
        }

        private void SetBroadCastMessage()
        {
            if (LibraryData.FuturamaSys.Selection.TryGetValue(Cell, out _broadCastMessage)) return;
            LibraryData.FuturamaSys.Selection.Add(Cell, BroadCastMessage.None);
            _broadCastMessage = BroadCastMessage.None;
        }

        protected virtual void OnChanged(SelectionEventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}