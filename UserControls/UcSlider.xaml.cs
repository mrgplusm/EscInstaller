#region

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace EscInstaller.UserControls
{
    /// <summary>
    ///     Interaction logic for UcSlider.xaml
    /// </summary>
    public partial class UcSlider : UserControl
    {
        public static readonly DependencyProperty SliderValueProperty =
            DependencyProperty.Register("SliderValue", typeof (double), typeof (UcSlider),
                new FrameworkPropertyMetadata(default(double)) {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof (double), typeof (UcSlider),
                new FrameworkPropertyMetadata(6.0,
                    ((o, args) =>

                    {
                        var s = o as UcSlider;
                        if (s == null) return;

                        var q = (double) args.NewValue;
                        s.top.Content = q + "dB";
                    })
                    ));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof (double), typeof (UcSlider), new PropertyMetadata(-80.0,
                (o, args) =>
                {
                    var s = o as UcSlider;
                    if (s == null) return;

                    var q = (double) args.NewValue;
                    s.bottom.Content = q + "dB";
                }));

        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof (double), typeof (UcSlider),
                new PropertyMetadata(default(double)));

        public UcSlider()
        {
            InitializeComponent();
            slider.ValueChanged += (sender, args) => OnHasMoved(args);

            bottom.Content = MinValue + "dB";
            top.Content = MaxValue + "dB";
            //DataContext = this;
        }

        public double TickFrequency
        {
            get { return (double) GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public string Top
        {
            get { return "+" + MaxValue + "dB"; }
        }

        public string Bottom
        {
            get { return MinValue + "dB"; }
        }

        public double SliderValue
        {
            get { return (double) GetValue(SliderValueProperty); }
            set { SetValue(SliderValueProperty, value); }
        }

        public double MaxValue
        {
            get { return (double) GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public double MinValue
        {
            get { return (double) GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public event EventHandler HasMoved;

        public void OnHasMoved(EventArgs e)
        {
            var handler = HasMoved;
            if (handler != null) handler(this, e);
        }
    }
}