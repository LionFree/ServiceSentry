// -----------------------------------------------------------------------
//  <copyright file="LogLevelWidthConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    internal class LogLevelWidthConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var control = value as Control;
            if (control == null) throw new ArgumentException("value must be a Control.");

            var maxWidth = 0.0;
            
            foreach (var item in Enum.GetValues(typeof(LogLevel)))
            {
                var formattedText = new FormattedText(
                    item.ToString(),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(control.FontFamily, control.FontStyle, control.FontWeight,
                                 control.FontStretch),
                    control.FontSize,
                    Brushes.Black);

                var width = formattedText.Width + 35;
                if (width > maxWidth) maxWidth = width;
            }
            return maxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}