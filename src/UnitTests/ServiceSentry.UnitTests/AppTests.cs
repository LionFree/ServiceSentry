using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceSentry.Client.Infrastructure;
using ServiceSentry.Client.UNTESTED.Infrastructure;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.UnitTests.Common;

namespace ServiceSentry.UnitTests
{
    [TestFixture]
    public class AppTests
    {
        [SetUp]
        public void SetUp()
        {
            _state = new Mock<ApplicationState>();
            _controller = new Mock<ViewController>();
            _extensionHandler = new Mock<ExtensionHandler>();
            _exceptionHandler = new Mock<ExceptionHandler>();
            _factory = new Mock<ClientClassFactory>();
            //_watchdog=new Mock<MonitorServiceWatchdog>();
            _config = new Mock<ConfigFile>();
            _processor = new Mock<CommandLineProcessor>();
            _configHandler = new Mock<ConfigFileHandler>();
            _logger = new Mock<Logger>();
        }

        private Mock<ApplicationState> _state;
        private Mock<ViewController> _controller;
        private Mock<ExtensionHandler> _extensionHandler;
        private Mock<ExceptionHandler> _exceptionHandler;
        private Mock<ClientClassFactory> _factory;
        //private Mock<MonitorServiceWatchdog> _watchdog;
        private Mock<ConfigFile> _config;
        private Mock<CommandLineProcessor> _processor;
        private Mock<ConfigFileHandler> _configHandler;
        private Mock<Logger> _logger;

        [Test]
        public void Test_OnStartup(bool minimized)
        {
            // Arrange
            var loggerCreated = false;
            var loggerConfigured = false;
            var exceptionHandlerCreated = false;
            var exceptionHandlerAttached = false;
            var extensionsComposed = false;
            var extensionHandlerCreated = false;
            var fixApplied = false;
            var viewControllerCreated = false;
            var actualMinimized = false;
            var extensionsActivated = false;
            var mainWindowShown = false;
            var settingsUpgraded = false;
            var viewsPublished = false;

            var tabExtensions = new List<ImportedTabItem>();
            var optionTabExtensions = new List<ImportedOptionsTabItem>();
            var contextMenuExtensions = new List<ImportedContextMenu>();
            
            _processor.Setup(m => m.Process(It.IsAny<string[]>())).Returns(true);
            _processor.Setup(m => m.UnitTest).Returns(true);
            _processor.Setup(m => m.StartMinimized).Returns(minimized);

            _logger.Setup(m => m.Configure(It.IsAny<LogConfiguration>())).Callback(() => { loggerConfigured = true; });

            _exceptionHandler.Setup(m => m.AttachExceptionHandler(It.IsAny<Logger>()))
                .Callback(() => { exceptionHandlerAttached = true; });

            _extensionHandler.Setup(m => m.ComposeExtensions())
                             .Callback(() => { extensionsComposed = true; });
            _extensionHandler.Setup(m => m.ActivateExtensions(_state.Object))
                .Callback(() => { extensionsActivated = true; });
            _extensionHandler.Setup(m => m.TabExtensions).Returns(tabExtensions);
            _extensionHandler.Setup(m => m.OptionTabExtensions).Returns(optionTabExtensions);
            _extensionHandler.Setup(m => m.ContextMenuExtensions).Returns(contextMenuExtensions);
            
            _configHandler.Setup(m => m.ReadConfigFile()).Returns(_config.Object);

            _controller.Setup(m => m.ApplyInternationalizationFix()).Callback(() => { fixApplied = true; });
            _controller.Setup(m => m.MinimizeToNotificationArea()).Callback(() => { actualMinimized = true; });
            _controller.Setup(m => m.UpgradeSettings()).Callback(() => { settingsUpgraded = true; });
            _controller.Setup(m => m.PublishViews(tabExtensions, optionTabExtensions, contextMenuExtensions))
                       .Callback(() => { viewsPublished = true; });
            _controller.Setup(m => m.ShowMainWindow()).Callback(() => { mainWindowShown = true; });
            
            _factory.Setup(m => m.GetViewController(_config.Object)).Returns(_controller.Object).Callback(() =>
                { viewControllerCreated = true; });
            _factory.Setup(m => m.StateObject).Returns(_state.Object);
            _factory.Setup(m => m.ConfigFileHandler).Returns(_configHandler.Object);
            _factory.Setup(m => m.Logger).Returns(_logger.Object).Callback(() => { loggerCreated = true; });
            _factory.Setup(m => m.ExceptionHandler).Returns(_exceptionHandler.Object).Callback(() =>
                { exceptionHandlerCreated = true; });
            _factory.Setup(m => m.ExtensionHandler).Returns(_extensionHandler.Object).Callback(() =>
            { extensionHandlerCreated = true; });

            var sut = Engine.CreateObjectGraph(_processor.Object, _factory.Object);
            
            // Act
            sut.Start(new string[] {});
            
            // Assert
            Assert.IsTrue(loggerCreated, "Logger was not created.");
            Assert.IsTrue(loggerConfigured, "Logger was not configured.");
            Assert.IsTrue(exceptionHandlerCreated, "ExceptionHandler was not attached.");
            Assert.IsTrue(exceptionHandlerAttached, "ExceptionHandler was not attached.");
            Assert.IsTrue(extensionHandlerCreated, "ExtensionHandler was not created.");
            Assert.IsTrue(extensionsComposed, "Extensions were not composed.");
            Assert.IsTrue(extensionsActivated, "Extensions were not activated.");
            Assert.IsTrue(viewControllerCreated, "ViewController was not created.");

            Assert.IsTrue(fixApplied, "InternationalizationFix was not applied.");
            Assert.IsTrue(settingsUpgraded, "Settings were not upgraded.");
            Assert.IsTrue(viewsPublished, "Views were not published.");
            Assert.AreEqual(minimized, actualMinimized, "Start Minimized: expected '{0}', was '{1}'.", minimized, actualMinimized);
            Assert.AreEqual(!minimized, mainWindowShown, "MainWindowShown: expected '{0}', was '{1}'.", !minimized, mainWindowShown);
        }

