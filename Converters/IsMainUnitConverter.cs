using System;
using System.Globalization;
using System.Windows.Data;
using Common.Converters;
using Common.Model;
using EscInstaller.ViewModel;

namespace EscInstaller.Converters
{
    public class IsEligibleForRemove : BaseConverter, IValueConverter
    {

        public IsEligibleForRemove() { }
        #region IValueConverter Members

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value is MainUnitViewModel;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}