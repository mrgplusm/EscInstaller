using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace EscInstaller.Adorners
{
    internal class ListBoxAdorner : Adorner
    {
        private readonly bool _isAboveElement;

        public ListBoxAdorner(UIElement adornedElement, AdornerLayer adornerLayer, bool isAboveElement)
            : base(adornedElement)
        {            
            _isAboveElement = isAboveElement;
            adornerLayer.Add(this);
            adornerLayer.Update(AdornedElement);
            Visibility = Visibility.Visible;
        }

        public bool IsAboveElement
        {
            get { return _isAboveElement; }
        }

        //public void Remove()
        //{
        //    Visibility = Visibility.Collapsed;
        //}

    

        protected override void OnRender(DrawingContext drawingContext)
        {            
            var adornedElementRect = new Rect(AdornedElement.DesiredSize);
            var renderBrush = new SolidColorBrush(Colors.Red) {Opacity = 0.5};
            var renderPen = new Pen(new SolidColorBrush(Colors.White), 1.5);
            var linepen = new Pen(Brushes.CornflowerBlue, 3);
            const double renderRadius = 5.0;

            if (_isAboveElement)
            {
                drawingContext.DrawLine(linepen, adornedElementRect.TopLeft, adornedElementRect.TopRight );
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            }
            else
            {
                drawingContext.DrawLine(linepen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight);
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
            }
        }
    }
}