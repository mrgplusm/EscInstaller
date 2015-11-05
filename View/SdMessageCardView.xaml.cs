#region

using System;
using System.Windows;

#endregion

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for SdMessageCardView.xaml
    /// </summary>
    public partial class SdMessageCardView
    {
        private static SdMessageCardView _openwindow;

        public SdMessageCardView()
        {
            Loaded += SmvLoaded;
            Closed += SmvClosed;
            InitializeComponent();
        }

        private static void SmvClosed(object sender, EventArgs e)
        {
            _openwindow = null;
        }

        private void SmvLoaded(object sender, RoutedEventArgs e)
        {
            if (_openwindow != null)
            {
                _openwindow.Activate();
                Close();
            }

            _openwindow = this;
        }
    }
}