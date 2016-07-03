// -----------------------------------------------------------------------
//  <copyright file="MonitorEndpoint.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ServiceModel;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;
using ServiceSentry.Monitor.Mediator;

#endregion

namespace ServiceSentry.Monitor.Infrastructure
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true)]
    public abstract class MonitorEndpoint : IMonitorService
    {
        internal abstract TrackingList State { get; }
        public abstract bool GetCanStop(string serviceName);
        public abstract string GetDisplayName(string serviceName);
        public abstract bool GetIsInstalled(string serviceName);
        public abstract PollResult GetStatus(string serviceName);

        public abstract void Start(SubscriptionPacket serviceData);
        public abstract void Stop(SubscriptionPacket serviceData);
        public abstract void Refresh(SubscriptionPacket serviceData);
        public abstract void WaitForStatus(SubscriptionPacket serviceData, ServiceState desiredStatus);
        public abstract SubscriptionPacket[] GetServices();
        public abstract void Subscribe(SubscriptionPacket serviceData);
        public abstract void Unsubscribe(string serviceName);
        public abstract void UpdateSubscription(SubscriptionPacket serviceData);

        public static MonitorEndpoint GetInstance(Logger logger, ConsoleHarness harness, ref TrackingList state)
        {
            return GetInstance(logger, harness,
                               ref state,
                               MonitorClassFactory.GetInstance(logger),
                               LocalServiceFinder.Default,
                               ConfigFileHandler.GetInstance(logger),
                               ModelClassFactory.GetInstance(logger));
        }

        internal static MonitorEndpoint GetInstance(Logger logger,
                                                    ConsoleHarness harness,
                                                    ref TrackingList state,
                                                    MonitorClassFactory factory,
                                                    LocalServiceFinder reader,
                                                    ConfigFileHandler handler, ModelClassFactory modelFactory)
        {
            return new Implementation(logger, harness, ref state, factory, reader, handler, modelFactory);
        }

        public abstract void Shutdown(object sender, EventArgs e);
        public abstract void Startup(object sender, EventArgs e);

        private sealed class Implementation : MonitorEndpoint
        {
            private readonly MonitorClassFactory _factory;
            private readonly ConfigFileHandler _fileHandler;
            private readonly ConsoleHarness _harness;
            private readonly Logger _logger;
            private readonly ModelClassFactory _modelFactory;
            private readonly LocalServiceFinder _reader;
            private TrackingList _state;

            internal Implementation(Logger logger,
                                    ConsoleHarness harness,
                                    ref TrackingList state,
                                    MonitorClassFactory factory,
                                    LocalServiceFinder reader,
                                    ConfigFileHandler handler,
                                    ModelClassFactory modelFactory)
            {
                _logger = logger;
                _state = state;
                _harness = harness;
                _factory = factory;
                _modelFactory = modelFactory;
                _reader = reader;
                _fileHandler = handler;
            }

            internal override TrackingList State
            {
                get { return _state; }
            }

            public override bool GetCanStop(string serviceName)
            {
                using (
                    var controller = _factory.GetController(new SubscriptionPacket {ServiceName = serviceName},
                                                            _modelFactory, _harness))
                {
                    return controller.CanStop;
                }
            }

            public override string GetDisplayName(string serviceName)
            {
                using (
                    var controller = _factory.GetController(new SubscriptionPacket {ServiceName = serviceName},
                                                            _modelFactory, _harness))
                {
                    return controller.DisplayName;
                }
            }

            public override bool GetIsInstalled(string serviceName)
            {
                using (
                    var controller = _factory.GetController(new SubscriptionPacket {ServiceName = serviceName},
                                                            _modelFactory, _harness))
                {
                    return controller.IsInstalled;
                }
            }

            public override PollResult GetStatus(string serviceName)
            {
                try
                {
                    var packet = new SubscriptionPacket {ServiceName = serviceName};
                    using (var controller = _factory.GetController(packet, _modelFactory, _harness))
                    {
                        if (!State.ContainsKey(serviceName))
                        {
                            Subscribe(packet);
                        }

                        var result = PollResult.GetInstance(serviceName,
                                                            controller.Status,
                                                            State[serviceName].Exceptions);

                        State[serviceName].Exceptions.Clear();

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                    _harness.WriteLine(ex.Message);
                    throw;
                }
            }

            public override void Start(SubscriptionPacket serviceData)
            {
                using (var controller = _factory.GetController(serviceData, _modelFactory, _harness))
                {
                    _logger.Debug(Strings.Debug_EndpointReceivedSTART, serviceData.ServiceName);
                    controller.Start(ref _state);
                }
            }

            public override void Stop(SubscriptionPacket serviceData)
            {
                using (var controller = _factory.GetController(serviceData, _modelFactory, _harness))
                {
                    _logger.Debug(Strings.Debug_EndpointReceivedSTOP, serviceData.ServiceName);
                    controller.Stop(ref _state);
                }
            }

            public override void Refresh(SubscriptionPacket serviceData)
            {
                try
                {
                    using (var controller = _factory.GetController(serviceData, _modelFactory, _harness))
                    {
                        controller.Refresh(serviceData.ServiceName, _state);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
            }

            public override void WaitForStatus(SubscriptionPacket serviceData, ServiceState desiredStatus)
            {
                using (var controller = _factory.GetController(serviceData, _modelFactory, _harness))
                {
                    controller.WaitForStatus(desiredStatus, serviceData.Timeout);
                }
            }

            public override SubscriptionPacket[] GetServices()
            {
                return _reader.GetServices();
            }

            public override void Subscribe(SubscriptionPacket serviceData)
            {
                using (var controller = _factory.GetController(serviceData, _modelFactory, _harness))
                {
                    _logger.Debug(Strings.Debug_EndpointReceivedSubscription, serviceData.ServiceName);
                    controller.Subscribe(_state);
                }
            }

            public override void Unsubscribe(string serviceName)
            {
                var packet = new SubscriptionPacket {ServiceName = serviceName};
                using (var controller = _factory.GetController(packet, _modelFactory, _harness))
                {
                    _logger.Debug(Strings.Debug_EndpointReceivedUnsubscription, serviceName);
                    controller.Unsubscribe(ref _state);
                }
            }

            public override void UpdateSubscription(SubscriptionPacket serviceData)
            {
                using (var controller = _factory.GetController(serviceData, _modelFactory, _harness))
                {
                    _logger.Debug(Strings.Debug_EndpointReceivedUpdate, serviceData.ServiceName);
                    controller.UpdateSubscription(serviceData, ref _state);
                }
            }

            public override void Startup(object sender, EventArgs e)
            {
                try
                {
                    _harness.WriteLine(ConsoleColor.White, Strings.Status_ReadingConfigFile);
                    var file = _fileHandler.ReadConfigFile();
                    _harness.WriteLine(ConsoleColor.White, Strings.Status_Done);

                    _logger.Configure(file.DebugLogConfiguration);

                    _logger.Trace(Strings.Trace_Subscribing, file.Services.Items.Count);
                    _harness.WriteLine(Strings.Trace_Subscribing, file.Services.Items.Count);

                    // Load the log files here

                    foreach (var item in file.Services.Items)
                    {
                        var subscriptionData = new SubscriptionPacket
                            {
                                LogFiles = item.LogFiles,
                                ArchiveLogs = file.LogDetails.ArchiveLogs,
                                ClearLogs = file.LogDetails.ClearLogs,
                                DisplayName = item.DisplayName,
                                ServiceName = item.ServiceName,
                                Timeout = item.Details.Timeout == 0
                                              ? TimeSpan.MaxValue
                                              : new TimeSpan(0, 0, 0, item.Details.Timeout),
                                NotifyOnUnexpectedStop = item.Details.NotifyOnUnexpectedStop,
                                EmailInfo = file.NotificationDetails.EmailInfo,
                                SMTPInfo = file.NotificationDetails.SMTPInfo,
                            };

                        using (var controller = _factory.GetController(subscriptionData, _modelFactory, _harness))
                        {
                            controller.UpdateSubscription(subscriptionData, ref _state);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                    _harness.WriteToConsole(ConsoleColor.Red, Strings.EXCEPTION, ex.Message);
                }
            }

            public override void Shutdown(object sender, EventArgs e)
            {
                _logger.Trace(Strings.Debug_ShuttingDown);
                _harness.WriteLine(Strings.Debug_ShuttingDown);

                foreach (var item in _state)
                {
                    Unsubscribe(item.Key);
                    item.Value.Timer.Enabled = false;
                    item.Value.Timer.Dispose();
                }

                _logger.Trace(Strings.Debug_UnsubscribeAll);
                _harness.WriteLine(Strings.Debug_UnsubscribeAll);

                _state = null;

                _logger.Trace(Strings.Debug_WatchlistDisposed);
                _harness.WriteLine(Strings.Debug_WatchlistDisposed);
            }
        }
    }
}