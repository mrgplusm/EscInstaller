#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using EscInstaller.View;
using EscInstaller.View.DragnDrop;

#endregion

namespace EscInstaller.Behaviors
{
    /// <summary>
    ///     For enabling Drop on Datagrid
    /// </summary>
    public class DataGridDropBehaviour : Behavior<DataGrid>
    {
        private Type _dataType; //the type of the data that can be dropped into this control
        private ListBoxAdornerManager _insertAdornerManager;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (_insertAdornerManager != null)
                _insertAdornerManager.Clear();

            //if the data type can be dropped 
            if (_dataType == null) return;
            if (!e.Data.GetDataPresent(_dataType)) return;
            //first find the UIElement that it was dropped over, then we determine if it's 
            //dropped above or under the UIElement, then insert at the correct index.
            var dropContainer = sender as DataGrid;
            if (dropContainer == null) return;
            //get the UIElement that was dropped over
            var droppedOverItem = UIHelper.GetUIElement(dropContainer, e.GetPosition(dropContainer));
            //the location where the item will be dropped
            var dropIndex = dropContainer.ItemContainerGenerator.IndexFromContainer(droppedOverItem) + 1;

            //find if it was dropped above or below the index item so that we can insert 
            //the item in the correct place
            if (UIHelper.IsPositionAboveElement(droppedOverItem, e.GetPosition(droppedOverItem))) //if above
            {
                dropIndex = dropIndex - 1; //we insert at the index above it
            }

            //remove the data from the source
            var source = e.Data.GetData(_dataType) as IDragable;
            if (source != null) source.Remove();

            //drop the data
            var target = AssociatedObject.DataContext as IDropable;
            if (target != null) target.Drop(e.Data.GetData(_dataType), dropIndex);
        }

        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
            //check conditions are right to leave adorner
            //if ((e.Data.GetDataPresent(_dataType)) && (sender as ItemsControl != null)) return;
            //otherwise remove it
            if (_insertAdornerManager != null)
                _insertAdornerManager.Clear();
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            var update = false;
            var isAboveElement = false;
            UIElement droppedOverItem = null;
            try
            {
                e.Handled = true;
                if (_dataType == null) return;
                if (!e.Data.GetDataPresent(_dataType)) return;
                SetDragDropEffects(e);
                if (_insertAdornerManager == null) return;
                var dropContainer = sender as DataGrid;
                if (dropContainer == null) return;
                droppedOverItem = UIHelper.GetUIElement(dropContainer, e.GetPosition(dropContainer));
                if (droppedOverItem == null) return;
                isAboveElement = UIHelper.IsPositionAboveElement(droppedOverItem, e.GetPosition(droppedOverItem));
                update = true;
            }
            finally
            {
                if (_insertAdornerManager != null)
                {
                    if (update)
                        _insertAdornerManager.Update(droppedOverItem, isAboveElement);
                    else
                        _insertAdornerManager.Clear();
                }
            }
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;

            //initialize adorner manager with the adorner layer of the itemsControl
            if (_insertAdornerManager != null) return;
            var s = sender as DataGrid;
            if (s != null) _insertAdornerManager = new ListBoxAdornerManager(AdornerLayer.GetAdornerLayer(s));

            if (_dataType != null) return;
            //if the DataContext implements IDropable, record the data type that can be dropped
            if (AssociatedObject.DataContext == null) return;
            if (AssociatedObject.DataContext as IDropable != null)
            {
                _dataType = ((IDropable) AssociatedObject.DataContext).DataType;
            }
        }

        /// <summary>
        ///     Provides feedback on if the data can be dropped
        /// </summary>
        /// <param name="e"></param>
        private void SetDragDropEffects(DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(_dataType) ? DragDropEffects.Move : DragDropEffects.None;
        }
    }
}