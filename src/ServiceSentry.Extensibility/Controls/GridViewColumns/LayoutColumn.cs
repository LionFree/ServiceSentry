// -- FILE ------------------------------------------------------------------
// name       : LayoutColumn.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------

#region References

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public abstract class LayoutColumn
    {
        protected static bool HasPropertyValue(GridViewColumn column, DependencyProperty dp)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }
            var value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return true;
            }

            return false;
        }

        protected static double? GetColumnWidth(GridViewColumn column, DependencyProperty dp)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }
            var value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return (double) value;
            }

            return null;
        }
    }
}