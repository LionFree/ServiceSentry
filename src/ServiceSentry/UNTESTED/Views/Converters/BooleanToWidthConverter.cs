#region References

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    public class BooleanToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? double.Parse(parameter.ToString()) : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}