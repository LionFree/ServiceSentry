// -----------------------------------------------------------------------
//  <copyright file="ServiceParameter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    public class ServiceCommandParameter
    {
        public object Control { get; set; }
        public object Parameter { get; set; }
    }


    public class ServiceParameterConverter : IMultiValueConverter
    {
        // Return a single object containing both 
        //  the control from which the context menu is sprung, and
        //  a parameter.

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values[0] == null || values[1] == null) return null;

            if (values[0] == DependencyProperty.UnsetValue) return null;

            var param = new ServiceCommandParameter
            {
                Control = values[0],
                Parameter = values[1]
            };
            return param;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}