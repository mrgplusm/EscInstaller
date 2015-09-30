using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common;
using Common.Commodules;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EscInstaller.ViewModel.Connection
{
    public interface ITabControl
    {
        int Id { get; }
    }

    public class CommunicationViewModel : ViewModelBase, ITabControl
    {
        public static readonly ObservableCollection<ConnectionViewModel> OpenConnections =
            new ObservableCollection<ConnectionViewModel>();

        private int _tabIndex;

        public static void AddData(IDispatchData data)
        {
            var con = OpenConnections.FirstOrDefault(d => d.UnitId == data.DestinationAddress);
            if (con != null)
                con.Connection.EnQueue(data);
        }

        public static void AddData(IEnumerable<IDispatchData> data)
        {
            foreach (var dispatchData in data)
            {
                AddData(dispatchData);
            }
        }

        public CommunicationViewModel()
        {

        }

        public void UpdateConnections()
        {
            if (LibraryData.FuturamaSys.Connections == null) return;

            foreach (var c in LibraryData.FuturamaSys.Connections.Select(model => new ConnectionViewModel(model)))
            {
                OpenConnections.Add(c);
            }
        }



        public int Id
        {
            get { return 99; }
        }

        public ObservableCollection<ConnectionViewModel> Connections
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return new ObservableCollection<ConnectionViewModel>(new List<ConnectionViewModel>()
                    {
                        new ConnectionViewModel(new ConnectionModel()),
                        new ConnectionViewModel(new ConnectionModel(){IsInDetailMode = true}),
                        
                    });
                }
#endif

                return OpenConnections;
            }
        }

        public bool IsMonitoring { get; set; }

        public int TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                RaisePropertyChanged(() => TabIndex);
            }
        }



        public ICommand AddConnection
        {
            get
            {
                return new RelayCommand<string>((q) =>
                {
                    var z = new ConnectionModel { IsNetConnect = (q == "net") };
                    var vm = new ConnectionViewModel(z);

                    vm.Connection.ConnectModeChanged += Connection_ConnectModeChanged;
                    vm.Connection.UnitIdChanged += Connection_ConnectModeChanged;
                    OpenConnections.Add(vm);

                    if (!LibraryData.SystemIsOpen) return;
                    if (LibraryData.FuturamaSys.Connections == null)
                        LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
                    LibraryData.FuturamaSys.Connections.Add(z);
                });
            }
        }

        void Connection_ConnectModeChanged(object sender, ConnectionModeChangedEventArgs c)
        {
            OnConnectionModeChanged(c);
        }

        public event EventHandler<ConnectionModeChangedEventArgs> ConnectionModeChanged;

        protected virtual void OnConnectionModeChanged(ConnectionModeChangedEventArgs e)
        {
            EventHandler<ConnectionModeChangedEventArgs> handler = ConnectionModeChanged;
            if (handler != null) handler(this, e);
        }

        public ICommand RemoveConnection
        {
            get
            {
                return new RelayCommand<ConnectionViewModel>(s =>
                    {
                        //stop connection
                        s.EndConnection();

                        //remove from view
                        OpenConnections.Remove(s);

                        if (!LibraryData.SystemIsOpen) return;
                        if (LibraryData.FuturamaSys.Connections == null)
                        {
                            LibraryData.FuturamaSys.Connections = new List<ConnectionModel>();
                            return;
                        }
                        LibraryData.FuturamaSys.Connections.Remove(s.DataModel);
                    });
            }
        }

        public bool ReadDisclaimer
        {
            get { return Properties.Settings.Default.ReadCommunicationDisclaimer; }
            set
            {
                Properties.Settings.Default.ReadCommunicationDisclaimer = value;
                RaisePropertyChanged(() => ReadDisclaimer);
            }
        }
    }


}