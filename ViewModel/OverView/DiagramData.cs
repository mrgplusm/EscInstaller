#region

using System.Windows;
using GalaSoft.MvvmLight;
using Xceed.Wpf.Toolkit.Converters;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public abstract class DiagramData : ViewModelBase
    {
        private bool _animateWhenLoaded;
        private BindablePoint _location;
        public abstract Point Size { get; }

        public virtual int ZIndex => 0;

        public virtual bool AnimateWhenLoaded
        {
            get { return _animateWhenLoaded; }
            set
            {
                _animateWhenLoaded = value;
                RaisePropertyChanged(() => AnimateWhenLoaded);
            }
        }

        public virtual bool IsEnabled => true;

        public virtual BindablePoint Location => _location ?? (_location = new BindablePoint());        
        
    }
}