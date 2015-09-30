using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System.Windows;
using System.Collections.Generic;

namespace EscInstaller.ViewModel.OverView
{

    public abstract class DiagramData : ViewModelBase
    {
        public abstract Point Size { get; }

        public virtual int ZIndex
        {
            get
            {
                return 0;
            }
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
            get
            {
                return true;
            }
        }

        private BindablePoint _location;
        private bool _animateWhenLoaded;

        public virtual BindablePoint Location
        {
            get
            {
                return _location ?? (_location = new BindablePoint());
            }
        }
    }


}