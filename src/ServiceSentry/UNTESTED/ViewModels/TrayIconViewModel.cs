// -----------------------------------------------------------------------
//  <copyright file="TrayIconViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.ViewModels.Commands;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Client;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class TrayIconViewModel : PropertyChangedBase
    {
        internal static TrayIconViewModel GetInstance(Logger logger,
            ApplicationState applicationState,
            ShellCommands shellCommands,
            ServiceTogglingCommands serviceCommands,
            MonitorServiceWatchdog watchdog)
        {
            return GetInstance(applicationState, shellCommands, serviceCommands,
                watchdog, Publisher.GetInstance(logger, serviceCommands));
        }


        internal static TrayIconViewModel GetInstance(ApplicationState applicationState,
            ShellCommands shellCommands,
            ServiceTogglingCommands serviceCommands,
            MonitorServiceWatchdog watchdog,
            Publisher publisher)
        {
            return new TrayIconVMImplementation(applicationState, shellCommands, serviceCommands,
                watchdog, publisher);
        }

        #region Abstract members

        public abstract ObservableCollection<Control> MenuItems { get; set; }
        public abstract ContextMenu ContextMenu { get; set; }
        public abstract bool AreServiceGroupsVisible { get; set; }
        public abstract bool AreOtherFilesVisible { get; set; }
        public abstract bool IsMonitorServiceAvailable { get; }
        public abstract string TrayIconTooltipText { get; }
        public abstract ServiceList Services { get; }

        public abstract ServiceTogglingCommands ToggleCommands { get; }
        public abstract ShellCommands ShellCommands { get; }
        public abstract Visibility ServiceGroupVisibility { get; }
        public abstract Visibility OtherFilesVisibility { get; }

        public abstract BitmapImage NotificationAreaIcon { get; }

        internal abstract void PublishAll(List<ImportedContextMenu> importedContextMenus);

        protected abstract void OnAvailabilityChanged(object sender, AvailabilityChangedEventArgs e);

        #endregion

        private sealed class TrayIconVMImplementation : TrayIconViewModel
        {
            #region Fields

            private static BitmapImage _monitorUnavailableImage;
            private static BitmapImage _monitorAvailableImage;
            private readonly ApplicationState _applicationState;
            private readonly Publisher _publisher;
            private readonly ServiceTogglingCommands _serviceCommands;
            private readonly ShellCommands _shellCommands;
            private bool _areOtherFilesVisible;
            private bool _areServiceGroupsVisible;
            private ContextMenu _contextMenu;
            private bool _isAvailable;

            #endregion

            internal TrayIconVMImplementation(ApplicationState applicationState,
                ShellCommands shellCommands,
                ServiceTogglingCommands serviceCommands,
                MonitorServiceWatchdog watchdog,
                Publisher publisher)
            {
                _applicationState = applicationState;
                _shellCommands = shellCommands;
                _serviceCommands = serviceCommands;
                _publisher = publisher;

                _isAvailable = watchdog.IsAvailable;
                watchdog.AvailabilityChanged += OnAvailabilityChanged;

                _applicationState.UpdateContextMenu += OnUpdateContextMenu;

                MenuItems = new ObservableCollection<Control>();
                AreServiceGroupsVisible = false;

                _monitorAvailableImage = (BitmapImage) Application.Current.TryFindResource("ApplicationIcon");
                _monitorUnavailableImage = (BitmapImage) Application.Current.TryFindResource("MonitorNotAvailableIcon");
            }

            private void OnUpdateContextMenu(object sender, EventArgs e)
            {
                _publisher.UpdateTrayMenu(MenuItems, _applicationState.ServiceGroups, _applicationState.LogList.Items);
            }

            protected override void OnAvailabilityChanged(object sender, AvailabilityChangedEventArgs e)
            {
                _isAvailable = e.Availability;
                //Trace.WriteLine("Monitor service is available: " + _isAvailable);
                OnPropertyChanged("IsMonitorServiceAvailable");
                OnPropertyChanged("NotificationAreaIcon");
                OnPropertyChanged("TrayIconTooltipText");
            }

            #region Properties

            public override ShellCommands ShellCommands
            {
                get { return _shellCommands; }
            }

            public override ServiceTogglingCommands ToggleCommands
            {
                get { return _serviceCommands; }
            }

            public override ObservableCollection<Control> MenuItems { get; set; }

            public override ContextMenu ContextMenu
            {
                get { return _contextMenu; }
                set
                {
                    if (Equals(_contextMenu, value)) return;
                    _contextMenu = value;
                    OnPropertyChanged("ContextMenu");
                }
            }

            public override bool AreServiceGroupsVisible
            {
                get { return _areServiceGroupsVisible; }
                set
                {
                    if (_areServiceGroupsVisible == value) return;
                    _areServiceGroupsVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ServiceGroupVisibility");
                }
            }

            public override bool AreOtherFilesVisible
            {
                get { return _areOtherFilesVisible; }
                set
                {
                    if (_areOtherFilesVisible == value) return;
                    _areOtherFilesVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged("OtherFilesVisibility");
                }
            }

            public override BitmapImage NotificationAreaIcon
            {
                get { return _isAvailable ? _monitorAvailableImage : _monitorUnavailableImage; }
            }

            public override bool IsMonitorServiceAvailable
            {
                get { return _isAvailable; }
            }

            public override string TrayIconTooltipText
            {
                get
                {
                    return IsMonitorServiceAvailable
                        ? Strings._ApplicationName
                        : Strings.Warn_MonitorServiceNotAvailable;
                }
            }

            public override ServiceList Services
            {
                get { return _applicationState.Services; }
            }

            #endregion

            #region Methods

            public override Visibility ServiceGroupVisibility
            {
                get { return AreServiceGroupsVisible ? Visibility.Visible : Visibility.Collapsed; }
            }

            public override Visibility OtherFilesVisibility
            {
                get { return AreOtherFilesVisible ? Visibility.Visible : Visibility.Collapsed; }
            }

            internal override void PublishAll(List<ImportedContextMenu> extensions)
            {
                _publisher.PublishTrayMenu(MenuItems, _applicationState.ServiceGroups, _applicationState.LogList.Items,
                    extensions);
                AreServiceGroupsVisible = _publisher.AreServiceGroupsVisible;
                AreOtherFilesVisible = _publisher.AreOtherFilesVisible;
            }

            #endregion
        }
    }

    internal abstract class Publisher
    {
        internal abstract bool AreOtherFilesVisible { get; }
        internal abstract bool AreServiceGroupsVisible { get; }

        internal static Publisher GetInstance(Logger logger, ServiceTogglingCommands commands)
        {
            return GetInstance(MainPublisher.GetInstance(logger, commands));
        }

        internal static Publisher GetInstance(MainPublisher publisher)
        {
            return new PublisherImplementation(publisher);
        }


        internal abstract void PublishTrayMenu(ObservableCollection<Control> menuItems,
            ObservableCollection<ServiceGroup> groups,
            ObservableCollection<ExternalFile> files,
            List<ImportedContextMenu> importedContextMenus);

        internal abstract void UpdateTrayMenu(ObservableCollection<Control> menuItems,
            ObservableCollection<ServiceGroup> groups,
            ObservableCollection<ExternalFile> files);

        private sealed class PublisherImplementation : Publisher
        {
            private readonly MainPublisher _publisher;
            private bool _areExtensionsVisible;
            private bool _areOtherFilesVisible;
            private bool _areServiceGroupsVisible;

            internal PublisherImplementation(MainPublisher publisher)
            {
                _publisher = publisher;
            }

            internal override bool AreOtherFilesVisible
            {
                get { return _areOtherFilesVisible; }
            }

            internal override bool AreServiceGroupsVisible
            {
                get { return _areServiceGroupsVisible; }
            }

            internal override void PublishTrayMenu(ObservableCollection<Control> menuItems,
                ObservableCollection<ServiceGroup> groups,
                ObservableCollection<ExternalFile> files,
                List<ImportedContextMenu> extensions)
            {
                _publisher.PublishBasicItems(menuItems);
                _publisher.PublishServices(groups, menuItems);
                _publisher.PublishExtensions(extensions, menuItems);
                _publisher.PublishOtherFiles(files, menuItems);

                _areOtherFilesVisible = (files != null && files.Count > 0);
                _areServiceGroupsVisible = (groups != null && groups.Count > 0);
                _areExtensionsVisible = (extensions != null && extensions.Count > 0);

                _publisher.SetVisibilities(_areServiceGroupsVisible, _areOtherFilesVisible, _areExtensionsVisible);
            }

            internal override void UpdateTrayMenu(ObservableCollection<Control> menuItems,
                ObservableCollection<ServiceGroup> groups,
                ObservableCollection<ExternalFile> files)
            {
                _publisher.PublishServices(groups, menuItems);
                _publisher.PublishOtherFiles(files, menuItems);

                _areOtherFilesVisible = (files != null && files.Count > 0);
                _areServiceGroupsVisible = (groups != null && groups.Count > 0);

                _publisher.SetVisibilities(_areServiceGroupsVisible, _areOtherFilesVisible, _areExtensionsVisible);
            }
        }
    }

    internal abstract class MainPublisher
    {
        internal static MainPublisher GetInstance(Logger logger, ServiceTogglingCommands commands)
        {
            return GetInstance(logger, LogOpener.GetInstance(logger), PublisherHelper.GetInstance(logger, commands),
                ContextMenuAdder.GetInstance());
        }

        internal static MainPublisher GetInstance(Logger logger, LogOpener logOpener, PublisherHelper helper,
            ContextMenuAdder adder)
        {
            return new MainPublisherImplementation(logger, logOpener, helper, adder);
        }


        internal abstract void PublishBasicItems(ObservableCollection<Control> menuItems);

        internal abstract void PublishExtensions(IEnumerable<ImportedContextMenu> extensionMenuItems,
            ObservableCollection<Control> menuItems);

        internal abstract void PublishServices(ObservableCollection<ServiceGroup> groups,
            ObservableCollection<Control> collection);

        internal abstract void PublishOtherFiles(ObservableCollection<ExternalFile> files,
            ObservableCollection<Control> menuItems);

        internal abstract void SetVisibilities(bool serviceGroupsVisible, bool otherFilesVisible, bool extensionsVisible);

        private sealed class MainPublisherImplementation : MainPublisher
        {
            private readonly ContextMenuAdder _adder;
            private readonly MenuItem _archiveLogsMenuItem;
            private readonly Separator _archiveLogsSeparator;
            private readonly Separator _bottomSeparator;
            private readonly MenuItem _exitMenuItem;
            private readonly Separator _extensionsSeparator;
            private readonly PublisherHelper _helper;
            private readonly Style _logFileStyle;
            private readonly LogOpener _logOpener;
            private readonly Logger _logger;
            private readonly MenuItem _optionsMenuItem;
            private readonly Separator _otherFilesSeparator;
            private readonly Separator _quickAdminSeparator;
            private readonly MenuItem _reopenEngineItem;
            private readonly MenuItem _restartAllMenuItem;
            private readonly Separator _serviceGroupSeparator;
            private readonly MenuItem _startAllMenuItem;
            private readonly MenuItem _stopAllMenuItem;
            private readonly Separator _titleBar;

            internal MainPublisherImplementation(Logger logger, LogOpener logOpener, PublisherHelper helper,
                ContextMenuAdder adder)
            {
                _logger = logger;
                _logOpener = logOpener;
                _helper = helper;
                _adder = adder;

                var contextMenuSeparatorStyle = (Style) Application.Current.Resources["ContextMenuSeparatorStyle"];
                _extensionsSeparator = new Separator {Style = contextMenuSeparatorStyle, Name = "extensionsSeparator"};
                _bottomSeparator = new Separator {Style = contextMenuSeparatorStyle, Name = "bottomSeparator"};
                _archiveLogsSeparator = new Separator
                {
                    Style = contextMenuSeparatorStyle,
                    Name = "archiveLogsSeparator"
                };

                _startAllMenuItem = (MenuItem) Application.Current.Resources["StartAllMenuItem"];
                _stopAllMenuItem = (MenuItem) Application.Current.Resources["StopAllMenuItem"];
                _restartAllMenuItem = (MenuItem) Application.Current.Resources["RestartAllMenuItem"];
                _archiveLogsMenuItem = (MenuItem) Application.Current.Resources["ArchiveLogsMenuItem"];
                _reopenEngineItem = (MenuItem) Application.Current.Resources["ReopenEngineItem"];
                _optionsMenuItem = (MenuItem) Application.Current.Resources["OptionsMenuItem"];
                _exitMenuItem = (MenuItem) Application.Current.Resources["ExitMenuItem"];

                _quickAdminSeparator = (Separator) Application.Current.Resources["QuickAdminSeparator"];
                _otherFilesSeparator = (Separator) Application.Current.Resources["OtherFilesSeparator"];
                _serviceGroupSeparator = (Separator) Application.Current.Resources["ServiceGroupsSeparator"];
                _titleBar = (Separator) Application.Current.Resources["TitleBarSeparator"];


                _logFileStyle = (Style) Application.Current.TryFindResource("FileLinkStyleKey");
            }

            /// <summary>
            ///     Publishes the basic elements of the context menu.
            /// </summary>
            internal override void PublishBasicItems(ObservableCollection<Control> menuItems)
            {
                menuItems.Add(_titleBar);
                menuItems.Add(_reopenEngineItem);

                menuItems.Add(_serviceGroupSeparator);
                // Services here
                menuItems.Add(_otherFilesSeparator);
                // Other files here
                menuItems.Add(_quickAdminSeparator);
                menuItems.Add(_startAllMenuItem);
                menuItems.Add(_stopAllMenuItem);
                menuItems.Add(_restartAllMenuItem);
                menuItems.Add(_archiveLogsSeparator);
                menuItems.Add(_archiveLogsMenuItem);
                menuItems.Add(_extensionsSeparator);
                // Extensions here
                menuItems.Add(_bottomSeparator);
                menuItems.Add(_optionsMenuItem);
                menuItems.Add(_exitMenuItem);
            }

            /// <summary>
            ///     Publishes the service groups to the context menu.
            /// </summary>
            internal override void PublishServices(ObservableCollection<ServiceGroup> groups,
                ObservableCollection<Control> menuItems)
            {
                if (groups == null) return;

                int startIndex = menuItems.IndexOf(_serviceGroupSeparator);
                int endIndex = menuItems.IndexOf(_otherFilesSeparator);
                if (startIndex == -1 || endIndex == -1) return;

                // Remove existing 'other files'
                while (startIndex + 1 < endIndex)
                {
                    menuItems.RemoveAt(startIndex + 1);
                    endIndex--;
                }

                foreach (ServiceGroup group in groups)
                {
                    ContextMenu holdingMenu = _helper.PopulateServiceGroupContextMenu(group);

                    if (holdingMenu.Items.Count <= 0) continue;
                    do
                    {
                        var menuItem = (Control) holdingMenu.Items[0];
                        holdingMenu.Items.RemoveAt(0);
                        menuItems.Insert(endIndex, menuItem);
                        endIndex += 1;
                    } while (holdingMenu.Items.Count > 0);
                }
            }


            /// <summary>
            ///     Publishes the other logfiles to the context menu.
            /// </summary>
            internal override void PublishOtherFiles(ObservableCollection<ExternalFile> files,
                ObservableCollection<Control> menuItems)
            {
                int startIndex = menuItems.IndexOf(_otherFilesSeparator);
                int endIndex = menuItems.IndexOf(_quickAdminSeparator);
                if (startIndex == -1 || endIndex == -1) return;

                if (files == null) return;
                if (files.Count == 0) return;

                // Remove existing 'other files'
                while (startIndex + 1 < endIndex)
                {
                    menuItems.RemoveAt(startIndex + 1);
                    endIndex--;
                }

                foreach (ExternalFile otherFile in files)
                {
                    string header = otherFile.DisplayName;

                    if (!otherFile.ShowParentDirectory)
                    {
                        // Make sure the header is unique, e.g., when there are more than one web.config files.
                        for (int i = 0; i < files.Count; i++)
                        {
                            string testName = files[i].ShortParsedName;
                            int currentIndex = files.IndexOf(otherFile);

                            if (header == testName && currentIndex != i)
                            {
                                otherFile.ShowParentDirectory = true;
                            }
                        }
                    }

                    var tempLog = new MenuItem
                    {
                        Name = _adder.GetNewUIElementName() + "MenuItem",
                        DataContext = otherFile,
                        Style = _logFileStyle,
                        Tag = otherFile.ParsedName,
                        Header = header
                    };

                    tempLog.PreviewMouseDown += _logOpener.tempLog_PreviewMouseDown;
                    _adder.AddContextMenuToFile(tempLog, otherFile);

                    menuItems.Insert(endIndex, tempLog);
                    endIndex++;
                }
            }

            /// <summary>
            ///     Publishes the extension menu items to the context menu.
            /// </summary>
            internal override void PublishExtensions(IEnumerable<ImportedContextMenu> extensionMenuItems,
                ObservableCollection<Control> menuItems)
            {
                if (extensionMenuItems == null) return;

                int index = menuItems.IndexOf(_bottomSeparator);
                if (index == -1) return;

                foreach (ImportedContextMenu item in extensionMenuItems)
                {
                    if (!item.CanExecute) continue;
                    if (item.Menu == null) continue;

                    ContextMenu holdingMenu = item.Menu;

                    if (holdingMenu.Items.Count <= 0) continue;
                    do
                    {
                        var menuItem = (Control) holdingMenu.Items[0];
                        holdingMenu.Items.RemoveAt(0);
                        menuItems.Insert(index, menuItem);
                        index += 1;
                    } while (holdingMenu.Items.Count > 0);
                }
                _logger.Debug(Strings.Debug_MenuItemsPublishedToContextMenu);
            }

            internal override void SetVisibilities(bool serviceGroupsVisible, bool otherFilesVisible,
                bool extensionsVisible)
            {
                // Set visibilities
                _serviceGroupSeparator.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _quickAdminSeparator.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _startAllMenuItem.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _stopAllMenuItem.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _restartAllMenuItem.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _archiveLogsSeparator.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _archiveLogsMenuItem.Visibility = serviceGroupsVisible ? Visibility.Visible : Visibility.Collapsed;
                _otherFilesSeparator.Visibility = otherFilesVisible ? Visibility.Visible : Visibility.Collapsed;

                _extensionsSeparator.Visibility = extensionsVisible ? Visibility.Visible : Visibility.Collapsed;

                _bottomSeparator.Visibility = (serviceGroupsVisible || extensionsVisible)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }

    internal abstract class PublisherHelper
    {
        internal static PublisherHelper GetInstance(Logger logger, ServiceTogglingCommands commands)
        {
            return GetInstance(MenuItemCreator.GetInstance(logger), commands);
        }

        internal static PublisherHelper GetInstance(MenuItemCreator menuItemCreator, ServiceTogglingCommands commands)
        {
            return new PublisherHelperImplementation(menuItemCreator, commands);
        }

        internal abstract ContextMenu PopulateServiceGroupContextMenu(ServiceGroup group);

        internal abstract int GetIndexOf(Control menuItem, ObservableCollection<Control> menuItems);

        private sealed class PublisherHelperImplementation : PublisherHelper
        {
            private readonly MenuItemCreator _creator;
            private readonly Style _groupSeparatorStyle;
            private readonly Style _individualServiceStyle;
            private readonly Style _normalSeparatorStyle;
            private readonly ServiceTogglingCommands _serviceCommands;
            private readonly Style _serviceStatusStyle;

            internal PublisherHelperImplementation(MenuItemCreator creator, ServiceTogglingCommands serviceCommands)
            {
                _creator = creator;
                _serviceCommands = serviceCommands;

                _individualServiceStyle = (Style) Application.Current.TryFindResource("IndividualServiceStyleKey");
                _groupSeparatorStyle = (Style) Application.Current.TryFindResource("GroupSeparatorStyleKey");
                _serviceStatusStyle = (Style) Application.Current.TryFindResource("ServiceStatusMenuItemKey");
                _normalSeparatorStyle = (Style) Application.Current.TryFindResource("ContextMenuSeparatorStyle");
            }

            internal override ContextMenu PopulateServiceGroupContextMenu(ServiceGroup group)
            {
                ObservableCollection<Service> items = group.Items;
                string groupName = group.GroupName;

                // populate the context menu
                var contextMenu = new ContextMenu
                {
                    StaysOpen = true,
                    //Name = "ServiceGroup_"+groupName.Replace(" ", string.Empty).Trim(),
                };

                var rootItem = new MenuItem
                {
                    Header = groupName,
                    Style = _individualServiceStyle
                };

                MenuItem tempStartGroup = _creator.ServiceTogglingMenuItem(
                    Strings.ServicesView_StartButton_Text,
                    items, _serviceCommands.StartServiceGroupCommand);
                MenuItem tempStopGroup = _creator.ServiceTogglingMenuItem(
                    Strings.ServicesView_StopButton_Text,
                    items, _serviceCommands.StopServiceGroupCommand);
                MenuItem tempRestartGroup = _creator.ServiceTogglingMenuItem(
                    Strings.ServicesView_RestartButton_Text,
                    items, _serviceCommands.RestartServiceGroupCommand);

                if (items.Count == 1)
                {
                    rootItem.DataContext = items[0];
                    rootItem.Items.Add(new Separator
                    {
                        Tag = items[0].CommonName,
                        Style = _groupSeparatorStyle
                    });

                    var tempServiceStatusItem = new Separator
                    {
                        DataContext = items[0],
                        Style = _serviceStatusStyle
                    };
                    rootItem.Items.Add(tempServiceStatusItem);

                    rootItem.Items.Add(tempStartGroup);
                    rootItem.Items.Add(tempStopGroup);
                    rootItem.Items.Add(tempRestartGroup);

                    _creator.AddFileLinks(rootItem, items[0].ConfigFiles,
                        Strings.ServiceParametersView_ConfigFiles_Title);
                    _creator.AddFileLinks(rootItem, items[0].LogFiles, Strings.ServiceParametersView_LogFiles_Title);
                }
                else
                {
                    rootItem.DataContext = group;
                    rootItem.Items.Add(new Separator {Tag = groupName, Style = _groupSeparatorStyle, Name = "GroupName"});

                    foreach (Service service in items)
                    {
                        // Copy foreach variable to local variable to
                        // prevent unexpected compiler-specific behavior
                        Service serviceItem = service;

                        MenuItem tempStartItem = _creator.ServiceTogglingMenuItem(
                            Strings.ServicesView_StartButton_Text,
                            items, _serviceCommands.StartServiceCommand, serviceItem, group);
                        MenuItem tempStopItem = _creator.ServiceTogglingMenuItem(
                            Strings.ServicesView_StopButton_Text,
                            items, _serviceCommands.StopServiceCommand, serviceItem, group);
                        MenuItem tempRestartItem = _creator.ServiceTogglingMenuItem(
                            Strings.ServicesView_RestartButton_Text,
                            items, _serviceCommands.RestartServiceCommand, serviceItem, group);

                        var tempServiceStatusItem = new Separator
                        {
                            DataContext = serviceItem,
                            Style = _serviceStatusStyle
                        };

                        var tempServiceMenuItem = new MenuItem
                        {
                            Header = serviceItem.CommonName,
                            Style = _individualServiceStyle,
                            DataContext = serviceItem
                        };

                        tempServiceMenuItem.Items.Add(new Separator
                        {
                            Tag = serviceItem.CommonName,
                            Style = _groupSeparatorStyle
                        });


                        tempServiceMenuItem.Items.Add(tempServiceStatusItem);

                        tempServiceMenuItem.Items.Add(tempStartItem);
                        tempServiceMenuItem.Items.Add(tempStopItem);
                        tempServiceMenuItem.Items.Add(tempRestartItem);

                        _creator.AddFileLinks(tempServiceMenuItem, serviceItem.ConfigFiles,
                            Strings.ServiceParametersView_ConfigFiles_Title);
                        _creator.AddFileLinks(tempServiceMenuItem, serviceItem.LogFiles,
                            Strings.ServiceParametersView_LogFiles_Title);

                        rootItem.Items.Add(tempServiceMenuItem);
                    }

                    rootItem.Items.Add(new Separator {Style = _normalSeparatorStyle});

                    rootItem.Items.Add(tempStartGroup);
                    rootItem.Items.Add(tempStopGroup);
                    rootItem.Items.Add(tempRestartGroup);
                }

                contextMenu.Items.Add(rootItem);
                return contextMenu;
            }

            internal override int GetIndexOf(Control menuItem, ObservableCollection<Control> menuItems)
            {
                for (int index = 0; index < menuItems.Count; index++)
                {
                    if (menuItems[index].Name == menuItem.Name) return index;
                }
                return -1;
            }
        }
    }

    internal abstract class LogOpener
    {
        internal static LogOpener GetInstance(Logger logger)
        {
            return GetInstance(FileSystem.GetInstance(logger));
        }

        internal static LogOpener GetInstance(FileSystem fileSystem)
        {
            return new LogOpenerImplementation(fileSystem);
        }

        internal abstract void tempLog_PreviewMouseDown(object sender, MouseButtonEventArgs e);

        private sealed class LogOpenerImplementation : LogOpener
        {
            private readonly FileSystem _fileSystem;

            internal LogOpenerImplementation(FileSystem fileSystem)
            {
                _fileSystem = fileSystem;
            }

            internal override void tempLog_PreviewMouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    var item = sender as MenuItem;
                    if (item == null) return;

                    item.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
                else
                {
                    var item = sender as MenuItem;
                    if (item == null) return;

                    var file = item.Tag as string;
                    if (file == null) return;

                    // Attempt to open the path.
                    _fileSystem.OpenPath(file);
                }
            }
        }
    }

    internal abstract class ContextMenuAdder
    {
        internal static ContextMenuAdder GetInstance()
        {
            return new ContextMenuAdderImplementation();
        }

        internal abstract void AddContextMenuToFile(FrameworkElement item, ExternalFile file);

        internal abstract string GetNewUIElementName();

        private sealed class ContextMenuAdderImplementation : ContextMenuAdder
        {
            private readonly Style _containerStyle;
            private readonly Style _copyFileStyle;
            private readonly Style _openFolderStyle;

            internal ContextMenuAdderImplementation()
            {
                _openFolderStyle = (Style) Application.Current.TryFindResource("OpenFolderStyleKey");
                _copyFileStyle = (Style) Application.Current.TryFindResource("FileCopyStyleKey");
                _containerStyle = (Style) Application.Current.TryFindResource("ContextMenuSeparatorStyle");
            }

            internal override string GetNewUIElementName()
            {
                string name = "_" + string.Join("_", Guid.NewGuid().ToString().Split('{', '-', '}'));
                return name;
            }

            internal override void AddContextMenuToFile(FrameworkElement item, ExternalFile file)
            {
                var openFolderItem = new MenuItem
                {
                    DataContext = file,
                    Style = _openFolderStyle
                };
                var copyFileItem = new MenuItem
                {
                    DataContext = file,
                    Style = _copyFileStyle
                };
                var contextMenu = new ContextMenu
                {
                    DataContext = file,
                    PlacementTarget = item,
                    ItemContainerStyle = _containerStyle
                };
                contextMenu.Items.Add(openFolderItem);
                contextMenu.Items.Add(copyFileItem);

                item.ContextMenu = contextMenu;
            }
        }
    }

    internal abstract class MenuItemCreator
    {
        internal static MenuItemCreator GetInstance(Logger logger)
        {
            return GetInstance(logger, LogOpener.GetInstance(logger), ContextMenuAdder.GetInstance());
        }

        internal static MenuItemCreator GetInstance(Logger logger, LogOpener logOpener, ContextMenuAdder adder)
        {
            return new MenuItemImplementation(logger, logOpener, adder);
        }

        internal abstract MenuItem ServiceTogglingMenuItem(string header, ObservableCollection<Service> items,
            ICommand command, object parameter = null,
            object dataContext = null);

        internal abstract void AddFileLinks(ItemsControl menuItem, ObservableCollection<ExternalFile> files, string name);

        private sealed class MenuItemImplementation : MenuItemCreator
        {
            private readonly ContextMenuAdder _adder;
            private readonly Style _logFileStyle;
            private readonly LogOpener _logOpener;
            private readonly Logger _logger;
            private readonly Style _separatorStyle;
            private readonly Style _serviceGroupTogglingStyle;
            private readonly Style _serviceTogglingStyle;

            internal MenuItemImplementation(Logger logger, LogOpener logOpener, ContextMenuAdder adder)
            {
                _logger = logger;
                _logOpener = logOpener;
                _adder = adder;

                _serviceGroupTogglingStyle =
                    (Style) Application.Current.TryFindResource("ServiceGroupTogglingMenuItemStyleKey");
                _serviceTogglingStyle = (Style) Application.Current.TryFindResource("ServiceTogglingMenuItemStyleKey");

                _separatorStyle = (Style) Application.Current.TryFindResource("SubcontextSeparatorStyleKey");
                _logFileStyle = (Style) Application.Current.TryFindResource("FileLinkStyleKey");
            }

            internal override MenuItem ServiceTogglingMenuItem(string header, ObservableCollection<Service> items,
                ICommand command, object parameter = null,
                object dataContext = null)
            {
                string noun;
                Style style;
                object context;

                if (parameter == null)
                {
                    parameter = items;

                    if (items.Count > 1)
                    {
                        // whole group
                        noun = Strings.Noun_AllServices;
                        style = _serviceGroupTogglingStyle;
                        context = dataContext;
                    }
                    else
                    {
                        // group with only one item
                        noun = Strings.Noun_Service;
                        style = _serviceTogglingStyle;
                        context = items[0];
                    }
                }
                else
                {
                    // single item
                    noun = Strings.Noun_Service;
                    style = _serviceTogglingStyle;
                    context = parameter;
                }

                string resourceKey = String.Format("ServiceImage{0}Key", header);
                var icon = (Image) Application.Current.TryFindResource(resourceKey);

                var output = new MenuItem
                {
                    Name = _adder.GetNewUIElementName() + "MenuItem",
                    DataContext = context,
                    Header = header.Trim() + " " + noun,
                    Command = command,
                    CommandParameter = parameter,
                    Style = style,
                    Icon = icon
                };

                return output;
            }


            internal override void AddFileLinks(ItemsControl menuItem, ObservableCollection<ExternalFile> files,
                string name)
            {
                if (files.Count == 0) return;

                menuItem.Items.Add(new Separator
                {
                    Tag = name,
                    Style = _separatorStyle,
                    Name = _adder.GetNewUIElementName() + "FileSeparator"
                });

                foreach (ExternalFile configFile in files)
                {
                    var tempLog = new MenuItem
                    {
                        DataContext = configFile,
                        Style = _logFileStyle,
                        Tag = configFile.ParsedName,
                        Header = configFile.ShortParsedName
                    };

                    tempLog.PreviewMouseDown += _logOpener.tempLog_PreviewMouseDown;

                    _adder.AddContextMenuToFile(tempLog, configFile);

                    menuItem.Items.Add(tempLog);
                }
            }
        }
    }
}