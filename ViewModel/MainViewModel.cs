using System;
using System.Collections.Generic;
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
using Common.Model;
using EscInstaller.ImportSpeakers;
using EscInstaller.UserControls;
using EscInstaller.View;
using EscInstaller.View.Communication;
using EscInstaller.ViewModel.EscCommunication;
using EscInstaller.ViewModel.Matrix;
using EscInstaller.ViewModel.SDCard;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using Common.Commodules;
using System.Collections.ObjectModel;
using CommunicationViewModel = EscInstaller.ViewModel.Connection.CommunicationViewModel;
using ConnectionViewModel = EscInstaller.ViewModel.Connection.ConnectionViewModel;
using ITabControl = EscInstaller.ViewModel.Connection.ITabControl;

namespace EscInstaller.ViewModel
{
    public enum BusyDownloading
    {
        No,
        ToEsc,
        FromEsc,
        WaitAcknowledge,
    }


    public class MainViewModel : ViewModelBase
    {
        private readonly ObservableCollection<ITabControl> _tabs = new ObservableCollection<ITabControl>();
        private readonly RecentFilesLogic _recentFilesLogic;

        private readonly CommunicationViewModel _comViewModel = new CommunicationViewModel();


        public MainViewModel()
        {
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

            Thread dispThread = Thread.CurrentThread;
            Dispatcher.FromThread(dispThread);


            _tabs.Add(_comViewModel);


#if DEBUG
            if (IsInDesignMode)
            {
                LibraryData.CreateEmptySystem();
            }
            Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\d.esc");
#endif
        }

        public CommunicationViewModel Communication
        {
            get { return _comViewModel; }
        }


        private static void DisconnectAllUnists()
        {
            var any = false;
            var str = new StringBuilder();
            str.AppendLine();
            foreach (var connectionVm in CommunicationViewModel.OpenConnections.Where(c => c.ConnectMode == ConnectMode.Install))
            {
                any = true;
                str.AppendLine(connectionVm.Ipaddress);
                connectionVm.Connection.Disconnect();
            }
            if (!any) return;
            MessageBox.Show(string.Format(Main.DisconnectedUnits, str), Main.DisconnectedUnitsTitle,
                    MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        public string FileName
        {
            get { return LibraryData.SystemFileName; }
            set
            {
                LibraryData.SystemFileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        public string NotConnectedMcu
        {
            get { return Main.NotConnectedMcu; }
        }

        public string ConnectedMcu
        {
            get { return Main.ConnectedMcu; }
        }

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
            get
            {
                if (IsInDesignMode)
                {
                    _tabs.Add(new MainUnitViewModel(LibraryData.EmptyEsc(0), this));
                }

                return _tabs;
            }
        }


        private readonly ObservableCollection<MenuItem> _recentFiles = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> RecentFiles
        {
            get { return _recentFiles; }
        }


        //Remove main unit
        public ICommand RemoveMainUnitCommand
        {
            get
            {
                return new RelayCommand<MainUnitViewModel>(RemoveMainUnit, q =>
                    {
                        if (q == null) return false; return q.Id > 0;
                    });
            }
        }

        public ICommand OpenMeasureMentSettings
        {
            get
            {
                return new RelayCommand<MainUnitViewModel>(m => m.OpenMeasurementSettings(), q =>
                {
                    //    return true;
                    if (q == null) return false; return q.Id > -1;
                });
            }
        }

        public ICommand SendUnitDataToEsc
        {
            get
            {
                return new RelayCommand(TimeStampAsync);
            }
        }

        private async void TimeStampAsync()
        {
            if (!AreConnections()) return;
            var q = await InformUserTimestampAsync(true);
            if (!q) return;
            SendUnitData();
        }

        private void SendUnitData()
        {
            if (TabCollection.OfType<MainUnitViewModel>().All(model => model.ConnectType == ConnectType.None))
            {
                MessageBox.Show(Main.MessageBoxUploadText, Main.MessageBoxUploadTitle,
                    MessageBoxButton.OK, MessageBoxImage.Question);
                SwitchToCommunicationTab();
            }
            else
            {
                var nq = new DownloadView
                {
                    Title = "UPLOAD to ESC: GUI => ESC",
                    Background = System.Windows.Media.Brushes.LightCoral
                };
                var qq = new CommunicationSend(this);//  EscCommunicationBase(this);
                nq.DataContext = qq;
                nq.Show();
            }

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
                str.AppendLine(" - " + mainUnitViewModel.Name);
            }

            var ret = MessageBox.Show(string.Format(isUploadText ? Main.TimestampVerifyUpload : Main.TimestampVerifyDownload, str), Main.TimestampVerifyTitle,
                MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.Cancel;
            if (!ret) return false;
            //update timestamp in datamodel if user confirms to do so
            UpdateTimestampConnectedUnits(affectedMainUnits);

            return true;
        }

        /// <summary>
        /// Set timestamp for all units when this is not already done. 
        /// Otherwise put downloaded esc timestamp in designfile.
        /// </summary>
        /// <param name="mainunits"></param>
        private static void UpdateTimestampConnectedUnits(IEnumerable<MainUnitViewModel> mainunits)
        {
            foreach (var mainUnitViewModel in mainunits)
            {
                //if no timestamp ever made avaiable, create one
                if (mainUnitViewModel.Timestamp == 0)
                    CommunicationViewModel.AddData(mainUnitViewModel.Sender.TimeStamp());
                else
                    mainUnitViewModel.DataModel.TimeStampWrittenToEsc = mainUnitViewModel.Timestamp;
            }
        }

        /// <summary>
        /// Checks timestamps 
        /// </summary>
        /// <returns>true in case timestamp does compare</returns>
        private async Task<List<MainUnitViewModel>> VerifyIntegratyAsync()
        {
            var list = new List<MainUnitViewModel>();
            foreach (var tab in TabCollection.OfType<MainUnitViewModel>()
                .Where(model => model.ConnectType != ConnectType.None))
            {
                MainUnitViewModel tab1 = tab;
                var q = await Task.Run(() => tab1.TimestampIsEqual());
                if (q) continue; //checksum O.K.

                list.Add(tab);
            }

            return list;
        }

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

        private void GetSystem()
        {


            foreach (var s in CommunicationViewModel.OpenConnections.Where(s => s.ConnectMode == ConnectMode.Install).Select(n => n.UnitId)
                            .Except(TabCollection.OfType<MainUnitViewModel>().Select(u => u.Id)))
            {
                AddMainUnit(s);
            }

            var nq = new DownloadView();
            nq.Title = "Download to PC ESC => GUI";
            nq.Background = System.Windows.Media.Brushes.LightGreen;
            var qq = new CommunicationReceive(this);//  EscCommunicationBase(this);
            nq.DataContext = qq;
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
            if (TabCollection.OfType<MainUnitViewModel>().Any(model => model.ConnectType != ConnectType.None)) return true;
            MessageBox.Show(Main.MessageBoxNoConnectionText, Main.MessageBoxNoConnectionTitle, MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            SelectedTab = TabCollection.FirstOrDefault(n => n is CommunicationViewModel);
            return false;
        }

        private void SwitchToCommunicationTab()
        {
            var comm = TabCollection.OfType<CommunicationViewModel>().FirstOrDefault();
            if (comm == null) return;
            SelectedTab = comm;
            comm.TabIndex = 0;
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


        public ICommand OpenFile
        {
            get { return new RelayCommand<string>(Open); }
        }

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

        public string AppVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public string[] InstallerVersion
        {
            get
            {
                return LibraryData.FuturamaSys == null ? null : new[]
                {
                    LibraryData.FuturamaSys.CreatedInstallerVersion, LibraryData.FuturamaSys.LastSavedInstallerVersion
                };
            }
        }



        public ICommand OpenSdCardMannager
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        var t = new SdLibraryEditorViewModel();
                        var z = new SdMessageCardView { DataContext = t };
                        z.Show();

                    });
            }
        }

        public ICommand OpenSpeakerLibEditor
        {
            get { return new RelayCommand(() => _tabs.Add(new LibraryEditorViewModel()), () => !_tabs.Any(q => q is LibraryEditorViewModel)); }
        }

        private ITabControl _selectedTab;
        public ITabControl SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                RaisePropertyChanged(() => SelectedTab);
            }
        }

