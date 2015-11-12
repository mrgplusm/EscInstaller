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
        }
#endif

        public BlSpMatrix(MainUnitViewModel main, CardModel card)
        {
            _main = main;
            _card = card;

            Location.X = XLocation;

            if (main.DataModel.LoudSpeakerMatrix == null)
                main.DataModel.LoudSpeakerMatrix = new Dictionary<int, int>();
            
        }

        public override bool IsEnabled => false;

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

        public override int Id => _card.Id;

        public string DisplayId
        {
            get
            {
                var baseId = _card.Id*4 + _main.Id*12;

                return $"{baseId + 1}-{baseId + 4}";
            }
        }

        public override string SettingName => "loudspeakermatrix";

        public override Point Size => new Point(Width, 3*RowHeight + UnitHeight);

        public override void SetYLocation()
        {
            Location.Y = RowHeight*5*_card.Id + InnerSpace*_card.Id;
        }        
    }
}