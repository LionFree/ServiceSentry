// -----------------------------------------------------------------------
//  <copyright file="MonitorService.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ServiceModel;
using System.ServiceProcess;
using ServiceSentry.Common.Communication;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Server;
using ServiceSentry.Monitor.Mediator;
using ServiceSentry.ServiceFramework;

#endregion

namespace ServiceSentry.Monitor.Infrastructure
{
    internal abstract class MonitorService : WindowsService
    {
        private const string PrivateServiceName = "ServiceSentry.Monitor";
        private const string PrivateDisplayName = "ServiceSentry Monitor";
        private const string EventLogSourceName = "ServiceSentry Monitor Service";

        private const string Description = "Provides service status updates to ServiceSentry, " +
                                           "and sends notifications when monitored services " +
                                           "stop unexpectedly.";

        /// <summary>
        ///     Creates a new instance of the <see cref="MonitorService" />
        ///     using the designated <see cref="Logger" />.
        /// </summary>
        internal static MonitorService GetInstance(Logger logger)
        {
            return GetInstance(logger, MonitorClassFactory.GetInstance(logger));
        }

        internal static MonitorService GetInstance(Logger logger, MonitorClassFactory factory)
        {
            return new Implementation(logger, factory);
        }

        public event EventHandler Started;
        public event EventHandler Stopped;

        [WindowsService(PrivateServiceName,
            CanPauseAndContinue = false,
            CanShutdown = false,
            CanStop = true,
            Description = Description,
            DisplayName = PrivateDisplayName,
            EventLogSource = EventLogSourceName,
            EventLogName = PrivateServiceName,
            ServiceName = PrivateServiceName,
            StartMode = ServiceStartMode.Automatic)]
        private sealed class Implementation : MonitorService
        {
            private readonly WindowsServiceAttribute _configuration;
            private readonly MonitorClassFactory _factory;
            private readonly ConsoleHarness _harness;
            private readonly Logger _logger;
            private readonly MonitorEndpoint _service;
            private readonly TrackingList _state;
            private string _endpoint;
            private ServiceHost _serviceHost;

            internal Implementation(Logger logger, MonitorClassFactory factory)
            {
                _logger = logger;
                _factory = factory;
                _configuration = GetType().GetAttribute<WindowsServiceAttribute>();
                _harness = _factory.GetHarness();
                _state = _factory.GetState();
                _service = _factory.GetEndpoint(_harness, ref _state);

                BuildHost();
            }

            public override ConsoleHarness Harness
            {
                get { return _harness; }
            }

            public override string ServiceName
            {
                get { return _configuration.ServiceName; }
            }

            public override string Endpoint
            {
                get { return _endpoint; }
            }

            private void BuildHost()
            {
                if (_serviceHost != null)
                {
                    _serviceHost.Close();
                }

                var serviceDescription = new WindowsServiceDescription
                    {
                        ServiceObject = _service,
                        Contract = typeof (IMonitorService),
                        EndpointSuffix = _configuration.ServiceName
                    };

                // Create a ServiceHost for the MonitorService type and 
                // provide the base address.
                _serviceHost = _factory.GetHost(serviceDescription);
                _serviceHost.Opened += _service.Startup;
                _serviceHost.Closing += _service.Shutdown;
                _endpoint = _serviceHost.Description.Endpoints[0].Address.ToString();
            }

            public override void OnStart(string[] args)
            {
                // Open the ServiceHostBase to create listeners and start 
                // listening for messages.
                try
                {
                    _logger.Info(Model.Strings.Info_StartingService, _configuration.ServiceName);
                    _serviceHost.Open();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
                OnStarted();
            }

            public override void OnCustomCommand(int command)
            {
                // pass the custom command to the service.
                switch ((ServiceCustomCommands) command)
                {
                    case (ServiceCustomCommands.ReportEndpoint):
                        Harness.WriteToConsole(ConsoleColor.White, Strings.Noun_Endpoint, Endpoint);
                        break;

                    default:
                        Harness.WriteToConsole(ConsoleColor.White, Strings.Error_InvalidCommand, command);
                        break;
                }
            }

            public override void OnStop()
            {
                _logger.Info(Model.Strings.Info_StoppingService, _configuration.ServiceName);
                Harness.WriteLine(Model.Strings.Info_StoppingService, _configuration.ServiceName);
                if (_serviceHost == null) return;

                try
                {
                    _serviceHost.Close();
                    _serviceHost = null;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                    Harness.WriteLine(Strings.EXCEPTION, ex.Message);
                }
                OnStopped();
            }

            private void HandleException(Exception ex)
            {
                if (ex is AddressAlreadyInUseException)
                {
                    _logger.ErrorException(ex, Strings.Error_CannotOpenHost_AddressInUse,
                                           _configuration.ServiceName);

                    Harness.WriteToConsole(ConsoleColor.White,
                                           Strings.Error_CannotOpenHost_AddressInUse,
                                           _configuration.ServiceName);
                }
                else if (ex is AddressAccessDeniedException)
                {
                    _logger.ErrorException(ex, Strings.Error_CannotOpenHost_AccessDenied,
                                           _configuration.ServiceName);

                    Harness.WriteToConsole(ConsoleColor.White,
                                           Strings.Error_CannotOpenHost_AccessDenied,
                                           _configuration.ServiceName);
                }
                else
                {
                    _logger.ErrorException(ex, Strings.Error_CannotOpenHost,
                                           _configuration.ServiceName);

                    Harness.WriteToConsole(ConsoleColor.White,
                                           Strings.Error_CannotOpenHost,
                                           _configuration.ServiceName);
                    Harness.WriteToConsole(ConsoleColor.White, Strings.EXCEPTION, ex.Message);
                }

                if (ex.InnerException == null) return;
                _harness.WriteLine(ex.InnerException.Message);
            }


            private void OnStarted()
            {
                var handler = Started;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            private void OnStopped()
            {
                var handler = Stopped;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
                Harness.WriteLine(Strings.Status_ServiceStopped);
            }
        }
    }
}