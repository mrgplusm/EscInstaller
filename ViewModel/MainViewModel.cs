#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ImportSpeakers;
using EscInstaller.UserControls;
using EscInstaller.View.Communication;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Matrix;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

#endregion

namespace EscInstaller.ViewModel
{
    public enum BusyDownloading
    {
        No,
        ToEsc,
        FromEsc,
        WaitAcknowledge
    }


    public class MainViewModel : ViewModelBase
    {
        private readonly RecentFilesLogic _recentFilesLogic;
        private ITabControl _selectedTab;

        public MainViewModel()
        {
            if (base.IsInDesignMode)
            {
                return;
            }
            SpeakerMethods.Initialize();

            if (Application.Current != null)
                Application.Current.Exit += (sender, args) =>
                {
                    //close connections 
                    DisconnectAllUnists();

                    //save speaker library
                    SpeakerMethods.SaveSpeakerlib();

                    //save project settings
                    Properties.Settings.Default.Save();
                };
            _recentFilesLogic = new RecentFilesLogic(this);

            var dispThread = Thread.CurrentThread;
            Dispatcher.FromThread(dispThread);

            TabCollection = new ObservableCollection<ITabControl> { Communication };         


#if DEBUG
            if (IsInDesignMode)
            {
                LibraryData.CreateEmptySystem();
                TabCollection.Add(new MainUnitViewModel(LibraryData.EmptyEsc(0), this));
            }
            else
            {
                Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\d.esc");
            }
#endif
        }




        public bool MeasurementSettingsEnabled
        {
            get
            {
                bool measurementSettingsEnabled;
                return bool.TryParse(LibraryData.Settings["MeasurementSettingsEnabled"], out measurementSettingsEnabled) && measurementSettingsEnabled;
            }
        }



        public CommunicationViewModel Communication { get; } = new CommunicationViewModel();

