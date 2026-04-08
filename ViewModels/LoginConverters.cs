using System;
using System.Globalization;
using System.Windows.Data;

namespace NewAPP.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (bool)value ? "ВХОД..." : "ВОЙТИ";
        }

        public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToEnableConverter : IValueConverter
    {
        public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return !(bool)value;
        }

        public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
