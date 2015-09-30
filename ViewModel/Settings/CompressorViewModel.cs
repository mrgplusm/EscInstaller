using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EscFileHandler.Model;
using GalaSoft.MvvmLight.Command;
using McuCommunication;
using McuCommunication.Commodules;
using Microsoft.Practices.Unity;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;

namespace EscInstaller.ViewModel.Settings
{
    public class CompressorViewModel : SettingsBaseViewModel
    {

        public static int CompXdBStart = int.Parse(LibraryData.Settings["CompXdBStart"]);

        private ObservableCollection<DraggablePoint> _points;

        [InjectionConstructor]
        public CompressorViewModel()
        {
        }


        public bool IsEnabled
        {
            get { return bool.Parse(LibraryData.Settings["CompressorEnabled"]); }
        }

        public ICommand GetValues
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MainUnitModel m;
                    
                    if (!TryGetMainUnitFromId(Id, LibraryData.FuturamaSys, out m)) return;
                    Decay = GetCompressor.Decay(m.DspCopy, Id);
                    Hold = GetCompressor.Hold(m.DspCopy, Id);
                });                
            }
        }

        public CompressorModel Compressor
        {
            get
            {
                var card = ((ExtensionCardModel)CurrentCard);
                if ((CurrentFlow.Id - ConnStatMethods.StartCountFrom) % 5 == 2)
                {
                    return CurrenttMainUnit.Compressor1 ?? (CurrenttMainUnit.Compressor1 = EmptyCompressor);
                }
                return CurrenttMainUnit.Compressor2 ?? (CurrenttMainUnit.Compressor2 = EmptyCompressor);
            }
        }



        public ObservableCollection<DraggablePoint> LineData
        {
            get
            {
                if (_points == null)
                {
                    _points = new ObservableCollection<DraggablePoint>();
                    GeneratePointsFromLine();
                }

                return _points;
            }
        }

        public double Decay
        {
            get { return Compressor.Decay; }
            set
            {
                Compressor.Decay = (int)value;
                RaisePropertyChanged(() => Decay);
                AddData(new SetCompressorProps(Id, Hold, value));
            }
        }

        public double Hold
        {
            get { return Compressor.Hold; }
            set
            {
                
                Compressor.Hold = (int)value;
                RaisePropertyChanged(() => Hold);
                AddData(new SetCompressorProps(Id, value, Decay));
            }
        }

        public ICommand ResetButton
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        Compressor.CompressorValues = new List<double>();
                        LineData.Clear();
                        GeneratePointsFromLine();
                    });
            }
        }

        private void GeneratePointsFromLine()
        {
            for (var i = 0; i < Compressor.CompressorValues.Count; i++)
            {
                var xDb = XValue(i);
                if ((Math.Abs(xDb - CompXdBStart) > .5) && (Math.Abs(xDb - (CompXdBStart + 96)) > .5)
                    && (Math.Abs(((Compressor.CompressorValues[i - 1] + Compressor.CompressorValues[i + 1]) / 2) - (Compressor.CompressorValues[i])) < .5))
                    continue;

                AddPointToList(xDb, Compressor.CompressorValues[i], true);
            }
        }

        public static CompressorModel EmptyCompressor
        {
            get
            {
                return new CompressorModel()
                {
                    CompressorValues = new List<double>(Enumerable.Range(0, 33).Select(DbForX))
                };
            }
        }

        public double XValue(int pointNum)
        {
            return pointNum * 3 + CompXdBStart;
        }

        public static double DbForX(int x)
        {
            return x * 3 - 90;
        }

        private static int XforDb(double db)
        {
            return (int)((db - CompXdBStart) / 3);
        }

        private void AddPointToList(double x, double y, bool endOfList)
        {
            //check if point is not allready in the chart
            if (_points.Any(q => Math.Abs(q.Position.X - x) < .2)) return;

            var p = new DraggablePoint(new Point(x, y));

            p.PositionChanged += PPositionChanged;
            if (endOfList) _points.Add(p);
            else _points.Insert(0, p);
        }

        private void PPositionChanged(object sender, PositionChangedEventArgs args)
        {
            var xDatamodelCurrent = XforDb(args.Position.X);
            var draggablePoint = (DraggablePoint)sender;

            //add node to replace moving sidenode (left).
            if (Math.Abs(args.PreviousPosition.X - CompXdBStart) < 3 && _points.First().Equals(draggablePoint))
            {
                AddPointToList(CompXdBStart, args.PreviousPosition.Y, false);
            }
            //add node to replace moving sidenode (right).
            if (Math.Abs(args.PreviousPosition.X - (CompXdBStart + 96)) < 3 && _points.Last().Equals(draggablePoint))
            {
                AddPointToList(CompXdBStart + 96, args.PreviousPosition.Y, true);
            }

            //from previous to current
            if (LineData.Any(q => q.Position.X < args.Position.X))
            {
                var xDatamodelPrevious =
                    XforDb(LineData.Where(q => q.Position.X < args.Position.X).Max(q => q.Position.X));
                double positions = Math.Abs(xDatamodelPrevious - xDatamodelCurrent);
                var previousdB = Compressor.CompressorValues[xDatamodelPrevious];

                for (var position = 0; position <= positions; position++)
                {
                    Compressor.CompressorValues[position + xDatamodelPrevious] = previousdB +
                                                           (args.Position.Y - previousdB) * (position / positions);
                }
            }
            //from current to next
            if (LineData.Any(q => q.Position.X > args.Position.X))
            {
                var xDatamodelNext = XforDb(LineData.Where(q => q.Position.X > args.Position.X).Min(q => q.Position.X));
                double positions = Math.Abs(xDatamodelCurrent - xDatamodelNext);
                var addition = 1 / positions;
                var nextDb = Compressor.CompressorValues[xDatamodelNext];

                for (var position = 0; position < positions; position++)
                {
                    Compressor.CompressorValues[position + xDatamodelCurrent] = args.Position.Y + addition * (nextDb - args.Position.Y) * position;
                }
            }
            SendCompressor(Id, Compressor);

        }

        public static void SendCompressor(int id, CompressorModel compressor)
        {
            if (!bool.Parse(LibraryData.Settings["CompressorEnabled"])) return;
            if (compressor == null) return;
            if (compressor.CompressorValues == null) return;

            try
            {
                for (var i = 0; i < 7; i++)
                {
                    LibraryData.AddData(new SetCompressor(id, i, compressor.CompressorValues));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to upload compressor: " + e);
            }
        }
    }
}