// -----------------------------------------------------------------------
//  <copyright file="ServiceListBehavior.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Diagnostics.Contracts;
using ServiceSentry.Common;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.ExternalFiles;

#endregion

namespace ServiceSentry.Model.Client
{
    public abstract class ServiceListBehavior
    {
        public static ServiceListBehavior GetInstance(Logger logger, MonitorServiceWatchdog watchdog)
        {
            return GetInstance(logger,
                               ServiceTogglingBehavior.GetInstance(watchdog, logger),
                               AsyncWorker.Default,
                               LogArchiver.GetInstance(logger));
        }

        internal static ServiceListBehavior GetInstance(Logger logger,
                                                        ServiceTogglingBehavior behavior,
                                                        AsyncWorker worker,
                                                        LogArchiver logArchiver)
        {
            return new ServiceListBehaviorImplementation(logger, behavior, worker, logArchiver);
        }

        public event EventHandler WorkerCompleted;
        public abstract void OnWorkerCompleted(EventArgs e);

        public abstract void RestartAll(ServiceList list);
        public abstract void StartAll(ServiceList list);
        public abstract void StopAll(ServiceList list);

        public abstract void RestartGroup(ObservableCollection<Service> items, LoggingDetails logDetails);
        public abstract void StartGroup(ObservableCollection<Service> items, LoggingDetails logDetails);
        public abstract void StopGroup(ObservableCollection<Service> items, LoggingDetails logDetails);

        public abstract void Start(Service service, LoggingDetails logDetails);
        public abstract void Stop(Service service, LoggingDetails logDetails);
        public abstract void Restart(Service service, LoggingDetails logDetails);

        public abstract void RefreshStatus(Service service);
        public abstract void Unsubscribe(Service service);

        private sealed class ServiceListBehaviorImplementation : ServiceListBehavior
        {
            private readonly ServiceTogglingBehavior _behavior;
            private readonly LogArchiver _logArchiver;
            private readonly Logger _logger;
            private readonly AsyncWorker _worker;

            public ServiceListBehaviorImplementation(Logger logger, ServiceTogglingBehavior behavior, AsyncWorker worker,
                                                     LogArchiver logArchiver)
            {
                _logger = logger;
                _behavior = behavior;
                _worker = worker;
                _logArchiver = logArchiver;
            }

            public override void OnWorkerCompleted(EventArgs e)
            {
                var handler = WorkerCompleted;
                if (handler == null) return;
                handler(this, e);
            }

            public override void Start(Service service, LoggingDetails logDetails)
            {
                ToggleService(service, ServiceAction.Start, logDetails);
            }

            public override void Stop(Service service, LoggingDetails logDetails)
            {
                ToggleService(service, ServiceAction.Stop, logDetails);
            }

            public override void Restart(Service service, LoggingDetails logDetails)
            {
                ToggleService(service, ServiceAction.Restart, logDetails);
            }


            public override void RestartAll(ServiceList list)
            {
                ToggleAllServicesTo(ServiceAction.Restart, list);
            }

            public override void StartAll(ServiceList list)
            {
                ToggleAllServicesTo(ServiceAction.Start, list);
            }

            public override void StopAll(ServiceList list)
            {
                ToggleAllServicesTo(ServiceAction.Stop, list);
            }


            public override void RestartGroup(ObservableCollection<Service> items, LoggingDetails logDetails)
            {
                ToggleServiceGroupTo(ServiceAction.Restart, items, logDetails);
            }

            public override void StartGroup(ObservableCollection<Service> items, LoggingDetails logDetails)
            {
                ToggleServiceGroupTo(ServiceAction.Start, items, logDetails);
            }

            public override void StopGroup(ObservableCollection<Service> items, LoggingDetails logDetails)
            {
                ToggleServiceGroupTo(ServiceAction.Stop, items, logDetails);
            }


            private Service GetService(IEnumerable<Service> collection, Guid guid)
            {
                //Contract.Requires(collection != null);

                foreach (var item in collection)
                {
                    if (item.Guid == guid)
                        return item;
                }
                return null;
            }

            private List<Guid> StartStopOrder(ServiceState desiredStatus,
                                              ICollection<Service> collection)
            {
                //Contract.Requires(collection != null);

                if (desiredStatus != ServiceState.Stopped && desiredStatus != ServiceState.Running)
                    throw new ArgumentException("StartStopOrder: desiredStatus must be either Stopped or Running.");

                // Create the output list.
                // The list needs to be exactly as large as the service list.
                var orderedList = new List<Guid>();
                for (var i = 0; i < collection.Count; i++)
                {
                    orderedList.Add(Guid.Empty);
                }

                // Populate the list
                foreach (var service in collection)
                {
                    // Decide which list we're talking about
                    var order = (desiredStatus == ServiceState.Stopped
                                     ? service.Details.StopOrder
                                     : service.Details.StartOrder);

                    if (order == 0)
                    {
                        // If the order is 0, then we don't care when it starts.
                        // Start it after the ones we care about.
                        orderedList.Add(service.Guid);
                    }
                    else
                    {
                        // If the order is non-zero, then we care when it starts.
                        // Start it then.
                        orderedList.Insert(order - 1, service.Guid);
                    }
                }

                // Remove any extras
                for (var i = 0; i < orderedList.Count; i++)
                {
                    if (orderedList[i] != Guid.Empty) continue;

                    orderedList.Remove(orderedList[i]);
                    i--;
                }

                //if (orderedList.Count == 0) throw new ArgumentException("Start/Stop list cannot be empty.");

                // Return the final list
                return orderedList;
            }

