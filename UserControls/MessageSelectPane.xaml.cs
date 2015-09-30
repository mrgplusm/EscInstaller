using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Futurama.UserControls
{
    /// <summary>
    /// Interaction logic for MessageSelectPane.xaml
    /// </summary>
    public partial class MessageSelectPane : UserControl
    {
        public static DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof (string),
            typeof (MessageSelectPane), new PropertyMetadata(string.Empty));

        public MessageSelectPane()
        {
            InitializeComponent();
        }

        public object GroupName
        {
            get { return GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty StartFromProperty =
            DependencyProperty.Register("StartFrom", typeof (int), typeof (MessageSelectPane), new PropertyMetadata(default(int)));

        public int StartFrom
        {
            get { return (int) GetValue(StartFromProperty); }
            set { SetValue(StartFromProperty, value); }
        }

        public static readonly DependencyProperty MessagesAProperty =
            DependencyProperty.Register("MessagesA", typeof (ObservableCollection<string>), typeof (MessageSelectPane), new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> MessagesA
        {
            get { return (ObservableCollection<string>) GetValue(MessagesAProperty); }
            set { SetValue(MessagesAProperty, value); }
        }

        public static readonly DependencyProperty MessagesBProperty =
            DependencyProperty.Register("MessagesB", typeof (ObservableCollection<string>), typeof (MessageSelectPane), new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> MessagesB
        {
            get { return (ObservableCollection<string>) GetValue(MessagesBProperty); }
            set { SetValue(MessagesBProperty, value); }
        }

        public static readonly DependencyProperty MessagesProperty =
            DependencyProperty.Register("Messages", typeof (List<int>), typeof (MessageSelectPane), new PropertyMetadata(default(List<int>)));

        public List<int> Messages
        {
            get { return (List<int>) GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }   
    }
}
