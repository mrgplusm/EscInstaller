using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for PageGainView.xaml
    /// </summary>
    public partial class PageGainView : UserControl
    {
        public PageGainView()
        {
            //DataContext = ((ViewModelLocator)Application.Current.Resources["Locator"])
            //    .PageGain(Guid.NewGuid());
            InitializeComponent();
            KeyDown += OnButtonKeyDown;
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { }
        
        }
    }
}