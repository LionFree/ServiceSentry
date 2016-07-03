// -----------------------------------------------------------------------
//  <copyright file="ServiceParametersView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for ServiceParametersView.xaml
    /// </summary>
    public abstract partial class ServiceParametersView
    {
        internal static ServiceParametersView GetInstance(Logger logger, ServiceParametersViewModel viewModel)
        {
            return new ServiceParametersViewImplementation(logger, viewModel, SPVHelper.GetInstance(logger));
        }

        internal abstract void OnButtonClick(object sender, RoutedEventArgs e);
        internal abstract void OnOkClick(object sender, RoutedEventArgs e);
        internal abstract void OnRemoveItemClick(object sender, RoutedEventArgs e);

        private sealed class ServiceParametersViewImplementation : ServiceParametersView
        {
            private readonly SPVHelper _helper;
            private readonly Logger _logger;

            internal ServiceParametersViewImplementation(Logger logger, ServiceParametersViewModel viewModel, SPVHelper helper)
            {
                _logger = logger;
                _helper = helper;
                DataContext = viewModel;
                InitializeComponent();
            }

            internal override void OnOkClick(object sender, RoutedEventArgs e)
            {
                _helper.UpdateSources(ConfigFileListView, CommonConfigNames);
                _helper.UpdateSources(LogFileListView, CommonLogNames);

                Close();
            }

            internal override void OnButtonClick(object sender, RoutedEventArgs e)
            {
                Close();
            }

            internal override void OnRemoveItemClick(object sender, RoutedEventArgs e)
            {
                var vm = DataContext as ServiceParametersViewModel;
                var item = sender as Button;
                if (vm == null || item == null) return;

                var lview = _helper.FindVisualParent<ListView>(item);
                var tag = lview.Tag as string;
                vm.RemoveFile(item.CommandParameter, tag);
            }
        }
    }

    internal abstract class SPVHelper
    {
        internal static SPVHelper GetInstance(Logger logger)
        {
            return new SPVHelperImplementation(logger);
        }

        internal abstract void UpdateSources(ListView listView, GridViewColumn column);
        internal abstract T GetFrameworkElementByName<T>(FrameworkElement referenceElement) where T : FrameworkElement;
        internal abstract T FindVisualParent<T>(FrameworkElement element) where T : FrameworkElement;

        private sealed class SPVHelperImplementation : SPVHelper
        {
            private readonly Logger _logger;

            internal SPVHelperImplementation(Logger logger)
            {
                _logger = logger;
            }

            internal override void UpdateSources(ListView listView, GridViewColumn column)
            {
                var gridView = listView.View as GridView;
                if (gridView == null) return;

                foreach (var item in listView.Items)
                {
                    var lvi = listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                    var rowPresenter = GetFrameworkElementByName<GridViewRowPresenter>(lvi);

                    if (rowPresenter != null)
                    {
                        var templatedParent =
                            VisualTreeHelper.GetChild(rowPresenter, 0) as ContentPresenter;

                        var template = column.CellTemplate;

                        if (template != null && templatedParent != null)
                        {
                            var textBox = template.FindName("FileCommonName", templatedParent) as TextBox;
                            if (textBox != null)
                            {
                                var be = textBox.GetBindingExpression(TextBox.TextProperty);
                                be.UpdateSource();
                            }
                        }
                    }
                }
            }

            internal override T GetFrameworkElementByName<T>(FrameworkElement referenceElement)
            {
                if (referenceElement == null)
                {
                    _logger.Error("GetFrameWorkElementByName<T> was passed a null referenceElement.  Type T: {0}",
                                  typeof (T).Name);
                    return null;
                }

                FrameworkElement child = null;
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
                {
                    child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                    if (child != null && child.GetType() == typeof (T))
                    {
                        break;
                    }

                    if (child == null) continue;

                    child = GetFrameworkElementByName<T>(child);
                    if (child != null && child.GetType() == typeof (T))
                    {
                        break;
                    }
                }
                return child as T;
            }

            internal override T FindVisualParent<T>(FrameworkElement element)
            {
                var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
                while (parent != null)
                {
                    var correctlyTyped = parent as T;
                    return correctlyTyped ?? FindVisualParent<T>(parent);
                }
                return null;
            }
        }
    }
}