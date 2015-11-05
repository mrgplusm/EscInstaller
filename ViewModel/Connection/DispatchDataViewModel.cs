#region

using System;
using Common.Commodules;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Connection
{
    public class DispatchDataViewModel : ViewModelBase, IEquatable<DispatchDataViewModel>
    {
        public DispatchDataViewModel(IDispatchData data)
        {
            DataModel = data;
            DataModel.Error += (q, r) =>
            {
                RaisePropertyChanged(() => ErrorMessage);
                RaisePropertyChanged(() => HasErrors);
            };

            DataModel.Dispatched += () => RaisePropertyChanged(() => IsDispatched);
        }

        public DateTime CreationDate
        {
            get { return DataModel.CreationDate; }
        }

        public string ErrorMessage
        {
            get { return DataModel.ErrorMessage; }
        }

        public int DestinationAddress
        {
            get { return DataModel.DestinationAddress; }
        }

        public bool HasErrors
        {
            get { return !DataModel.ErrorMessage.Equals(string.Empty); }
        }

        public int Id
        {
            get { return DataModel.Id; }
        }

        public string ModuleName
        {
            get { return DataModel.ModuleName; }
        }

        public bool IsDispatched
        {
            get { return DataModel.IsDispatched; }
        }

        public IDispatchData DataModel { get; }

        public bool Equals(DispatchDataViewModel other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return DataModel.Equals(other.DataModel);
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
            return DataModel.GetHashCode();
        }

        public void Remove()
        {
            DataModel.OnRemove();
        }
    }
}