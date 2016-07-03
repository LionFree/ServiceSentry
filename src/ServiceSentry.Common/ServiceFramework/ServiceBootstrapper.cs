// -----------------------------------------------------------------------
//  <copyright file="ServiceBootstrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using ServiceSentry.Common.CommandLine;
using ServiceSentry.Common.Resources;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.ServiceFramework;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    /// <summary>
    ///     A class that wraps a service endpoint in a service, and starts that service.
    ///     Example: Start a service using
    ///     ServiceBootstrapper.GetInstance(MonitorWindowsService.GetInstance()).Startup(args);
    /// </summary>
    public abstract class ServiceBootstrapper
    {
        /// <summary>
        ///     Creates a new instance of the designated <see cref="WindowsService" />.
        /// </summary>
        /// <param name="service">
        ///     The <see cref="WindowsService" /> in question.
        /// </param>
        /// <param name="logger"></param>
        public static ServiceBootstrapper GetInstance<T>(T service, Logger logger)
            where T : WindowsService
        {
            return GetInstance(service,
                               Environment.UserInteractive,
                               logger,
                               WindowsServiceHarness.GetInstance(service),
                               CommandLineParser.GetInstance(),
                               AssemblyWrapper.Default);
        }

        internal static ServiceBootstrapper GetInstance<T>(T service,
                                                           bool interactive,
                                                           Logger logger,
                                                           WindowsServiceHarness serviceHarness,
                                                           CommandLineParser parser,
                                                           AssemblyWrapper attributes)
            where T : WindowsService
        {
            return new Bootstrapper<T>(service, interactive, logger, serviceHarness, parser, attributes);
        }


        /// <summary>
        ///     Starts the <see cref="WindowsService" /> with no command-line arguments.
        /// </summary>
        public abstract void Startup();

        /// <summary>
        ///     Starts the <see cref="WindowsService" /> with the given command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to pass to the service.</param>
        public abstract void Startup(string[] args);


        private sealed class Bootstrapper<TService> :
            ServiceBootstrapper where TService : WindowsService
        {
            private readonly AssemblyWrapper _attributes;
            private readonly WindowsServiceAttribute _configuration;
            private readonly ConsoleHarness _harness;
            private readonly bool _interactive;
            private readonly Logger _logger;
            private readonly CommandLineParser _parser;
            private readonly WindowsService _service;
            private readonly WindowsServiceHarness _serviceHarness;

            internal Bootstrapper(TService service,
                                  bool interactive,
                                  Logger logger,
                                  WindowsServiceHarness serviceHarness,
                                  CommandLineParser parser,
                                  AssemblyWrapper attributes)
            {
                _logger = logger;
                if (!typeof (WindowsService).IsAssignableFrom(typeof (TService)))
                {
                    _logger.Error(CommonStrings.EXCEPTION_ServiceMustInheritWindowsService);
                    throw new ArgumentException(CommonStrings.EXCEPTION_ServiceMustInheritWindowsService, "service");
                }

                var attribute = service.GetType().GetAttribute<WindowsServiceAttribute>();
                if (attribute == null)
                {
                    _logger.Error(CommonStrings.EXCEPTION_ServiceMustBeMarkedWithAttribute);
                    throw new ArgumentException(CommonStrings.EXCEPTION_ServiceMustBeMarkedWithAttribute, "service");
                }

                _parser = parser;
                _configuration = attribute;
                _service = service;
                _interactive = interactive;
                _serviceHarness = serviceHarness;
                _harness = _service.Harness;
                _attributes = attributes;
            }


            public override void Startup()
            {
                Startup(null);
            }

            public override void Startup(string[] args)
            {
                using (var implementation = _service)
                {
                    if (_interactive)
                    {
                        RunInteractiveSession(implementation, args);
                    }
                    else
                    {
                        _serviceHarness.Run();
                    }
                }
            }


            private void RunInteractiveSession(WindowsService service, string[] args)
            {
                if (Environment.UserInteractive)
                {
                    var description = new ServiceMetadata
                        {
                            Implementation = service,
                            ServiceName = _configuration.ServiceName,
                            EventLogName = _configuration.EventLogName,
                            EventLogSource = _configuration.EventLogSource,
                            LongDescription = _configuration.Description,
                        };

                    ParseArgs(args, description);
                }
            }

            private void ParseArgs(string[] args, ServiceMetadata metadata)
            {
                if (args == null) return;

                if (args.Length == 0)
                {
                    ShowDefaultHelpOnConsole(metadata);
                    return;
                }

                var commandLine = _parser.Parse(args);
                if (commandLine.Length == 0) ShowDefaultHelpOnConsole(metadata);

                if (commandLine["q"] != null || commandLine["quiet"] != null)
                {
                    metadata.Quiet = true;
                }

                if (commandLine["si"] != null || commandLine["silent"] != null)
                {
                    metadata.Silent = true;
                }

                if (commandLine["d"] != null || commandLine["debug"] != null)
                {
                    PauseForDebugger();
                    if (commandLine.Length == 1) _harness.Run(args, metadata.Implementation);
                }

                var logToConsole = (commandLine["l"] != null || commandLine["logtoconsole"] != null);
                var manager = WindowsServiceManager
                    .GetInstance(metadata, _harness, logToConsole);

                if (commandLine["i"] != null || commandLine["install"] != null)
                {
                    manager.Install();
                }

                if (commandLine["u"] != null || commandLine["uninstall"] != null)
                {
                    manager.Uninstall();
                }

                if (commandLine["is"] != null || commandLine["installandstart"] != null)
                {
                    manager.InstallAndStart();
                }

                if (commandLine["s"] != null || commandLine["start"] != null)
                {
                    manager.StartService();
                }

                if (commandLine["x"] != null || commandLine["stop"] != null)
                {
                    manager.StopService();
                }

                if (commandLine["status"] != null)
                {
                    manager.ShowStatus();
                }
            }

            private void PauseForDebugger()
            {
                _harness.WriteLine(" ");
                _harness.WriteLine("Paused.  Attach to process, then press Enter to continue.");
                _harness.WriteLine(" ");
                _harness.ReadLine();
            }

            private void ShowDefaultHelpOnConsole(ServiceMetadata metadata)
            {
                var name = metadata.ServiceName + "\t(" + _attributes.BuildDate + ")";

                var description = metadata.ShortDescription;
                var synopsis = metadata.ServiceName +
                               ".exe [OPTIONS]";
                const string dnw =
                    "Does not prevent installer logs from appearing on the console if the -l (log to console) option is enabled.";
                const string siOption = "--si, --silent";
                const string siDetails = "Enable silent mode.  Will display nothing (not even errors) on the console.";
                const string qOption = "--q, --quiet";
                const string qDetails = "Enable quiet mode.  Will display only errors on the console.";
                const string uOption = "--u, --uninstall";
                const string uDetails = "Uninstalls the service.";
                const string iOption = "--i, --install";
                const string iDetails = "Installs the service.";
                const string lOption = "--l, --logtoconsole";
                const string lDetails = "Instructs the installer/uninstaller to log the output to the console.";
                const string isOption = "--is, --installandstart";
                const string isDetails = "Installs and then starts the service.";
                const string sOption = "--s, --start";
                const string sDetails = "Starts the service.";
                const string xOption = "--x, --stop";
                const string xDetails = "Stops the service.";
                const string stOption = "--status";
                const string stDetails = "Displays the status of the service.";

                _harness.WriteLine(" ");
                _harness.WriteLine("NAME");
                _harness.WriteLine("\t" + name);
                _harness.WriteLine("\t" + description);
                _harness.WriteLine(" ");
                _harness.WriteLine("USAGE");
                _harness.WriteLine("\t" + synopsis);
                _harness.WriteLine(" ");
                _harness.WriteLine("OPTIONS");
                _harness.WriteLine("\t" + lOption);
                _harness.WriteLine("\t   " + lDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + uOption);
                _harness.WriteLine("\t   " + uDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + iOption);
                _harness.WriteLine("\t   " + iDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + isOption);
                _harness.WriteLine("\t   " + isDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + sOption);
                _harness.WriteLine("\t   " + sDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + xOption);
                _harness.WriteLine("\t   " + xDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + stOption);
                _harness.WriteLine("\t   " + stDetails);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + siOption);
                _harness.WriteLine("\t   " + siDetails);
                _harness.WriteLine("\t   " + dnw);
                _harness.WriteLine(" ");
                _harness.WriteLine("\t" + qOption);
                _harness.WriteLine("\t   " + qDetails);
                _harness.WriteLine("\t   " + dnw);
            }
        }
    }
}