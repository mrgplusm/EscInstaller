#region

using System;
using System.Linq;
using System.Windows;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public enum LineType
    {
        Emergency, //red
        PublicAddress, //gray
        LinkLine, //none
        Special //none
    }

    public class LineViewModel : DiagramData
    {
        private SnapDiagramData _first;
        private LineType _lineType;
        private int? _rowFirst;
        private int? _rowIdSecond;
        private SnapDiagramData _second;
        private int _snapFirst;
        private int _snapSecond;
        public Action OnEndPointRemoved;

        public LineViewModel(SnapDiagramData first, SnapDiagramData second, int? rowFirst = null, int? rowSecond = null)
        {
            _first = first;
            _second = second;
            _rowFirst = rowFirst;
            _rowIdSecond = rowSecond;
            Initiate();
        }

        public int? RowIdFirst
        {
            get { return _rowFirst; }
            set
            {
                _rowFirst = value;
                DetermineAttachedSnap();
                RaisePropertyChanged(() => Start);
            }
        }

        public int? RowIdSecond
        {
            get { return _rowIdSecond; }
            set
            {
                _rowIdSecond = value;
                DetermineAttachedSnap();
                RaisePropertyChanged(() => End);
            }
        }

        public SnapDiagramData First
        {
            get { return _first; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Points cannot be null");

                RemovePositionChangeHandlers(_first);
                _first = value;

                AddPositionChangeHandlers(_first);
                DetermineAttachedSnap();
                RaisePropertyChanged(() => Start);
                // _figure.StartPoint = Start.Value;
            }
        }

        public SnapDiagramData Second
        {
            get { return _second; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Points cannot be null");

                RemovePositionChangeHandlers(_second);
                _second = value;


                AddPositionChangeHandlers(_second);
                DetermineAttachedSnap();
                RaisePropertyChanged(() => End);
            }
        }

        public override bool IsEnabled
        {
            get { return false; }
        }

        public LineType LineType
        {
            get { return _lineType; }
            set
            {
                _lineType = value;
                RaisePropertyChanged(() => LineType);
            }
        }

        public SnapShot SnapStart
        {
            get { return _first.Snapshots[_snapFirst]; }
        }

        public SnapShot SnapEnd
        {
            get { return _second.Snapshots[_snapSecond]; }
        }

        public BindablePoint Start
        {
            get
            {
                if (_first.Snapshots.Count <= _snapFirst) return null;
                return _first.Snapshots[_snapFirst].Location;
            }
        }

        public BindablePoint End
        {
            get
            {
                if (_second.Snapshots.Count <= _snapSecond) return null;
                return _second.Snapshots[_snapSecond].Location;
            }
        }

        public int Id { get; set; }
        public string ShortName { get; set; }

        public override Point Size
        {
            get { return new Point(2, 2); }
        }

        private void Initiate()
        {
            AddPositionChangeHandlers(_first);
            AddPositionChangeHandlers(_second);
            DetermineAttachedSnap();
        }

        public void EndPointRemoved()
        {
            if (OnEndPointRemoved != null)
            {
                OnEndPointRemoved();
            }
        }

        private void RemovePositionChangeHandlers(SnapDiagramData data)
        {
            data.Location.OnValueChanged -= DetermineAttachedSnap;
            data.OnRemoveChildren -= EndPointRemoved;
        }

        private void AddPositionChangeHandlers(SnapDiagramData data)
        {
            data.Location.OnValueChanged += DetermineAttachedSnap;
            data.OnRemoveChildren += EndPointRemoved;
        }

        private void DetermineAttachedSnap()
        {
            double smallestDist = 9000000;
            var fs = 0; //first snapshot
            var ss = 0; //second snapshot


            var t = (_rowFirst == null ? _first.Snapshots : _first.Snapshots.Where(s => s.RowId == _rowFirst)).ToArray();
            var y =
                (_rowIdSecond == null ? _second.Snapshots : _second.Snapshots.Where(s => s.RowId == _rowIdSecond))
                    .ToArray();

            foreach (var fP in t)
            {
                foreach (var sP in y)
                {
                    var x1 = fP.Location.X;
                    var y1 = fP.Location.Y;

                    var x2 = sP.Location.X;
                    var y2 = sP.Location.Y;
                    //determine distance.
                    var dist = ((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

                    //check if smallest                    
                    if (dist >= smallestDist) continue;

                    smallestDist = dist;
                    fs = _first.Snapshots.IndexOf(fP);
                    ss = _second.Snapshots.IndexOf(sP);
                }
            }

            SetSnapFirst(fs);
            SetSnapSecond(ss);
        }

        private void SetSnapFirst(int x)
        {
            // if (_snapFirst == x) return;
            _snapFirst = x;
            //_first.AttachedLines.Add(this);

            RaisePropertyChanged(() => Start);
        }

        private void SetSnapSecond(int x)
        {
            // if (_snapSecond == x) return;
            _snapSecond = x;
            //_second.AttachedLines.Add(this);

            RaisePropertyChanged(() => End);
        }
    }
}