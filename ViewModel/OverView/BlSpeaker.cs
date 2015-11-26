#region

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class BlSpeaker : SnapDiagramData
    {
        public const int Width = 30;
        public const int XLocation = BlSpMatrix.Width + Distance + BlSpMatrix.XLocation;
        private readonly FlowModel _flow;
        private readonly MainUnitViewModel _main;
        private double[] _l;
        private BindablePoint _location;
        private List<SnapShot> _snapShots;

        public BlSpeaker(FlowModel flow, MainUnitViewModel main, int line)
        {
            _flow = flow;
            _main = main;
            Line = line;

            Location.X = XLocation + (line > 0 ? 15 : 5);
            UpdateLoads();
            main.KreisUpdated += MainOnKreisUpdated;
        }

        private void MainOnKreisUpdated(object sender, EventArgs eventArgs)
        {
            UpdateLoads();
        }

        public override sealed BindablePoint Location => _location ?? (_location = new BindablePoint());

        public override string SettingName => string.Format("Speaker Line " + (Line > 0 ? "B" : "A"), _flow.Id + 1);

        public override sealed List<SnapShot> Snapshots => _snapShots ?? (_snapShots = new List<SnapShot>()
        {
            new SnapShot(this) {Offset = {X = 0, Y = 8}}
        });

        public override int Id => _flow.Id;

        public override Point Size => new Point(Width, 8);

        public override bool IsEnabled => false;

        public int Line { get; }

        public string LoadDisplay
        {
            get
            {
                var s = new StringBuilder();
                s.Append((_flow.Id + 1).ToString("N0"));
                s.Append(Line < 1 ? "B " : "A "); //incorrect but anyway
                s.Append(Load > 10 ? Load.ToString("N0") + "W" : "Not installed");

                return s.ToString();
            }
        }

        public double Load => _l[1 - Line];

        public override void SetYLocation()
        {
            var row = Id%12;
            var yspace = row > 3 ? (InnerSpace + RowHeight)*(row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight*row + yspace + (Line > 0 ? 5 : 15);
        }

        public void UpdateLoads()
        {
            _l = BlMonitor.GetLoads(_flow, _main.DataModel);
        }
    }
}