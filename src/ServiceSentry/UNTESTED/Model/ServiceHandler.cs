// -----------------------------------------------------------------------
//  <copyright file="ServiceHandler.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;
using ServiceSentry.Model.Services;

#endregion

namespace ServiceSentry.Client.UNTESTED.Model
{
    internal abstract class ServiceHandler : PropertyChangedBase
    {
        public static ServiceHandler GetInstance(Logger logger, ApplicationState applicationState,
                                                 ConfigFileHandler fileHandler,
                                                 ServiceListBehavior serviceBehavior,
                                                 ConfigFile configFile,
                                                 MonitorServiceWatchdog monitor)
        {
            return new ServiceHandlerImplementation(logger, applicationState, fileHandler, serviceBehavior,
                                                    Dialogs.GetInstance(),
                                                    configFile,
                                                    ClientList.GetInstance(),
                                                    monitor, 
                                                    FileAttacher.GetInstance());
        }

        
        #region Abstract Members

        internal abstract void UpdateServices(NotificationSettings notificationDetails, WardenServerDetails wardenDetails);

        internal abstract void AddService(Service service, ref ApplicationState state);
        internal abstract void RemoveService(string serviceName, ref ApplicationState state);

        internal abstract bool AddOrRemoveServiceSelected(string serviceName, ApplicationState state,
                                                          ServiceListBehavior behavior, Logger logger);

        internal abstract void StartCheckingServices();
        internal abstract void RegenerateServiceGroups();
        internal abstract void UpdateListOfAllServices();

        internal abstract void StopService(object serviceObject);
        internal abstract void StartService(object serviceObject);
        internal abstract void RestartServiceGroup(object serviceGroupObject);
        internal abstract void StartServiceGroup(object serviceGroupObject);
        internal abstract void StopServiceGroup(object serviceGroupObject);
        internal abstract void RestartAllServices(object sender);
        internal abstract void StartAllServices(object sender);
        internal abstract void StopAllServices(object sender);
        internal abstract void RestartService(object serviceObject);

        internal event MonitorErrorEventHandler MonitorError;
        internal event EventHandler WorkerCompleted;

        protected abstract void OnWorkerCompleted(object sender, EventArgs eventArgs);
        protected abstract void OnServiceFellOver(object sender, StatusChangedEventArgs e);
        protected abstract void OnServiceStatusChanged(object sender, StatusChangedEventArgs e);

        #endregion

        private sealed class ServiceHandlerImplementation : ServiceHandler
        {
            #region Constructors

            private static Timer _timer;
            private readonly ApplicationState _applicationState;
            private readonly ServiceListBehavior _behavior;
            private readonly ClientList _clients;
            private readonly Dialogs _dialogs;
            private readonly ConfigFileHandler _fileHandler;
            private readonly Logger _logger;
            private readonly MonitorServiceWatchdog _monitor;
            private readonly ServiceList _serviceList;
            private readonly FileAttacher _attacher;

            public ServiceHandlerImplementation(Logger logger,
                                                ApplicationState applicationState,
                                                ConfigFileHandler fileHandler,
                                                ServiceListBehavior serviceBehavior,
                                                Dialogs dialogs,
                                                ConfigFile configFile,
                                                ClientList clients,
                                                MonitorServiceWatchdog monitor, 
                FileAttacher attacher)
            {
                _logger = logger;
                _attacher = attacher;
                _applicationState = applicationState;
                _behavior = serviceBehavior;
                _fileHandler = fileHandler;
                _serviceList = applicationState.Services;
                _dialogs = dialogs;
                _monitor = monitor;
                _clients = clients;

                _clients.GetClient(".");

                PopulateServiceHandlerFromConfigFile(applicationState, configFile);

                _applicationState.Services.Items.CollectionChanged += OnServiceCollectionChanged;
                _applicationState.LogList.Items.CollectionChanged += OnCollectionChanged;
                _behavior.WorkerCompleted += OnWorkerCompleted;

                StartCheckingServices();
            }

            #endregion

