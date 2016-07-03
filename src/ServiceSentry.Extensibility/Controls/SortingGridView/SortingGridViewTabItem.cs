// -----------------------------------------------------------------------
//  <copyright file="SortingGridViewTabItem.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ComponentModel;
using System.Windows.Controls;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public class SortingGridViewTabItem : TabItem, ISortingGridView
    {
        public SortingGridViewTabItem()
        {
            LastDirection=ListSortDirection.Ascending;
        }

        public ListSortDirection LastDirection { get; set; }
        public GridViewColumnHeader LastHeaderClicked { get; set; }
    }
}