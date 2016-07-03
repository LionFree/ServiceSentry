// -----------------------------------------------------------------------
//  <copyright file="ServicesViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using ServiceSentry.Client.Properties;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.ViewModels.Commands;
using ServiceSentry.Client.UNTESTED.Views.Converters;
using ServiceSentry.Client.UNTESTED.Views.Dialogs;
using ServiceSentry.Client.UNTESTED.Views.Helpers;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Client;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    /// <summary>
    /// </summary>
    public abstract class ServicesViewModel : PropertyChangedBase
    {
        public abstract ApplicationState State { get; }
        public abstract FileList LogList { get; }
        public abstract object SelectedItem { get; set; }
        public abstract ServiceList Services { get; }

        /// <summary>
        ///     Determines the visibility of the other logs section, depending on
        ///     whether other logs are selected.
        /// </summary>
        public abstract Visibility OtherLogsVisibility { get; }

        public abstract ICommand OpenAddServicesViewCommand { get; }
        public abstract ICommand ModifyServiceCommand { get; }
        public abstract ICommand ColumnSelectedCommand { get; }
        public abstract ServiceTogglingCommands ServiceCommands { get; }
        internal abstract FileCommands FileCommands { get; }
        public abstract ICommand AddOtherFileCommand { get; }
        public abstract ICommand RemoveOtherFileCommand { get; }
        protected abstract void ColumnSelected(object setting);
        public abstract void RemoveFile(object externalFile);

        internal static ServicesViewModel GetInstance(Logger logger, ApplicationState state,
                                                      AddServicesViewModel addServicesViewModel,
                                                      ServiceParametersViewModel serviceParametersViewModel,
                                                      ServiceTogglingCommands serviceCommands,
                                                      FileCommands fileCommands,
                                                      MonitorServiceWatchdog monitor)
        {
            return GetInstance(logger, state, addServicesViewModel, serviceParametersViewModel, serviceCommands,
                               fileCommands, SVMHelper.GetInstance(logger, ConfigFileHandler.GetInstance(logger)),
                               monitor);
        }

        internal static ServicesViewModel GetInstance(Logger logger, ApplicationState state,
                                                      AddServicesViewModel addServicesViewModel,
                                                      ServiceParametersViewModel serviceParametersViewModel,
                                                      ServiceTogglingCommands serviceCommands,
                                                      FileCommands fileCommands,
                                                      SVMHelper helper,
                                                      MonitorServiceWatchdog monitor)
        {
            return new ServicesViewModelImplementation(logger, state, addServicesViewModel, serviceParametersViewModel,
                                                       serviceCommands, fileCommands, helper, monitor);
        }

        internal abstract void SelectionChanged(object sender, SelectionChangedEventArgs e);
        internal abstract void ContextMenuClosed(object sender, RoutedEventArgs e);

        private sealed class ServicesViewModelImplementation : ServicesViewModel
        {
            private readonly AddServicesViewModel _addServicesViewModel;
            private readonly FileCommands _fileCommands;
            private readonly SVMHelper _helper;
            private readonly ServiceTogglingCommands _serviceCommands;
            private readonly ServiceParametersViewModel _serviceParametersViewModel;
            private readonly ApplicationState _state;
            private ICommand _addOtherFileCommand;
            private ICommand _columnSelectedCommand;
            private RelayCommand _modifyServiceCommand;
            private RelayCommand _openAddServicesViewCommand;
            private ICommand _removeOtherFileCommand;
            private object _selectedItem;
            private readonly MonitorServiceWatchdog _monitor;
            private readonly Logger _logger;

            internal ServicesViewModelImplementation(Logger logger,
                ApplicationState state,
                                                     AddServicesViewModel addServicesViewModel,
                                                     ServiceParametersViewModel serviceParametersViewModel,
                                                     ServiceTogglingCommands commands,
                                                     FileCommands fileCommands,
                                                     SVMHelper helper,
                                                     MonitorServiceWatchdog monitor)
            {
                _serviceCommands = commands;
                _fileCommands = fileCommands;
                _state = state;
                _addServicesViewModel = addServicesViewModel;
                _serviceParametersViewModel = serviceParametersViewModel;
                _helper = helper;
                _helper.SetViewModel(this);
                _monitor = monitor;
                _logger = logger;

                LogList.Items.CollectionChanged += _helper.ConfigFile_Changed;
                Services.Items.CollectionChanged += _helper.ConfigFile_Changed;
            }


            public override ApplicationState State
            {
                get { return _state; }
            }

            public override FileList LogList
            {
                get { return _state.LogList; }
            }

            public override ServiceList Services
            {
                get { return _state.Services; }
            }

            public override object SelectedItem
            {
                get { return _selectedItem; }
                set
                {
                    if (_selectedItem == value) return;
                    _selectedItem = value;
                    OnPropertyChanged();
                }
            }

            public override ServiceTogglingCommands ServiceCommands
            {
                get { return _serviceCommands; }
            }

            internal override FileCommands FileCommands
            {
                get { return _fileCommands; }
            }

            public override Visibility OtherLogsVisibility
            {
                get
                {
                    var hasExtraLogs = State.LogList.Items.Count > 0;
                    return (hasExtraLogs ? Visibility.Visible : Visibility.Collapsed);
                }
            }


            internal override void ContextMenuClosed(object sender, RoutedEventArgs e)
            {
                SelectedItem = null;

                // Remove the listview selection so that when we open the context menu
                // elsewhere, it won't "remember" what it was last looking at. 

                var contextMenu = e.Source as ContextMenu;
                if (contextMenu == null) return;
                var lv = contextMenu.PlacementTarget as ListView;
                if (lv == null) throw new Exception("Curses! @ ServicesViewModel.ContextMenuClosed"); // :)
                lv.UnselectAll();
                SelectedItem = null;
            }


            internal override void SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                var servicesListView = sender as ListView;
                if (servicesListView == null)
                {
                    SelectedItem = null;
                    return;
                }

                SelectedItem = servicesListView.SelectedItem;
            }

            #region Commands

            public override ICommand AddOtherFileCommand
            {
                get
                {
                    return _addOtherFileCommand ??
                           (_addOtherFileCommand = new RelayCommand("AddOtherFile", param => _helper.AddFile()));
                }
            }

            public override ICommand RemoveOtherFileCommand
            {
                get
                {
                    return _removeOtherFileCommand ??
                           (_removeOtherFileCommand =
                            new RelayCommand("RemoveOtherFile",
                                             param => _helper.RemoveFile((LogFileCommandParameter) param)));
                }
            }

            public override ICommand OpenAddServicesViewCommand
            {
                get
                {
                    return _openAddServicesViewCommand ??
                           (_openAddServicesViewCommand = new RelayCommand("OpenAddServicesView", OpenAddServicesView));
                }
            }

            public override ICommand ModifyServiceCommand
            {
                get
                {
                    return _modifyServiceCommand ??
                           (_modifyServiceCommand = new RelayCommand("ModifyService",
                                                                     ModifyService));
                }
            }

            public override ICommand ColumnSelectedCommand
            {
                get
                {
                    return _columnSelectedCommand ??
                           (_columnSelectedCommand = new RelayCommand("ColumnSelected",
                                                                      ColumnSelected));
                }
            }

            protected override void ColumnSelected(object setting)
            {
                var settingName = setting as string;
                if (settingName == null) return;

                var value = (bool) Settings.Default[settingName];
                Settings.Default[settingName] = !value;

                Settings.Default.Save();
            }

            public override void RemoveFile(object externalFile)
            {
                var param = new LogFileCommandParameter {LogFile = (ExternalFile) externalFile};
                _helper.RemoveFile(param);
            }

            private void ModifyService(object cmdParameter)
            {
                var parameter = cmdParameter as ServiceCommandParameter;
                if (parameter == null) return;

                var service = parameter.Parameter as Service;
                if (service == null)
                {
                    // throw an error.
                    throw new ArgumentNullException("servicesListView parameter (service) is empty.",
                                                    new ArgumentNullException());
                }
                
                var servicesListView = parameter.Control as ListView;
                if (servicesListView == null)
                {
                    // throw an error.
                    throw new ArgumentNullException("servicesListView parameter (control) is empty.",
                                                    new ArgumentNullException());
                }

                _serviceParametersViewModel.PopulateViewModel(service);
                ServiceParametersView.GetInstance(_logger, _serviceParametersViewModel).ShowCenteredDialog();

                service.UpdateParameters(_monitor, _state.LocalConfigs.NotificationDetails, _state.LocalConfigs.WardenDetails);

                // Force refresh the group titles in the servicesListView, in case some changed.
                var dataView = CollectionViewSource.GetDefaultView(servicesListView.ItemsSource);
                dataView.Refresh();
            }

            private void OpenAddServicesView(object parameter)
            {
                AddServicesView.GetInstance(_addServicesViewModel).ShowDialog();
            }

            #endregion
        }
    }

    internal abstract class SVMHelper : PropertyChangedBase
    {
        internal static SVMHelper GetInstance(Logger logger, ConfigFileHandler handler)
        {
            return new SVMHelperImplementation(logger, handler);
        }

        internal abstract void RemoveFile(LogFileCommandParameter param);
        internal abstract void AddFile();

        internal abstract void SetViewModel(ServicesViewModel servicesViewModel);

        public abstract void WriteFile();

        internal abstract void ConfigFile_Changed(object sender, NotifyCollectionChangedEventArgs e);
        internal abstract void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e);

        private sealed class SVMHelperImplementation : SVMHelper
        {
            private readonly ConfigFileHandler _handler;
            private readonly Logger _logger;
            private ServicesViewModel _viewModel;

            internal SVMHelperImplementation(Logger logger, ConfigFileHandler handler)
            {
                _logger = logger;
                _handler = handler;
            }

            internal override void AddFile()
            {
                // Open the file dialog
                var dialog = new OpenFileDialog
                    {
                        Filter = Strings.Noun_AllFilesFilter,
                        FilterIndex = 0,
                        Multiselect = true,
                        ShowReadOnly = false,
                        Title = Strings.Verb_AddFile,
                    };

                if (dialog.ShowDialog() != true) return;

                foreach (var item in dialog.FileNames)
                {
                    var file = ExternalFile.GetInstance(_logger, item, Path.GetFileNameWithoutExtension(item));
                    _logger.Info(Strings.Info_AddingFile, file.FullPath);
                    _viewModel.State.LocalConfigs.LogList.Items.Add(file);
                    _viewModel.State.LogList.Items.Add(file);
                }
                //_handler.WriteConfigFile(_viewModel.State.LocalConfigs);
            }

            internal override void RemoveFile(LogFileCommandParameter param)
            {
                if (param == null) return;

                var file = param.LogFile;
                if (file == null) return;

                _logger.Info(Strings.Info_RemovingFile, file.FullPath);
                _viewModel.State.LocalConfigs.LogList.Items.Remove(file);
                _viewModel.State.LogList.Items.Remove(file);
            }

            internal override void SetViewModel(ServicesViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public override void WriteFile()
            {
                _handler.WriteConfigFile(_viewModel.State.LocalConfigs);
            }

            internal override void ConfigFile_Changed(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (PropertyChangedBase item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (PropertyChangedBase item in e.OldItems)
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                }

                WriteFile();
            }

            internal override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);

                // Only write if a datamember has changed.
                // DataMemberAttribute doesn't inherit, so I have to
                // fish around in the base type, if there is one.
                var type = sender.GetType().BaseType ?? sender.GetType();
                
                var dataMembers =
                    type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof (DataMemberAttribute)));
                var individual = dataMembers.SingleOrDefault(prop => prop.Name == e.PropertyName);
                if (individual == null) return;
                
                _logger.Trace(Strings.Trace_PropertyChanged, e.PropertyName, type.Name, sender.ToString());
                WriteFile();
            }
        }
    }
}