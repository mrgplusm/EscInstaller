using System;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.Settings
{
    public class RadioButton : ViewModelBase
    {
        private readonly Action _onCheck;

        private string _content;
        private bool _isChecked;
        private bool _isEnabled;

        public RadioButton(Action onCheck, bool isChecked)
        {
            _onCheck = onCheck;
            _isChecked = isChecked;
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                if (value)
                    _onCheck.Invoke();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }
    }
}