            #region Events

            protected override void OnServiceFellOver(object sender, StatusChangedEventArgs e)
            {
                var service = sender as Service;

                if (service == null) throw new ArgumentNullException(nameof(sender));
                if (e == null) throw new ArgumentNullException(nameof(e));

                var message = "The \"" + service.CommonName + "\" service has stopped unexpectedly.";
                //NotificationArea.ShowBalloonTip(Resources.ApplicationName, message, BalloonIcon.Warning);
                _dialogs.ShowError(message, Application.Current.MainWindow);
            }

            protected override void OnServiceStatusChanged(object sender, StatusChangedEventArgs e)
            {
                var service = sender as Service;

                if (service == null) throw new ArgumentNullException(nameof(sender));
                if (e == null) throw new ArgumentNullException(nameof(e));

                // Don't really care if it started without permission.
                // We care if it STOPPED unexpectedly.
                if (e.NewStatus != ServiceState.Stopped) return;
                var message = "The status of service \"" + service.CommonName + "\"  has changed.";
                //NotificationArea.ShowBalloonTip(Resources.ApplicationName, message, BalloonIcon.Warning);
                _dialogs.ShowWarning(message, Application.Current.MainWindow);
            }

            internal override void UpdateServices(NotificationSettings notificationDetails, WardenServerDetails wardenDetails)
            {
                foreach (var service in _applicationState.LocalConfigs.Services.Items)
                {
                    service.UpdateParameters(_monitor, notificationDetails, wardenDetails);
                }
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (PropertyChangedBase item in e.NewItems)
                    {
                        //_modifiedItems.Add(item);
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (PropertyChangedBase item in e.OldItems)
                {
                    //_modifiedItems.Add(item);
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            private void OnServiceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (Service item in e.NewItems)
                    {
                        //_modifiedItems.Add(item);
                        item.PropertyChanged += OnItemPropertyChanged;
                        item.ExternalStatusChanged += OnServiceStatusChanged;
                        item.ServiceFellOver += OnServiceFellOver;
                        item.MonitorError += OnMonitorError;
                    }
                }

                if (e.OldItems == null) return;
                foreach (Service item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                    item.ExternalStatusChanged -= OnServiceStatusChanged;
                    item.ServiceFellOver -= OnServiceFellOver;
                    item.MonitorError -= OnMonitorError;
                }

                // Update the context menu?
                //RegenerateServiceGroups();
            }

            private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }

            protected override void OnWorkerCompleted(object sender, EventArgs e)
            {
                var handler = WorkerCompleted;
                handler?.Invoke(this, e);
            }

            private void OnMonitorError(object sender, MonitorErrorEventArgs e)
            {
                var handler = MonitorError;
                handler?.Invoke(this, e);
            }

            #endregion

            #region Methods

            private void PopulateServiceHandler(ApplicationState state)
            {
                
                var services = state.LocalConfigs.Services.Items;
                //var files = state.LocalConfigs.LogList.Items;

                state.SMTPDetails = state.LocalConfigs.NotificationDetails.EmailInfo;

                //foreach (var item in state.LocalConfigs.Services.Items)
                foreach (var item in services)
                {
                    var mediator = ClientMediator.GetInstance(_logger, _clients, item.Packet);
                    item.AttachMediator(mediator);
                    item.MonitorError += OnMonitorError;
                    
                    _attacher.AttachToList(_logger, item.LogFiles);
                    _attacher.AttachToList(_logger, item.ConfigFiles);

                    state.Services.Items.Add(item);

                }

                state.Services.LogDetails = state.LocalConfigs.LogDetails;

                foreach (var item in state.LocalConfigs.LogList.Items)
                {
                    _attacher.AttachToList(_logger, state.LocalConfigs.LogList.Items);
                    state.LogList.Items.Add(item);
                }
            }

