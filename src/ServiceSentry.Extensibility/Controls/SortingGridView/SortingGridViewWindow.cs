using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ServiceSentry.Extensibility.Controls
{
    public class SortingGridViewWindow : Window, ISortingGridView
    {
        public SortingGridViewWindow()
        {
            LastDirection = ListSortDirection.Ascending;
        }

        public ListSortDirection LastDirection { get; set; }
        public GridViewColumnHeader LastHeaderClicked { get; set; }

        protected void OnGridColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            this.GridViewColumnHeaderClicked(sender, e);
        }
    }
}