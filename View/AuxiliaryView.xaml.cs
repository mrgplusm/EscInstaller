using System.Windows;
using System.Windows.Controls;

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for AuxiliaryView.xaml
    /// </summary>
    public partial class AuxiliaryView : UserControl
    {
        public AuxiliaryView()
        {
            //DataContext = ((ViewModelLocator)Application.Current.Resources["Locator"])
            //    .Auxiliary(Guid.NewGuid());
            InitializeComponent();
        }
    }
}