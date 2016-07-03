// -----------------------------------------------------------------------
//  <copyright file="ClientLocalMediator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using System.Net;
using System.ServiceModel;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;

#endregion

namespace ServiceSentry.Model.Client
{
    internal sealed class ClientLocalMediator : ClientMediator
    {
        private readonly ModelClassFactory _factory;
        private readonly ServiceWrapper _localController;
        private readonly Logger _logger;
        private readonly string _machineName;
        private readonly Responder _responder;
        private SubscriptionPacket _serviceData;
        private readonly string _serviceName;
        private ServiceClient<IMonitorService> _client;
        private bool _isToggling;
        private readonly ClientList _clientList;

        internal ClientLocalMediator(SubscriptionPacket serviceData, ClientList clients, ModelClassFactory factory,
                                     Logger logger)
        {
            //Contract.Requires(serviceData != null);
            //Contract.Requires(clients != null);
            //Contract.Requires(factory != null);
            _logger = logger;
            _factory = factory;
            _serviceData = serviceData;
            _serviceName = serviceData.ServiceName;
            _machineName = serviceData.MachineName;
            _clientList = clients;
            _client = _clientList.GetClient(_machineName);

            _localController = _factory.GetLocalServiceController(_serviceName);
            _responder = _factory.GetResponder();

            if (_client.IsAvailable)
                _client.Execute(_client.Service.UpdateSubscription, serviceData);
        }

        public override bool CanStop
        {
            get { return _localController.CanStop; }
        }

        public override string DisplayName
        {
            get { return _localController.DisplayName; }
        }

        public override bool IsInstalled
        {
            get { return _localController.IsInstalled; }
        }

        public override ServiceState Status
        {
            get { return _localController.Status; }
        }

        public override string ServiceName
        {
            get { return _serviceName; }
        }

        public override string MachineName
        {
            get { return _machineName; }
        }

        public override void Start()
        {
            _localController.Start();
            _localController.WaitForStatus(ServiceState.Running);
        }

        public override void Stop(MonitorServiceWatchdog monitor)
        {
            var monitorAvailable = monitor.IsAvailable;
            ServiceState newState;
            if (monitorAvailable)
            {
                _client.Execute(_client.Service.Stop, _serviceData);
                var result = _client.Service.GetStatus(_serviceName);
                newState = result.State;
            }
            else
            {
                _isToggling = true;
                _localController.Stop();
                _localController.WaitForStatus(ServiceState.Stopped,
                                               _serviceData.Timeout);
                _isToggling = false;

                Refresh(monitor);
                newState = _localController.Status;
            }


            if (newState == ServiceState.Stopped)
            {
                _logger.Info(monitorAvailable
                        ? Strings.Info_ServiceStoppedRemotely
                        : Strings.Info_ServiceStoppedLocally,
                    _serviceName);
            }
            else
            {
                _logger.Error(monitorAvailable
                                  ? Strings.Error_ServiceNotStoppedRemotely
                                  : Strings.Error_ServiceNotStoppedLocally,
                              _serviceName);
            }
        }
        
        public override void Refresh(MonitorServiceWatchdog monitor)
        {
            var oldStatus = _localController.Status;
            _localController.Refresh();

            if (monitor.IsAvailable)
            {
                DisplayMonitorExceptions();
            }
            else
            {
                var newStatus = Status;
                if (oldStatus == newStatus || _isToggling) return;
                if (newStatus == ServiceState.Stopped ||
                    newStatus == ServiceState.StopPending)
                {
                    _responder.HandleFailure(new TrackingObject
                        {
                            ServiceName = ServiceName,
                            NotifyOnUnexpectedStop = _serviceData.NotifyOnUnexpectedStop,
                            Packet = _serviceData,
                        });
                }
            }
        }

        public override void DisplayMonitorExceptions()
        {
            var machine = MachineName == "." ? Dns.GetHostEntry("LocalHost").HostName : MachineName;
            try
            {
                var pollResult = _client.Service.GetStatus(ServiceName);
                var count = pollResult.Exceptions.Length;
                if (count > 0)
                {
                    OnMonitorError(pollResult);
                }
            }
            catch (CommunicationObjectFaultedException ex)
            {
                
                _logger.ErrorException(ex,Strings.Error_CommunicationFaulted,ServiceName, machine);
                _client = _clientList.RefreshClient(_machineName);
            }

            catch (ProtocolException ex)
            {
                _logger.ErrorException(ex, Strings.Error_AcquiringNewClient, ServiceName, machine);
                _client = _clientList.RefreshClient(_machineName);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.ErrorException(ex, Strings.Error_AcquiringNewClient, ServiceName, machine);
                _client = _clientList.RefreshClient(_machineName);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
            }
        }

        public override void WaitForStatus(ServiceState desiredStatus)
        {
            _localController.WaitForStatus(desiredStatus, _serviceData.Timeout);
        }

        public override event MonitorErrorEventHandler MonitorError;

        public override void UpdateSubscription(MonitorServiceWatchdog monitor, SubscriptionPacket packet)
        {
            _logger.Info(Strings.Info_UpdatingSubscription, ServiceName);
            _serviceData = packet;

            var monitorAvailable = monitor.IsAvailable;
            if (monitorAvailable)
            {
                _client.Execute(_client.Service.Unsubscribe, _serviceName);
            }
            _client.Execute(_client.Service.UpdateSubscription, packet);
        }

        public override void Unsubscribe(MonitorServiceWatchdog monitor)
        {
            var monitorAvailable = monitor.IsAvailable;
            if (monitorAvailable)
            {
                _client.Execute(_client.Service.Unsubscribe, _serviceName);
            }
        }

        internal override void OnMonitorError(PollResult pollResult)
        {
            _logger.Warn(Strings.Warn_ReceivedExceptionsFromMonitor, pollResult.Exceptions.Length);

            foreach (var item in pollResult.Exceptions)
            {
                _logger.ErrorException(item);
            }

            var handler = MonitorError;
            if (handler == null) return;

            var args = new MonitorErrorEventArgs
                {
                    Exceptions = pollResult.Exceptions,
                    ServiceName = pollResult.ServiceName,
                    MachineName = Environment.MachineName,
                };

            handler(this, args);
        }
    }
}