using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using Common.Commodules;
using Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class PeqDataViewModel : ViewModelBase
    {
        public const double MinBandWidth = 0.1;
        public const double MinQ = 0.1;
        public const double MinFreq = 10;

        private static readonly Brush[] LinebBrushes =
        {
            Brushes.SpringGreen, Brushes.PowderBlue, Brushes.SaddleBrown, Brushes.LightSalmon,
            Brushes.SeaGreen, Brushes.YellowGreen, Brushes.Violet, Brushes.SlateBlue,
            Brushes.NavajoWhite, Brushes.Olive, Brushes.Pink, Brushes.LightGoldenrodYellow,
            Brushes.LightSlateGray, Brushes.LawnGreen, Brushes.LightSkyBlue, Brushes.Magenta, Brushes.MediumPurple
        };


        public event EventHandler<BiquadsChangedEventArgs> BiquadsChanged;

        public void OnBiquadsChanged(BiquadsChangedEventArgs e)
        {
            EventHandler<BiquadsChangedEventArgs> handler = BiquadsChanged;
            if (handler != null) handler(this, e);
        }

        private Arrow _bandwidthArrow;
        private DraggablePoint _bandwidthPoint;
        private DraggablePoint _draggablePoint;
        private bool _isMousOver;
        private LineGraph _lineData;



        public PeqDataViewModel(PeqDataModel peq)
        {
            PeqDataModel = peq;
            if (PeqDataModel.BandWidth < MinBandWidth) PeqDataModel.BandWidth = MinBandWidth;
            if (PeqDataModel.Frequency < MinFreq) PeqDataModel.Frequency = MinFreq;

            DraggablePoint.PositionCoerceCallbacks.Add((container, position) =>
                {
                    var point = position;
                    if (Math.Abs(point.Y - 0) > .1 && FilterType != FilterType.Peaking
                        && FilterType != FilterType.LowShelf && FilterType != FilterType.HighShelf)
                        point = new Point(point.X, 0);

                    if (point.X > 20000) point = new Point(20000, point.Y);
                    if (point.Y > 15) point = new Point(point.X, 15);
                    if (point.X < 10) point = new Point(10, point.Y);
                    if (point.Y < -15) point = new Point(point.X, -15);
                    return point;
                });

            DraggablePoint.PositionChanged += (sender, e) =>
                {
                    if ((Math.Abs(Math.Log10(e.Position.X) - Math.Log10(Frequency)) < .05) &&
                        (Math.Abs(e.Position.Y - Boost) < 0.2)) return;

                    Frequency = e.Position.X;
                    Boost = e.Position.Y;
                };
        }




        public DraggablePoint BandWidthPoint
        {
            get
            {
                if (_bandwidthPoint != null)
                {
                    _bandwidthPoint.Position = Posb();
                    return _bandwidthPoint;
                }

                _bandwidthPoint = new DraggablePoint(Posb()) { Foreground = Brushes.Coral };

                OnBandwithPositionChange(_bandwidthPoint.Position);

                _bandwidthPoint.PositionChanged += (o, eventArgs) => OnBandwithPositionChange(eventArgs.Position);

                _bandwidthPoint.PositionCoerceCallbacks.Add((container, position) =>
                    {
                        var dist = 10 * Math.Abs(Math.Log10(position.X) - Math.Log10(DraggablePoint.Position.X));
                        double x;
                        if (dist > 6.67) x = Math.Pow(10, Math.Log10(DraggablePoint.Position.X) - .667);
                        else if (dist < .1) x = Math.Pow(10, Math.Log10(DraggablePoint.Position.X) - .01);
                        else x = position.X;
                        return new Point(x, Boost);
                    });
                return _bandwidthPoint;
            }
        }




        public Arrow BandwidthArrow
        {
            get { return _bandwidthArrow ?? (_bandwidthArrow = new Arrow(Posb(), PosE())); }
        }


        public int Index
        {
            get
            {
                // var i = _speaker.PeqDataViewModels.IndexOf(this);
                //return i > 0 ? i : 0;
                return PeqDataModel.Id;
            }
            set { PeqDataModel.Id = value; }
        }


        public Brush Color
        {
            get { return LinebBrushes[Index]; }
        }

        public LineGraph LineData
        {
            get
            {
                return _lineData ?? (_lineData = new LineGraph(GenLine()
                                                     )
                    {
                        Stroke = LinebBrushes[Index],
                        StrokeThickness = 1,
                        Visibility = IsEnabled
                                         ? Visibility.Visible
                                         : Visibility.Collapsed,
                        ZIndex = Index,
                    });
            }
        }


        public bool IsMouseOver
        {
            get { return _isMousOver; }
            set
            {
                _isMousOver = value;
                LineData.StrokeThickness = (value) ? 3 : 1;
                //RaisePropertyChanged(() => LineData);
            }
        }

        public DraggablePoint DraggablePoint
        {
            get
            {
                return _draggablePoint ?? (_draggablePoint = new DraggablePoint(
                                                                 new Point(Frequency, Boost)
                                                                 )
                    {
                        //IsEnabled = _speaker.Id != 15,
                        Visibility = IsEnabled ? Visibility.Visible : Visibility.Collapsed
                    });
            }
        }

        public PeqDataModel PeqDataModel { get; private set; }

        public int Order
        {
            get
            {

                return PeqDataModel.Order;
            }
            set
            {
                if (PeqDataModel.Order == value || value == -1) return;
                if ((FilterType == FilterType.LinkWitzHp || FilterType == FilterType.LinkWitzLp) && value % 2 != 0)
                    return;
                {
                    OnBiquadsChanged(new BiquadsChangedEventArgs()
                    {
                        PeqField = PeqField.Order,
                        RequestOrder = value
                    });
                }
            }
        }

        public void SetType(FilterType type)
        {
            PeqDataModel.FilterType = type;            
        }

        public void ResetType()
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                new Action(async () =>
                {
                    await Task.Delay(30);
                    RaisePropertyChanged(() => FilterType);
                })
                );
        }

        public void SetOrder(int order)
        {
            PeqDataModel.Order = order;
        }

        public void UpdateFilterType()
        {
            RaisePropertyChanged(() => Order);
            RaisePropertyChanged(() => FilterType);
            UpdateGraphics();
        }

        public double Frequency
        {
            get { return PeqDataModel.Frequency; }
            set
            {
                if (Math.Abs(PeqDataModel.Frequency - value) < .01) return;
                PeqDataModel.Frequency = value;
                UpdateGraphics();
                OnBiquadsChanged(new BiquadsChangedEventArgs() { PeqField = PeqField.Frequency }); //true
                RaisePropertyChanged(() => Frequency);
            }
        }

        public virtual FilterType FilterType
        {
            get { return PeqDataModel.FilterType; }
            set
            {
                OnBiquadsChanged(new BiquadsChangedEventArgs()
                {
                    PeqField = PeqField.FilterType,
                    RequestFilterType = value
                });

            }
        }

        public double BandWidth
        {
            get { return PeqDataModel.BandWidth; }
            set
            {
                if (Math.Abs(PeqDataModel.BandWidth - value) < .001) return;
                PeqDataModel.BandWidth = value;
                UpdateGraphics();
                OnBiquadsChanged(new BiquadsChangedEventArgs() { PeqField = PeqField.Bandwidth }); //true
                RaisePropertyChanged(() => BandWidth);
            }
        }

        public double Boost
        {
            get { return !HasBoost() ? 0 : PeqDataModel.Boost; }
            set
            {
                if (Math.Abs(PeqDataModel.Boost - value) < 0.001) return;
                PeqDataModel.Boost = value;
                UpdateGraphics();
                OnBiquadsChanged(new BiquadsChangedEventArgs() { PeqField = PeqField.Boost }); //true
                RaisePropertyChanged(() => Boost);
            }
        }

        /// <summary>
        ///     Enable or disable whole eq param setting
        /// </summary>
        public bool IsEnabled
        {
            get { return PeqDataModel.IsEnabled; }
            set
            {
                PeqDataModel.IsEnabled = value;
                UpdateGraphics();
                OnBiquadsChanged(new BiquadsChangedEventArgs() { PeqField = PeqField.IsEnabled }); //true
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public virtual IEnumerable<SOS> FilterData
        {
            get
            {
                return DspCoefficients.GetXoverSOS(PeqDataModel.Frequency, PeqDataModel.Order,
                                                         PeqDataModel.FilterType,
                                                         DspCoefficients.Fs, PeqDataModel.Gain);
            }
        }

        public double Gain
        {
            get
            {
                return PeqDataModel.Gain;
            }
            set
            {
                PeqDataModel.Gain = value;
                RaisePropertyChanged(() => Gain);
            }
        }

        private Point Posb()
        {
            return new Point(Math.Pow(10, Math.Log10(DraggablePoint.Position.X) - BandWidth / 10),
                             DraggablePoint.Position.Y);
        }

        private Point PosE()
        {
            return new Point(Math.Pow(10, Math.Log10(DraggablePoint.Position.X) + BandWidth / 10),
                             DraggablePoint.Position.Y);
        }

        public bool HasBandwidth()
        {
            return ((FilterType == FilterType.Peaking
                     || FilterType == FilterType.LowShelf
                     || FilterType == FilterType.HighShelf
                     || FilterType == FilterType.Notch
                    ));
        }

        private void OnBandwithPositionChange(Point position)
        {
            var bdwtemp = 10 * Math.Abs(Math.Log10(position.X) - Math.Log10(DraggablePoint.Position.X));
            if (Math.Abs(BandWidth - bdwtemp) < .1) return;
            BandWidth = bdwtemp;
            BandwidthArrow.StartPoint = Posb();
            BandwidthArrow.EndPoint = PosE();
        }

        private CompositeDataSource GenLine()
        {
            var xs = DspCoefficients.XList.ToArray();
            var xDs = xs.AsXDataSource();

            var t = (from d in xs select DspCoefficients.FilterFuncs[FilterType](d, PeqDataModel).DbMagnitude());
            return t.AsYDataSource().Join(xDs);
        }

        public bool HasBoost()
        {
            return (FilterType == FilterType.Peaking || FilterType == FilterType.HighShelf ||
                    FilterType == FilterType.LowShelf
                   );
        }

        public bool IsSecondOrder()
        {
            return PeqDataModel.FilterType == FilterType.Peaking
                   || PeqDataModel.FilterType == FilterType.Notch
                   || PeqDataModel.FilterType == FilterType.HighShelf
                   || PeqDataModel.FilterType == FilterType.LowShelf;
        }

        public ICommand RemoveParam
        {
            get
            {
                return new RelayCommand(OnRemoveThisParam);
            }
        }

        public event EventHandler RemoveThisParam;

        private void OnRemoveThisParam()
        {
            EventHandler handler = RemoveThisParam;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void UpdateGraphics()
        {
            DraggablePoint.Position = new Point(Frequency, Boost);
            LineData.DataSource = GenLine();
            LineData.Stroke = LinebBrushes[Index];
            LineData.Visibility = IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            DraggablePoint.Visibility = IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            BandWidthPoint.Visibility = IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            BandwidthArrow.Visibility = IsEnabled ? Visibility.Visible : Visibility.Collapsed;

            if (BandwidthArrow != null)
            {
                BandwidthArrow.StartPoint = Posb();
                BandwidthArrow.EndPoint = PosE();
            }
            if (BandWidthPoint != null)
            {
                BandWidthPoint.Position = Posb();
            }
            RaisePropertyChanged(() => Color);
        }
    }

    public class BiquadsChangedEventArgs : CancelEventArgs
    {
        public PeqField PeqField { get; set; }
        public FilterType RequestFilterType { get; set; }
        public int RequestOrder { get; set; }
    }
}