// -----------------------------------------------------------------------
//  <copyright file="Visibility.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public sealed class ColumnVisibility : LayoutColumn
    {
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof (bool), typeof (ColumnVisibility),
                                                new UIPropertyMetadata(true, OnIsVisibleChanged));

        private ColumnVisibility()
        {
        }

        public static bool GetIsVisible(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsVisibleProperty);
        }

        public static void SetIsVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisibleProperty, value);
        }

        public static bool IsVisibilityColumn(GridViewColumn column)
        {
            return column != null && HasPropertyValue(column, IsVisibleProperty);
        }

        private static void OnIsVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var gc = sender as GridViewColumn;
            if (gc == null) return;

            if (GetIsVisible(gc) == false)
            {
                gc.Width = 0;
            }
            else
            {
                if (Math.Abs(gc.Width - 0) < Double.Epsilon)
                {
                    if (double.IsNaN(gc.Width))
                    {
                        gc.Width = gc.ActualWidth;
                    }
                    gc.Width = double.NaN;
                }
            }
        }
    }
}