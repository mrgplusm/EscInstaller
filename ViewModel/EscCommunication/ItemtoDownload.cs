using System;
using Common;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.EscCommunication
{
    public class ItemtoDownload : ViewModelBase
    {
        private bool _receiveCompleted;
        private bool _doDownload;
        private double _progressBar;


        public event EventHandler DownloadClicked;

        protected virtual void OnDownloadClicked()
        {
            EventHandler handler = DownloadClicked;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public void SelectDownload(bool value)
        {
            _doDownload = value;
            RaisePropertyChanged(() => DoDownload);
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

        public string ItemName { get; set; }
        /// <summary>
        /// to execute when this item is selected
        /// </summary>
        public Action Function { get; set; }


        public ItemtoDownload()
        {

        }

        public bool ReceiveCompleted
        {
            get { return _receiveCompleted; }
            set
            {
                _receiveCompleted = value;
                RaisePropertyChanged(() => ReceiveCompleted);
            }
        }

        /// <summary>
        /// Item finished downloading
        /// </summary>
        public event EventHandler DownloadCompleted;

        protected virtual void OnDownloadCompleted()
        {
            EventHandler handler = DownloadCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Indicates progress from 0 - 100;
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

        public virtual void OnCompleted(object sender, DownloadProgressEventArgs e)
        {

            if (e.Progress >= e.Total - .01)
            {
                ReceiveCompleted = true;
                OnDownloadCompleted();
                ProgressBar = 100;
            }
            else
            {
                ProgressBar = (double)e.Progress / e.Total * 100;
            }
        }

        public void OnCompleted(object sender, EventArgs e)
        {
            ReceiveCompleted = true;
            OnDownloadCompleted();
            ProgressBar = 100;
        }
    }

    public class ItemToEeprom : ItemtoDownload
    {
        public E2PromArea Area { get; set; }

        public void OnCompleted(object sender, DownloadEepromEventArgs e)
        {
            if (Area == e.Area)
                base.OnCompleted(sender, e);
        }
    }
}