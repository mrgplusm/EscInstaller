#region

using System;
using System.Threading.Tasks;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication.Logic;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.EscCommunication
{
    public abstract class ItemtoDownload : ViewModelBase, IProgress<DownloadProgress>
    {
        protected readonly MainUnitViewModel Main;
        private bool _doDownload;
        private double _progressBar;
        private bool _receiveCompleted;

        protected ItemtoDownload(MainUnitViewModel main)
        {
            Main = main;
        }

        public bool DoDownload
        {
            get { return _doDownload; }
            set
            {
                _doDownload = value;
                OnDownloadClicked();
                RaisePropertyChanged(() => DoDownload);
            }
        }

        public abstract string ItemName { get; }

        /// <summary>
        ///     to execute when this item is selected
        /// </summary>
        public abstract Task Function { get; }

        public bool ReceiveCompleted
        {
            get { return _receiveCompleted; }
            set
            {
                Done();
                _receiveCompleted = value;
                RaisePropertyChanged(() => ReceiveCompleted);
            }
        }

        /// <summary>
        ///     Indicates progress from 0 - 100;
        /// </summary>
        public double ProgressBar
        {
            get { return _progressBar; }
            private set
            {
                _progressBar = value;
                RaisePropertyChanged(() => ProgressBar);
            }
        }

        public void Report(DownloadProgress e)
        {
            if (e.Progress >= e.Total - .01)
            {
                ReceiveCompleted = true;

                ProgressBar = 100;
            }
            else
            {
                ProgressBar = (double) e.Progress/e.Total*100;
            }
        }

        public event EventHandler DownloadClicked;

        protected virtual void OnDownloadClicked()
        {
            DownloadClicked?.Invoke(this, EventArgs.Empty);            
        }

        public void SelectDownload(bool value)
        {
            _doDownload = value;
            RaisePropertyChanged(() => DoDownload);
        }

        protected Progress<DownloadProgress> ProgressFactory()
        {
            return new Progress<DownloadProgress>(Report);
        }

        protected virtual void Done() {}

        public void Reset()
        {
            ProgressBar = 0;
            _receiveCompleted = false;
            RaisePropertyChanged(() => ReceiveCompleted);
        }
    }
}