// -----------------------------------------------------------------------
//  <copyright file="ComboBoxWidthConverter.cs" company="Curtis Kaler">
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

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    internal class ComboBoxWidthConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var control = value as ComboBox;
            if (control == null) throw new ArgumentException("value must be a ComboBox.");

            var maxWidth = 0.0;
            foreach (ComboBoxItem item in control.Items)
            {
                var formattedText = new FormattedText(
                    item.Content.ToString(),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(control.FontFamily, control.FontStyle, control.FontWeight,
                                 control.FontStretch),
                    control.FontSize,
                    Brushes.Black);

                var width = formattedText.Width + 25;
                if (width > maxWidth) maxWidth = width;
            }
            return maxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}