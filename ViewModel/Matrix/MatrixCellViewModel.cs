using System;
using System.Collections;
using System.Linq;
using Common.Model;
using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight;
using Common;
using Common.Commodules;

namespace EscInstaller.ViewModel.Matrix
{
    public class MatrixCellViewModel : ViewModelBase
    {
        private readonly ColumnHeaderViewModel _column;
        private MatrixCell _data;
        private readonly int _relativeFlowId;       

        public MatrixCellViewModel(ColumnHeaderViewModel column, int relativeFlowId)
        {
            _column = column;
            _relativeFlowId = relativeFlowId;
            

            _data = LibraryData.FuturamaSys.MatrixSelection.FirstOrDefault(b => b.ButtonId == 0
                && b.FlowId == _relativeFlowId) ??
                    new MatrixCell
                    {
                        ButtonId = 0,
                        FlowId = _relativeFlowId,
                        BroadcastMessage = BroadCastMessage.None
                    };
            column.DataSourceChanged += column_DataSourceChanged;

            column.AlarmSelectionChanged += ColumnOnAlarmSelectionChanged;
            column.CardsUpdated += (sender, args) => RaisePropertyChanged(() => IsVisible);
            column.ColumEnabledChanged += (sender, args) => RaisePropertyChanged(() => IsEnabled);
        }

        private void ColumnOnAlarmSelectionChanged(object sender, MessageSelectionEventArgs eventArgs)
        {
            if (eventArgs.ChangeAll && IsEnabled)
            {
                if (_column.AllAlarm1)
                    _data.BroadcastMessage = BroadCastMessage.Alarm1;
                else if(_column.AllAlarm2)
                    _data.BroadcastMessage = BroadCastMessage.Alarm2;
                else
                    _data.BroadcastMessage = BroadCastMessage.None;
            }

            RaisePropertyChanged(() => Alarm);
            RaisePropertyChanged(() => Alert);
        }

        void column_DataSourceChanged(object sender, DataSourceChangedEventArgs e)
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
            RaisePropertyChanged(()=> ButtonId);
        }

        //todo: attach handler
        private BlLink Link()
        {
            return _column.MainUnit.DiagramObjects.OfType<BlLink>().FirstOrDefault();
        }

        public bool IsEnabled
        {
            get
            {                                
                var bllink = Link().LinkOptions.FirstOrDefault(d => d.Flow.Id == (_relativeFlowId + _column.MainUnit.Id * 12));
                if (bllink != null && bllink.LinkPath != LinkTo.No && bllink.Flow.Id != 0) return false;

                if (ButtonId < 192 || ButtonId > 203) return true;
                if (LibraryData.FuturamaSys.Messages == null) return true;

                var bitArray = new BitArray(12);
                int i = 0;
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
        }

        public int ButtonId
        {
            get
            {
                return _data.ButtonId;
            }
        }

        public int FlowId
        {
            get { return _data.FlowId; }
        }

        public bool Alert
        {
            get
            {

                return _data.BroadcastMessage == BroadCastMessage.Alarm2;
            }
            set
            {

                if (value && _data.BroadcastMessage == BroadCastMessage.Alarm2) return;
                _data.BroadcastMessage = (value) ? BroadCastMessage.Alarm2 : BroadCastMessage.None;               
                _column.OnAlarmSelectionChanged(new MessageSelectionEventArgs());
            }
        }

        public bool IsVisible
        {
            get
            {                
                return _column.MainUnit.DataModel.ExpansionCards * 4 +4 > _data.FlowId%12;
            }
        }

        public bool Alarm
        {
            get { return _data.BroadcastMessage == BroadCastMessage.Alarm1; }
            set
            {
                if (value && _data.BroadcastMessage == BroadCastMessage.Alarm1) return;
                _data.BroadcastMessage = (value) ? BroadCastMessage.Alarm1 : BroadCastMessage.None;
                _column.OnAlarmSelectionChanged(new MessageSelectionEventArgs());
            }
        }        

        private static MatrixCell Data(int flowId, int buttonId)
        {
            var q = LibraryData.FuturamaSys.MatrixSelection.FirstOrDefault(b => b.ButtonId == buttonId && b.FlowId == flowId);
            if (q != null) return q;
            q = new MatrixCell { FlowId = flowId, ButtonId = buttonId };
            LibraryData.FuturamaSys.MatrixSelection.Add(q);

            return q;
        }
    }
}