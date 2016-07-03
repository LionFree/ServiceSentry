// -----------------------------------------------------------------------
//  <copyright file="HasValueConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    public class HasValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null) return false;
            var list = values[0] as ListView;

            var param = parameter as string;
            if (param == null) return (values[1] != null);

            // Convert the parameter into a boolean (param = ListView Must Have Items).
            bool mustHaveItems;
            if (!Boolean.TryParse(param, out mustHaveItems) || mustHaveItems)
            {
                //if (values[1] == null) return false;
                return list != null && list.Items.Count != 0;
            }

            if (values.Length <= 2) return true;

            if (values[2] == DependencyProperty.UnsetValue) return false;
            return (bool) values[2];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}