            private void PopulateServiceHandlerFromConfigFile(ApplicationState state, ConfigFile file)
            {
                // TODO: Do we need this?
                if (state == null) state = ApplicationState.GetInstance();

                if (file == null)
                {
                    _logger.Trace(Strings.Debug_CreatingConfigFile);
                    file = _fileHandler.NewConfigFile;
                    _fileHandler.WriteConfigFile(file);
                }
                state.LocalConfigs = file;
                
                PopulateServiceHandler(state);
            }

            
            private ObservableCollection<Service> ConvertObjectToCollection(object serviceGroupObject)
            {
                // The list of services to toggle can come from the view,
                // in the form of a CollectionViewGroup, or from the 
                // ServiceGroup class, in the form of an ObservableCollection<Service>.
                // Handle either.

                if (serviceGroupObject is ObservableCollection<Service>)
                {
                    return serviceGroupObject as ObservableCollection<Service>;
                }

                var serviceGroup = serviceGroupObject as CollectionViewGroup;
                if (serviceGroup == null)
                {
                    throw new ArgumentNullException(nameof(serviceGroupObject));
                }

                var list = new ObservableCollection<Service>();
                foreach (Service item in serviceGroup.Items)
                {
                    list.Add(item);
                }
                return list;
            }

            /// <summary>
            ///     Starts the timer using a given interval.  If no interval is given, the timer defaults to 500ms.
            /// </summary>
            internal override void StartCheckingServices()
            {
                const int interval = 1000;

                _timer = new Timer {Interval = interval};
                _timer.Elapsed += (x, p) =>
                    {
                        // This is janky.  
                        // It only happens if the app is disposed before the timer.
                        // Dispose of the timers.
                        if (Application.Current == null) return;

                        Application.Current.Dispatcher.Invoke((Action) delegate
                            {
                                foreach (var service in _applicationState.Services.Items)
                                {
                                    _behavior.RefreshStatus(service);
                                }
                            });
                    };
                _timer.Enabled = true;
            }

            internal override void RegenerateServiceGroups()
            {
                _applicationState.ServiceGroups = new ObservableCollection<ServiceGroup>();

                var items = _applicationState.Services.Items;

                var groupNames = new List<string>();
                foreach (var item in items.Where(item => !groupNames.Contains(item.ServiceGroup)))
                {
                    groupNames.Add(item.ServiceGroup);
                }

                foreach (var groupName in groupNames)
                {
                    var group = ServiceGroup.GetInstance(groupName, items);
                    _applicationState.ServiceGroups.Add(group);
                    _logger.Trace(Strings.Debug_CreatedServiceGroup, group.GroupName, group.Items.Count);
                }

                _applicationState.OnServicesChanged();
            }

            internal override void RemoveService(string serviceName, ref ApplicationState state)
            {
                if (string.IsNullOrEmpty(serviceName)) return;

                var index = -1;
                foreach (var service in state.Services.Items)
                {
                    if (service.ServiceName != serviceName) continue;

                    index = state.Services.Items.IndexOf(service);
                    break;
                }

                if (index != -1)
                {
                    _behavior.Unsubscribe(state.Services.Items[index]);
                    state.Services.Items.RemoveAt(index);
                }

                index = -1;
                foreach (var service in state.LocalConfigs.Services.Items)
                {
                    if (service.ServiceName != serviceName) continue;

                    index = state.LocalConfigs.Services.Items.IndexOf(service);
                    break;
                }
                if (index != -1)
                {
                    state.LocalConfigs.Services.Items.RemoveAt(index);
                    _fileHandler.WriteConfigFile(state.LocalConfigs);
                }

                RegenerateServiceGroups();
            }

            /// <summary>
            ///     Add a new service.
            /// </summary>
            /// <param name="service">The service to add.</param>
            /// <param name="state"></param>
            internal override void AddService(Service service, ref ApplicationState state)
            {
                if (service == null) return;
                if (state.Services.Items.Contains(service)) return;

                _logger.Info(Strings.Info_AddingService, service.ServiceName);

                var mediator = ClientMediator.GetInstance(_logger, _clients, service.Packet);
                service.AttachMediator(mediator);

                service.CommonName = service.DisplayName;
                service.MonitorError += OnMonitorError;

                state.LocalConfigs.Services.Items.Add(service);

                var track = new TrackingObject {ServiceName = service.ServiceName};
                state.State.Add(service.ServiceName, track);

                _fileHandler.WriteConfigFile(state.LocalConfigs);
                service.CanToggle = true;

                state.Services.Items.Add(service);

                RegenerateServiceGroups();
                UpdateListOfAllServices();
            }

