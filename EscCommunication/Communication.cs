#region

using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using bbv.Common.StateMachine;
using bbv.Common.StateMachine.Internals;
using Common;
using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.EscCommunication.UploadItem;
using EscInstaller.ViewModel;
using GalaSoft.MvvmLight.Command;

#endregion

namespace EscInstaller.EscCommunication
{
    enum BtSt
    {
        Ready,
        Working,
        Finished,
    }

    enum BtActions
    {
        Press,
        Finished,
    }

    public class Communication : DownloadNode
    {


        private readonly PassiveStateMachine<BtSt, BtActions> _stButton = new PassiveStateMachine<BtSt, BtActions>();


        public Communication()
        {
            SetSend();
            _buttonName = "Start";
            DownloadCommand = new RelayCommand(() =>
            {
                _stButton.Fire(BtActions.Press);
            });

            Completed += CompletedEvent;

            _stButton.Initialize(BtSt.Ready);
            _stButton.In(BtSt.Ready).On(BtActions.Press).Goto(BtSt.Working).Execute(Start);
            _stButton.In(BtSt.Working).On(BtActions.Press).Goto(BtSt.Finished).Execute(StopDownload);
            _stButton.In(BtSt.Working).On(BtActions.Finished).Goto(BtSt.Finished).Execute(Finished);
            _stButton.In(BtSt.Finished).On(BtActions.Press).Goto(BtSt.Ready).Execute(ResetStatus);
            _stButton.Start();
        }

        public string ButtonName
        {
            get { return _buttonName; }
            set
            {
                _buttonName = value;
                RaisePropertyChanged(() => ButtonName);
            }
        }

        private void ResetStatus()
        {
            Reset();
            ButtonName = "Start";
        }

        private void Start()
        {
            StartDownload(DataChilds);
            ButtonName = "Cancel";
        }

        private void StopDownload()
        {
            StopDownload(DataChilds);
            Finished();
        }

        private void Finished()
        {
            ButtonName = "Reset";
        }


        private void CompletedEvent(object sender, NodeUpdatedEventArgs e)
        {
            if (DataChilds.All(s => s.IsCompleted))
                _stButton.Fire(BtActions.Finished);
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
        private string _buttonName;

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
            Background = Brushes.LightCyan;

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
            Background = Brushes.LightPink;

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