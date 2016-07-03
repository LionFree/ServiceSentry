using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ServiceSentry.Armor.SingleInstance;
using ServiceSentry.Client.UNTESTED.Infrastructure;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

namespace ServiceSentry.Client.Infrastructure
{
    /// <summary>
    ///     Contains the startup routines for the ServiceSentry application.
    /// </summary>
    public abstract partial class Engine : ISingleInstanceApp
    {
        /// <summary>
        ///     Gets a new instance of the Application, using the default values.
        /// </summary>
        internal static Engine CreateObjectGraph()
        {
            var logger = Logger.GetInstance();
            return CreateObjectGraph(
                CommandLineProcessor.GetInstance(), 
                ClientClassFactory.GetInstance(logger)
                );
        }

        /// <summary>
        ///     Gets a new instance of the Application, using the specified values.
        /// </summary>
        internal static Engine CreateObjectGraph(
            CommandLineProcessor commandLine,
            ClientClassFactory factory)
        {
            if(factory==null) throw new ArgumentNullException(nameof(factory));
            return new EngineImplementation(commandLine, factory);
        }

        #region Abstract Members

        /// <summary>
        ///     The application <see cref="Logger"/>.
        ///     Mocked for unit testing.
        /// </summary>
        internal abstract Logger Logger { get; }

        /// <summary>
        ///     Handle arguments passed from an attempt
        ///     to create a new instance of the app.
        /// </summary>
        /// <param name="args">
        ///     List of arguments to supply the first
        ///     instance of the application.
        /// </param>
        /// <returns>
        ///     <c>true</c> if successful, otherwise <c>false</c>.
        /// </returns>
        public abstract bool SignalExternalCommandLineArgs(IList<string> args);

        /// <summary>
        ///     The method that will run when the entry point method
        ///     (<see cref="Start" />) tells the application to Run().
        /// </summary>
        protected abstract void OnStartup(object sender, StartupEventArgs e);

        /// <summary>
        ///     The application entry point.
        /// </summary>
        internal abstract void Start(string[] args);

        #endregion

        private sealed class EngineImplementation : Engine
        {
            private readonly ApplicationState _applicationState;
            private readonly CommandLineProcessor _commandLine;
            private readonly ExceptionHandler _exceptionHandler;
            private readonly ClientClassFactory _factory;
            private readonly Logger _logger;
            private string[] _args;
            private ConfigFileHandler _configHandler;
            private ConfigFile _configuration;
            private ExtensionHandler _extensionHandler;
            private ViewController _viewController;

            internal EngineImplementation(CommandLineProcessor commandLine,
                                       ClientClassFactory factory)
            {
                if(commandLine==null) throw new ArgumentNullException(nameof(commandLine));
                if (factory== null) throw new ArgumentNullException(nameof(factory));
                _commandLine = commandLine;
                _commandLine.CommandLineArgsPassed += OnCommandLineArgsPassed;

                _factory = factory;
                _logger = _factory.Logger;
                _exceptionHandler = _factory.ExceptionHandler;
                _applicationState = _factory.StateObject;
                _configHandler = _factory.ConfigFileHandler;
            }

            internal override Logger Logger => _logger;

            public override bool SignalExternalCommandLineArgs(IList<string> args)
            {
                var newArgs = args.ToList();

                // The first argument is always the executable path.
                newArgs.RemoveAt(0);
                var result = _commandLine.Process(newArgs.ToArray());
                return result;
            }

            protected override void OnStartup(object sender, StartupEventArgs e)
            {
                _configuration = _configHandler.ReadConfigFile();
                _logger.Configure(_configuration.DebugLogConfiguration);
                _configHandler = _factory.ConfigFileHandler;

                _applicationState.LocalConfigs = _configuration;
                _exceptionHandler.AttachExceptionHandler(_logger);

                _extensionHandler = _factory.ExtensionHandler;
                _extensionHandler.ComposeExtensions();
                _extensionHandler.ActivateExtensions(_applicationState);
                
                _applicationState.AttachLoggerToFiles(_logger);

                _viewController = _factory.GetViewController(_configuration);

                _viewController.ApplyInternationalizationFix();
                _viewController.UpgradeSettings();
                _viewController.PublishViews(_extensionHandler.TabExtensions,
                                             _extensionHandler.OptionTabExtensions,
                                             _extensionHandler.ContextMenuExtensions);

                if (_commandLine.StartMinimized)
                {
                    _viewController.MinimizeToNotificationArea();
                }
                else
                {
                    _viewController.ShowMainWindow();
                }

                if (_commandLine.UnitTest) Shutdown();
            }

            private void OnCommandLineArgsPassed(object sender, CommandLineArgs e)
            {
                // Enter new switches here.
            }

            internal override void Start(string[] args)
            {
                if (!SingleInstance<Engine>.InitializeAsFirstInstance(Strings._ApplicationName)) return;

                _args = args;
                if (!_commandLine.Process(_args)) return;

                InitializeComponent();
                Run();

                SingleInstance<Engine>.Cleanup();
            }
        }
    }
}