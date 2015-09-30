using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Common.Model;
using Common.Commodules;

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlAmplifier : SnapDiagramData
    {
        private readonly FlowModel _flow;

        public const int Width = BlBackupAmp.Width;
        public const int XLocation = BlOutput.Width + Distance + BlOutput.XLocation;

        public BlAmplifier(FlowModel flow)
        {
            _flow = flow;
            Location.X = XLocation;
        }

        public override int Id
        {
            get
            {
                return _flow.Id;
            }
        }

        public override bool IsEnabled
        {
            get { return false; }
        }

        public AmplifierOperationMode OperationMode
        {
            get { return _flow.AmplifierOperationMode; }
        }

        public override void SetYLocation()
        {
            var row = Id % 12;
            var yspace = row > 3 ? (InnerSpace + RowHeight) * (row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight * row + yspace;
        }

        public override string SettingName
        {
            get
            {
                return "Amplifier";
            }
        }

        public string DisplaySetting
        {
            get { return "Ampliefier"; }
        }

        public string DisplayId
        {
            get { return (_flow.Id + 1).ToString(CultureInfo.InvariantCulture); }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        private List<SnapShot> _snapShots;

        public override List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>
                    {
                        //top
                        new SnapShot(this) {Offset = {X = SnapshotWidth , Y = 0}},
                        //left
                    new SnapShot(this) {Offset = {X = 0, Y = SnapshotHeight}},
                    //right
                    new SnapShot(this) {Offset = {X = Size.X, Y = SnapshotHeight}},
                    //add bottom point
                    new SnapShot(this) {Offset = {X = SnapshotWidth , Y = Size.Y}},
                });
            }
        }
    }
}