        [Test]
        public void Test_App_SignalExternalCommandLineArgs()
        {
            // Arrange
            var wasCalled = false;
            var count = Tests.Random<int>(1, 5);
            var args = new string[count];
            for (var i = 0; i < count; i++)
            {
                args[i] = Tests.Random<string>();
            }

            var expected = Tests.Random<bool>();

            _processor.Setup(m => m.Process(It.IsAny<string[]>())).Returns(expected).Callback(() =>
                { wasCalled = true; });

            var sut = Engine.CreateObjectGraph(_processor.Object, _factory.Object);

            // Act
            var actual = sut.SignalExternalCommandLineArgs(args);

            // Assert
            Assert.AreEqual(expected, actual, "Did not receive the expected value from Process.");
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_App_Constructor()
        {
            // Arrange
            var loggerCreated = false;
            var exceptionHandlerCreated = false;
            var applicationStateCreated = false;
            var configHandlerCreated = false;

            _factory.Setup(m => m.Logger).Returns(_logger.Object).Callback(() =>
            { loggerCreated = true; });
            _factory.Setup(m => m.ExceptionHandler).Returns(_exceptionHandler.Object).Callback(() =>
                { exceptionHandlerCreated = true; });
            _factory.Setup(m => m.StateObject).Returns(_state.Object).Callback(() =>
            { applicationStateCreated = true; });
            _factory.Setup(m => m.ConfigFileHandler).Returns(_configHandler.Object).Callback(() =>
            { configHandlerCreated = true; });
            
            // Act
            var sut = Engine.CreateObjectGraph(_processor.Object, _factory.Object);
            
            // Assert
            Assert.IsTrue(loggerCreated,"Logger was not created.");
            Assert.IsTrue(exceptionHandlerCreated, "ExceptionHandler was not created.");
            Assert.IsTrue(applicationStateCreated, "ApplicationState was not created.");
            Assert.IsTrue(configHandlerCreated, "ConfigFileHandler was not created.");
            Assert.AreEqual(_logger.Object, sut.Logger, "Logger not set correctly.");
        }
        
        [Test, Explicit("Creates an app.")]
        public void Test_App_Start_Maximized()
        {
            Test_OnStartup(false);
        }

        [Test, Explicit("Creates an app.")]
        public void Test_App_Start_Minimized()
        {
            Test_OnStartup(true);
        }
    }
}