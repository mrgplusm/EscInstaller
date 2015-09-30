using System.Diagnostics;
using GalaSoft.MvvmLight;
using System;
using System.Windows;
using System.Windows.Media;


namespace EscInstaller.ViewModel.OverView
{
    public enum SnapType
    {
        Red,
        Gray,
        Brown,
    }

    public class SnapShot : DiagramData
    {
        public DiagramData Parent { get; set; }


        public SnapType SnapType { get; set; }

        private BindablePoint _offset;
        public BindablePoint Offset
        {
            get { return _offset ?? (_offset = new BindablePoint()); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged(() => IsConnected);

            }
        }

        public SnapShot(DiagramData parent)
        {
            Parent = parent;
            Offset.OnValueChanged += Calculate;
        }

        public void Calculate()
        {
            // Parent.Size.X  Parent.Size.Y
            Location.Value = Point.Add(Parent.Location.Value, new Vector(Offset.X, Offset.Y));
            Location.ValueChanged();
            
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
    }
}