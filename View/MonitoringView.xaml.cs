#region

using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Common;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.OverView;

#endregion

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for MonitoringView.xaml
    /// </summary>
    public partial class MonitoringView
    {
        private static readonly Thread Thread = Thread.CurrentThread;

        public MonitoringView()
        {
            InitializeComponent();
        }

        public string Play
        {
            get { return View.Monitoring._monitorPlay; }
        }

        public string Stop
        {
            get { return View.Monitoring._monitorStop; }
        }

        private void CalibrateButton(object sender, RoutedEventArgs e)
        {
            ButtonAction(sender, ButtonType.Calibrate);
        }

        private void MeasureButton(object sender, RoutedEventArgs e)
        {
            ButtonAction(sender, ButtonType.Measure);
        }

        private void ButtonAction(object sender, ButtonType button)
        {
            if (CommunicationViewModel.OpenConnections.All(q => q.ConnectMode != ConnectMode.Install))
                return;
            var t = (Button) sender;
            var oldText = t.Content;
            var dc = (BlMonitor) t.CommandParameter;

            t.IsEnabled = false;

            var a = new Thread(() =>
            {
                var are = new AutoResetEvent(false);
                var quit = false;
                if (button == ButtonType.Calibrate)
                {
                    dc.CalibrateFinished += () => quit = true;
                    dc.CalibrateUserCancel += () =>
                    {
                        quit = true;
                        are.Set();
                    };
                }
                if (button == ButtonType.Measure)
                    dc.MeasureLoadFinished += () => quit = true;

                if (button == ButtonType.Calibrate)
                    dc.CalibrateStarted += () => are.Set();

                if (button == ButtonType.Measure)
                    dc.MeasureLoadStarted += () => are.Set();

                are.WaitOne();
                var fromThread = Dispatcher.FromThread(Thread);
                if (fromThread == null) return;
                const int wt = 120;

                for (var i = 0; i < wt; i++)
                {
                    var i1 = i;
                    fromThread.Invoke(
                        () => { t.Content = "Wait " + (wt - i1) + "s..."; });
                    if (quit) break;
                    Thread.Sleep(1000);
                }
                fromThread.Invoke(() =>
                {
                    t.IsEnabled = true;
                    t.Content = oldText;
                });
            }) {IsBackground = true};

            a.Start();
        }

        private enum ButtonType
        {
            Calibrate,
            Measure
        }
    }
}