// -----------------------------------------------------------------------
//  <copyright file="ServiceTogglingBehavior.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;

#endregion

namespace ServiceSentry.Model.Client
{
    public abstract class ServiceTogglingBehavior
    {
        internal const double GlobalTimeout = 30;

        internal static ServiceTogglingBehavior GetInstance(MonitorServiceWatchdog monitor, Logger logger)
        {
            return new ServiceTogglingBehaviorImplementation(monitor, logger);
        }

        public abstract ServiceState Start(Service service);
        public abstract ServiceState Stop(Service service);
        public abstract void Refresh(Service service);

        private sealed class ServiceTogglingBehaviorImplementation : ServiceTogglingBehavior
        {
            private readonly Logger _logger;
            private readonly MonitorServiceWatchdog _monitor;


            internal ServiceTogglingBehaviorImplementation(MonitorServiceWatchdog monitor, Logger logger)
            {
                _logger = logger;
                _monitor = monitor;
            }

            public override void Unsubscribe(Service service)
            {
                service.Unsubscribe(_monitor);
            }

            public override ServiceState Stop(Service service)
            {
                if (service.Status != ServiceState.Running)
                {
                    return ServiceState.Stopped;
                }

                try
                {
                    if (!service.CanStop) return service.Status;

                    service.IsReceivingInternalUpdate = true;
                    service.CanToggle = false;

                    service.Stop(_monitor);

                    WaitForStatus(service, ServiceState.Stopped);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex, Strings.EXCEPTION_StoppingService, ex.Message);
                }


                service.Refresh(_monitor);

                if (!service.IsRestarting) service.CanToggle = true;
                service.IsReceivingInternalUpdate = false;

                return service.Status;
            }

            public override ServiceState Start(Service service)
            {
                if (service.Status == ServiceState.Running)
                {
                    _logger.Info(Strings.Info_ServiceAlreadyRunning, service.ServiceName);
                    return service.Status;
                }

                try
                {
                    if (service.Status == ServiceState.Stopped)
                    {
                        _logger.Info(Strings.Info_StartingService, service.ServiceName);

                        service.IsReceivingInternalUpdate = true;
                        service.CanToggle = false;
                        service.Start();
                        var success = WaitForStatus(service, ServiceState.Running);

                        if (success)
                        {
                            _logger.Info(Strings.Info_ServiceStarted, service.ServiceName);
                        }
                        else
                        {
                            _logger.Error(Strings.Error_ServiceNotStarted, service.ServiceName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }

                Refresh(service);

                service.CanToggle = true;
                service.IsReceivingInternalUpdate = false;
                return service.Status;
            }

            public override void Refresh(Service service)
            {
                var oldStatus = service.Status;
                service.Refresh(_monitor);
                var newStatus = service.Status;
                service.OnPropertyChanged("Status");

                if (oldStatus == newStatus || service.IsReceivingInternalUpdate) return;

                // The value has changed, and the change is external.
                var args = new StatusChangedEventArgs
                    {
                        NewStatus = newStatus,
                        OldStatus = oldStatus
                    };

                if (service.Details.NotifyOnUnexpectedStop &&
                    (newStatus == ServiceState.Stopped ||
                     newStatus == ServiceState.StopPending))
                {
                    service.OnServiceFellOver(args);
                    return;
                }

                service.OnExternalStatusChange(args);
            }


            private bool WaitForStatus(Service service, ServiceState desiredStatus)
            {
                //Contract.Requires(service != null);

                try
                {
                    if (service.Details.Timeout == 0)
                    {
                        // If the service immediately crashes, that's a problem.
                        Refresh(service);

                        //if (service.Status != desiredStatus)
                        //{
                        //    // wait for GlobalTimeout (30) seconds.
                        //    service.WaitForStatus(desiredStatus, TimeSpan.FromSeconds(GlobalTimeout));
                        //    return true;
                        //}

                        service.WaitForStatus(desiredStatus);
                    }
                    else
                    {
                        //var timeout = TimeSpan.FromMilliseconds(TimeoutDouble(service.Details.Timeout, tickCount));
                        service.WaitForStatus(desiredStatus);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    // Something has happened, and the service won't toggle.
                    // Perhaps a prerequisite service is stopped?
                    _logger.ErrorException(ex, Strings.Error_ServiceWontToggle, ex.Message);

                    return false;
                }
            }

            //private double TimeoutDouble(int timeout, double start)
            //{
            //    if (timeout == 0) return 0;

            //    var tick = Environment.TickCount;

            //    var used = Math.Abs(start - 0) < Double.Epsilon ? 0 : tick - start;

            //    var current = double.Parse(timeout.ToString(CultureInfo.CurrentCulture));

            //    return current - used;
            //}
        }

        public abstract void Unsubscribe(Service service);
    }
}