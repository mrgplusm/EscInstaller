using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace EscInstaller.ViewModel.EscCommunication
{
    public abstract class CommunicationBase : ViewModelBase
    {
        protected MainViewModel Main { get; private set; }
        private bool _downloadFinished;
        private bool _allFromAllSources = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="main"></param>        
        protected CommunicationBase(MainViewModel main)
        {
            Main = main;
        }


        protected void AttachHandlers(Downloader esc)
        {
            esc.DownloadItemStateChanged += n_SelectedItemsFinished;
            esc.AllItemsChecked += NOnDownloadItemStateChanged;
        }

        public bool AllFromAllSources
        {
            get { return _allFromAllSources; }
            set
            {
                _allFromAllSources = value;
                foreach (var s in Escs) s.AllChecked = value;
                RaisePropertyChanged(() => AllFromAllSources);
            }
        }



        private void NOnDownloadItemStateChanged(object sender, EventArgs eventArgs)
        {
            _allFromAllSources = Escs.All(n => n.AllChecked);
            RaisePropertyChanged(() => AllFromAllSources);
        }

        void n_SelectedItemsFinished(object sender, EventArgs e)
        {
            DownloadFinished = (Escs.All(s => s.EscDownloadCompleted));
        }

        public bool DownloadFinished
        {
            get { return _downloadFinished; }
            set
            {
                _downloadFinished = value;
                if (value)
                    _isDownloading = false;
                RaisePropertyChanged(() => DownloadFinished);
            }
        }

        public abstract ObservableCollection<Downloader> Escs { get; }


        private bool _isDownloading;

        public ICommand StartDownload
        {
            get
            {
                return new RelayCommand(() =>
                {
                    foreach (var downloadViewModel in Escs)
                    {
                        downloadViewModel.StartDownload();
                    }
                    _isDownloading = true;
                }, () => !_isDownloading);
            }
        }


    }
}