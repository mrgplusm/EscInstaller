#region

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.View.Communication
{
    /// <summary>
    ///     Interaction logic for SendList.xaml
    /// </summary>
    public partial class SendList : UserControl
    {
        public static readonly DependencyProperty SendItemsProperty =
            DependencyProperty.Register("SendItems", typeof (ObservableCollection<DispatchDataViewModel>),
                typeof (SendList),
                new FrameworkPropertyMetadata(
                    default(ObservableCollection<DispatchDataViewModel>),
                    FrameworkPropertyMetadataOptions.None, ItemsChanged));

        public SendList()
        {
            InitializeComponent();
        }

        public ObservableCollection<DispatchDataViewModel> SendItems
        {
            get { return (ObservableCollection<DispatchDataViewModel>) GetValue(SendItemsProperty); }
            set { SetValue(SendItemsProperty, value); }
        }

        private static void ItemsChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var o = (SendList) dependencyObject;
            o.Lst.ItemsSource =
                (ObservableCollection<DispatchDataViewModel>) dependencyPropertyChangedEventArgs.NewValue;
        }
    }
}