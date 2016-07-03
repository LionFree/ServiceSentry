// -----------------------------------------------------------------------
//  <copyright file="ColumnWidthConverter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Converters
{
    internal class ColumnWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var list = values[0] as ListView;
            var column = values[1] as GridViewColumn;
            if (list==null) throw new ArgumentException("First value must be the parent ListView.", "values");
            
            var grid = list.View as GridView;
            if (grid == null) throw new ArgumentException("ListView.View must be a GridView.", "values");

            double total = 0;
            foreach (var col in grid.Columns)
            {
                if (col.Equals(column))
                {
                    continue;
                }
                total += col.Width;
            }
            
            return (list.ActualWidth - total);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}