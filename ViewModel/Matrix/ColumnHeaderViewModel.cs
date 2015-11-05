#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Common;
using Common.Commodules;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    /// <summary>
    ///     Used to define columns in the matrix panel.
    /// </summary>
    public class ColumnHeaderViewModel : ViewModelBase
    {
        /// <summary>
        ///     always 12 columheaderviewmodels are initiated, they have their own id
        /// </summary>
        private readonly int _relativeButtonId;

        private BroadCastMessage _messagetype;

        public ColumnHeaderViewModel(int relativeButtonId, MainUnitViewModel mainUnit, PanelViewModel panelViewModel)
        {
            _relativeButtonId = relativeButtonId;
            MainUnit = mainUnit;
            ButtonId = relativeButtonId;


            Cells = new ObservableCollection<MatrixCellViewModel>(GenCells());

            UpdateAllHeaders();

            panelViewModel.ButtonChanged += PanelViewModelOnButtonChanged;

            panelViewModel.McuChanged += PanelViewModelOnMcuChanged;
            MainUnit.CardsUpdated += NewMcu_CardsUpdated;

            AlarmSelectionChanged += OnAlarmSelectionChanged;
            mainUnit.RoutingTableUpdated += Receiver_EepromValuesReceived;
        }

        public bool IsEnabled
        {
            get { return Cells[0].IsEnabled; }
        }

        public ObservableCollection<MatrixCellViewModel> Cells { get; }
        public MainUnitViewModel MainUnit { get; private set; }

        public bool AllAlarm2
        {
            get { return _messagetype == BroadCastMessage.Alarm2; }
            set
            {
                if (value && _messagetype != BroadCastMessage.Alarm2)
                    _messagetype = BroadCastMessage.Alarm2;
                else if (_messagetype != BroadCastMessage.None)
                    _messagetype = BroadCastMessage.None;
                else return;
                OnAlarmSelectionChanged(new MessageSelectionEventArgs() {ChangeAll = true});
            }
        }

        public bool AllAlarm1
        {
            get { return _messagetype == BroadCastMessage.Alarm1; }
            set
            {
                if (value && _messagetype != BroadCastMessage.Alarm1)
                    _messagetype = BroadCastMessage.Alarm1;
                else if (_messagetype != BroadCastMessage.None)
                    _messagetype = BroadCastMessage.None;
                else return;
                OnAlarmSelectionChanged(new MessageSelectionEventArgs() {ChangeAll = true});
            }
        }

        /// <summary>
        ///     check if the culomn belongs to ABCD on fire/evac/fds module buttons
        /// </summary>
        private bool IsAlphaButton
        {
            get { return ButtonId > 191 && ButtonId < 204; }
        }

        /// <summary>
        ///     The text in the column header
        /// </summary>
        public string DisplayValue
        {
            get
            {
                var num = ButtonId;
                if (IsAlphaButton)
                    return ((char) ((num%4) + 65)).ToString(CultureInfo.InvariantCulture);
                if (ButtonId > 203)
                    return ((num%12) + 1).ToString(CultureInfo.InvariantCulture);
                return (num + 1).ToString(CultureInfo.InvariantCulture);
            }
        }

        public int ButtonId { get; private set; }

        private void Receiver_EepromValuesReceived(object sender, EventArgs e)
        {
            OnDataSourceChanged(new DataSourceChangedEventArgs() {BaseButtonId = ButtonId, MainUnitViewModel = MainUnit});
            UpdateAllHeaders();
            OnAlarmSelectionChanged(new MessageSelectionEventArgs());
        }

        private void OnAlarmSelectionChanged(object sender, EventArgs eventArgs)
        {
            UpdateAllHeaders();
            SendData();
        }

        private void UpdateAllHeaders()
        {
            _messagetype = Cells.All(a => a.Alarm || !a.IsEnabled || !a.IsVisible)
                ? BroadCastMessage.Alarm1
                : Cells.All(a => a.Alert || !a.IsEnabled || !a.IsVisible)
                    ? BroadCastMessage.Alarm2
                    : BroadCastMessage.None;

            RaisePropertyChanged(() => AllAlarm1);
            RaisePropertyChanged(() => AllAlarm2);
        }

        private void PanelViewModelOnMcuChanged(object sender, McuChangedEventArgs rangeChangedEventArgs)
        {
            MainUnit.CardsUpdated -= NewMcu_CardsUpdated;
            MainUnit.RoutingTableUpdated -= Receiver_EepromValuesReceived;

            MainUnit = rangeChangedEventArgs.NewMcu;

            MainUnit.RoutingTableUpdated += Receiver_EepromValuesReceived;
            rangeChangedEventArgs.NewMcu.CardsUpdated += NewMcu_CardsUpdated;
            Update();
        }

        private void NewMcu_CardsUpdated(object sender, MainUnitUpdatedEventArgs e)
        {
            OnCardsUpdated();
        }

        public event EventHandler CardsUpdated;

        protected virtual void OnCardsUpdated()
        {
            var handler = CardsUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void PanelViewModelOnButtonChanged(object sender, RangeChangedEventArgs rangeChangedEventArgs)
        {
            ButtonId = rangeChangedEventArgs.NewId*12 + _relativeButtonId;
            AttachEnabledHandlers();
            Update();
        }

        /// <summary>
        ///     When abcd buttons is selected, enabling columns depend on abcd message selection
        /// </summary>
        private void AttachEnabledHandlers()
        {
            if (IsAlphaButton)
            {
                for (var i = 0; i < 3; i++)
                {
                    MainUnit.AlarmMessages.Messages[i].SelectionChanged += ColumnHeaderViewModel_SelectionChanged;
                }
            }
            else
            {
                for (var i = 0; i < 3; i++)
                {
                    MainUnit.AlarmMessages.Messages[i].SelectionChanged -= ColumnHeaderViewModel_SelectionChanged;
                }
            }
        }

        private void ColumnHeaderViewModel_SelectionChanged(object sender, int e)
        {
            if (192 + e == ButtonId)
            {
                OnColumEnabledChanged();
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        /// <summary>
        ///     Occurs when user selects a abcd message
        /// </summary>
        public event EventHandler ColumEnabledChanged;

        protected virtual void OnColumEnabledChanged()
        {
            var handler = ColumEnabledChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<MessageSelectionEventArgs> AlarmSelectionChanged;

        public virtual void OnAlarmSelectionChanged(MessageSelectionEventArgs e)
        {
            var handler = AlarmSelectionChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<DataSourceChangedEventArgs> DataSourceChanged;

        protected virtual void OnDataSourceChanged(DataSourceChangedEventArgs e)
        {
            var handler = DataSourceChanged;
            if (handler != null) handler(this, e);
        }

        private void SendData()
        {
            CommunicationViewModel.AddData(new RoutingTable(new[] {ButtonId}, MainUnit.Id,
                LibraryData.FuturamaSys.MatrixSelection));
        }

        /// <summary>
        ///     Update cell according to button and mainunitid
        /// </summary>
        private void Update()
        {
            OnDataSourceChanged(new DataSourceChangedEventArgs()
            {
                BaseButtonId = ButtonId,
                MainUnitViewModel = MainUnit
            });

            UpdateAllHeaders();
            OnCardsUpdated();
            RaisePropertyChanged(() => DisplayValue);
            RaisePropertyChanged(() => ButtonId);
            RaisePropertyChanged(() => IsEnabled);
        }

        private IEnumerable<MatrixCellViewModel> GenCells()
        {
            if (IsInDesignMode)
            {
                for (var i = 0; i < 12; i++)
                {
                    yield return new MatrixCellViewModel(this, i);
                }
            }

            for (var x = 0; x < 12; x++)
            {
                yield return new MatrixCellViewModel(this, x);
            }
        }
    }

    public class DataSourceChangedEventArgs : EventArgs
    {
        public MainUnitViewModel MainUnitViewModel { get; set; }
        public int BaseButtonId { get; set; }
    }

    public class MessageSelectionEventArgs : EventArgs
    {
        public bool ChangeAll { get; set; }
    }
}