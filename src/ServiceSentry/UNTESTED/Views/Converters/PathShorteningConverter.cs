// -----------------------------------------------------------------------
//  <copyright file="PathShorteningConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    internal class PathShorteningMultiConverter : IMultiValueConverter
    {
        internal static double MinWidth = 200;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var path = values[0] as string;
            if (path == null) return string.Empty;

            var minWidth = (parameter as double?) ?? MinWidth;
            var width = (values[1] as double?) ?? minWidth;
            
            var control = values[2] as TextBlock;
            if (control == null) return string.Empty;

            var output = PathShortener.GetInstance(EllipsisFormat.Start).Compact(path, control, width);
            return output;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}