﻿#region

using System;
using System.Windows;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class BindablePoint : ViewModelBase
    {
        private Point _value;
        public Action OnValueChanged;

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
    }
}