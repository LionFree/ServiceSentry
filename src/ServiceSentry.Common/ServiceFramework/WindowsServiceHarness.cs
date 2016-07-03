// -----------------------------------------------------------------------
//  <copyright file="WindowsServiceHarness.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ServiceProcess;
using ServiceSentry.Common.Resources;
using ServiceSentry.ServiceFramework;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    internal abstract class WindowsServiceHarness : ServiceBase
    {
        public static WindowsServiceHarness GetInstance(WindowsService service)
        {
            return new Implementation(service);
        }

        /// <summary>
        /// Provides the main entry point for a service executable.
        /// </summary>
        public abstract void Run();

        private sealed class Implementation : WindowsServiceHarness
        {
            private readonly WindowsService _implementer;

            // internal constructor to take in the implementation to delegate to
            internal Implementation(WindowsService serviceImplementation)
            {
                if (serviceImplementation == null)
                {
                    throw new ArgumentNullException("serviceImplementation", CommonStrings.EXCEPTION_IWindowsServiceCannotBeNull);
                }

                // set instance
                _implementer = serviceImplementation;

                // Configure our service
                ConfigureServiceFromAttributes(serviceImplementation);
            }

            // because all of these available overrides are protected, we can't
            // call them directly from our console harness, so instead we will
            // just delegate to the IWindowsService interface which is internal.
            protected override void OnStart(string[] args)
            {
                _implementer.OnStart(args);
            }

            protected override void OnStop()
            {
                _implementer.OnStop();
            }

            protected override void OnPause()
            {
                _implementer.OnPause();
            }

            protected override void OnShutdown()
            {
                _implementer.OnShutdown();
            }

            private void ConfigureServiceFromAttributes(WindowsService serviceImplementation)
            {
                var attribute = serviceImplementation.GetType().GetAttribute<WindowsServiceAttribute>();

                if (attribute != null)
                {
                    EventLog.Source = string.IsNullOrEmpty(attribute.EventLogSource)
                                          ? "WindowsServiceHarness"
                                          : attribute.EventLogSource;

                    CanStop = attribute.CanStop;
                    CanPauseAndContinue = attribute.CanPauseAndContinue;
                    CanShutdown = attribute.CanShutdown;

                    // we don't handle: laptop power change event
                    CanHandlePowerEvent = false;

                    // we don't handle: Term Services session event
                    CanHandleSessionChangeEvent = false;

                    // always auto-event-log
                    AutoLog = true;
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format("WindowsService implementer {0} must have a WindowsServiceAttribute.",
                                      serviceImplementation.GetType().FullName));
                }
            }


            public override void Run()
            {
                Run(this);
            }
        }
    }
}