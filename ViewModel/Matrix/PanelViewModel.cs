using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EscInstaller.View;
using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Common;
using ITabControl = EscInstaller.ViewModel.Connection.ITabControl;


namespace EscInstaller.ViewModel.Matrix
{
    public class PanelViewModel : ViewModelBase, ITabControl
    {
        private readonly MainViewModel _main;


        public PanelViewModel(MainViewModel main)
        {
            _main = main;

            InitiateSelectors();

        }

        public int ButtonStartId
        {
            get { return _buttonStartId; }
            set
            {
                _buttonStartId = value;
                RaisePropertyChanged(() => ButtonStartId);
            }
        }


        public MainUnitViewModel SelectedMcu
        {
            get { return _selectedMcu; }
            set
            {
                _selectedMcu = value;
                RaisePropertyChanged(() => SelectedMcu);
            }
        }

        

#if DEBUG
        public PanelViewModel()
        {
            _main = new MainViewModel();
            InitiateSelectors();
        }
#endif

        private void InitiateSelectors()
        {
            ButtonRangeSelectors = new ObservableCollection<ButtonRangeSelector>(Enumerable.
                Range(0, 18).Select(i => new ButtonRangeSelector(i, this)));

            ButtonRangeSelectors[0].IsSelected = true;
            SelectedMcu = _main.TabCollection.OfType<MainUnitViewModel>().First(i => i.Id == 0);

            McuSelectors = new ObservableCollection<McuSelector>(
                _main.TabCollection.OfType<MainUnitViewModel>().Select(q => new McuSelector(q, this)));
            _main.SystemChanged+= MainOnSystemChanged;
            

            McuSelectors[0].IsSelected = true;

            ColumnHeaderViews = new ObservableCollection<ColumnHeaderViewModel>(Enumerable.Range(0, 12).Select(
                    x => new ColumnHeaderViewModel(x, SelectedMcu, this)));

            ButtonChanged += (sender, args) => ButtonStartId = args.NewId;

            McuChanged += OnMcuChanged;

            RowHeaders = new ObservableCollection<RowHeaderViewModel>(GenRows(SelectedMcu));
            SelectedMcu.CardsUpdated += NewMcuOnCardsUpdated;            
        }

        private void MainOnSystemChanged(object sender, SystemChangedEventArgs systemChangedEventArgs)
        {
            if (systemChangedEventArgs.NewMainUnit != null)
            {
                McuSelectors.Add(new McuSelector(systemChangedEventArgs.NewMainUnit, this));
            }
            else if(systemChangedEventArgs.OldMainUnit !=null)
            {
                var rem = McuSelectors.FirstOrDefault(s => s.MainUnitViewModel.Equals(systemChangedEventArgs.OldMainUnit));
                McuSelectors.Remove(rem);
            }
        }

        private void OnMcuChanged(object sender, McuChangedEventArgs rangeChangedEventArgs)
        {
            SelectedMcu.CardsUpdated -= NewMcuOnCardsUpdated;

            SelectedMcu = rangeChangedEventArgs.NewMcu;
            UpdateRowHeaders();

            rangeChangedEventArgs.NewMcu.CardsUpdated += NewMcuOnCardsUpdated;

        }        

        private void NewMcuOnCardsUpdated(object sender, MainUnitUpdatedEventArgs mainUnitUpdatedEventArgs)
        {
            UpdateRowHeaders();
        }

        private void UpdateRowHeaders()
        {
            RowHeaders.Clear();
            foreach (var rowHeaderViewModel in GenRows(SelectedMcu))
            {
                RowHeaders.Add(rowHeaderViewModel);
            }
        }

        private static IEnumerable<RowHeaderViewModel> GenRows(MainUnitViewModel selector)
        {
            return selector.DiagramObjects.OfType<BlOutput>().Select(result => new RowHeaderViewModel(result));
        }

        public ObservableCollection<RowHeaderViewModel> RowHeaders { get; private set; }


        /// <summary>
        ///     Column headers of matrix (cells reside in colums)
        /// </summary>
        public ObservableCollection<ColumnHeaderViewModel> ColumnHeaderViews { get; private set; }

        private int _buttonStartId;


        //used to set tab to right.
        public int Id
        {
            get { return 33; }
        }

        public static string PanelText
        {
            get
            {
                return Panel._matrixSelectionFP + "\t\t" + Panel._matrixSelectionEP + "\t\t" + Panel._matrixSelectionFDS;
            }
        }


        public ObservableCollection<ButtonRangeSelector> ButtonRangeSelectors { get; private set; }

        public ObservableCollection<McuSelector> McuSelectors { get; private set; }    

        

        public int MicRoutingOpt
        {
            get
            {
                return 1; //temporary on both.
                //return LibraryData.FuturamaSys.MicrophoneRouting;
            }
            set
            {
                LibraryData.FuturamaSys.MicrophoneRouting = value;
                RaisePropertyChanged(() => MicRoutingOpt);
            }
        }

        /// <summary>
        /// User selected button range 
        /// </summary>
        public event EventHandler<RangeChangedEventArgs> ButtonChanged;

        public virtual void OnButtonChanged(RangeChangedEventArgs e)
        {
            EventHandler<RangeChangedEventArgs> handler = ButtonChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<McuChangedEventArgs> McuChanged;
        private MainUnitViewModel _selectedMcu;

        public virtual void OnMcuChanged(McuChangedEventArgs e)
        {
            EventHandler<McuChangedEventArgs> handler = McuChanged;
            if (handler != null) handler(this, e);
        }
    }

    public class McuChangedEventArgs : EventArgs
    {
        public MainUnitViewModel NewMcu;
    }

    public class RangeChangedEventArgs : EventArgs
    {
        public int NewId { get; set; }
    }
}