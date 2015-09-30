using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using Common.Helpers;
using EscInstaller.Adorners;

namespace EscInstaller.View.DragnDrop
{
    internal class ListBoxAdornerManager
    {
        private readonly AdornerLayer _adornerLayer;
        private ListBoxAdorner _adorner;

        internal ListBoxAdornerManager(AdornerLayer layer)
        {
            _adornerLayer = layer;
        }

        private UIElement _lastElement;

        internal void Update(UIElement adornedElement, bool isAboveElement)
        {
            //exit if nothing changed
            if (_lastElement != null && _lastElement.Equals(adornedElement) && _adorner.IsAboveElement == isAboveElement) return;
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