using System;
using System.ComponentModel;
using System.Windows;
using EscInstaller.ViewModel;

namespace EscInstaller
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var s = DataContext as MainViewModel;
            if (s == null) return;
            if (!s.ExitCommand())
                e.Cancel = true;
        }
    }


}