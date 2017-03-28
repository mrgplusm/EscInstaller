#region

using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Common;
using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.EscCommunication.UploadItem;
using EscInstaller.ViewModel;
using GalaSoft.MvvmLight.Command;

#endregion

namespace EscInstaller.EscCommunication
{
    public class Communication : DownloadNode
    {
        private bool _downloading;

        public Communication()
        {
            SetSend();

            DownloadCommand = new RelayCommand(() =>
            {
                StartDownload(DataChilds);
                _downloading = true;
                DownloadCommand.RaiseCanExecuteChanged();
            }, () => !_downloading);

            Completed += CompletedEvent;
        }

        private void CompletedEvent(object sender, NodeUpdatedEventArgs e)
        {
            _downloading = false;
            DownloadCommand.RaiseCanExecuteChanged();
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
        private bool _direction;
        private Brush _background;

        public Brush Background
        {
            get { return _background; }
            private set
            {
                _background = value;
                RaisePropertyChanged(() => Background);
            }
        }

        private void SetReceive()
        {
            Value = "Download";
            Background = Brushes.Green;

            ClearChilds();
            ViewModelLocator.Main.PrepaireDesign();

            foreach (var mu in ConnectedUnits)
            {
                var node = new DownloadData(mu);
                foreach (var downloadNode in DownloadNodes(mu))
                {
                    node.AddChild(downloadNode);
                }
                AddChild(node);
            }
        }

        private void SetSend()
        {
            Value = "Upload";
            Background = Brushes.LightCoral;

            ClearChilds();

            foreach (var mu in ConnectedUnits)
            {
                var node = new Upload(mu);
                foreach (var uploadNode in UploadNodes(mu))
                {
                    node.AddChild(uploadNode);
                }
                AddChild(node);
            }
        }

        private static IEnumerable<MainUnitViewModel> ConnectedUnits
        {
            get
            {
                return ViewModelLocator.Main.TabCollection.OfType<MainUnitViewModel>().ToList()
                    .Where(d => d.ConnectType != ConnectType.None);
            }
        }

        public RelayCommand DownloadCommand { get; }


        private static IEnumerable<DownloadNode> DownloadNodes(MainUnitViewModel main)
        {
            yield return new Dsp(main);
            yield return new SpeakerRedundancy(main);

            yield return new InstalledPanels(main);
            yield return new CalibrationData(main);
            yield return new PresetNames(main);
            yield return new MatrixSelection(main);
            yield return new Sensitivity(main);
            yield return new Hardware(main);

            if (main.Id != 0) yield break;
            yield return new SdMessages(main);
            yield return new SdSelection(main);
        }

        private static IEnumerable<DownloadNode> UploadNodes(MainUnitViewModel main)
        {
            yield return new Auxlinks(main);
            yield return new Delaysettings(main);
            yield return new InputSensitivity(main);
            yield return new Linelinks(main);

            yield return new MatrixSelection(main);
            yield return new Messageselection(main);
            yield return new Peqpresetnames(main);
            yield return new InOutNames(main);

            yield return new PeqData(main);
            yield return new GainSliders(main);
            yield return new ToneControl(main);
        }
    };


}