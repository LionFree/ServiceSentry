// -----------------------------------------------------------------------
//  <copyright file="ISortingGridView.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
//using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public interface ISortingGridView
    {
        ListSortDirection LastDirection { get; set; }
        GridViewColumnHeader LastHeaderClicked { get; set; }
    }

    public static class SortingGridView
    {
        public static void GridViewColumnHeaderClicked(this ISortingGridView obj, object sender, RoutedEventArgs e)
        {
            //Contract.Requires(e != null);

            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked == null) return;
            var lv = e.Source as ListView;

            if (headerClicked.Role == GridViewColumnHeaderRole.Padding) return;

            ListSortDirection direction;

            if (!Equals(headerClicked, obj.LastHeaderClicked))
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                direction = obj.LastDirection == ListSortDirection.Ascending
                                ? ListSortDirection.Descending
                                : ListSortDirection.Ascending;
            }

            string bindingToSort;
            var dmb = headerClicked.Column.DisplayMemberBinding as Binding;
            if (dmb != null)
            {
                bindingToSort = dmb.Path.Path;
            }
            else
            {
                bindingToSort = headerClicked.Tag as String;
            }

            var success = Sort(lv, bindingToSort, direction);
            if (!success) return;

            // Add arrow to header
            if (direction == ListSortDirection.Ascending)
            {
                headerClicked.Column.HeaderTemplate =
                    ((FrameworkElement) obj).Resources["HeaderTemplateArrowUp"] as DataTemplate;
            }
            else
            {
                headerClicked.Column.HeaderTemplate =
                    ((FrameworkElement) obj).Resources["HeaderTemplateArrowDown"] as DataTemplate;
            }

            // Remove arrow from previously sorted header 
            if (obj.LastHeaderClicked != null && !Equals(obj.LastHeaderClicked, headerClicked))
            {
                obj.LastHeaderClicked.Column.HeaderTemplate = null;
            }

            obj.LastHeaderClicked = headerClicked;
            obj.LastDirection = direction;
        }

        public static void Thumb_DragDelta(this ISortingGridView obj, object sender, DragDeltaEventArgs e)
        {
            //Contract.Requires(e != null);

            var senderAsThumb = e.OriginalSource as Thumb;
            if (senderAsThumb == null) return;

            var header = senderAsThumb.TemplatedParent as GridViewColumnHeader;
            if (header == null) return;

            header.Column.Width = double.NaN;
        }

        //public static void ResizeColumns(this ISortingGridView obj, GridView gv)
        //{
        //    //Contract.Requires(gv != null);
        //    //Contract.Requires(gv.Columns != null);

        //    foreach (var c in gv.Columns)
        //    {
        //        var bnd = BindingOperations.GetBinding(c, ColumnVisibility.IsVisibleProperty);
        //        if (bnd != null)
        //        {
        //            var path = bnd.Path.Path;

        //            var rawSetting = Settings.Default[path];

        //            if (rawSetting != null)
        //            {
        //                var setting = (bool) rawSetting;
        //                if (setting == false) return;
        //            }
        //        }
               
        //        if (double.IsNaN(c.Width))
        //        {
        //            c.Width = c.ActualWidth;
        //        }
        //        c.Width = double.NaN;
        //    }
        //}

        private static bool Sort(ItemsControl lv, string sortBy, ListSortDirection direction)
        {
            //Contract.Requires(lv != null);

            if (string.IsNullOrEmpty(sortBy)) return false;
            var dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);
            if (dataView == null) return false;

            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
            return true;
        }
    }
}