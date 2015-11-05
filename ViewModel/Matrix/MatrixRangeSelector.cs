#region

using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    public abstract class MatrixRangeSelector : ViewModelBase
    {
        private bool _isSelected;

        protected MatrixRangeSelector(int id)
        {
            Id = id;
        }

        public int Id { get; protected set; }

        public bool IsEnabled
        {
            get { return !IsSelected; }
        }

        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                Update();
            }
        }

        public abstract string DisplayValue { get; }

        protected void Update()
        {
            RaisePropertyChanged(() => IsSelected);
            RaisePropertyChanged(() => IsEnabled);
        }
    }
}