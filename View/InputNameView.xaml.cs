using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for inputName.xaml
    /// </summary>
    public partial class InputNameView : UserControl
    {
        public InputNameView()
        {
            //DataContext = ((ViewModelLocator)Application.Current.Resources["Locator"])
            //    .InputName(Guid.NewGuid());
            InitializeComponent();


            KeyDown += OnButtonKeyDown;
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { };
        //        Close();
        }
    }

}