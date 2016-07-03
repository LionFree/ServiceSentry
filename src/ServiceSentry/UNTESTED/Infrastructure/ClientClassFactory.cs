using ServiceSentry.Client.Infrastructure;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Client;

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    internal abstract class ClientClassFactory
    {
        internal abstract ExtensionHandler ExtensionHandler { get; }
        internal abstract ConfigFileHandler ConfigFileHandler { get; }
        internal abstract Logger Logger { get; }
        internal abstract ExceptionHandler ExceptionHandler { get; }
        internal abstract ApplicationState StateObject { get; }
        internal abstract MonitorServiceWatchdog WatchDog { get; }

        internal static ClientClassFactory GetInstance(Logger logger)
        {
            return GetInstance(logger, ExceptionHandler.GetInstance(),
                               ConfigFileHandler.GetInstance(logger),
                               ExtensionHandler.GetInstance(Dialogs.GetInstance(), logger),
                               ApplicationState.GetInstance(),
                               MonitorServiceWatchdog.GetInstance(logger));
        }

        internal static ClientClassFactory GetInstance(Logger logger, ExceptionHandler exceptionHandler,
                                                       ConfigFileHandler fileHandler, ExtensionHandler extensionHandler,
                                                       ApplicationState stateObject, MonitorServiceWatchdog watchdog)
        {
            return new ClientClassFactoryImplementation(logger, exceptionHandler,
                                                        fileHandler, extensionHandler,
                                                        stateObject, watchdog);
        }


        internal abstract ViewController GetViewController(ConfigFile configFile);

        private sealed class ClientClassFactoryImplementation : ClientClassFactory
        {
            private readonly ApplicationState _applicationState;
            private readonly ConfigFileHandler _configFileHandler;
            private readonly ExceptionHandler _exceptionHandler;
            private readonly ExtensionHandler _extensionHandler;
            private readonly Logger _logger;
            private readonly MonitorServiceWatchdog _watchdog;

            internal ClientClassFactoryImplementation(Logger logger, ExceptionHandler exceptionHandler,
                                                      ConfigFileHandler fileHandler, ExtensionHandler extensionHandler,
                                                      ApplicationState stateObject, MonitorServiceWatchdog watchdog)
            {
                _logger = logger;
                _exceptionHandler = exceptionHandler;
                _configFileHandler = fileHandler;
                _extensionHandler = extensionHandler;
                _applicationState = stateObject;
                _watchdog = watchdog;
            }

            internal override ExceptionHandler ExceptionHandler
            {
                get { return _exceptionHandler; }
            }

            internal override Logger Logger
            {
                get { return _logger; }
            }

            internal override ApplicationState StateObject
            {
                get { return _applicationState; }
            }

            internal override ConfigFileHandler ConfigFileHandler
            {
                get { return _configFileHandler; }
            }

            internal override ExtensionHandler ExtensionHandler
            {
                get { return _extensionHandler; }
            }

            internal override MonitorServiceWatchdog WatchDog
            {
                get { return _watchdog; }
            }

            internal override ViewController GetViewController(ConfigFile configFile)
            {
                return ViewController.GetInstance(_logger, _exceptionHandler, _applicationState, _watchdog,
                                                  configFile);
            }
        }
    }
}