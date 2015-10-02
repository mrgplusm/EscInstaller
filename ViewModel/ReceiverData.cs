using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace EscInstaller.ViewModel
{
    public class ReceiverData : ViewModelBase
    {
        private readonly Func<ReceiverData, int> _count;
        private readonly Func<int, string> _current;
        private readonly Action<int, string> _data;
        private readonly Action<int> _remove;

        public ReceiverData(Action<int, string> updateData, Func<ReceiverData, int> count, Func<int, string> current,
                            Action<int> remove)
        {
            _current = current;
            _remove = remove;
            _data = updateData;
            _count = count;
        }

        public String ReceiverAddress
        {
            get { return _current.Invoke(_count.Invoke(this)); }
            set { _data.Invoke(_count.Invoke(this), value); }
        }

        public ICommand RemoveEmail
        {
            get { return new RelayCommand(() => _remove.Invoke(_count.Invoke(this))); }
        }
    }
}