// -----------------------------------------------------------------------
//  <copyright file="ServerLocalMediator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;

#endregion

namespace ServiceSentry.Monitor.Mediator
{
    public sealed class ServerLocalMediator : ServerMediator
    {
        private readonly ServiceWrapper _controller;
        private readonly List<Exception> _exceptions;
        private readonly ModelClassFactory _factory;
        private readonly ConsoleHarness _harness;
        private readonly Logger _logger;
        private readonly Responder _responder;
        private readonly SubscriptionPacket _serviceData;
        private readonly string _serviceName;
        private readonly int _trackingInterval;

        public ServerLocalMediator(Logger logger,
                                   SubscriptionPacket serviceData,
                                   ModelClassFactory factory,
                                   ConsoleHarness harness,
                                   int trackingInterval)
        {
            _logger = logger;
            _exceptions = new List<Exception>();
            _harness = harness;
            _factory = factory;
            _serviceData = serviceData;
            _serviceName = serviceData.ServiceName;

            _controller = _factory.GetLocalServiceController(_serviceName);

            _responder = _factory.GetResponder();
            _trackingInterval = trackingInterval;
        }

        public override bool CanStop
        {
            get { return _controller.CanStop; }
        }

        public override string DisplayName
        {
            get { return _controller.DisplayName; }
        }

        public override bool IsInstalled
        {
            get { return _controller.IsInstalled; }
        }

        public override ServiceState Status
        {
            get
            {
                try
                {
                    if (_controller == null)
                    {
                        _harness.WriteLine(Strings.EXCEPTION_ControllerIsNull);
                        return ServiceState.Stopped;
                    }

                    _controller.Refresh();
                    return _controller.Status;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                    _harness.WriteLine(Strings.EXCEPTION, ex.Message);
                    return ServiceState.Stopped;
                }
            }
        }

        public override string ServiceName
        {
            get { return _serviceName; }
        }

        public override string MachineName
        {
            get { return Environment.MachineName; }
        }

        public override List<Exception> Exceptions
        {
            get { return _exceptions; }
        }

        public override void Start(ref TrackingList state)
        {
            try
            {
                state[ServiceName].IsToggling = true;
                _controller.Start();
                _controller.WaitForStatus(ServiceState.Running,
                                          _serviceData.Timeout);

                if (_controller.Status == ServiceState.Running)
                {
                    _logger.Info(Model.Strings.Info_ServiceStarted, _serviceName);
                    _harness.WriteLine(Model.Strings.Info_ServiceStarted, _serviceName);
                }

                Refresh(ServiceName, state);

                state[ServiceName].IsStopped = false;
                state[ServiceName].IsToggling = false;
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                _harness.WriteLine(ex.Message);
            }
        }

        public override void WaitForStatus(ServiceState desiredStatus, TimeSpan timeout)
        {
            _controller.WaitForStatus(desiredStatus, timeout);
        }

        public override void Stop(ref TrackingList state)
        {
            try
            {
                state[ServiceName].IsToggling = true;
                state[ServiceName].Timer.Enabled = false;

                _controller.Stop();
                _controller.WaitForStatus(ServiceState.Stopped,
                                          _serviceData.Timeout);

                Refresh(ServiceName, state);

                if (_controller.Status == ServiceState.Stopped)
                {
                    _logger.Info(Model.Strings.Info_ServiceStopped, _serviceName);
                    _harness.WriteLine(Model.Strings.Info_ServiceStopped, _serviceName);
                }
                state[ServiceName].LastState = _controller.Status;
                state[ServiceName].IsStopped = true;
                state[ServiceName].Timer.Enabled = true;
                state[ServiceName].IsToggling = false;
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                var name = ServiceName;
                _harness.Write(name + ": ");
                _harness.WriteLine(ex.Message);
            }
        }

