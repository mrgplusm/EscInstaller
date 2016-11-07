#region

using System;
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
        private static DownloadView _openwindow;

        public DownloadView()
        {
            Loaded += DownloadView_Loaded;
            Closed += DownloadView_Closed;
            InitializeComponent();
        }

        public string TextControl { get; set; }

        private static void DownloadView_Closed(object sender, EventArgs e)
        {
            _openwindow = null;
        }

        private void DownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_openwindow != null)
            {
                _openwindow.Activate();
                Close();
            }

            _openwindow = this;
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }        
    }
}