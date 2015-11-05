#region

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for PanelView.xaml
    /// </summary>
    public partial class PanelView : UserControl
    {
        public PanelView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                string inner = null;
                if (ex.InnerException != null)
                    inner = ex.InnerException.Message;
                MessageBox.Show(ex.Message + "\n" + inner);
            }
        }
    }
}