// -- FILE ------------------------------------------------------------------
// name       : FixedColumn.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------

#region References

using System.Windows;
using System.Windows.Controls;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public sealed class FixedColumn : LayoutColumn
    {
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached(
                "Width",
                typeof (double),
                typeof (FixedColumn));

        private FixedColumn()
        {
        }

        public static double GetWidth(DependencyObject obj)
        {
            return (double) obj.GetValue(WidthProperty);
        }

        public static void SetWidth(DependencyObject obj, double width)
        {
            obj.SetValue(WidthProperty, width);
        }

        public static bool IsFixedColumn(GridViewColumn column)
        {
            if (column == null)
            {
                return false;
            }
            return HasPropertyValue(column, WidthProperty);
        }

        public static double? GetFixedWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, WidthProperty);
        }

        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double width)
        {
            SetWidth(gridViewColumn, width);
            return gridViewColumn;
        }
    }
}