// -----------------------------------------------------------------------
//  <copyright file="ServiceParametersViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.Views.Converters;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    internal abstract class ServiceParametersViewModel : PropertyChangedBase
    {
        public abstract string ServiceName { get; set; }
        public abstract ObservableCollection<ExternalFile> LogFiles { get; set; }
        public abstract ObservableCollection<ExternalFile> ConfigFiles { get; set; }
        public abstract bool ParametersUnlocked { get; }
        public abstract ServiceDetails Details { get; set; }
        public abstract string CommonName { get; set; }
        public abstract string DisplayName { get; set; }
        public abstract string SelectedItem { get; set; }
        public abstract List<string> ServiceGroupBoxListItems { get; set; }
        public abstract string NewItem { get; set; }

        public abstract ICommand RefreshSettingsCommand { get; }
        public abstract ICommand CommitCommand { get; }
        public abstract ICommand AddFileCommand { get; }
        public abstract ICommand RemoveFileCommand { get; }
        public abstract ICommand ModifyFileCommand { get; }

        internal static ServiceParametersViewModel GetInstance(Logger logger, ApplicationState applicationState,
                                                               ViewController viewController)
        {
            return new ServiceParametersVMImplementation(logger,
                                                         SPViewModelHelper.GetInstance(logger, Dialogs.GetInstance(),
                                                                                       viewController,
                                                                                       ConfigFileHandler
                                                                                           .GetInstance(
                                                                                               logger),
                                                                                       applicationState));
        }

        public abstract void PopulateViewModel(Service service);
        public abstract void RemoveFile(object commandParameter, string sender);

        private sealed class ServiceParametersVMImplementation : ServiceParametersViewModel
        {
            private readonly SPViewModelHelper _helper;
            private ICommand _addFileCommand;
            private ICommand _commitCommand;
            private ICommand _modifyFileCommand;
            private ICommand _refreshSettingsCommand;
            private ICommand _removeFileCommand;
            private string _selectedItem;
            private Service _service;

            public ServiceParametersVMImplementation(Logger logger,
                                                     SPViewModelHelper helper)
            {
                ConfigFiles = new ObservableCollection<ExternalFile>();
                LogFiles = new ObservableCollection<ExternalFile>();
                Details = ServiceDetails.Default;

                _helper = helper;
                _helper.SetViewModel(this);
            }

            public override string ServiceName { get; set; }
            public override string DisplayName { get; set; }
            public override string CommonName { get; set; }

            public override bool ParametersUnlocked
            {
                get { return _service.ParametersUnlocked; }
            }

            public override ServiceDetails Details { get; set; }
            public override ObservableCollection<ExternalFile> ConfigFiles { get; set; }
            public override ObservableCollection<ExternalFile> LogFiles { get; set; }


            public override List<string> ServiceGroupBoxListItems { get; set; }

            public override string SelectedItem
            {
                get { return _selectedItem; }
                set
                {
                    if (_selectedItem == value) return;
                    _selectedItem = value;
                    OnPropertyChanged();
                }
            }

            public override string NewItem
            {
                get { return _selectedItem; }
                set
                {
                    if (SelectedItem != null)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(value))
                    {
                        ServiceGroupBoxListItems.Add(value);
                        ServiceGroupBoxListItems.Sort();

                        SelectedItem = value;
                        OnPropertyChanged();
                    }
                }
            }

            public override ICommand CommitCommand
            {
                get
                {
                    return _commitCommand ??
                           (_commitCommand = new RelayCommand("Commit", param => _helper.Commit(_service)));
                }
            }

            public override ICommand RefreshSettingsCommand
            {
                get
                {
                    return _refreshSettingsCommand ??
                           (_refreshSettingsCommand = new RelayCommand("RefreshSettings", param =>
                                                                                          _helper.RefreshService(
                                                                                              _service)));
                }
            }

            public override ICommand AddFileCommand
            {
                get
                {
                    return _addFileCommand ??
                           (_addFileCommand =
                            new RelayCommand("AddFile", param => _helper.AddFile((string) param)));
                }
            }

            public override ICommand RemoveFileCommand
            {
                get
                {
                    return _removeFileCommand ??
                           (_removeFileCommand =
                            new RelayCommand("RemoveFile", param => _helper.RemoveFile((LogFileCommandParameter) param)));
                }
            }

            public override ICommand ModifyFileCommand
            {
                get
                {
                    return _modifyFileCommand ??
                           (_modifyFileCommand =
                            new RelayCommand("ModifyFile", param => _helper.ModifyFile((LogFileCommandParameter) param)));
                }
            }

            public override void PopulateViewModel(Service service)
            {
                _service = service;
                _helper.RefreshService(_service);
            }

            public override void RemoveFile(object externalFile, string type)
            {
                _helper.RemoveFile(new LogFileCommandParameter {LogFile = externalFile as ExternalFile, Type = type});
            }
        }
    }


    internal abstract class SPViewModelHelper
    {
        internal static SPViewModelHelper GetInstance(Logger logger, Dialogs dialogs,
                                                      ViewController viewController,
                                                      ConfigFileHandler fileHandler,
                                                      ApplicationState applicationState)
        {
            return new SPVMHImplementation(logger, dialogs, viewController, fileHandler, applicationState);
        }

        internal abstract void Commit(Service service);
        internal abstract void RefreshService(Service service);
        internal abstract void SetViewModel(ServiceParametersViewModel viewModel);
        
        internal abstract void AddFile(string fileType);
        internal abstract void RemoveFile(LogFileCommandParameter param);
        internal abstract void ModifyFile(LogFileCommandParameter param);

        private sealed class SPVMHImplementation : SPViewModelHelper
        {
            private readonly ApplicationState _applicationState;
            private readonly Dialogs _dialogs;
            private readonly ConfigFileHandler _fileHandler;
            private readonly Logger _logger;
            private readonly ViewController _viewController;
            private ServiceParametersViewModel _viewModel;


            internal SPVMHImplementation(Logger logger, Dialogs dialogs,
                                         ViewController viewController, ConfigFileHandler fileHandler,
                                         ApplicationState applicationState)
            {
                _logger = logger;
                _dialogs = dialogs;
                _viewController = viewController;
                _fileHandler = fileHandler;
                _applicationState = applicationState;
            }


            internal override void Commit(Service service)
            {
                try
                {
                    service.CommonName = _viewModel.CommonName;
                    service.ServiceGroup = _viewModel.SelectedItem;
                    service.ConfigFiles = _viewModel.ConfigFiles;
                    service.LogFiles = _viewModel.LogFiles;
                    service.Details.StartOrder = _viewModel.Details.StartOrder;
                    service.Details.StopOrder = _viewModel.Details.StopOrder;
                    service.Details.NotifyOnUnexpectedStop = _viewModel.Details.NotifyOnUnexpectedStop;
                    service.Details.Timeout = _viewModel.Details.Timeout;
                }
                catch (Exception exception)
                {
                    _logger.ErrorException(exception);
                    var msg = exception.Message;
                    _dialogs.ShowError(msg, Application.Current.MainWindow);
                    return;
                }

                _viewController.ServiceHandler.RegenerateServiceGroups();

                // Save the services.
                _fileHandler.WriteConfigFile(_applicationState.LocalConfigs);
            }

            // Reload the service details from the backup
            internal override void RefreshService(Service service)
            {
                if (service == null) return;
                service = service.Clone(_logger);

                _viewModel.ServiceName = service.ServiceName;
                _viewModel.DisplayName = service.DisplayName;
                _viewModel.CommonName = service.CommonName;
                
                // Need to add these instead of setting collections equal.
                var duplicateDetails = ServiceDetails.Default;
                duplicateDetails.NotifyOnUnexpectedStop = service.Details.NotifyOnUnexpectedStop;
                duplicateDetails.StartOrder = service.Details.StartOrder;
                duplicateDetails.StopOrder = service.Details.StopOrder;
                duplicateDetails.Timeout = service.Details.Timeout;
                _viewModel.Details = duplicateDetails;

                var duplicateConfigFiles = new ObservableCollection<ExternalFile>();
                foreach (var item in service.ConfigFiles)
                {
                    duplicateConfigFiles.Add(item);
                }
                _viewModel.ConfigFiles = duplicateConfigFiles;
                //_viewModel.ConfigFiles = service.ConfigFiles;

                var duplicateLogFiles = new ObservableCollection<ExternalFile>();
                foreach (var item in service.LogFiles)
                {
                    duplicateLogFiles.Add(item);
                }
                _viewModel.LogFiles = duplicateLogFiles;
                //_viewModel.LogFiles = service.LogFiles;

                var groups = _applicationState.ServiceGroups;
                var output = new List<string>();
                if (groups == null)
                {
                    _viewModel.ServiceGroupBoxListItems = output;
                    return;
                }

                output.AddRange(groups.Select(@group => @group.GroupName));

                _viewModel.ServiceGroupBoxListItems = output;
                _viewModel.SelectedItem = service.ServiceGroup;
            }

            internal override void SetViewModel(ServiceParametersViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            internal override void AddFile(string type)
            {
                _logger.Trace("Adding {0} file.", type.ToLower());

                // Open the file dialog
                var dialog = new OpenFileDialog
                    {
                        Filter =
                            (type == Strings.CONST_CONFIG) ? Strings.Noun_ConfigFileFilter : Strings.Noun_LogFileFilter,
                        FilterIndex = 0,
                        Multiselect = true,
                        ShowReadOnly = false,
                        Title = (type == Strings.CONST_CONFIG) ? Strings.Verb_AddConfigFiles : Strings.Verb_AddLogFiles,
                    };

                if (dialog.ShowDialog() != true) return;

                foreach (var item in dialog.FileNames)
                {
                    var file = ExternalFile.GetInstance(_logger, item, Path.GetFileNameWithoutExtension(item));

                    // Assign the file to the appropriate list
                    if (type == Strings.CONST_CONFIG)
                    {
                        _viewModel.ConfigFiles.Add(file);
                        break;
                    }

                    if (type != Strings.CONST_LOG) continue;
                    _viewModel.LogFiles.Add(file);
                    break;
                }
            }

            internal override void ModifyFile(LogFileCommandParameter param)
            {
                Trace.WriteLine(string.Format("Modifying {0} file.", param.Type));
            }

            internal override void RemoveFile(LogFileCommandParameter param)
            {
                if (param == null) return;
                
                var file = param.LogFile;
                if (file == null) return;

                var type = param.Type;
                if (string.IsNullOrEmpty(type)) throw new ArgumentException("type");

                if (type == Strings.CONST_LOG)
                {
                    _viewModel.LogFiles.Remove(file);
                    return;
                }

                if (type != Strings.CONST_CONFIG) return;
                _viewModel.ConfigFiles.Remove(file);

            }
        }
    }
}