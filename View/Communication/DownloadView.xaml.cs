#region

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

#endregion

namespace EscInstaller.View.Communication
{
    /// <summary>
    ///     Interaction logic for DownloadView.xaml
    /// </summary>
    public partial class DownloadView : Window
    {
        private static DownloadView _downloadView;

        public static DownloadView DownloadViewFactory()
        {
            return _downloadView ?? (_downloadView = new DownloadView());
        }


        public DownloadView()
        {
            Closed += CloseWindow;
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true; 
            Hide();      
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            var d = (EscCommunication.Communication)DataContext;
            d.CloseWindow();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}