            internal override void UpdateListOfAllServices()
            {
                _applicationState.ServiceInfos = new ObservableCollection<ServiceInfo>();
                var services = LocalServiceFinder.Default.GetServices();

                foreach (var item in services)
                {
                    var svcInfo = ServiceInfo.GetInstance();
                    svcInfo.ServiceName = item.ServiceName;
                    svcInfo.DisplayName = item.DisplayName;

                    foreach (var service in _applicationState.Services.Items)
                    {
                        if (service.ServiceName == item.ServiceName) svcInfo.Selected = true;
                    }

                    _applicationState.ServiceInfos.Add(svcInfo);
                }
            }

            internal override bool AddOrRemoveServiceSelected(string serviceName, ApplicationState state,
                                                              ServiceListBehavior behavior, Logger logger)
            {
                var remove = false;
                foreach (var service in state.ServiceInfos)
                {
                    if (service.ServiceName == serviceName && service.Selected)
                    {
                        RemoveService(serviceName, ref state);
                        service.Selected = false;
                        remove = true;
                        break;
                    }
                }
                if (remove) return false;

                var srvc = Service.GetInstance(serviceName, ".", _logger);
                AddService(srvc, ref state);
                UpdateListOfAllServices();
                return true;
            }

            #endregion

            #region Service Commands

            internal override void RestartServiceGroup(object serviceGroupObject)
            {
                var list = ConvertObjectToCollection(serviceGroupObject);
                _behavior.RestartGroup(list, _serviceList.LogDetails);
            }

            internal override void StartServiceGroup(object serviceGroupObject)
            {
                var list = ConvertObjectToCollection(serviceGroupObject);
                _behavior.StartGroup(list, _serviceList.LogDetails);
            }

            internal override void StopServiceGroup(object serviceGroupObject)
            {
                var list = ConvertObjectToCollection(serviceGroupObject);
                _behavior.StopGroup(list, _serviceList.LogDetails);
            }


            internal override void RestartAllServices(object sender)
            {
                _behavior.RestartAll(_serviceList);
            }

            internal override void StartAllServices(object sender)
            {
                _behavior.StartAll(_serviceList);
            }

            internal override void StopAllServices(object sender)
            {
                _behavior.StopAll(_serviceList);
            }


            internal override void RestartService(object serviceObject)
            {
                var service = serviceObject as Service;
                if (service == null)
                {
                    _logger.Error(Strings.Error_RestartPressedButServiceIsNull);
                    throw new ArgumentNullException("serviceObject", Strings.Error_RestartPressedButServiceIsNull);
                }
                try
                {
                    // Start the service.
                    _behavior.Restart(service, _serviceList.LogDetails);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
            }

            internal override void StartService(object serviceObject)
            {
                var service = serviceObject as Service;
                if (service == null)
                {
                    _logger.Error(Strings.Error_StopPressedButServiceIsNull);
                    throw new ArgumentNullException("serviceObject", Strings.Error_StartPressedButServiceIsNull);
                }
                try
                {
                    // Start the service.
                    _behavior.Start(service, _serviceList.LogDetails);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
            }

            internal override void StopService(object serviceObject)
            {
                // Determine the service.
                var service = serviceObject as Service;
                if (service == null)
                {
                    _logger.Error(Strings.Error_StopPressedButServiceIsNull);
                    throw new ArgumentNullException("serviceObject", Strings.Error_StopPressedButServiceIsNull);
                }
                try
                {
                    // Stop the service.
                    _behavior.Stop(service, _serviceList.LogDetails);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
            }

            #endregion
        }
    }
}