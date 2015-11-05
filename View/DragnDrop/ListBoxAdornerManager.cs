#region

using System.Windows;
using System.Windows.Documents;
using EscInstaller.Adorners;

#endregion

namespace EscInstaller.View.DragnDrop
{
    internal class ListBoxAdornerManager
    {
        private readonly AdornerLayer _adornerLayer;
        private ListBoxAdorner _adorner;
        private UIElement _lastElement;

        internal ListBoxAdornerManager(AdornerLayer layer)
        {
            _adornerLayer = layer;
        }

        internal void Update(UIElement adornedElement, bool isAboveElement)
        {
            //exit if nothing changed
            if (_lastElement != null && _lastElement.Equals(adornedElement) && _adorner.IsAboveElement == isAboveElement)
                return;
            _lastElement = adornedElement;

            if (_adorner == null)
                _adorner = new ListBoxAdorner(adornedElement, _adornerLayer, isAboveElement);
            _adorner.InvalidateVisual();
        }

        public void Clear()
        {
            if (_adorner != null)
                _adornerLayer.Remove(_adorner);
        }
    }
}