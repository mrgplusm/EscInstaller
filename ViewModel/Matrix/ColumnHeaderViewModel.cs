#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.OverView;
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

        
        public ColumnHeaderViewModel(int relativeButtonId, MainUnitViewModel mainUnit, PanelViewModel panelViewModel)
        {
            _relativeButtonId = relativeButtonId;
            MainUnit = mainUnit;
            ButtonId = relativeButtonId;


            Cells = new ObservableCollection<MatrixCellViewModel>(GenCells());

            
            panelViewModel.ButtonChanged += PanelViewModelOnButtonChanged;

            panelViewModel.McuChanged += PanelViewModelOnMcuChanged;
            MainUnit.CardsUpdated += NewMcu_CardsUpdated;

            AlarmSelectionChanged += SetAlarmSelectionChanged;
            mainUnit.RoutingTableUpdated += Receiver_EepromValuesReceived;
            
        }

        public bool IsEnabled => Cells[0].IsEnabled;



        public ObservableCollection<MatrixCellViewModel> Cells { get; }
        public MainUnitViewModel MainUnit { get; private set; }

        public bool AnyAlarm1(MatrixCellViewModel[] exceptItem)
        {
            return Cells.Except(exceptItem).Any(d => d.Alarm && d.FlowId < MainUnit.DataModel.ExpansionCards * 4 + 4);
        }


        private bool _allAlarm;
        public bool AllAlarm1
        {
            get { return _allAlarm; }
            set
            {
                _allAlarm = value;
                var newv = _allAlarm ? BroadCastMessage.Alarm1 : BroadCastMessage.None;
                //UpdateCells();
                OnAlarmSelectionChanged(new MessageSelectionEventArgs() {ColumnSelection = true, ButtonId = ButtonId,
                    NewValue = newv});
            
            }
        }

        

        /// <summary>
        ///     check if the culomn belongs to ABCD on fire/evac/fds module buttons
        /// </summary>
        private bool IsAlphaButton => ButtonId > 191 && ButtonId < 204;

        /// <summary>
        ///     The text in the column header
        /// </summary>
        public string DisplayValue
        {
            get
            {
                var num = ButtonId;
                if (IsAlphaButton)
                    return ((char)((num % 4) + 65)).ToString(CultureInfo.InvariantCulture);
                if (ButtonId > 203)
                    return ((num % 12) + 1).ToString(CultureInfo.InvariantCulture);
                return (num + 1).ToString(CultureInfo.InvariantCulture);
            }
        }

        public int ButtonId { get; private set; }

        private void Receiver_EepromValuesReceived(object sender, EventArgs e)
        {
            OnDataSourceChanged(new DataSourceChangedEventArgs() { BaseButtonId = ButtonId, MainUnitViewModel = MainUnit });

            OnAlarmSelectionChanged(new MessageSelectionEventArgs());
        }

        private void SetAlarmSelectionChanged(object sender, MessageSelectionEventArgs eventArgs)
        {
            SendData();
            if(eventArgs.ColumnSelection) return;
            UpdateColumnSelection();
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



        private void PanelViewModelOnButtonChanged(object sender, RangeChangedEventArgs rangeChangedEventArgs)
        {
            ButtonId = rangeChangedEventArgs.NewId * 12 + _relativeButtonId;
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
            if (192 + e != ButtonId) return;
            OnColumEnabledChanged();
            RaisePropertyChanged(() => IsEnabled);
        }

        /// <summary>
        ///     Occurs when user selects a abcd message
        /// </summary>
        public event EventHandler ColumEnabledChanged;


        public event EventHandler<MessageSelectionEventArgs> AlarmSelectionChanged;



        public event EventHandler<DataSourceChangedEventArgs> DataSourceChanged;


        private void SendData()
        {
            CommunicationViewModel.AddData(new RoutingTable(new[] { ButtonId }, MainUnit.Id,
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

            UpdateColumnSelection();
            OnCardsUpdated();
            RaisePropertyChanged(() => DisplayValue);
            RaisePropertyChanged(() => ButtonId);
            RaisePropertyChanged(() => IsEnabled);

        }

        private void UpdateColumnSelection()
        {
            _allAlarm = Cells.All(n => n.Alarm || !n.IsVisible || !n.IsEnabled);
            RaisePropertyChanged(() => AllAlarm1);

        }

        private IEnumerable<MatrixCellViewModel> GenCells()
        {
            for (var x = 0; x < 12; x++)
            {
                var t = new MatrixCellViewModel(x, MainUnit.Id, ButtonId);
                t.Changed += (sender, args) =>
                {
                    OnAlarmSelectionChanged(args);
                    UpdateAlarm2Enabled(sender,args);
                };
                t.IsVisible = GetVisibility(t);
                t.IsLinked = GetIsLinked(x);

                CardsUpdated += (sender, args) => t.IsVisible = GetVisibility(t);
                DataSourceChanged += (sender, args) => t.column_DataSourceChanged(args);
                ColumEnabledChanged += (sender, args) => t.UpdateEnabled();
                AlarmSelectionChanged += t.MessageSelectionChanged;
                yield return t;
            }
        }

        private void UpdateAlarm2Enabled(object sender, MessageSelectionEventArgs args)
        {
            var t = Cells.Except(new[] {sender as MatrixCellViewModel}) .Any(s => s.Alarm || !s.IsVisible || !s.IsEnabled);

            foreach (var cell in Cells)
            {
                cell.Alarm2Enabled = t;
            }
            
        }

        private bool GetIsLinked(int flowId)
        {
            var link = MainUnit.DiagramObjects.OfType<BlLink>().FirstOrDefault();
            var bllink =
                    link?.LinkOptions.FirstOrDefault(d => d.Flow.Id == (flowId + MainUnit.Id * 12));
            return bllink != null && bllink.LinkPath != LinkTo.No && bllink.Flow.Id != 0;
        }

        private bool GetVisibility(MatrixCellViewModel t)
        {
            return MainUnit.DataModel.ExpansionCards * 4 + 4 > t.FlowId % 12;
        }

        protected virtual void OnAlarmSelectionChanged(MessageSelectionEventArgs e)
        {
            AlarmSelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnCardsUpdated()
        {
            CardsUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDataSourceChanged(DataSourceChangedEventArgs e)
        {
            DataSourceChanged?.Invoke(this, e);
        }

        protected virtual void OnColumEnabledChanged()
        {
            ColumEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    public class DataSourceChangedEventArgs : EventArgs
    {
        public MainUnitViewModel MainUnitViewModel { get; set; }
        public int BaseButtonId { get; set; }
    }

    /// <summary>
    /// Matrix cell message changed value
    /// </summary>
    public class MessageSelectionEventArgs : EventArgs
    {
        public bool ColumnSelection { get; set; }
        public int ButtonId { get; set; }
        public int FlowId { get; set; }
        public bool AnyAlarm1 { get; set; }
        public int MainUnitId { get; set; }
        public BroadCastMessage NewValue { get; set; }
    }
}