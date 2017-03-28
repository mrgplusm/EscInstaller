#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.EscCommunication
{
    public abstract class Downloader : ViewModelBase, IProgress<DownloadProgress>, IDownloadableItem
    {
        protected readonly MainUnitViewModel Main;
        private bool _allChecked = true;
        private bool _escDownloadCompleted;

        protected Downloader(MainUnitViewModel main)
        {
            Main = main;
            DataChilds = new ObservableCollection<IDownloadableItem>();
        }

        public string Value => Main.DisplayValue;

        /// <summary>
        ///     Download all items of this esc
        /// </summary>
        public bool Checked
        {
            get { return _allChecked; }
            set
            {
                if (_allChecked == value) return;
                _allChecked = value;
                OnAllItemsChecked();
                CheckAllDownloadItems(value);
                RaisePropertyChanged(() => Checked);
            }
        }

        public bool Completed
        {
            get { return _escDownloadCompleted; }
            set
            {
                _escDownloadCompleted = value;
                RaisePropertyChanged(() => Completed);
            }
        }

        public ObservableCollection<IDownloadableItem> DataChilds { get; protected set; }

        protected void AttachHandler(ItemtoDownload itemtoDownload)
        {
            itemtoDownload.DownloadClicked += SubItemChecked;
            itemtoDownload.SelectDownload(true);
        }

        private void SubItemChecked(object sender, EventArgs eventArgs)
        {
            _allChecked = DataChilds.All(d => d.Checked);
            OnAllItemsChecked();
            RaisePropertyChanged(() => Checked);
        }

        /// <summary>
        ///     Occurs whenever state of allchecked changes
        /// </summary>
        public event EventHandler AllItemsChecked;

        protected virtual void OnAllItemsChecked()
        {

            AllItemsChecked?.Invoke(this, EventArgs.Empty);
        }

        private void CheckAllDownloadItems(bool value)
        {
            foreach (var itemtoDownload in DataChilds)((ItemtoDownload)itemtoDownload).SelectDownload(value);          
        }
        

        /// <summary>
        ///     One of the items finished downloading
        /// </summary>
        public event EventHandler DownloadItemStateChanged;

        protected virtual void OnDownloadItemStateChanged()
        {            
            DownloadItemStateChanged?.Invoke(this, EventArgs.Empty);
        }

        

        public async void StartDownload()
        {
            foreach (var source in DataChilds.Where(n => n.Checked))
            {
                ((ItemtoDownload)source).Reset();
            }
            foreach (var itemtoDownload in DataChilds.Where(n => n.Checked))
            {
                await ((ItemtoDownload)itemtoDownload).Function;
            }
        }

        private double _progressBar;

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
                Completed = true;

                ProgressBar = 100;
            }
            else
            {
                ProgressBar = (double)e.Progress / e.Total * 100;
            }
        }
    }
}