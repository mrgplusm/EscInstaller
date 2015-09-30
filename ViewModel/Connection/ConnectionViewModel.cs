using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EscInstaller.ViewModel.Connection
{
    public class ConnectionViewModel : ViewModelBase, IEquatable<ConnectionViewModel>
    {
        private string _errorInfo;

        public ConnectionViewModel(ConnectionModel connection)
        {
            DataModel = connection;
            Connection = new Common.Connection();

            Connection.ErrorOccured += message => Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorInfo = message;

                RaisePropertyChanged(() => ConnectMode);
            });

            Connection.ConnectModeChanged += (sender, mode) => Application.Current.Dispatcher.Invoke(() =>
            {            
                ErrorInfo = string.Empty;
                RaisePropertyChanged(() => ConnectMode);
            });

            Connection.UnitIdChanged += (sender, args) => UnitId = args.UnitId;
#if DEBUG
            if (ConnectType == ConnectType.Ethernet)
            {
                Ipaddress = "127.0.0.1";
            }
#endif
        }

        public Common.Connection Connection { get; private set; }


        public ConnectionModel DataModel { get; private set; }

        public bool IsNetwork
        {
            get { return DataModel.IsNetConnect; }
        }

        public ConnectType ConnectType
        {
            get { return DataModel.IsNetConnect ? ConnectType.Ethernet : ConnectType.USB; }
        }

        public int UnitId
        {
            get { return _unitId; }
            private set
            {
                _unitId = value;
                RaisePropertyChanged(()=> UnitId);
            }
        }

        private ObservableCollection<string> _ports;
        public ObservableCollection<string> Ports
        {
            get
            {
                return _ports ?? (_ports = new ObservableCollection<string>(PortList.GetList()));
            }
        }

        public string ErrorInfo
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return "This is an errorThis is an errorThis is an errorThis is an errorThis is " +
                           "an errorThis is an errorThis is an errorThis is an errorThis is an error";
                }
#endif
                return _errorInfo;
            }
            private set
            {
                _errorInfo = value;
                RaisePropertyChanged(() => ErrorInfo);
            }
        }

        public ConnectMode ConnectMode
        {
            get
            {
                return Connection.Mode;
            }
        }

        public ICommand ClearChangeList
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        foreach (var source in ChangeList.ToList())
                        {
                            source.Remove();
                        }

                        ChangeList.Clear();
                    });
            }
        }


        private ObservableCollection<DispatchDataViewModel> _changelist;
        private int _unitId = -1;

        public ObservableCollection<DispatchDataViewModel> ChangeList
        {
            get { return _changelist ?? (_changelist = new ObservableCollection<DispatchDataViewModel>()); }
        }

        public ICommand RemoveData
        {
            get
            {
                return new RelayCommand<DispatchDataViewModel>(data =>
                    {
                        data.Remove();
                        ChangeList.Remove(data);
                    });
            }
        }

        public bool IsInDetailMode
        {
            get { return DataModel.IsInDetailMode; }
            set
            {
                DataModel.IsInDetailMode = value;
                RaisePropertyChanged(() => IsInDetailMode);
            }
        }

        public string Ipaddress
        {
            get { return DataModel.IpAddress; }
            set
            {
                DataModel.IpAddress = value;
                RaisePropertyChanged(() => Ipaddress);
            }
        }

        public void EndConnection()
        {
            ErrorInfo = string.Empty;
            Connection.Disconnect();
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.GetType() == GetType() && Equals((ConnectionViewModel)other);
        }

        public override int GetHashCode()
        {
            return (DataModel != null ? DataModel.GetHashCode() : 0);
        }

        public bool Equals(ConnectionViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DataModel, other.DataModel);
        }


    }
}