        public string FileName
        {
            get { return LibraryData.SystemFileName; }
            set
            {
                LibraryData.SystemFileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        public string NotConnectedMcu => Main.NotConnectedMcu;

        public string ConnectedMcu => Main.ConnectedMcu;

        public ICommand EnableDcOperation
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!IsInDcOperation) LibraryData.FuturamaSys.IsInDcOperation = true;
                    else if (MessageBox.Show(Main.lowvoltQuestion, Main.lowvoltQuestionTitle,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) ==
                             MessageBoxResult.Yes)
                        LibraryData.FuturamaSys.IsInDcOperation = false;
                    RaisePropertyChanged(() => IsInDcOperation);
                }, () => LibraryData.SystemIsOpen);
            }
        }

        public bool IsInDcOperation
        {
            get
            {
                if (!LibraryData.SystemIsOpen) return false;
                return LibraryData.FuturamaSys.IsInDcOperation;
            }
        }

        public ObservableCollection<ITabControl> TabCollection
        {
            get;
        }

        public ObservableCollection<MenuItem> RecentFiles { get; } = new ObservableCollection<MenuItem>();
        //Remove main unit
        public ICommand RemoveMainUnitCommand => new RelayCommand<MainUnitViewModel>(RemoveMainUnit);

        public ICommand OpenMeasureMentSettings
            => new RelayCommand<MainUnitViewModel>(m => m.OpenMeasurementSettings(), q => q?.Id > -1);

        public ICommand SendUnitDataToEsc => new RelayCommand(TimeStampAsync);

        public ICommand DownloadFromEsc
        {
            get
            {
                return
                    new RelayCommand(async
                        () =>
                        {
                            if (!SystemIsOpen())
                                return;

                            if (!AreConnections()) return;
                            var q = await InformUserTimestampAsync(false);
                            if (!q) return;

                            GetSystem();
                        });
            }
        }

        public ICommand OpenFile => new RelayCommand<string>(Open);

        public ICommand SaveAs
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsInDesignMode) return;
                    var q = new SystemFileSaveAs();
                    q.Save();
                }, () => LibraryData.SystemIsOpen);
            }
        }

        public ICommand OpenAbout
        {
            get
            {
                return new RelayCommand(() => Dispatcher.CurrentDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBox.Show(Main.AboutwindowText + Environment.NewLine +
                                            Assembly.GetExecutingAssembly().GetName().Version,
                                Main.AboutWindowTitle, MessageBoxButton.OK, MessageBoxImage.Information,
                                MessageBoxResult.OK, MessageBoxOptions.None))));
            }
        }

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string[] InstallerVersion => LibraryData.FuturamaSys == null
            ? null
            : new[]
            {
                LibraryData.FuturamaSys.CreatedInstallerVersion, LibraryData.FuturamaSys.LastSavedInstallerVersion
            };



        public ITabControl SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                RaisePropertyChanged(() => SelectedTab);
            }
        }

        public ICommand OpenSDCardManager => new RelayCommand(OpenExternalExecutable);

        private static void OpenExternalExecutable()
        {
            try
            {
                var path = Directory.GetCurrentDirectory();
                const string folder = "\\DirMp3\\";
                Process.Start(path + folder + "DIR_MP3.exe");

            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message, "Error starting external application", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public ICommand CloseFile => new RelayCommand(() => Close(), () => LibraryData.SystemIsOpen);
        public ICommand InsertNewUnit => new RelayCommand(AddMainUnit, () => LibraryData.SystemIsOpen);

        public ICommand NewSystem
        {
            get { return new RelayCommand(() => New()); }
        }

        public ICommand Exit
        {
            get { return new RelayCommand(() => ExitCommand()); }
        }

        private static void DisconnectAllUnists()
        {
            var any = false;
            var str = new StringBuilder();
            str.AppendLine();
            foreach (
                var connectionVm in
                    CommunicationViewModel.OpenConnections.Where(c => c.ConnectMode == ConnectMode.Install))
            {
                any = true;
                str.AppendLine(connectionVm.Ipaddress);
                connectionVm.Connection.Disconnect();
            }
            if (!any) return;
            MessageBox.Show(string.Format(Main.DisconnectedUnits, str), Main.DisconnectedUnitsTitle,
                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private async void TimeStampAsync()
        {
            if (!AreConnections()) return;
            var q = await InformUserTimestampAsync(true);
            if (!q) return;
            SendUnitData();
        }

        private static void SendUnitData()
        {
            var nq = new DownloadView();
            nq.Show();
        }

        private async Task<bool> InformUserTimestampAsync(bool isUploadText)
        {
            var affectedMainUnits = await VerifyIntegratyAsync();
            if (affectedMainUnits.Count <= 0) return true;
            var str = new StringBuilder();
            str.AppendLine();
            str.AppendLine();
            foreach (var mainUnitViewModel in affectedMainUnits)
            {
                str.AppendLine(" - " + mainUnitViewModel.DisplayValue);
            }

            //if(MessageBox.Show(
            //    string.Format(isUploadText ? Main.TimestampVerifyUpload : Main.TimestampVerifyDownload, str),
            //    Main.TimestampVerifyTitle,
            //    MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel) return false;

            UpdateTimestampConnectedUnits(affectedMainUnits);

            return true;
        }

        /// <summary>
        ///     Set timestamp for all units when this is not already done.
        ///     Otherwise put downloaded esc timestamp in designfile.
        /// </summary>
        /// <param name="mainunits"></param>
        private static void UpdateTimestampConnectedUnits(IEnumerable<MainUnitViewModel> mainunits)
        {
            foreach (var mainUnitViewModel in mainunits)
            {
                //if no timestamp ever made avaiable, create one
                if (mainUnitViewModel.Timestamp == 0)
                {
                    var d = new TimeStampUpdater(mainUnitViewModel.DataModel);
                    foreach (var dispatchData in d.TimeStamp())
                    {
                        CommunicationViewModel.AddData(dispatchData);
                    }
                    
                }
                else
                    mainUnitViewModel.DataModel.TimeStampWrittenToEsc = mainUnitViewModel.Timestamp;
            }
        }

        /// <summary>
        ///     Checks timestamps
        /// </summary>
        /// <returns>true in case timestamp does compare</returns>
        private async Task<List<MainUnitViewModel>> VerifyIntegratyAsync()
        {
            var list = new List<MainUnitViewModel>();
            foreach (var tab in TabCollection.OfType<MainUnitViewModel>()
                .Where(model => model.ConnectType != ConnectType.None))
            {
                var tab1 = tab;
                var q = await Task.Run(() => tab1.TimestampIsEqual());
                if (q) continue; //checksum O.K.

                list.Add(tab);
            }

            return list;
        }

        public void PrepaireDesign()
        {
            foreach (
    var s in
        CommunicationViewModel.OpenConnections.Where(s => s.ConnectMode == ConnectMode.Install)
            .Select(n => n.UnitId)
            .Except(TabCollection.OfType<MainUnitViewModel>().Select(u => u.Id)))
            {
                AddMainUnit(s);
            }

        }

        private static void GetSystem()
        {
            var nq = new DownloadView();

            nq.Show();
        }

        private bool SystemIsOpen()
        {
            if (LibraryData.SystemIsOpen) return true;
            var q = MessageBox.Show(Main.CreateNewDesign, Main.CreateNewDesignHeader, MessageBoxButton.OKCancel,
                MessageBoxImage.Question, MessageBoxResult.OK);
            return q == MessageBoxResult.OK && New();
        }

        private bool AreConnections()
        {
            if (Communication.Connections.Any(connection => connection.ConnectMode == ConnectMode.Install))
                return true;
            MessageBox.Show(Main.MessageBoxNoConnectionText, Main.MessageBoxNoConnectionTitle, MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            SelectedTab = TabCollection.FirstOrDefault(n => n is CommunicationViewModel);
            return false;
        }



        /// <summary>
        ///     Removes a slave from the system and reorders unit numbers and cardnumbers
        /// </summary>
        private void RemoveMainUnit(MainUnitViewModel m)
        {
            if (MessageBox.Show(Main._mainMSGDelUnitConfirmation, Main._mainMSGDelUnitTitle,
                MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                RemoveUnit(m);
        }

        public bool ExitCommand()
        {
            if (IsInDesignMode) return true;
            if (LibraryData.SystemIsOpen)
            {
                if (IsInDesignMode) return false;
                var g = new SystemFileSaveAsk();
                return g.Save();
            }

            Application.Current.Shutdown();
            return true;
        }

        public static void AddData(IDispatchData data)
        {
            CommunicationViewModel.AddData(data);
        }

        /// <summary>
        ///     Create a new system
        /// </summary>
        /// <returns>true if file creation succeeded, false if user cancelled or otherwise</returns>
        private bool New()
        {
            if (!Close())
                return false;

            MessageBox.Show(Main.SaveNewSystem, Main.SaveNewSystemTitle, MessageBoxButton.OK,
                MessageBoxImage.Information, MessageBoxResult.OK);

            LibraryData.CreateEmptySystem();

            LibraryData.FuturamaSys.CreatedInstallerVersion =
                Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (!IsInDesignMode)
            {
                var q = new SystemFileSaveAs();
                if (!q.Save())
                {
                    LibraryData.CloseProject();
                    return false;
                }
            }

            RaisePropertyChanged(() => FileName);
            AddMainUnitsToTab();
            _recentFilesLogic.AddFile(FileName);

            RaisePropertyChanged(() => InstallerVersion);

            return true;
        }

        /// <summary>
        ///     Adds al the mainunits and the matrix tab
        /// </summary>
        private void AddMainUnitsToTab()
        {
            if (!LibraryData.SystemIsOpen) return;

            TabCollection.Clear();

            foreach (var m in LibraryData.FuturamaSys.MainUnits) TabCollection.Add(MainUnitFactory(m));
            SelectedTab = TabCollection.FirstOrDefault(i => i.Id == 0);

            TabCollection.Add(PanelViewFactory);
            TabCollection.Add(Communication);

        }

        private MainUnitViewModel MainUnitFactory(MainUnitModel m)
        {
            var ret = new MainUnitViewModel(m, this);

            return ret;
        }

        public PanelViewModel PanelViewFactory
        {
            get
            {
                var ret = new PanelViewModel(this);
                ret.MessageSelectionChanged += (sender, args) => OnMessageSelectionChanged(args);
                return ret;
            }
        }



        public event EventHandler<SelectionEventArgs> MessageSelectionChanged;

        /// <summary>
        ///     Opens esc file
        /// </summary>
        private void Open(string filename)
        {
            var initialDirectory = string.Empty;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RecentLocationProject))
                initialDirectory = Properties.Settings.Default.RecentLocationProject;

            if (string.IsNullOrWhiteSpace(filename) || filename == "open")
                filename = FileManagement.OpenFileDialog(initialDirectory).FileName;


            if (string.IsNullOrWhiteSpace(filename)) return;

            FuturamaSysModel t;

            try
            {
                t = FileManagement.OpenSystemFile(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Could't open file", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
                return;
            }

            Properties.Settings.Default.RecentLocationProject = Path.GetDirectoryName(filename);

            if (LibraryData.SystemIsOpen)
            {
                if (!Close()) return;
            }

            FileName = filename;

            LibraryData.OpenSystem(t);
            _recentFilesLogic.AddFile(filename);
            UpdateGraphics();
        }

        void UpdateGraphics()
        {
            Communication.UpdateConnections();
            RaisePropertyChanged(() => InstallerVersion);

            AddMainUnitsToTab();
            AddConnections();

            var master = TabCollection.OfType<MainUnitViewModel>().FirstOrDefault(i => i.Id == 0);
            SelectedTab = master;

            master?.UpdateHardware();
            master?.OnPresetNamesUpdated();
            master?.OnDspMirrorUpdated();
        }

        public void AddConnections()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Connections == null)
                LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
            foreach (var q in LibraryData.FuturamaSys.Connections.Select(connection => new ConnectionViewModel(connection))
                .Where(q => !CommunicationViewModel.OpenConnections.Contains(q)))
            {
                CommunicationViewModel.OpenConnections.Add(q);
            }
        }

        /// <summary>
        ///     Close current system
        /// </summary>
        private bool Close()
        {
            if (!LibraryData.SystemIsOpen) return true;


            if (!IsInDesignMode)
            {
                var qq = new SystemFileSaveAsk();
                if (!qq.Save())
                    return false;
            }


            LibraryData.CloseProject();
            TabCollection.RemoveAll(q => q is MainUnitViewModel || q is PanelViewModel);

            LibraryData.SystemFileName = string.Empty;

            RaisePropertyChanged(() => FileName);


            RaisePropertyChanged(() => InstallerVersion);
            return true;
        }

        /// <summary>
        ///     adds a new mainunit to the system
        /// </summary>
        private void AddMainUnit()
        {
            if (!LibraryData.SystemIsOpen)
                LibraryData.CreateEmptySystem();

            var t = new UcEscNum(LibraryData.FuturamaSys.MainUnits.Select(n => n.Id).ToList());
            if (t.ShowDialog() == false) return;

            var mu = LibraryData.AddEsc(t.Result);
            if (mu == null) return;
            var newunit = new MainUnitViewModel(mu, this);
            if (!TabCollection.Contains(newunit, new TabComparer()))
                TabCollection.Add(newunit);
            OnSystemChanged(new SystemChangedEventArgs() { NewMainUnit = newunit });
        }

        private void AddMainUnit(int id)
        {
            if (!LibraryData.SystemIsOpen)
                LibraryData.CreateEmptySystem();
            var esc = LibraryData.AddEsc(id);
            if (esc == null) return;
            var escview = new MainUnitViewModel(esc, this);
            if (!TabCollection.Contains(escview, new TabComparer()))
                TabCollection.Add(escview);
            OnSystemChanged(new SystemChangedEventArgs() { NewMainUnit = escview });
        }

        class TabComparer : EqualityComparer<ITabControl>
        {
            public override bool Equals(ITabControl x, ITabControl y)
            {
                return x.Id == y.Id;
            }

            public override int GetHashCode(ITabControl obj)
            {
                return obj?.Id ?? 0;
            }
        }

        public void RemoveUnit(MainUnitViewModel mainUnit)
        {
            OnSystemChanged(new SystemChangedEventArgs()
            {
                OldMainUnit = mainUnit
            });

            TabCollection.Remove((mainUnit));
            LibraryData.FuturamaSys.MainUnits.Remove(mainUnit.DataModel);
        }

        public event EventHandler<SystemChangedEventArgs> SystemChanged;


        protected virtual void OnSystemChanged(SystemChangedEventArgs e)
        {
            SystemChanged?.Invoke(this, e);
        }


        protected virtual void OnMessageSelectionChanged(SelectionEventArgs e)
        {
            MessageSelectionChanged?.Invoke(this, e);
        }
    }

    public class SystemChangedEventArgs : EventArgs
    {
        public MainUnitViewModel NewMainUnit { get; set; }
        public MainUnitViewModel OldMainUnit { get; set; }
    }
}