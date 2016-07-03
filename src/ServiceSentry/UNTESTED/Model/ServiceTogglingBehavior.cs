// -----------------------------------------------------------------------
//  <copyright file="ServiceTogglingBehavior.cs" company="Accelrys, Inc.">
//      Copyright (c) 2014 Accelrys, Inc.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.Globalization;
using ServiceSentry.Model.DataClasses;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events.Arguments;
using ServiceSentry.Model.Wrappers;

#endregion

namespace ServiceSentry.Shit.Model
{
    public abstract class ServiceTogglingBehavior
    {
        public static ServiceTogglingBehavior GetInstance()
        {
            return new Implementation();
        }

        public abstract ServiceState Start(Service service);
        public abstract ServiceState Stop(Service service);
        public abstract void Refresh(Service service);

        private sealed class Implementation : ServiceTogglingBehavior
        {
            private const double GlobalTimeout = 30;

            public override ServiceState Stop(Service service)
            {
                var mediator = ServiceMediator.GetInstance(service.ServiceName, service.MachineName);

                if (mediator.Status != ServiceState.Running)
                {
                    Debug.WriteLine("Service not running.");
                    return ServiceState.Stopped;
                }

                try
                {
                    if (!mediator.CanStop) return mediator.Status;

                    Debug.Write("  Stopping service: " + service.ServiceName + "...  ");
                    service.IsReceivingInternalUpdate = true;
                    service.CanToggle = false;

                    mediator.Stop();


                    var success = WaitForStatus(service, ServiceState.Stopped);
                    if (success) Debug.WriteLine("  service stopped.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Stop EXCEPTION: " + ex.Message);
                }


                mediator.Refresh();

                if (!service.IsRestarting) service.CanToggle = true;
                service.IsReceivingInternalUpdate = false;

                return mediator.Status;
            }

            public override ServiceState Start(Service service)
            {
                var mediator = ServiceMediator.GetInstance(service.ServiceName, service.MachineName);

                if (mediator.Status == ServiceState.Running)
                {
                    Debug.WriteLine("Service already running: " + service.ServiceName);
                    return mediator.Status;
                }

                try
                {
                    if (mediator.Status == ServiceState.Stopped)
                    {
                        Debug.Write("  Starting service: " + service.ServiceName + "...  ");

                        service.IsReceivingInternalUpdate = true;
                        service.CanToggle = false;
                        mediator.Start();
                        var success = WaitForStatus(service, ServiceState.Running);

                        if (success) Debug.WriteLine("  service started.");
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Start EXCEPTION: " + ex.Message);
                }

                Refresh(service);

                service.CanToggle = true;
                service.IsReceivingInternalUpdate = false;
                return mediator.Status;
            }

            public override void Refresh(Service service)
            {
                var mediator = ServiceMediator.GetInstance(service.ServiceName, service.MachineName);

                var oldStatus = mediator.Status;
                mediator.Refresh();
                var newStatus = mediator.Status;
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


            private bool WaitForStatus(Service service, ServiceState desiredStatus,
                                       int tickCount = 0)
            {
                var mediator = ServiceMediator.GetInstance(service.ServiceName, service.MachineName);

                try
                {
                    if (service.Details.Timeout == 0)
                    {
                        // If the service immediately crashes, that's a problem.
                        Refresh(service);

                        if (mediator.Status != desiredStatus)
                        {
                            // wait for GlobalTimeout (30) seconds.
                            mediator.WaitForStatus(desiredStatus, TimeSpan.FromSeconds(GlobalTimeout));
                            return true;
                        }

                        mediator.WaitForStatus(desiredStatus, TimeSpan.MaxValue);
                    }
                    else
                    {
                        var timeout = TimeSpan.FromMilliseconds(TimeoutDouble(service.Details.Timeout, tickCount));
                        mediator.WaitForStatus(desiredStatus, timeout);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    // Something has happened, and the service won't toggle.
                    // Perhaps a prerequisite service is stopped?
                    Trace.WriteLine(" ");
                    Trace.WriteLine("  EXCEPTION: " + ex.Message);

                    // TODO: Raise an event to indicate that the service could not be started.
                    //var text = desiredStatus == ServiceControllerStatus.Stopped ? "stopped" : "started";
                    //Dialogs.ShowError("\"" + CommonName + "\" could not be " + text + ":\n" + ex.Message + "\n\nPerhaps a prerequisite service is not running?");
                    return false;
                }
            }

            private double TimeoutDouble(int timeout, double start)
            {
                if (timeout == 0) return 0;

                var tick = Environment.TickCount;

                var used = Math.Abs(start - 0) < Double.Epsilon ? 0 : tick - start;

                var current = double.Parse(timeout.ToString(CultureInfo.CurrentCulture));

                return current - used;
            }
        }
    }
}