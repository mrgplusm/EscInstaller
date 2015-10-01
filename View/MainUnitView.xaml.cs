using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EscInstaller.ViewModel;
using Microsoft.Research.DynamicDataDisplay;


namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for MainUnitView.xaml
    /// </summary>
    public partial class MainUnitView
    {
        public MainUnitView()
        {
            InitializeComponent();

        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var z = (MainUnitViewModel)DataContext;
            z.ItemsControlClick();
        }

        private double _bottomGbFrom;

        private void GroupBoxBottom_OnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            var child = GroupBoxBottom.FindVisualChildren<Grid>().FirstOrDefault();
            if (child == null) return;

            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var animation = new DoubleAnimation
            {
                From = _bottomGbFrom,
                To = child.DesiredSize.Height + 10,
                Duration = new Duration(TimeSpan.FromSeconds(1)),

                EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut
                }
            };


            Storyboard.SetTarget(animation, GroupBoxBottom);
            Storyboard.SetTargetProperty(animation, new PropertyPath(HeightProperty));

            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Completed += sb_Completed;
            sb.Begin();

            var z = (MainUnitViewModel)DataContext;
            if (z == null) return;
            z.OnPanelSelectionChanged();
        }




        void sb_Completed(object sender, EventArgs e)
        {
            _bottomGbFrom = GroupBoxBottom.ActualHeight;
        }




    }

    public static class AnimationHelper
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }


    internal class AnimatedBorder : Border
    {
        public static readonly DependencyProperty AnimateToHeightProperty = DependencyProperty.Register(
            "AnimateToHeight", typeof(Double), typeof(AnimatedBorder), new PropertyMetadata(default(Double),
                (o, args) => HeightAnimation((double)args.NewValue, (FrameworkElement)o)));

        public Double AnimateToHeight
        {
            get { return (Double)GetValue(AnimateToHeightProperty); }
            set { SetValue(AnimateToHeightProperty, value); }
        }

        internal static void HeightAnimation(double newHeight, FrameworkElement dependencyObject)
        {

            var from = dependencyObject.Height.IsNotNaN() ? dependencyObject.Height : 0;
            var animation = new DoubleAnimation
            {
                From = from,
                To = newHeight,
                Duration = new Duration(TimeSpan.FromSeconds(1)),

                EasingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };


            Storyboard.SetTarget(animation, dependencyObject);
            Storyboard.SetTargetProperty(animation, new PropertyPath(HeightProperty));

            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Begin();
        }
    }


}