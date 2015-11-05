#region

using System.Windows;
using System.Windows.Media;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public enum SnapType
    {
        Red,
        Gray,
        Brown
    }

    public class SnapShot : DiagramData
    {
        private bool _isConnected;
        private BindablePoint _offset;

        public SnapShot(DiagramData parent)
        {
            Parent = parent;
            Offset.OnValueChanged += Calculate;
        }

        public DiagramData Parent { get; set; }
        public SnapType SnapType { get; set; }

        public BindablePoint Offset
        {
            get { return _offset ?? (_offset = new BindablePoint()); }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged(() => IsConnected);
            }
        }

        public virtual Brush Color
        {
            get { return Brushes.Orange; }
        }

        public override Point Size
        {
            get { return new Point(); }
        }

        public int RowId { get; set; } //used with channel link

        public void Calculate()
        {
            // Parent.Size.X  Parent.Size.Y
            Location.Value = Point.Add(Parent.Location.Value, new Vector(Offset.X, Offset.Y));
            Location.ValueChanged();
        }
    }
}