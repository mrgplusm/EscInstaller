#region

using System.Windows;
using System.Windows.Media;

#endregion

namespace EscInstaller.View
{
    /// <summary>
    ///     Interaction logic for LibraryEditor.xaml
    /// </summary>
    public partial class LibraryEditor
    {
        public delegate Point GetDragDropPosition(IInputElement element);

        //private int _prevRowIndex = -1;

        public LibraryEditor()
        {
            InitializeComponent();
        }

        public bool IsMouseOnTargetRow(Visual target, GetDragDropPosition position)
        {
            if (target == null) return false;
            var posbounds = VisualTreeHelper.GetDescendantBounds(target);
            var mousepos = position((IInputElement) target);
            return posbounds.Contains(mousepos);
        }
    }
}