        public ICommand CloseFile
        {
            get { return new RelayCommand(() => Close(), () => LibraryData.SystemIsOpen); }
        }



        public ICommand InsertNewUnit
        {
            get
            {                
                return new RelayCommand(AddMainUnit, () => LibraryData.SystemIsOpen);
            }
        }

        public ICommand NewSystem
        {
            get { return new RelayCommand(() => New()); }
        }

        public ICommand Exit
        {
            get
            {
                return new RelayCommand(() => ExitCommand());
            }
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
        /// Create a new system
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
        /// Adds al the mainunits and the matrix tab
        /// </summary>
        private async void AddMainUnitsToTab()
        {
            if (!LibraryData.SystemIsOpen) return;

            TabCollection.Clear();
            await Task.Delay(150);
            foreach (var m in LibraryData.FuturamaSys.MainUnits) TabCollection.Add(new MainUnitViewModel(m, this));
            SelectedTab = TabCollection.FirstOrDefault(i => i.Id == 0);

            TabCollection.Add(new PanelViewModel(this));
            TabCollection.Add(_comViewModel);
        }

        /// <summary>
        /// Opens esc file
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

            _comViewModel.UpdateConnections();
            RaisePropertyChanged(() => InstallerVersion);
            _recentFilesLogic.AddFile(filename);
            AddMainUnitsToTab();
            AddConnections();
            SelectedTab = TabCollection.FirstOrDefault(i => i.Id == 0);
        }

        public void AddConnections()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Connections == null) LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
            foreach (var connection in LibraryData.FuturamaSys.Connections)
            {
                var q = new ConnectionViewModel(connection);
                if (!CommunicationViewModel.OpenConnections.Contains(q)) CommunicationViewModel.OpenConnections.Add(q);
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
            if (!TabCollection.Contains(escview))
                TabCollection.Add(escview);
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
            EventHandler<SystemChangedEventArgs> handler = SystemChanged;
            if (handler != null) handler(this, e);
        }
    }

    public class SystemChangedEventArgs : EventArgs
    {
        public MainUnitViewModel NewMainUnit { get; set; }
        public MainUnitViewModel OldMainUnit { get; set; }
    }
}