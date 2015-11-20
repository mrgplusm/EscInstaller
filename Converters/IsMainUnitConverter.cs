#region

using System;
using System.Globalization;
using System.Windows.Data;
using Common.Commodules;
using Common.Converters;
using Common.Model;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.Converters
{
    public class BroadCastMessageAlarm1Converter : BaseConverter, IValueConverter
    {



        public BroadCastMessageAlarm1Converter() { }
        #region IValueConverter Members

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((BroadCastMessage)value) == BroadCastMessage.Alarm1;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            var input = (bool) value;
            return (input) ? BroadCastMessage.Alarm1 : BroadCastMessage.None;            
        }

        #endregion
    }

    public class BroadCastMessageAlarm2Converter : BaseConverter, IValueConverter
    {



        public BroadCastMessageAlarm2Converter() { }
        #region IValueConverter Members

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((BroadCastMessage)value) == BroadCastMessage.Alarm2;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            var input = (bool)value;
            return (input) ? BroadCastMessage.Alarm2 : BroadCastMessage.None;
        }

        #endregion
    }

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