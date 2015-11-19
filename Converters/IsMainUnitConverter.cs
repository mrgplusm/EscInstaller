#region

using System;
using System.Globalization;
using System.Windows.Data;
using Common.Converters;
using Common.Model;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.Converters
{
    public class IsFdsConverter : BaseConverter, IValueConverter
    {

        public IsFdsConverter() { }
        #region IValueConverter Members

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((MatrixCell)value).ButtonId > 203;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }

        #endregion
    }


    public class IsEligibleForRemove : BaseConverter, IValueConverter
    {
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