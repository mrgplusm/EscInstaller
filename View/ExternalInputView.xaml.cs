#region

using System.Windows.Controls;
using System.Windows.Data;
using EscInstaller.ViewModel.SDCard;

#endregion

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for ExternalInput.xaml
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