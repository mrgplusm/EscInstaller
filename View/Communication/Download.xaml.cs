using System.Windows.Input;
using EscInstaller.UserControls;

namespace EscInstaller.View.Communication
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download 
    {
        public Download()
        {
            InitializeComponent();
        }
        private void DataGridCellPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PEQParams.Dgcpmlbd(sender, e);
        }

    }


}
