#region

using System.Collections.Generic;
using System.Windows;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlBackupAmp : SnapDiagramData
    {
        public const int Width = 85;
        public const int XLocation = BlOutput.Width + Distance + BlOutput.XLocation;
        private readonly CardModel _card;
        private readonly MainUnitViewModel _main;
        private List<SnapShot> _snapShots;

        public BlBackupAmp(MainUnitViewModel main, CardModel card)
        {
            Location.X = XLocation;
            _main = main;
            _card = card;
        }

        public override bool IsEnabled
        {
            get { return false; }
        }

        public override int Id
        {
            get { return _card.Id; }
        }

        public override string SettingName
        {
            get { return "backupAmp"; }
        }

        public string DisplaySetting
        {
            get { return "backup"; }
        }

        public string DisplayId
        {
            get { return (_card.Id + 1).ToString("N0"); }
        }

        public override Point Size
        {
            get { return new Point(Width, UnitHeight); }
        }

        public override List<SnapShot> Snapshots
        {
            get
            {
                if (_snapShots != null) return _snapShots;
                _snapShots = (_snapShots = new List<SnapShot>
                {
                    new SnapShot(this) {Offset = {X = 0, Y = SnapshotHeight}},
                    new SnapShot(this) {Offset = {X = Size.X, Y = SnapshotHeight}},
                    //add bottom point
                    new SnapShot(this) {Offset = {X = SnapshotWidth, Y = 0}}
                });


                return _snapShots;
            }
        }

        public override void SetYLocation()
        {
            Location.Y = (InnerSpace + RowHeight*5)*_card.Id + RowHeight*4;
        }
    }
}