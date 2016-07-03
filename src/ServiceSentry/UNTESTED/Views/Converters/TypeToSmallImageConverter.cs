// -----------------------------------------------------------------------
//  <copyright file="TypeToSmallImageConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ServiceSentry.Extensibility.Logging;

#endregion

#region License

//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    [ValueConversion(typeof (string), typeof (ImageSource))]
    public class TypeToSmallImageConverter : IValueConverter
    {
        protected ImageQuality Quality = ImageQuality.BestAvailable;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var logLevel = (LogLevel)value;
            
            {
                var resourceKey = String.Format("LoggerImage{0}Key", logLevel.ToString());
                var bitmap = (BitmapImage) Application.Current.TryFindResource(resourceKey);
                return bitmap;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public static bool IsNullOrWhiteSpace(String value)
        {
            if (value == null) return true;

            for (var i = 0; i < value.Length; i++)
            {
                if (!Char.IsWhiteSpace(value[i])) return false;
            }

            return true;
        }
    }

    public enum ImageQuality
    {
        Small,
        Medium,
        Large,
        BestAvailable
    }
}