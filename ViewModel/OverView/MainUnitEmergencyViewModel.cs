#region

using System.Collections.Generic;
using System.Windows;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class MainUnitEmergencyViewModel : SnapDiagramData
    {
        private readonly MainUnitModel _main;
        private List<SnapShot> _snapShots;

        public MainUnitEmergencyViewModel(MainUnitModel main, int xLocation)
        {
            _main = main;
            Location.Y = 0;
            Location.X = xLocation;
        }

        public override Point Size
        {
            get { return new Point(50, 150); }
        }

        public override string SettingName
        {
            get { return "MainUnit"; }
        }

        public override List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>
                {
                    new SnapShot(this) {Offset = {X = 0, Y = 35}},
                    new SnapShot(this) {Offset = {X = 0, Y = 135}}
                });
            }
        }

        public override int Id
        {
            get { return 0; }
        }

        public override void SetYLocation()
        {
        }
    }
}