using System.Collections.Generic;
using System.Windows;

namespace EscInstaller.ViewModel.OverView
{
    public abstract class PanelBase : SnapDiagramData
    {
        public int PanelCount { get; set; }
    }

    public sealed class FdsViewModel : PanelBase
    {
        private readonly int _bus;

        public FdsViewModel(int bus, int xLocation)
        {
            _bus = bus;
            Location.Y = _bus * 100;
            Location.X = xLocation;
        }

        public override Point Size
        {
            get
            {
                return new Point(70, 70);
            }
        }
        private List<SnapShot> _snapShots;
        public override List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>
                {
                    new SnapShot(this) {Offset = {X = 0, Y =35}},
                    new SnapShot(this) {Offset = {X = Size.X, Y =35}},
                });
            }
        }



        public override string SettingName
        {
            get { return "Fire detection"; }
        }

        public override void SetYLocation()
        {

        }

        public override int Id
        {
            get { return _bus; }
        }
    }
}