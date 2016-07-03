﻿using System;
using System.Windows.Data;

namespace ServiceSentry.Extensibility.Controls
{
  public class InverseBoolConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      return !( bool )value;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotSupportedException();
    }
  }
}
