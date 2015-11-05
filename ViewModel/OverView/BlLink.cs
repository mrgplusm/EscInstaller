#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Common;
using Common.Model;
using EscInstaller.View;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class BlLink : SnapDiagramData
    {
        public const int Width = 90;
        public const int XLocation = BlToneControl.Width + Distance + BlToneControl.XLocation;
        private readonly Func<int, double> _innerSpace = x => x > 4 ? (x/5)*InnerSpace : 0;
        private readonly MainUnitViewModel _main;
        private ObservableCollection<LinkCardOption> _linkOptions;
        private Point _size;
        private List<SnapShot> _snapShots;
        private ObservableCollection<VCAController> _vcaControllers;
#if DEBUG
        public BlLink()
        {
            _main = new MainUnitViewModel(LibraryData.EmptyEsc(0), new MainViewModel());
            //buttom snapshot
        }
#endif

        public BlLink(MainUnitViewModel main)
        {
            LinkOptions = new ObservableCollection<LinkOption>();
            _main = main;
            Location.X = XLocation;
            Cards(main.DataModel.ExpansionCards);

            for (var x = 0; x < 15; x++)
            {
                Snapshots.Add(new SnapShot(this)
                {
                    Offset = {X = 0, Y = SnapshotHeight + RowHeight*x + _innerSpace(x)},
                    SnapType = SnapType.Gray,
                    RowId = x
                });

                Snapshots.Add(new SnapShot(this)
                {
                    Offset = {X = Size.X, Y = SnapshotHeight + RowHeight*x + _innerSpace(x)},
                    SnapType = SnapType.Red,
                    RowId = x
                });
            }

            Snapshots.Add(new SnapShot(this) {Offset = {X = SnapshotWidth, Y = Size.Y}, RowId = 30});
        }

        public override int Id
        {
            get { return 0; }
        }

        public override List<SnapShot> Snapshots
        {
            get { return _snapShots ?? (_snapShots = new List<SnapShot>()); }
        }

        public ObservableCollection<VCAController> VcaControllers
        {
            get
            {
                if (_vcaControllers == null && _main != null && _main.DataModel.Cards != null)
                {
                    _vcaControllers = new ObservableCollection<VCAController>();
                    if (_main != null)
                        _vcaControllers =
                            new ObservableCollection<VCAController>(
                                _main.DataModel.Cards.First().Flows.Select(n => new VCAController(n)));
                }
                return _vcaControllers;
            }
        }

        public override string SettingName
        {
            get { return Link._linkBlockTitle; }
        }

        public override Point Size
        {
            get { return _size; }
        }

        public ObservableCollection<LinkOption> LinkOptions { get; set; }

        public ObservableCollection<LinkCardOption> LinkCardOptions
        {
            get
            {
                return _linkOptions ??
                       (_linkOptions =
                           new ObservableCollection<LinkCardOption>(
                               _main.DataModel.Cards.OfType<CardModel>().Select(n => new LinkCardOption(n, _main, this))));
            }
        }

        public override void SetYLocation()
        {
            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }
        }

        /// <summary>
        ///     update snapshorts and size of link block.
        /// </summary>
        /// <param name="count">Expansion cards</param>
        public void Cards(int count)
        {
            if (count > 2) count = 2;
            if (count < 0) count = 0;
            _size = new Point(Width, RowHeight*4 + UnitHeight + (RowHeight*5*count)
                                     + _main.DataModel.ExpansionCards*InnerSpace);

            RaisePropertyChanged(() => Size);


            //set bottom snapshot location
            var t = Snapshots.FirstOrDefault(s => s.RowId == 30);
            if (t != null)
                t.Offset.Y = Size.Y;

            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }
            UpdateLinkOptions(count);
        }

        /// <summary>
        ///     Update link options
        /// </summary>
        /// <param name="count"></param>
        private void UpdateLinkOptions(int count)
        {
            var removeList = LinkOptions.Where(n => n.Flow.Id%12 > 2 + 4*count).ToArray();
            foreach (var linkOption in removeList)
            {
                LinkOptions.Remove(linkOption);
            }

            var lst = _main.DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows).ToArray();

            for (var i = LinkOptions.Count + 1; i < count*4 + 4; i++)
            {
                LinkOptions.Add(new LinkOption(lst.Skip(i).First(), _main, this));
            }
        }

        public event EventHandler<LinkChangedEventArgs> LinkChanged;

        public void OnLinkChanged(LinkChangedEventArgs e)
        {
            var handler = LinkChanged;
            if (handler != null) handler(this, e);
        }
    }
}