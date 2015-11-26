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
            DataSourceChanged += OnDataSourceChanged;

            CardsUpdated += OnCardsUpdated;

            CardMessageChange += OnColumEnabledChanged;
            AlarmSelectionChanged += OnAlarmSelectionChanged;
            UpdateColumnSelection();
        }

        private void OnAlarmSelectionChanged(object sender, SelectionEventArgs selectionEventArgs)
        {            
            foreach (var cell in Cells)
            {
                cell.MessageSelectionChanged(selectionEventArgs);
            }
        }

        private void OnColumEnabledChanged(object sender, CardMessageEventArgs eventArgs)
        {
            AllAlarm1 = false;

            foreach (var cell in Cells)
            {
                cell.UpdateEnabled();
            }
        }

        private void OnCardsUpdated(object sender, EventArgs eventArgs)
        {
            foreach (var cell in Cells)
            {
                cell.IsVisible = GetVisibility(cell);
            }
        }

        //update cells in this column
        private void OnDataSourceChanged(object sender, DataSourceChangedEventArgs dataSourceChangedEventArgs)
        {
            for (var flow = 0; flow < 12; flow++)
            {
                var matrixcell = new MatrixCell(flow + MainUnit.Id * 12, ButtonId);
                Cells[flow].UpdatePosition(matrixcell);
            }
        }

        public bool IsEnabled => Cells[0].IsEnabled;



        public ObservableCollection<MatrixCellViewModel> Cells { get; }
        public MainUnitViewModel MainUnit { get; private set; }        

        private bool _allAlarm;
        public bool AllAlarm1
        {
            get { return _allAlarm; }
            set
            {
                _allAlarm = value;
                RaisePropertyChanged(()=> AllAlarm1);
                var newv = _allAlarm ? BroadCastMessage.Alarm1 : BroadCastMessage.None;
                UpdateColumn(AllAlarm1 ? BroadCastMessage.Alarm1 : BroadCastMessage.None, MainUnit.Id, ButtonId);
                OnAlarmSelectionChanged(new SelectionEventArgs()
                {
                    ColumnSelection = true,
                    ButtonId = ButtonId,
                    NewValue = newv
                });

            }
        }

        public static IEnumerable<MatrixCell> CellsForUnit(int mainUnitId, int buttonId)
        {            
            return Enumerable.Range(mainUnitId*12, 12).Select(n => new MatrixCell(n, buttonId));                     
        }

        public static IEnumerable<MatrixCell> ColumnSelection(int mainUnitId, int buttonId, bool wholeColumn = false)
        {
            var mu = GenericMethods.GetMainUnit(mainUnitId);
            return CellsForUnit(mainUnitId, buttonId)
                .Where(s => s.FlowId%12 < (wholeColumn ? 12 : mu.ExpansionCards*4 + 4));
        }
        
                
        private static void UpdateColumn(BroadCastMessage message, int mainUnit, int buttonId)
        {
            var cellSelection = ColumnSelection(mainUnit, buttonId, message == BroadCastMessage.None).ToArray();

            foreach (var matrixCell in cellSelection)
            {
                LibraryData.FuturamaSys.Selection[matrixCell] = message;
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

            OnAlarmSelectionChanged(new SelectionEventArgs());
        }

        private void SetAlarmSelectionChanged(object sender, SelectionEventArgs eventArgs)
        {
            SendData();
            if (eventArgs.ColumnSelection) return;
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
                    MainUnit.AlarmMessages.Messages[i].CardMessageChange += ColumnHeaderViewModel_SelectionChanged;
                }
            }
            else
            {
                for (var i = 0; i < 3; i++)
                {
                    MainUnit.AlarmMessages.Messages[i].CardMessageChange -= ColumnHeaderViewModel_SelectionChanged;
                }
            }
        }

        private void ColumnHeaderViewModel_SelectionChanged(object sender, CardMessageEventArgs args)
        {
            if (192 + args.ButtonId != ButtonId) return;
            OnCardMessageChange(args);
            RaisePropertyChanged(() => IsEnabled);
        }        

        public event EventHandler<SelectionEventArgs> AlarmSelectionChanged;
        public event EventHandler<DataSourceChangedEventArgs> DataSourceChanged;


        private void SendData()
        {
            CommunicationViewModel.AddData(new RoutingTable(new[] { ButtonId }, MainUnit.Id,
                LibraryData.FuturamaSys.Selection));
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
            //_allAlarm = Cells.All(n => n.Alarm || !n.IsVisible || !n.IsEnabled);
            _allAlarm =
                ColumnSelection(MainUnit.Id, ButtonId)
                    .Select(MatrixCellViewModel.TryGetSelection)
                    .All(n => n == BroadCastMessage.Alarm1);
            RaisePropertyChanged(() => AllAlarm1);
        }

        private IEnumerable<MatrixCellViewModel> GenCells()
        {
            for (var flow = 0; flow < 12; flow++)
            {
                var t = new MatrixCellViewModel(new MatrixCell(flow + MainUnit.Id*12, ButtonId));
                t.Changed += (sender, args) =>
                {
                    OnAlarmSelectionChanged(args);

                };
                t.IsVisible = GetVisibility(t);

                yield return t;
            }
        }

        private bool GetVisibility(MatrixCellViewModel t)
        {
            return MainUnit.DataModel.ExpansionCards * 4 + 4 > t.FlowId % 12;
        }

        protected virtual void OnAlarmSelectionChanged(SelectionEventArgs e)
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

        public event EventHandler<CardMessageEventArgs> CardMessageChange;

        protected virtual void OnCardMessageChange(CardMessageEventArgs e)
        {
            CardMessageChange?.Invoke(this, e);
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
    public class SelectionEventArgs : EventArgs
    {
        public bool ColumnSelection { get; set; }
        public int ButtonId { get; set; }
        public int FlowId { get; set; }
        public bool Alarm2Removed { get; set; }
        public int MainUnitId { get; set; }
        public BroadCastMessage NewValue { get; set; }
    }
}