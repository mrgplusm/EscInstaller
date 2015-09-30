using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.EscCommunication;

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlEmergency : SnapDiagramData
    {
        private readonly MainUnitViewModel _main;
        public const int Width = BlLink.Width;
        public const int XLocation = BlInputPeq.XLocation + Distance + BlInputPeq.Width;

#if DEBUG
        public BlEmergency()
        {
            _main = new MainUnitViewModel();
        }
#endif

        public BlEmergency(MainUnitViewModel main)
        {
            _main = main;

            for (var x = 0; x < 5; x++)
            {
                Snapshots.Add(new SnapShot(this) { Offset = { X = 0, Y = SnapshotHeight + RowHeight * x }, SnapType = SnapType.Gray, RowId = x });
            }
            Snapshots.Add(new SnapShot(this) { Offset = { X = SnapshotWidth, Y = 0 }, RowId = 30 });

            Location.X = XLocation;
            SetYLocation();

            UpdatePanelCount();
            _main.Receiver.EepromReceived += Receiver_EepromReceived;

        }

        void UpdatePanelCount()
        {
            var d = new Dictionary<PanelType, PanelBase>()
            {
             {PanelType.Fire, _snapShots.OfType<FirePanelViewModel>().FirstOrDefault()}   ,
             {PanelType.Evacuation , _snapShots.OfType<EvacuationPanelViewModel>().FirstOrDefault()}   ,
             {PanelType.Fds, _snapShots.OfType<FdsViewModel>().FirstOrDefault()}   
            };

            foreach (var @base in d.Where(@base => @base.Value != null))
            {
                @base.Value.PanelCount = _main.DataModel.AttachedPanelsBus2.Count(z => z.IsInstalled && z.PanelType == @base.Key);
            }
        }

        void Receiver_EepromReceived(object sender, DownloadEepromEventArgs e)
        {
            UpdatePanelCount();
        }

        private readonly Point _size = new Point(Width, RowHeight * 4 + UnitHeight);
        public override Point Size
        {
            get
            {
                return _size;
            }
        }

        public override string SettingName
        {
            get { return EmergencyPanel.DisplayTitle; }
        }

        public override void SetYLocation()
        {
            Location.Y = _main.DataModel.ExpansionCards * 5 * RowHeight + 5 * RowHeight
                + (InnerSpace) * (1 + _main.DataModel.ExpansionCards);


            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }

            Location.ValueChanged();
        }


        private List<SnapShot> _snapShots;

        public override List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>());

            }
        }

        public override int Id
        {
            get { return _main.Id; }
        }

        private void SetLines()
        {
            var list = new List<DiagramData>();

            for (var q = 0; q < 1; q++)
            {
                list.Add(new LineViewModel(_snap.OfType<FdsViewModel>().First(i => i.Id == q), _snap.OfType<EvacuationPanelViewModel>().First(i => i.Id == q)));
                list.Add(new LineViewModel(_snap.OfType<EvacuationPanelViewModel>().First(i => i.Id == q), _snap.OfType<FirePanelViewModel>().First(i => i.Id == q)));
                list.Add(new LineViewModel(_snap.OfType<FirePanelViewModel>().First(i => i.Id == q), _snap.OfType<MainUnitEmergencyViewModel>().First()));
            }

            foreach (var diagramData in list)
            {
                _snap.Add(diagramData);
            }

        }

        private ObservableCollection<DiagramData> _snap;
        public ObservableCollection<DiagramData> SnapDiagram
        {
            get
            {
                if (_snap == null)
                {
                    _snap = new ObservableCollection<DiagramData>();
                    for (int i = 0; i < 1; i++)
                    {
                        _snap.Add(new FdsViewModel(i, 0));
                        _snap.Add(new EvacuationPanelViewModel(i, 100));
                        _snap.Add(new FirePanelViewModel(i, 200));
                        _snap.Add(new MainUnitEmergencyViewModel(_main.DataModel, 300));
                    }
                    SetLines();
                }

                return _snap;
            }
        }
    }
}