using System;
using Common.Commodules;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.Connection
{
    public class DispatchDataViewModel : ViewModelBase, IEquatable<DispatchDataViewModel>
    {
        private readonly IDispatchData _data;

        public DispatchDataViewModel(IDispatchData data)
        {
            _data = data;            
            _data.Error += (q, r) =>
                {
                    RaisePropertyChanged(() => ErrorMessage);
                    RaisePropertyChanged(() => HasErrors);
                };

            _data.Dispatched += () => RaisePropertyChanged(() => IsDispatched);            
        }        

        public bool Equals(DispatchDataViewModel other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _data.Equals(other._data);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as DispatchDataViewModel);
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        public DateTime CreationDate
        {
            get { return _data.CreationDate; }
        }

        public string ErrorMessage
        {
            get { return _data.ErrorMessage; }
        }

        public int DestinationAddress
        {
            get { return _data.DestinationAddress; }
        }

        public bool HasErrors
        {
            get { return !_data.ErrorMessage.Equals(string.Empty); }
        }

        public int Id
        {
            get { return _data.Id; }
        }

        public string ModuleName
        {
            get { return _data.ModuleName; }
        }

        public bool IsDispatched
        {
            get { return _data.IsDispatched; }
        }

        public IDispatchData DataModel
        {
            get { return _data; }
        }

        public void Remove()
        {
            _data.OnRemove();
        }
    }
}