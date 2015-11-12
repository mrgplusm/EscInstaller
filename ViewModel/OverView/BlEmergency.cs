#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using EscInstaller.View;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlEmergency : SnapDiagramData
    {
        public const int Width = BlLink.Width;
        public const int XLocation = BlInputPeq.XLocation + Distance + BlInputPeq.Width;
        private readonly MainUnitViewModel _main;
        private ObservableCollection<DiagramData> _snap;
        private List<SnapShot> _snapShots;
#if DEBUG
        public BlEmergency()
        {
            _main = new MainUnitViewModel();
        }
#endif

        public override bool IsEnabled => false;

        public BlEmergency(MainUnitViewModel main)
        {
            _main = main;

            for (var x = 0; x < 5; x++)
            {
                Snapshots.Add(new SnapShot(this)
                {
                    Offset = {X = 0, Y = SnapshotHeight + RowHeight*x},
                    SnapType = SnapType.Gray,
                    RowId = x
                });
            }
            Snapshots.Add(new SnapShot(this) {Offset = {X = SnapshotWidth, Y = 0}, RowId = 30});

            Location.X = XLocation;
            SetYLocation();            
        }

        public override Point Size { get; } = new Point(Width, RowHeight*4 + UnitHeight);

        public override string SettingName => EmergencyPanel.DisplayTitle;

        public override List<SnapShot> Snapshots => _snapShots ?? (_snapShots = new List<SnapShot>());

        public override int Id => _main.Id;

        public ObservableCollection<DiagramData> SnapDiagram
        {
            get
            {
                if (_snap == null)
                {
                    _snap = new ObservableCollection<DiagramData>();
                    for (var i = 0; i < 1; i++)
                    {
                        _snap.Add(new FdsViewModel(i, 0));
                        _snap.Add(new EvacuationPanelViewModel(i, 100));
                        _snap.Add(new FirePanelViewModel(i, 200));
                        _snap.Add(new MainUnitEmergencyViewModel(_main.DataModel, 300));
                    }                    
                }

                return _snap;
            }
        }

        public override void SetYLocation()
        {
            Location.Y = _main.DataModel.ExpansionCards*5*RowHeight + 5*RowHeight
                         + (InnerSpace)*(1 + _main.DataModel.ExpansionCards);


            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }

            Location.ValueChanged();
        }        
    }
}