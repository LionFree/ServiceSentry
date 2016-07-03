// -----------------------------------------------------------------------
//  <copyright file="OptionsViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.Views.Controls;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class OptionsViewModel
    {
        public abstract ObservableCollection<TabItem> Tabs { get; set; }

        public abstract ICommand RefreshSettingsCommand { get; }
        public abstract ICommand CommitChangesCommand { get; }

        internal static OptionsViewModel GetInstance(ConfigFile file, Logger logger,
            ServiceHandler serviceHandler,
            GeneralOptionsViewModel generalOptionsViewModel,
            WardenOptionsViewModel wardenOptionsViewModel,
            NotificationOptionsViewModel notificationOptionsViewModel,
            DebugOptionsViewModel debugOptionsViewModel)
        {
            return new OptionsVMImplementation(logger, file, ConfigFileHandler.GetInstance(logger),
                serviceHandler,
                generalOptionsViewModel, 
                wardenOptionsViewModel,
                notificationOptionsViewModel,
                debugOptionsViewModel);
        }

        internal abstract void RefreshSettings();
        internal abstract void CommitChanges();

        internal abstract void PublishTabs(List<ImportedOptionsTabItem> extensionTabs);

        private sealed class OptionsVMImplementation : OptionsViewModel
        {
            private readonly DebugOptionsViewModel _debugOptionsViewModel;
            private readonly WardenOptionsViewModel _wardenOptionsViewModel;
            private readonly ConfigFile _file;
            private readonly ConfigFileHandler _fileHandler;
            private readonly GeneralOptionsViewModel _generalOptionsViewModel;
            private readonly Logger _logger;
            private readonly NotificationOptionsViewModel _notificationOptionsViewModel;
            private ICommand _commitChangesCommand;
            private List<ImportedOptionsTabItem> _extensions;
            private ICommand _refreshSettingsCommand;
            private readonly ServiceHandler _serviceHandler;


            internal OptionsVMImplementation(Logger logger, ConfigFile file, ConfigFileHandler fileHandler,
                ServiceHandler serviceHandler,
                GeneralOptionsViewModel generalOptionsViewModel,
                WardenOptionsViewModel wardenOptionsViewModel,
                NotificationOptionsViewModel notificationOptionsViewModel,
                DebugOptionsViewModel debugOptionsViewModel)
            {
                _logger = logger;
                _fileHandler = fileHandler;
                _file = file;
                _serviceHandler = serviceHandler;

                _generalOptionsViewModel = generalOptionsViewModel;
                _wardenOptionsViewModel = wardenOptionsViewModel;
                _notificationOptionsViewModel = notificationOptionsViewModel;
                _debugOptionsViewModel = debugOptionsViewModel;
                
            }

            public override ObservableCollection<TabItem> Tabs { get; set; }

            public override ICommand RefreshSettingsCommand
            {
                get
                {
                    return _refreshSettingsCommand ??
                           (_refreshSettingsCommand = new RelayCommand("RefreshSettings", param => RefreshSettings()));
                }
            }

            public override ICommand CommitChangesCommand
            {
                get
                {
                    return _commitChangesCommand ??
                           (_commitChangesCommand = new RelayCommand("CommitChanges", param => CommitChanges()));
                }
            }

            internal override void CommitChanges()
            {
                _generalOptionsViewModel.Commit(_file);
                _wardenOptionsViewModel.Commit(_file.WardenDetails);
                _notificationOptionsViewModel.Commit(_file.NotificationDetails);
                _debugOptionsViewModel.Commit(_file.DebugLogConfiguration);

                foreach (var item in _extensions)
                {
                    if (item.CanExecute)
                    {
                        item.CommitOptions();
                    }
                }

                _fileHandler.WriteConfigFile(_file);
                _serviceHandler.UpdateServices(_file.NotificationDetails, _file.WardenDetails);
                
            }

            internal override void RefreshSettings()
            {
                _generalOptionsViewModel.RefreshSettings(_file);
                _wardenOptionsViewModel.RefreshSettings(_file.WardenDetails);
                _notificationOptionsViewModel.RefreshSettings(_file.NotificationDetails);
                _debugOptionsViewModel.RefreshSettings(_file.DebugLogConfiguration);

                foreach (var item in _extensions.Where(item => item.CanExecute))
                {
                    item.RefreshOptionSettings();
                }
            }

            internal override void PublishTabs(List<ImportedOptionsTabItem> extensionTabs)
            {
                if (Tabs != null) return;

                _extensions = extensionTabs;

                Tabs = new ObservableCollection<TabItem>
                    {
                        GeneralOptionsTabHarness.GetInstance(_generalOptionsViewModel).GetTabItem(),
                        WardenOptionsTabHarness.GetInstance(_wardenOptionsViewModel).GetTabItem(),
                        NotificationOptionsTabHarness.GetInstance(_notificationOptionsViewModel).GetTabItem(),
                        DebugOptionsTabHarness.GetInstance(_debugOptionsViewModel).GetTabItem(),
                    };

                PublishExtensionTabs(extensionTabs);
            }

            /// <summary>
            ///     Publish the extensions into the view.
            /// </summary>
            private void PublishExtensionTabs(List<ImportedOptionsTabItem> extensionTabs)
            {
                // Move this to app controller?

                _logger.Trace(Strings.Debug_PublishingExtensionsToOptions);

                var tabStyle = Application.Current.FindResource("TabItemStyle") as Style;

                // Get the list of extensions controls.
                var list = extensionTabs;

                // Check if there are any extension Controls.
                if (list == null || list.Count == 0)
                {
                    _logger.Debug(Strings.Debug_NothingToPublish);
                    return;
                }

                if (list.Count == 0) return;

                // publish the tab extensions into the options ShellWindow.
                foreach (var item in list)
                {
                    if (!item.CanExecute) continue;
                    item.OptionTabItem.Style = tabStyle;
                    Tabs.Add(item.OptionTabItem);
                }
            }
        }
    }
}