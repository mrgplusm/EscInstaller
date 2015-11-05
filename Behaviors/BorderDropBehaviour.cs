#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using EscInstaller.View;

#endregion

namespace EscInstaller.Behaviors
{
    public class BorderDropBehaviour : Behavior<Border>
    {
        private Type _dataType; //the type of the data that can be dropped into this control

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (_dataType != null)
            {
            }
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            //remove the data from the source
            var source = e.Data.GetData(_dataType) as IDragable;
            if (source != null) source.Remove();

            //drop the data
            var target = AssociatedObject.DataContext as IDropable;
            if (target != null) target.Drop(e.Data.GetData(_dataType));

            e.Handled = true;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (_dataType != null) return;
            //if the DataContext implements IDropable, record the data type that can be dropped
            if (AssociatedObject.DataContext == null) return;
            if (AssociatedObject.DataContext as IDropable == null) return;
            _dataType = ((IDropable) AssociatedObject.DataContext).DataType;
        }
    }
}