#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EscInstaller.ViewModel.Settings.Peq;
using Xceed.Wpf.Toolkit;

#endregion

namespace EscInstaller.UserControls
{
    public partial class PEQParams
    {
        public PEQParams()
        {
            InitializeComponent();
        }

        private void DataGridCellPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Dgcpmlbd(sender, e);
        }

        /// <summary>
        ///     single click editing in datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Dgcpmlbd(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell == null || cell.IsEditing || cell.IsReadOnly) return;
            if (!cell.IsFocused)
            {
                cell.Focus();
            }
            var dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null) return;
            if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
            {
                if (!cell.IsSelected)
                    cell.IsSelected = true;
            }
            else
            {
                var row = FindVisualParent<DataGridRow>(cell);
                if (row != null && !row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void RowMouseMove(object sender, MouseEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null) return;
            //var datagrid = FindVisualParent<DataGrid>(row);
            var item = (PeqDataViewModel) row.Item;
            item.IsMouseOver = true;
        }

        private void RowMouseLeave(object sender, MouseEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null) return;
            //var datagrid = FindVisualParent<DataGrid>(row);
            var item = (PeqDataViewModel) row.Item;
            item.IsMouseOver = false;
        }
    }


    public class FrequencyUpDown : DoubleUpDown
    {
        protected override double IncrementValue(double value, double increment)
        {
            return Math.Pow(10, Math.Log10(value) + increment);
        }

        protected override double DecrementValue(double value, double increment)
        {
            return Math.Pow(10, Math.Log10(value) - increment);
        }
    }
}