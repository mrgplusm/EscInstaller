using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Converters;

namespace EscInstaller.Converters
{
    public class BoolToCrimson : BaseConverter, IValueConverter
    {
        #region IValueConverter Members

        public BoolToCrimson() {}

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.White : Brushes.Crimson;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}