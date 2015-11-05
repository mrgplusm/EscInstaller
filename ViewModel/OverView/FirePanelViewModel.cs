#region

using System.Collections.Generic;
using System.Windows;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class FirePanelViewModel : PanelBase
    {
        private readonly int _bus;
        private readonly int _xLocation;
        private List<SnapShot> _snapShots;

        public FirePanelViewModel(int bus, int xLocation)
        {
            _bus = bus;
            _xLocation = xLocation;
            Location.Y = _bus*100;
            Location.X = xLocation;
        }

        public override Point Size
        {
            get { return new Point(70, 70); }
        }

        public override List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>
                {
                    new SnapShot(this) {Offset = {X = 0, Y = 35}},
                    new SnapShot(this) {Offset = {X = Size.X, Y = 35}}
                });
            }
        }

        public override string SettingName
        {
            get { return "Fire "; }
        }

        public override int Id
        {
            get { return _bus; }
        }

        public override void SetYLocation()
        {
        }
    }
}