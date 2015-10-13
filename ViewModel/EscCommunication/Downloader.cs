using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.EscCommunication
{
    public abstract class Downloader : ViewModelBase
    {
        protected readonly MainUnitViewModel Main;
        private bool _allChecked = true;
        private bool _escDownloadCompleted;

        protected Downloader(MainUnitViewModel main)
        {
            Main = main;
            ItemstoDownload = new ObservableCollection<ItemtoDownload>();
        }

        public string DisplayValue
        {
            get { return Main.DisplayValue; }
        }

        /// <summary>
        ///     Download all items of this esc
        /// </summary>
        public bool AllChecked
        {
            get { return _allChecked; }
            set
            {
                if (_allChecked == value) return;
                _allChecked = value;
                OnAllItemsChecked();
                CheckAllDownloadItems(value);
                RaisePropertyChanged(() => AllChecked);
            }
        }

        public bool EscDownloadCompleted
        {
            get { return _escDownloadCompleted; }
            set
            {
                _escDownloadCompleted = value;
                RaisePropertyChanged(() => EscDownloadCompleted);
            }
        }

        public ObservableCollection<ItemtoDownload> ItemstoDownload { get; protected set; }

        protected void AttachHandler(ItemtoDownload itemtoDownload)
        {
            itemtoDownload.DownloadClicked += SubItemChecked;
            itemtoDownload.SelectDownload(true);
        }


        private void SubItemChecked(object sender, EventArgs eventArgs)
        {
            _allChecked = ItemstoDownload.All(d => d.DoDownload);
            OnAllItemsChecked();
            RaisePropertyChanged(() => AllChecked);
        }

        /// <summary>
        ///     Occurs whenever state of allchecked changes
        /// </summary>
        public event EventHandler AllItemsChecked;

        protected virtual void OnAllItemsChecked()
        {
            var handler = AllItemsChecked;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void CheckAllDownloadItems(bool value)
        {
            foreach (var itemtoDownload in ItemstoDownload) itemtoDownload.SelectDownload(value);
        }


        private void itemtoDownload_DownloadCompleted(object sender, EventArgs e)
        {
            if (ItemstoDownload.Where(q => q.DoDownload).All(s => s.ReceiveCompleted))
            {
                EscDownloadCompleted = true;
            }
            OnDownloadItemStateChanged();
        }

        /// <summary>
        ///     One of the items finished downloading
        /// </summary>
        public event EventHandler DownloadItemStateChanged;

        protected virtual void OnDownloadItemStateChanged()
        {
            var handler = DownloadItemStateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public async void StartDownload()
        {
            foreach (var itemtoDownload in ItemstoDownload.Where(n => n.DoDownload))
            {
                await itemtoDownload.Function;
            }
        }
    }
}