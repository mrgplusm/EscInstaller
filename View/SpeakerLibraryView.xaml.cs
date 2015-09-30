using System.Windows;
using System.Windows.Input;

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for SpeakerLibrary.xaml
    /// </summary>
    public partial class SpeakerLibraryView
    {
        public SpeakerLibraryView()
        {
            InitializeComponent();
            KeyDown += OnButtonKeyDown;
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { }
                //Close();
        }
    }
}