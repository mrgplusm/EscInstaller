#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlSpMatrix : SnapDiagramData
    {
        /// <summary>
        ///     Installed speaker in matrix
        /// </summary>
        [Flags]
        public enum InstSpeaker
        {
            First = 1,
            Second = 2,
            Third = 4,
            Fourth = 8,
            None = 0
        }

        public const int Width = 20;
        public const int XLocation = BlAmplifier.Width + Distance + BlAmplifier.XLocation;
        public const double NodeSize = 50;
        private readonly CardModel _card;
        private readonly MainUnitViewModel _main;
        private List<SnapShot> _snapShots;
#if DEBUG
        public BlSpMatrix()
        {
            _main = new MainUnitViewModel();
            _card = new CardModel();

            ChkVms = new ObservableCollection<ChkVm>(GenMatrix());
        }
#endif

        public BlSpMatrix(MainUnitViewModel main, CardModel card)
        {
            _main = main;
            _card = card;

            Location.X = XLocation;

            if (main.DataModel.LoudSpeakerMatrix == null)
                main.DataModel.LoudSpeakerMatrix = new Dictionary<int, int>();

            ChkVms = new ObservableCollection<ChkVm>(GenMatrix());
        }

        public override bool IsEnabled
        {
            get { return true; }
        }

        public override List<SnapShot> Snapshots
        {
            get
            {
                if (_snapShots == null)
                {
                    _snapShots = new List<SnapShot>();
                    for (var i = 0; i < 4; i++)
                    {
                        _snapShots.Add(new SnapShot(this)
                        {
                            Offset = {X = 0, Y = SnapshotHeight + RowHeight*i},
                            RowId = i
                        });

                        _snapShots.Add(new SnapShot(this)
                        {
                            Offset = {X = Size.X, Y = SnapshotHeight + RowHeight*i + 6},
                            RowId = i
                        });
                        _snapShots.Add(new SnapShot(this)
                        {
                            Offset = {X = Size.X, Y = SnapshotHeight + RowHeight*i - 4},
                            RowId = i
                        });
                    }
                }
                return _snapShots;
            }
        }

        public override int Id
        {
            get { return _card.Id; }
        }

        public string DisplayId
        {
            get
            {
                var baseId = _card.Id*4 + _main.Id*12;

                return string.Format("{0}-{1}", baseId + 1, baseId + 4);
            }
        }

        public override string SettingName
        {
            get { return "loudspeakermatrix"; }
        }

        public ObservableCollection<ChkVm> ChkVms { get; private set; }

        public override Point Size
        {
            get { return new Point(Width, 3*RowHeight + UnitHeight); } //to be determined by system overview
        }

        public override void SetYLocation()
        {
            Location.Y = RowHeight*5*_card.Id + InnerSpace*_card.Id;
        }

        public IEnumerable<ChkVm> GenMatrix()
        {
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    yield return new LspNode(i, j, _card, _main.DataModel);
                }
                yield return new LspLeftHeader(i, _card, _main.DataModel);
            }
        }
    }
}