using System;
using System.Globalization;
using System.Windows.Data;
using Common.Converters;

namespace EscInstaller.Converters
{
    public class LinkClrCnvrt : BaseConverter, IValueConverter
    {
        #region IValueConverter Members

        public LinkClrCnvrt() {}

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((int)value) > 0 ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}