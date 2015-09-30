using System.Windows;
using System.Windows.Input;

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for BatteryCalc.xaml
    /// </summary>
    public partial class BatteryCalcView : Window
    {
        public BatteryCalcView()
        {
            InitializeComponent();
            KeyDown += OnButtonKeyDown;
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}