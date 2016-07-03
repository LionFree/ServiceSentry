// -----------------------------------------------------------------------
//  <copyright file="LogFileParameterConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows.Data;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    public class LogFileCommandParameter
    {
        public ExternalFile LogFile { get; set; }
        public string Type { get; set; }
    }


    public class LogFileParameterConverter : IValueConverter
    {
        // Return a single object containing both 
        //  the externalfile from which the context menu is sprung, and
        //  a parameter (the type of log file).
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var param = new LogFileCommandParameter
                {
                    LogFile = (ExternalFile) value,
                    Type = (string) parameter
                };
            return param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}