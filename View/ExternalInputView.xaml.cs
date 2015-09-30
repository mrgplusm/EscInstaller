using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.SDCard;

namespace EscInstaller.View
{
    /// <summary>
    /// Interaction logic for ExternalInput.xaml
    /// </summary>
    public partial class ExternalInputView : UserControl
    {
        public ExternalInputView()
        {
            InitializeComponent();
        }

        private void PreAnnounce_OnFilter(object sender, FilterEventArgs e)
        {
            var m = e.Item as SdFileVM;
            if (m != null)
                e.Accepted = m.Position != 0xff;
        }

        private void Alarm_OnFilter(object sender, FilterEventArgs e)
        {
            var m = e.Item as SdFileVM;
            if (m != null)
                e.Accepted = m.Position != 0xff;
        }

        private void MessagesBvs_OnFilter(object sender, FilterEventArgs e)
        {
            var m = e.Item as SdFileVM;
            if (m != null)
                e.Accepted = m.Position != 0xff;
        }

        private void MessagesAvs_OnFilter(object sender, FilterEventArgs e)
        {
            var m = e.Item as SdFileVM;
            if (m != null)
                e.Accepted = m.Position != 0xff;
        }


    }

    public class MessageBBox : ComboBox
    {
        
    }
}
