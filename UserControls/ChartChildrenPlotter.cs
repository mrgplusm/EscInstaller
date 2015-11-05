#region

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Microsoft.Research.DynamicDataDisplay.Navigation;

#endregion

namespace EscInstaller.UserControls
{
    public class ChartChildrenPlotter : ChartPlotter
    {
        public static readonly DependencyProperty PlotChildrenProperty = DependencyProperty.Register("PlotChildren",
            typeof (ObservableCollection<IPlotterElement>),
            typeof (ChartChildrenPlotter),
            new PropertyMetadata(default(ObservableCollection<IPlotterElement>), (o, args) =>
            {
                var t = (ChartChildrenPlotter) o;
                var n = args.NewValue as ObservableCollection<IPlotterElement>;
                var old = args.OldValue as ObservableCollection<IPlotterElement>;

                if (old != null)
                {
                    foreach (var plotterElement in old)
                    {
                        t.Children.Remove(plotterElement);
                    }
                    old.CollectionChanged -= t.t_CollectionChanged;
                }

                if (n != null)
                {
                    //s(args, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, n, old ?? new ObservableCollection<IPlotterElement>()));
                    n.CollectionChanged += t.t_CollectionChanged;

                    if (old != null)
                    {
                        t.t_CollectionChanged(null,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction
                                    .Replace, n, old));
                    }
                    else
                    {
                        t.t_CollectionChanged(null,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Add,
                                n));
                    }
                }
            }));

        public static readonly DependencyProperty VisibleAreaProperty = DependencyProperty.Register(
            "VisibleArea", typeof (DataRect), typeof (ChartChildrenPlotter), new PropertyMetadata(default(DataRect),
                (q, e) =>
                {
                    var s = (ChartChildrenPlotter) q;
                    s.Visible = (DataRect) e.NewValue;
                }));

        public ChartChildrenPlotter()
        {
            Children.RemoveAll(typeof (Legend));
            Children.RemoveAll(typeof (DefaultContextMenu));
            Children.RemoveAll(typeof (KeyboardNavigation));
            Children.RemoveAll(typeof (AxisNavigation));
            Children.RemoveAll(typeof (MouseNavigation));

            var tp = new NumericTicksProvider();


            //Visible = new DataRect(1, -15, 3.4, 30);
            MainVerticalAxis = new VerticalAxis
            {
                TicksProvider = tp,
                LabelProvider = new ToStringLabelProvider()
            };
        }

        public DataRect VisibleArea
        {
            get { return (DataRect) GetValue(VisibleAreaProperty); }
            set { SetValue(VisibleAreaProperty, value); }
        }

        public ObservableCollection<IPlotterElement> PlotChildren
        {
            get { return (ObservableCollection<IPlotterElement>) GetValue(PlotChildrenProperty); }
            set { SetValue(PlotChildrenProperty, value); }
        }

        private void t_CollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.NewItems != null)
                foreach (var result in eventArgs.NewItems.Cast<IPlotterElement>())
                {
                    if (result == null) continue;
                    if (Children.Contains(result))
                        continue;

                    //hack to prevent child to be added to previous and this plotter.
                    if (result.Plotter != null)
                        result.Plotter.Children.Remove(result);

                    result.AddToPlotter(this);
                    //t.plotter.Children.Add(result);
                }
            if (eventArgs.OldItems != null)
                foreach (var result in eventArgs.OldItems.Cast<IPlotterElement>())
                {
                    if (result == null) continue;
                    Children.Remove(result);
                }
            if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                var q =
                    Children.OfType<DraggablePoint>()
                        .Cast<IPlotterElement>()
                        .Concat(Children.OfType<LineGraph>())
                        .ToArray();

                foreach (var plotterElement1 in q)
                {
                    Children.Remove(plotterElement1);
                }
            }
        }
    }
}