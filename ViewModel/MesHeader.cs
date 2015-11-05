#region

using System;
using System.Globalization;
using System.Windows.Media;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel
{
    public class MesHeader : ViewModelBase
    {
        private readonly Func<bool> _isChecked;
        private bool _isMousePointer;

        public MesHeader(int id, Action<bool> change, Func<bool> isChecked)
        {
            Id = id;
            Change = change;
            _isChecked = isChecked;
        }

        public Action<bool> Change { get; }
        public int Id { get; }

        public string DisplayId
        {
            get { return (Id + 1).ToString(CultureInfo.InvariantCulture); }
        }

        public bool IsChecked
        {
            get { return _isChecked.Invoke(); }
            set { Change.Invoke(value); }
        }

        public bool IsMousePointer
        {
            get { return _isMousePointer; }
            set
            {
                if (value == _isMousePointer) return;
                _isMousePointer = value;
                RaisePropertyChanged(() => Background);
            }
        }

        public Brush Background
        {
            get { return IsMousePointer ? Brushes.BlueViolet : Brushes.White; }
        }

        public void Notifychange()
        {
            RaisePropertyChanged(() => IsChecked);
        }
    }
}