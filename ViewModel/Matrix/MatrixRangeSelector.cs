using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.Matrix
{
    public abstract class MatrixRangeSelector : ViewModelBase
    {
        private bool _isSelected;
        public int Id { get; protected set; }

        protected MatrixRangeSelector(int id)
        {     
            Id = id;
        }

        public bool IsEnabled
        {
            get
            {
                return !IsSelected;
            }
        }

        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(_isSelected == value) return;                
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