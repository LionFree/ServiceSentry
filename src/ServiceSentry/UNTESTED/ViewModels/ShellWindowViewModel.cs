// -----------------------------------------------------------------------
//  <copyright file="ShellWindowViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ServiceSentry.Client.UNTESTED.ViewModels.Commands;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class ShellWindowViewModel : PropertyChangedBase
    {
        internal static ShellWindowViewModel GetInstance(Logger logger, ServiceList services,
                                                         ShellCommands shellCommands)
        {
            return new ShellWindowViewModelImplementation(logger, services, shellCommands);
        }

        #region Abstract Methods

        public abstract ObservableCollection<TabItem> Tabs { get; set; }
        public abstract int SelectedTabIndex { get; set; }
        internal abstract ServiceList ServiceList { get; }

        public abstract ShellCommands ShellCommands { get; }
        public abstract Style TabStyle { get; }
        internal abstract void PublishTabs(TabItem[] tabs, List<ImportedTabItem> extensionTabs);

        #endregion

        private sealed class ShellWindowViewModelImplementation : ShellWindowViewModel
        {
            private readonly Logger _logger;
            private readonly ServiceList _services;
            private readonly ShellCommands _shellCommands;

            internal ShellWindowViewModelImplementation(Logger logger, ServiceList services, ShellCommands shellCommands)
            {
                _logger = logger;
                _services = services;
                _shellCommands = shellCommands;
            }

            public override ObservableCollection<TabItem> Tabs { get; set; }

            public override int SelectedTabIndex { get; set; }

            internal override ServiceList ServiceList
            {
                get { return _services; }
            }

            public override ShellCommands ShellCommands
            {
                get { return _shellCommands; }
            }

            public override Style TabStyle
            {
                get { return Application.Current.TryFindResource("TabItemStyle") as Style; }
            }

            internal override void PublishTabs(TabItem[] tabs, List<ImportedTabItem> extensionTabs)
            {
                if (Tabs != null) return;

                Tabs = new ObservableCollection<TabItem>();

                foreach (var item in tabs)
                {
                    item.Style = TabStyle;
                    item.IsEnabled = true;
                    Tabs.Add(item);
                }

                PublishExtensionTabs(extensionTabs);
            }

            /// <summary>
            ///     Publish the extensions into the view.
            /// </summary>
            private void PublishExtensionTabs(List<ImportedTabItem> extensionTabs)
            {
                _logger.Trace(Strings.Debug_PublishingExtensionsToMainWindow);

                // Get the list of extensions controls.
                var list = extensionTabs;

                // Check if there are any extension Controls.
                if (list == null || list.Count == 0)
                {
                    _logger.Debug(Strings.Debug_NothingToPublish);
                    return;
                }

                if (list.Count == 0) return;

                // publish the tab extensions into the main ShellWindow.
                foreach (var item in list)
                {
                    if (!item.CanExecute) continue;
                    item.ExtensionTabItem.Style = TabStyle;
                    Tabs.Add(item.ExtensionTabItem);
                }
            }
        }
    }
}