#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using EscInstaller.ViewModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

#endregion

namespace EscInstaller.EscCommunication
{
    public class Communication : ViewModelBase, IDownloadableItem
    {
        private bool _allFromAllSources = true;
        private bool _downloadFinished;

        private bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                _isDownloading = value;
                StartDownload.RaiseCanExecuteChanged();
            }
        }

        private string _value;
        private bool _direction;
        private Brush _background;
        private bool _isDownloading;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
            }
        }

        public Communication()
        {        
            DataChilds = new ObservableCollection<IDownloadableItem>();
            SetSend();
        }

        
        public bool Checked
        {
            get { return _allFromAllSources; }
            set
            {
                _allFromAllSources = value;
                foreach (var s in DataChilds) s.Checked = value;
                RaisePropertyChanged(() => Checked);
            }
        }

        public bool Completed
        {
            get { return _downloadFinished; }
            set
            {
                _downloadFinished = value;
                if (value)
                    IsDownloading = false;
                RaisePropertyChanged(() => Completed);
            }
        }

        public bool Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                if (_direction) SetReceive(); else SetSend();
                RaisePropertyChanged(() => Direction);
            }
        }

        private void SetReceive()
        {
            Value = "Download";
            Background = Brushes.Green;
            foreach (var downloadableItem in DataChilds)
            {
                RemoveHandlers((Downloader)downloadableItem);
            }
            DataChilds.Clear();
            ViewModelLocator.Main.PrepaireDesign();
            foreach (var q in ViewModelLocator.Main.TabCollection.OfType<MainUnitViewModel>()
                    .Where(d => d.ConnectType != ConnectType.None).Select(esc => new ReceiveData(esc)))
            {
                AttachHandlers(q);
                DataChilds.Add(q);
            }
        }

        private void SetSend()
        {
            Value = "Upload";
            Background = Brushes.LightCoral;
            foreach (var downloadableItem in DataChilds)
            {
                RemoveHandlers((Downloader)downloadableItem);
            }
            DataChilds.Clear();
            foreach (var q in ViewModelLocator.Main.TabCollection.OfType<MainUnitViewModel>().ToList()
                    .Where(d => d.ConnectType != ConnectType.None).Select(esc => new SendData(esc)))
            {
                AttachHandlers(q);
                DataChilds.Add(q);
            }
        }

        public Brush Background
        {
            get { return _background; }
            private set
            {
                _background = value;
                RaisePropertyChanged(() => Background);
            }
        }

        public ObservableCollection<IDownloadableItem> DataChilds { get; }

        public RelayCommand StartDownload
        {
            get
            {
                return new RelayCommand(() =>
                {
                    foreach (var downloadViewModel in DataChilds.Where(s => s.Checked))
                    {
                        ((Downloader)downloadViewModel).StartDownload();
                    }
                    IsDownloading = true;
                    
                }, () => !IsDownloading);
            }
        }

        protected void AttachHandlers(Downloader esc)
        {
            esc.DownloadItemStateChanged += n_SelectedItemsFinished;
            esc.AllItemsChecked += NOnDownloadItemStateChanged;
        }

        protected void RemoveHandlers(Downloader esc)
        {
            esc.DownloadItemStateChanged -= n_SelectedItemsFinished;
            esc.AllItemsChecked -= NOnDownloadItemStateChanged;

        }

        private void NOnDownloadItemStateChanged(object sender, EventArgs eventArgs)
        {
            _allFromAllSources = DataChilds.All(n => n.Checked);
            RaisePropertyChanged(() => Checked);
        }

        private void n_SelectedItemsFinished(object sender, EventArgs e)
        {
            Completed = (DataChilds.All(s => s.Completed));
            IsDownloading = false;
        }
    }
}