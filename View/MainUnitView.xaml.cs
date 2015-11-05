#region

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

#endregion

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for MainUnitView.xaml
    /// </summary>
    public partial class MainUnitView
    {
        private double _bottomGbFrom;

        public MainUnitView()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var z = (MainUnitViewModel) DataContext;
            z.ItemsControlClick();
        }

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

            var z = (MainUnitViewModel) DataContext;            
            z?.VuMeter?.StopVuMeter();
        }

        private void sb_Completed(object sender, EventArgs e)
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
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T) child;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
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
            "AnimateToHeight", typeof (double), typeof (AnimatedBorder), new PropertyMetadata(default(double),
                (o, args) => HeightAnimation((double) args.NewValue, (FrameworkElement) o)));

        public double AnimateToHeight
        {
            get { return (double) GetValue(AnimateToHeightProperty); }
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