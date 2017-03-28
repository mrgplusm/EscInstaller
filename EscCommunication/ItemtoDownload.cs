#region

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.EscCommunication
{
    public abstract class ItemtoDownload : ViewModelBase, IProgress<DownloadProgress>, IDownloadableItem
    {
        protected readonly MainUnitViewModel Main;
        private bool _doDownload;
        private double _progressBar;
        private bool _receiveCompleted;

        protected ItemtoDownload(MainUnitViewModel main)
        {
            Main = main;
            DataChilds = new ObservableCollection<IDownloadableItem>();
        }

        public bool Checked
        {
            get { return _doDownload; }
            set
            {
                _doDownload = value;
                OnDownloadClicked();
                RaisePropertyChanged(() => Checked);
            }
        }

        public ObservableCollection<IDownloadableItem> DataChilds { get; }

        public abstract string Value { get; }

        /// <summary>
        ///     to execute when this item is selected
        /// </summary>
        public abstract Task Function { get; }

        public bool Completed
        {
            get { return _receiveCompleted; }
            set
            {
                Done();
                _receiveCompleted = value;
                RaisePropertyChanged(() => Completed);
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

        public event EventHandler HasCompleted;

        public void Report(DownloadProgress e)
        {
            if (e.Progress >= e.Total - .01)
            {
                Completed = true;
                OnHasCompleted();
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
            RaisePropertyChanged(() => Checked);
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
            RaisePropertyChanged(() => Completed);
        }

        protected virtual void OnHasCompleted()
        {
            HasCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}