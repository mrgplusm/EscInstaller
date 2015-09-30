

using System.Windows;
using System.Windows.Controls;

namespace EscInstaller.UserControls
{
    /// <summary>
    /// Interaction logic for CharsTextBox.xaml
    /// </summary>
    public partial class CharsTextBox 
    {
        public CharsTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CountedTextProperty =
            DependencyProperty.Register("CountedText", typeof(string), typeof(CharsTextBox), new FrameworkPropertyMetadata(default(string),
                (o, args) =>
                {
                    var obj = o as CharsTextBox;
                    if (obj == null) return;
                    var count = args.NewValue as string;
                    if (count == null) return;
                    obj.count.Content = "< " + (13 - count.Length);
                }) { BindsTwoWayByDefault = true });

        
        public string CountedText
        {
            get { return (string)GetValue(CountedTextProperty); }
            set { SetValue(CountedTextProperty, value); }
        }
    }
}
