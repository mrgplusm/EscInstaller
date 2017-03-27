#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Model;
using EscInstaller.View.Communication;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

#endregion

namespace EscInstaller.ViewModel.Connection
{
    public class ConnectionViewModel : ViewModelBase, IEquatable<ConnectionViewModel>
    {
        
        private string _errorInfo;
        private ObservableCollection<string> _ports;
        private int _unitId = -1;

        public ConnectionViewModel(ConnectionModel connection)
        {
            DataModel = connection;
            Connection = new Common.Connection();

            Connection.ErrorOccured += (a, b) => Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorInfo = Communication.ErrorConnection;//  b.Exception.Message;

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

        public Common.Connection Connection { get; }
        public ConnectionModel DataModel { get; }

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
                RaisePropertyChanged(() => UnitId);
            }
        }

        public ObservableCollection<string> Ports => _ports ?? (_ports = new ObservableCollection<string>(PortList.GetList()));

        public string ErrorInfo
        {
            get
            {
#if DEBUG
                return IsInDesignMode ? "No connection could be established" : _errorInfo;
#endif
            }
            private set
            {
                _errorInfo = value;
                RaisePropertyChanged(() => ErrorInfo);
            }
        }

        public ConnectMode ConnectMode
        {
            get { return Connection.Mode; }
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

        public bool Equals(ConnectionViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DataModel, other.DataModel);
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
            return other.GetType() == GetType() && Equals((ConnectionViewModel) other);
        }

        public override int GetHashCode()
        {
            return (DataModel != null ? DataModel.GetHashCode() : 0);
        }
    }
}