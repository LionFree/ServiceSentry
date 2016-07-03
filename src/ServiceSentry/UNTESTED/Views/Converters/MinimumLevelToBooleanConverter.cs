// -----------------------------------------------------------------------
//  <copyright file="MinimumLevelToBooleanConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows.Data;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    public class MinimumLevelToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var minLevel = (LogLevel)value;
            
            var levelString = parameter as string;
            if (levelString == null) throw new ArgumentException("parameter must be a string.");

            var level = (LogLevel) Enum.Parse(typeof (LogLevel), levelString);

            var isChecked = (minLevel <= level);
            return isChecked;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}