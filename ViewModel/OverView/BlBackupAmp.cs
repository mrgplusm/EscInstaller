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
        private List<SnapShot> _snapShots;

        public BlBackupAmp(MainUnitViewModel main, CardModel card)
        {
            Location.X = XLocation;
            _card = card;
            main.BackupConfigChanged += (sender, args) =>
            {
                Visibility = main.BackupAmp[card.Id] ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        public override bool IsEnabled => false;

        public override int Id => _card.Id;

        public override string SettingName => "backupAmp";

        public string DisplaySetting => "backup";

        private Visibility _visibility = Visibility.Collapsed;

        public void ChangeVisibility(Visibility visibility)
        {
            _visibility = visibility;
            RaisePropertyChanged(() => Visibility);
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                RaisePropertyChanged(() => Visibility);
            }
        }


        public string DisplayId => (_card.Id + 1).ToString("N0");

        public override Point Size => new Point(Width, UnitHeight);

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
            Location.Y = (InnerSpace + RowHeight * 5) * _card.Id + RowHeight * 4;
        }
    }
}