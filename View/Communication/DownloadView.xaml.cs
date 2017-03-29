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
        public DownloadView()
        {
            Closed += CloseWindow;
            InitializeComponent();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            var d = (EscCommunication.Communication) DataContext;
            d.CloseWindow();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }        
    }
}