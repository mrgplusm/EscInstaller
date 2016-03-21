using System;
using System.Globalization;
using System.Windows.Data;
using Common.Converters;

namespace EscInstaller.Converters
{
    public class AmplifierOpacityCvtr : BaseConverter, IValueConverter
    {

        public AmplifierOpacityCvtr() { }
        #region IValueConverter Members

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((bool)value) ? 1 : .5;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}