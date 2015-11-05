#region

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight.CommandWpf;

#endregion

namespace EscInstaller.UserControls
{
    public partial class UcEscNum : Window
    {
        private readonly List<int> _existingSlaves;
        public ICommand OnEnter = new RelayCommand(() => { });

        public UcEscNum(List<int> existingSlaves)
        {
            _existingSlaves = existingSlaves;
            InitializeComponent();
            FocusManager.SetFocusedElement(this, ResponseTextBox);
        }

        public int Result { get; private set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            int parsedint;
            if (!int.TryParse(ResponseTextBox.Text, out parsedint) || parsedint < 1 || parsedint > LibraryData.MaxSlaves)
            {
                MessageBox.Show("Please enter a number between 1 and " + LibraryData.MaxSlaves, "Incorrect slave number",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                e.Handled = false;
            }
            else if (_existingSlaves.Contains(parsedint))
            {
                MessageBox.Show("Each slave has its own number. Please choose a different number",
                    "This slave already exist",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                e.Handled = false;
            }

            else
            {
                Result = parsedint;
                DialogResult = true;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}