            private void ToggleService(Service service, ServiceAction desiredAction, LoggingDetails logDetails)
            {
                var arguments = new List<object> {desiredAction, service, logDetails};
                _worker.Run(_backgroundWorker_ToggleService,
                            _backgroundWorker_RunWorkerCompleted,
                            arguments);
            }

            private void ToggleAllServicesTo(ServiceAction desiredAction, ServiceList serviceList)
            {
                //Contract.Requires(serviceList != null);
                ToggleServiceGroupTo(desiredAction, serviceList.Items, serviceList.LogDetails);
            }

            private void ToggleServiceGroupTo(ServiceAction desiredAction, ObservableCollection<Service> collection,
                                              LoggingDetails logDetails)
            {
                var arguments = new List<object> {desiredAction, collection, logDetails};
                _worker.Run(_backgroundWorker_ToggleGroup,
                            _backgroundWorker_RunWorkerCompleted,
                            arguments);
            }

            private void _backgroundWorker_ToggleService(object sender, DoWorkEventArgs e)
            {
                var genericList = e.Argument as List<object>;
                if (genericList == null) return;
                if (genericList.Count != 3) return;

                // which contains the relevant details.
                var desiredAction = (ServiceAction) genericList[0];
                var service = (Service) genericList[1];
                var logDetails = (LoggingDetails) genericList[2];

                // prepare the result.
                var output = new ServiceState();

                switch (desiredAction)
                {
                    case ServiceAction.Stop:
                        output = _behavior.Stop(service);
                        HandleLogFile(service, logDetails);
                        break;

                    case ServiceAction.Start:
                        output = _behavior.Start(service);
                        break;

                    case ServiceAction.Restart:
                        service.IsRestarting = true;
                        _behavior.Stop(service);
                        HandleLogFile(service, logDetails);
                        output = _behavior.Start(service);
                        service.IsRestarting = false;
                        service.CanToggle = true;
                        break;
                }

                // Return the result.
                e.Result = output;
            }

            private void _backgroundWorker_ToggleGroup(object sender, DoWorkEventArgs e)
            {
                // Get the argument....
                var arguments = e.Argument as List<object>;
                if (arguments == null) return;
                if (arguments.Count != 3) return;

                // which contains the relevant details.
                var desiredAction = (ServiceAction) arguments[0];
                var collection = (ObservableCollection<Service>) arguments[1];
                var logDetails = (LoggingDetails) arguments[2];

                // Get the desired status.
                var desiredStatus = (desiredAction == ServiceAction.Stop
                                         ? ServiceState.Stopped
                                         : ServiceState.Running);
                e.Result = desiredStatus;
                ServiceState result;

                // Get the appropriate list.
                var list = StartStopOrder(desiredStatus, collection);

                // Toggle each service appropriately.
                foreach (var guid in list)
                {
                    // Get the service.
                    var service = GetService(collection, guid);
                    if (service == null) return;
                    // Toggle the service.

                    result = (desiredAction == ServiceAction.Start) ? _behavior.Start(service) : _behavior.Stop(service);

                    // Catch problems...
                    if (result == desiredStatus ||
                        (desiredAction == ServiceAction.Restart && result == ServiceState.Stopped)) continue;

                    _logger.Error(Strings.Error_ServiceNotInDesiredStatus, service.CommonName, desiredStatus);
                    e.Result = result;
                }


                // Handle the logs if necessary.
                if (desiredAction != ServiceAction.Start)
                {
                    // Handle logs
                    HandleLogFiles(collection, logDetails);
                }


                // If we're not supposed to restart the services, then get out.
                if (desiredAction != ServiceAction.Restart) return;


                // Restart the services.
                foreach (var guid in list)
                {
                    // Get the service.
                    var service = GetService(collection, guid);

                    // Restart the service.
                    result = _behavior.Start(service);

                    // Catch problems...
                    if (result == desiredStatus) continue;

                    _logger.Error(Strings.Error_ServiceDidNotRestart, service.CommonName);
                    e.Result = result;
                }
            }

            private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                // Fire an event to re-enable the UI.
                OnWorkerCompleted(e);
            }

            private void HandleLogFile(Service service, LoggingDetails logDetails)
            {
                //Contract.Requires(service != null);
                //Contract.Requires(logDetails != null);
                var collection = new ObservableCollection<Service> {service};

                HandleLogFiles(collection, logDetails);
            }

            private void HandleLogFiles(IEnumerable<Service> collection,
                                        LoggingDetails logDetails)
            {
                //Contract.Requires(collection != null);
                //Contract.Requires(logDetails != null);

                if (logDetails.IgnoreLogs)
                {
                    _logger.Info(Strings.Info_IgnoringLogs);
                    return;
                }

                // Create a list of log files.
                var fileList = new List<FileInfoWrapper>();

                foreach (var service in collection)
                {
                    //var service = GetService(collection, guid);

                    foreach (var item in service.LogFiles)
                    {
                        var filename = item.ParsedName;
                        fileList.Add(FileInfoWrapper.GetInstance(filename));
                    }
                }

                _logArchiver.ArchiveAndClearLogs(fileList, logDetails);
            }


            public override void RefreshStatus(Service service)
            {
                _behavior.Refresh(service);
            }

            public override void Unsubscribe(Service service)
            {
                _behavior.Unsubscribe(service);
            }
        }
    }
}