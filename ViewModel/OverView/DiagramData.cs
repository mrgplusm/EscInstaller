#region

using System.Windows;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public abstract class DiagramData : ViewModelBase
    {
        private bool _animateWhenLoaded;
        private BindablePoint _location;
        public abstract Point Size { get; }

        public virtual int ZIndex
        {
            get { return 0; }
        }

        public virtual bool AnimateWhenLoaded
        {
            get { return _animateWhenLoaded; }
            set
            {
                _animateWhenLoaded = value;
                RaisePropertyChanged(() => AnimateWhenLoaded);
            }
        }

        public virtual bool IsEnabled
        {
            get { return true; }
        }

        public virtual BindablePoint Location
        {
            get { return _location ?? (_location = new BindablePoint()); }
        }
    }
}