        public override void Refresh(string serviceName, TrackingList state)
        {
            try
            {
                _harness.WriteLine(Strings.Debug_ServerMediatorRefresh, serviceName);

                var oldStatus = state[serviceName].LastState;
                var newStatus = Status;
                state[serviceName].LastState = newStatus;

                CheckForServiceFailure(oldStatus, newStatus, state[serviceName]);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                _harness.WriteLine("FAILED.");
                _harness.WriteLine(ex.Message);
            }
        }

        internal void CheckForServiceFailure(ServiceState oldState, ServiceState newState, TrackingObject state)
        {
            try
            {
                var isToggling = state.IsToggling;
                if (oldState == newState || isToggling) return;

                state.IsStopped = (newState != ServiceState.Running);

                if (newState == ServiceState.Stopped ||
                    newState == ServiceState.StopPending)
                {
                    _responder.HandleFailure(state);
                }

                foreach (var ex in _responder.Exceptions)
                {
                    _logger.ErrorException(ex);
                    _exceptions.Add(ex);
                    _harness.WriteLine(ConsoleColor.Red,
                                       "-------- Exception added: {0}",
                                       ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                _harness.WriteLine(ConsoleColor.Red, "-------- Exception added: {0}", ex.Message);
            }
        }


        public override void Unsubscribe(ref TrackingList state)
        {
            _logger.Trace(Model.Strings.Info_UnsubscribingFromService, ServiceName);
            _harness.Write(Model.Strings.Info_UnsubscribingFromService, ServiceName);
            Trace.WriteLine(Model.Strings.Info_UnsubscribingFromService, ServiceName);
            try
            {
                state[ServiceName].Timer.Enabled = false;
                //state[ServiceName].Timer.Dispose();

                _logger.Info(Model.Strings.Info_Unsubscribed, ServiceName);
                _harness.WriteLine(Model.Strings.Info_Unsubscribed, ServiceName);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                _harness.WriteLine("FAILED.");
                _harness.WriteLine(ex.Message);
            }
        }

        public override void Subscribe(TrackingList state)
        {
            _logger.Trace(Model.Strings.Info_SubscribingToService, ServiceName);
            _harness.Write(Model.Strings.Info_SubscribingToService, ServiceName);

            try
            {
                var service = new TrackingObject
                    {
                        ServiceName = ServiceName,
                        LastState = Status,
                        NotifyOnUnexpectedStop = _serviceData.NotifyOnUnexpectedStop,
                        Timer = new Timer(_trackingInterval),
                        Packet = _serviceData,
                    };

                state.Add(ServiceName, service);
                state[ServiceName].Timer.Elapsed += (s, e) => Refresh(ServiceName, state);
                state[ServiceName].Timer.Enabled = _serviceData.NotifyOnUnexpectedStop;

                _logger.Info(Model.Strings.Info_Subscribed, ServiceName);
                _harness.WriteLine(Model.Strings.Info_Subscribed, ServiceName);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                _harness.WriteLine("FAILED.");
                _harness.WriteLine(ex.Message);
            }
        }

        public override void UpdateSubscription(SubscriptionPacket packet, ref TrackingList state)
        {
            _logger.Trace(Model.Strings.Info_UpdatingSubscription, ServiceName);
            _harness.Write(Model.Strings.Info_UpdatingSubscription, ServiceName);
            try
            {
                if (!state.ContainsKey(packet.ServiceName))
                {
                    Subscribe(state);
                    return;
                }

                state[packet.ServiceName].Packet = packet;
                state[packet.ServiceName].NotifyOnUnexpectedStop = packet.NotifyOnUnexpectedStop;
                state[packet.ServiceName].Timer.Enabled = packet.NotifyOnUnexpectedStop;
                
                _logger.Info(Model.Strings.Info_SubscriptionUpdated, ServiceName);
                _harness.WriteLine(Model.Strings.Info_SubscriptionUpdated, ServiceName);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
                _exceptions.Add(ex);
                _harness.WriteLine("FAILED.");
                _harness.WriteLine(ex.Message);
            }
        }
    }
}