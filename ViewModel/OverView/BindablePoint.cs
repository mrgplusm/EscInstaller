using System;
using System.Windows;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.OverView
{
    public class BindablePoint : ViewModelBase
    {
        public virtual double X
        {
            get { return Value.X; }
            set { Value = new Point(value, Value.Y); }
        }

        public virtual double Y
        {
            get { return Value.Y; }
            set { Value = new Point(Value.X, value); }
        }

        private Point _value;
        public Point Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
                RaisePropertyChanged(() => X);
                RaisePropertyChanged(() => Y);

                ValueChanged();
            }
        }


        public void ValueChanged()
        {
            if (OnValueChanged != null)
                OnValueChanged();
        }

        public Action OnValueChanged;
    }

}
