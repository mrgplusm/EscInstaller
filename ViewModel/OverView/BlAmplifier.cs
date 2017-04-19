#region

using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlAmplifier : SnapDiagramData
    {
        public const int Width = BlBackupAmp.Width;
        public const int XLocation = BlOutput.Width + Distance + BlOutput.XLocation;
        private readonly FlowModel _flow;
        private List<SnapShot> _snapShots;

        public BlAmplifier(FlowModel flow)
        {
            _flow = flow;
            Location.X = XLocation;
        }

        public override int Id => _flow.Id;

        public override bool IsEnabled => false;

        public AmplifierOperationMode OperationMode => _flow.AmplifierOperationMode;

        public override string SettingName => "Amplifier";

        public string DisplaySetting => "Ampliefier";

        public string DisplayId => (_flow.Id + 1).ToString(CultureInfo.InvariantCulture);

        public override Point Size => new Point(Width, UnitHeight);
       
        public override List<SnapShot> Snapshots => _snapShots ?? (_snapShots = new List<SnapShot>
        {
            //top
            new SnapShot(this) {Offset = {X = SnapshotWidth, Y = 0}},
            //left
            new SnapShot(this) {Offset = {X = 0, Y = SnapshotHeight}},
            //right
            new SnapShot(this) {Offset = {X = Size.X, Y = SnapshotHeight}},
            //add bottom point
            new SnapShot(this) {Offset = {X = SnapshotWidth, Y = Size.Y}}
        });

        public override void SetYLocation()
        {
            var row = Id % 12;
            var yspace = row > 3 ? (InnerSpace + RowHeight) * (row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight * row + yspace;
        }
    }
}