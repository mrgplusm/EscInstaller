﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Common.Model;
using EscInstaller.View;

namespace EscInstaller.ViewModel.OverView
{
    public class BlSpeaker : SnapDiagramData
    {
        readonly FlowModel _flow;
        private readonly MainUnitViewModel _main;
        private readonly int _line;
        public const int Width = 30;
        public const int XLocation = BlSpMatrix.Width + Distance + BlSpMatrix.XLocation;

        public BlSpeaker(FlowModel flow, MainUnitViewModel main, int line)
        {
            _flow = flow;
            _main = main;
            _line = line;

            Location.X = XLocation + (line > 0 ? 15 : 5);
            UpdateLoads();
        
        }

        


        private BindablePoint _location;
        public sealed override BindablePoint Location
        {
            get
            {
                return _location ?? (_location = new BindablePoint());
            }
        }

        private List<SnapShot> _snapShots;

        public override string SettingName
        {
            get { return string.Format("Speaker Line " + (Line > 0 ? "B" : "A"), _flow.Id + 1); }
        }

        public override void SetYLocation()
        {
            var row = Id % 12;
            var yspace = row > 3 ? (InnerSpace + RowHeight) * (row > 7 ? 2 : 1) : 0;
            Location.Y = RowHeight * row + yspace + (Line > 0 ? 5 : 15);            
        }

        public sealed override List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>()
                                                       {                                                           
                                                           new SnapShot(this) {Offset = {X = 0, Y = 8}},
                                                       });
            }
        }

        public override  int Id
        {
            get { return _flow.Id; }
        }

        public override Point Size
        {
            get { return new Point(Width, 8); }
        }

        public override bool IsEnabled
        {
            get { return false; }
        }

        private double[] _l;

        public void UpdateLoads()
        {
            _l = BlMonitor.GetLoads(_flow, _main.DataModel);
        }

        public int Line
        {
            get { return _line; }
        }

        public string LoadDisplay
        {
            get
            {
                var s = new StringBuilder();
                s.Append((_flow.Id +1).ToString("N0"));
                s.Append(_line < 1 ? "B " : "A "); //incorrect but anyway
                s.Append(Load > 10 ? Load.ToString("N0") + "W" : "Not installed");

                return s.ToString();
            }
        }

        public double Load
        {
            get
            {
                return _l[7 - _line];
            }
        }

    }
}