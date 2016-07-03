// -----------------------------------------------------------------------
//  <copyright file="ServicesTabHarness.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Extensibility.Controls;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Controls
{
    /// <summary>
    ///     Interaction logic for ServicesTabHarness.xaml
    /// </summary>
    public abstract partial class ServicesTabHarness
    {
        internal static ServicesTabHarness GetInstance(ServicesViewModel viewModel)
        {
            return new ServicesTabHarnessImplementation(viewModel);
        }


        internal abstract void OnRemoveItemClick(object sender, RoutedEventArgs e);

        #region Abstract Members

        protected abstract void Link_MouseDown(object sender, MouseButtonEventArgs e);
        protected abstract void Link_MouseMove(object sender, MouseEventArgs e);
        protected abstract void ArchiveLogs_OnClick(object sender, RoutedEventArgs e);
        protected abstract void GridViewColumnHeaderClick(object sender, RoutedEventArgs e);
        protected abstract void LogFileHyperlink_OnClick(object sender, RoutedEventArgs e);
        protected abstract void Folder_Link_OnClick(object sender, RoutedEventArgs e);
        protected abstract void LogFileContextMenu_OnClick(object sender, RoutedEventArgs e);
        protected abstract void IgnoreLogs_OnClick(object sender, RoutedEventArgs e);
        protected abstract void ClearLogs_OnClick(object sender, RoutedEventArgs e);
        protected abstract void OnStretchedHeaderTemplateLoaded(object sender, RoutedEventArgs e);
        protected abstract void AddGroupsToListView(IEnumerable items, string groupName);
        protected abstract bool GroupIsAlreadyLoaded(ICollectionView view, string groupName);

        internal abstract TabItem GetTabItem();

        #region Commands

        protected abstract void StartServiceCommand(object sender, ExecutedRoutedEventArgs e);
        protected abstract void RestartServiceCommand(object sender, ExecutedRoutedEventArgs e);
        protected abstract void StopServiceCommand(object sender, ExecutedRoutedEventArgs e);

        protected abstract void RestartServiceGroupCommand(object sender, ExecutedRoutedEventArgs e);
        protected abstract void StartServiceGroupCommand(object sender, ExecutedRoutedEventArgs e);
        protected abstract void StopServiceGroupCommand(object sender, ExecutedRoutedEventArgs e);

        protected abstract void CanToggleServices(object sender, CanExecuteRoutedEventArgs e);

        #endregion

        #endregion

        internal sealed class ServicesTabHarnessImplementation : ServicesTabHarness
        {
            #region Constructor

            private readonly ServicesViewModel _viewModel;

            internal ServicesTabHarnessImplementation(ServicesViewModel viewModel)
            {
                _viewModel = viewModel;
                InitializeComponent();
                ServicesTab.DataContext = _viewModel;

                ServicesListView.SelectionChanged += _viewModel.SelectionChanged;
                ServicesListView.ContextMenu.Closed += _viewModel.ContextMenuClosed;

                AddGroupsToListView(_viewModel.Services.Items, "ServiceGroup");
                ServicesListView.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(ServicesTab.Thumb_DragDelta),
                                            true);
                //ServicesTab.ResizeColumns(OtherLogsGridView);
            }

            protected override void OnStretchedHeaderTemplateLoaded(object sender, RoutedEventArgs e)
            {
                // Makes the service group borders stretch across the whole listbox.

                var rootElem = sender as Border;

                if (rootElem == null) return;
                var contentPres = rootElem.TemplatedParent as ContentPresenter;

                if (contentPres == null) return;
                contentPres.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            #endregion

            #region Commands

            protected override void StartServiceCommand(object sender,
                                                        ExecutedRoutedEventArgs e)
            {
                _viewModel.ServiceCommands.StartService(e.Parameter);
            }

            protected override void RestartServiceCommand(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel.ServiceCommands.RestartService(e.Parameter);
            }

            protected override void StopServiceCommand(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel.ServiceCommands.StopService(e.Parameter);
            }

            protected override void RestartServiceGroupCommand(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel.ServiceCommands.RestartServiceGroup(e.Parameter);
            }

            protected override void StartServiceGroupCommand(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel.ServiceCommands.StartServiceGroup(e.Parameter);
            }

            protected override void StopServiceGroupCommand(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel.ServiceCommands.StopServiceGroup(e.Parameter);
            }

            protected override void LogFileHyperlink_OnClick(object sender, RoutedEventArgs e)
            {
                _viewModel.FileCommands.OpenFileLink(e.OriginalSource);
            }

            protected override void Folder_Link_OnClick(object sender, RoutedEventArgs e)
            {
                _viewModel.FileCommands.OpenFolderPath(e.OriginalSource);
            }

            protected override void LogFileContextMenu_OnClick(object sender, RoutedEventArgs e)
            {
                _viewModel.FileCommands.OpenFilePath(e.OriginalSource);
            }

            protected override void CanToggleServices(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel.ServiceCommands.EnableServiceButtons;
            }

            #endregion

            #region GridView Handling

            protected override void AddGroupsToListView(IEnumerable items, string groupName)
            {
                // Set the items list view source.
                ServicesListView.ItemsSource = items;


                // Add service groups to the datagrid.
                var dataView = CollectionViewSource.GetDefaultView(ServicesListView.ItemsSource);

                if (GroupIsAlreadyLoaded(dataView, groupName)) return;

                dataView.GroupDescriptions.Add(new PropertyGroupDescription(groupName));
            }

            protected override bool GroupIsAlreadyLoaded(ICollectionView view, string groupName)
            {
                if (view == null) return false;

                var groups = view.GroupDescriptions;
                if (groups == null) return false;

                foreach (PropertyGroupDescription item in groups)
                {
                    if (item.PropertyName == groupName) return true;
                }
                return false;
            }

            #endregion

            #region Click Handling

            protected override void GridViewColumnHeaderClick(object sender, RoutedEventArgs e)
            {
                ServicesTab.GridViewColumnHeaderClicked(sender, e);
            }

            protected override void IgnoreLogs_OnClick(object sender, RoutedEventArgs e)
            {
                if (IgnoreLogs.IsChecked == false) return;

                ArchiveLogs.IsChecked = false;
                ClearLogs.IsChecked = false;
            }

            protected override void ClearLogs_OnClick(object sender, RoutedEventArgs e)
            {
                if (ClearLogs.IsChecked == true)
                    IgnoreLogs.IsChecked = false;

                if (ArchiveLogs.IsChecked == false && ClearLogs.IsChecked == false)
                    IgnoreLogs.IsChecked = true;
            }

            protected override void ArchiveLogs_OnClick(object sender, RoutedEventArgs e)
            {
                if (ArchiveLogs.IsChecked == true)
                    IgnoreLogs.IsChecked = false;

                if (ArchiveLogs.IsChecked == false && ClearLogs.IsChecked == false)
                    IgnoreLogs.IsChecked = true;
            }

            #endregion

            #region Link Drag-n-Drop

            private Point _startPoint;

            private bool IsDragging { get; set; }

            protected override void Link_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton != MouseButtonState.Pressed || IsDragging) return;
                var position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDrag(sender);
                }
            }

            internal override TabItem GetTabItem()
            {
                var tab = ServicesTab;
                RootTabs.Items.Remove(tab);
                return tab;
            }

            protected override void Link_MouseDown(object sender, MouseButtonEventArgs e)
            {
                _startPoint = e.GetPosition(null);
            }

            private void StartDrag(object sender)
            {
                IsDragging = true;
                var link = sender as Hyperlink;
                if (link == null) return;

                var filePath = link.NavigateUri.LocalPath;

                if (File.Exists(filePath))
                {
                    string[] array = {filePath};
                    var data = new DataObject(DataFormats.FileDrop, array);
                    try
                    {
                        DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("EXCEPTION " + ex.Message);
                    }
                }
                IsDragging = false;
            }

            #endregion

            internal override void OnRemoveItemClick(object sender, RoutedEventArgs e)
            {
                var item = sender as Button;
                if (item == null) return;

                _viewModel.RemoveFile(item.CommandParameter);
            }
        }
    }
}