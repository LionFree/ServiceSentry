// -----------------------------------------------------------------------
//  <copyright file="MonitorEndpointTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.ServiceModel;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Services;
using ServiceSentry.Monitor.Infrastructure;
using ServiceSentry.Monitor.Mediator;
using ServiceSentry.ServiceFramework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Monitor.UnitTests.Infrastructure
{
    [TestFixture]
    public class MonitorEndpointTests
    {
        private readonly string _serviceName;
        private readonly SubscriptionPacket _serviceData;
        private readonly Mock<ServerMediator> _controller;
        private readonly Mock<MonitorClassFactory> _factory;
        private readonly Mock<ModelClassFactory> _modelFactory;
        private readonly Mock<LocalServiceFinder> _reader;
        private readonly Mock<AssemblyWrapper> _assembly;
        private readonly ConsoleHarness _harness;
        private TrackingList _state;
        private readonly Mock<ConfigFileHandler> _handler;
        private readonly Mock<Logger> _logger;
        private readonly Mock<ConsoleWrapper> _console;

        private static ConfigFile MockServiceFile()
        {
            var details = LoggingDetails.GetInstance();
            details.ArchiveLogs = Tests.Random<bool>();
            details.ClearLogs = Tests.Random<bool>();

            var serviceDetails = ServiceDetails.GetInstance();
            serviceDetails.Timeout = Tests.Random<int>();
            serviceDetails.NotifyOnUnexpectedStop = Tests.Random<bool>();


            var serviceItem = new Mock<Service>();
            serviceItem.Setup(m => m.DisplayName).Returns(Tests.Random<string>());
            serviceItem.Setup(m => m.ServiceName).Returns(Tests.Random<string>());
            serviceItem.Setup(m => m.Details).Returns(serviceDetails);
            var services = ServiceList.GetInstance();
            services.Items.Add(serviceItem.Object);

            var file = ConfigFile.GetInstance();
            file.Services = services;
            file.LogDetails = details;

            return file;
        }

        public MonitorEndpointTests()
        {
            _console=new Mock<ConsoleWrapper>();
            _assembly=new Mock<AssemblyWrapper>();
            _serviceName = Tests.Random<string>();
            
            _harness = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);
            _state = TrackingList.Default;

            _serviceData = new SubscriptionPacket
                {
                    ServiceName = _serviceName,
                    Timeout = TimeSpan.FromSeconds(Tests.Random<int>())
                };

            _state.Add(_serviceName, new TrackingObject
                {
                    ServiceName = _serviceName,
                    Packet = _serviceData
                }
                );

            _logger = new Mock<Logger>();
            _factory = new Mock<MonitorClassFactory>();
            _controller = new Mock<ServerMediator>();
            _reader = new Mock<LocalServiceFinder>();
            _modelFactory = new Mock<ModelClassFactory>();


            var file = MockServiceFile();
            _handler = new Mock<ConfigFileHandler>();
            _handler.Setup(m => m.ReadConfigFile()).Returns(file);
        }

        private MonitorEndpoint GetEndpoint()
        {
            _factory.Setup(m => m.GetController(It.IsAny<SubscriptionPacket>(),
                                                It.IsAny<ModelClassFactory>(),
                                                It.IsAny<ConsoleHarness>()))
                    .Returns(_controller.Object);

            return MonitorEndpoint.GetInstance(_logger.Object, _harness,
                                               ref _state,
                                               _factory.Object,
                                               _reader.Object,
                                               _handler.Object,
                                               _modelFactory.Object);
        }

        [Test]
        public void Test_Attributes()
        {
            var attribute = typeof (MonitorEndpoint).GetAttribute<ServiceBehaviorAttribute>();

            Assert.IsNotNull(attribute);
            Assert.AreEqual(InstanceContextMode.Single, attribute.InstanceContextMode);
            Assert.IsTrue(attribute.IncludeExceptionDetailInFaults);
        }

        [Test]
        public void Test_GetCanStop()
        {
            // Arrange
            var expected = Tests.Random<bool>();

            _controller.Setup(m => m.CanStop).Returns(expected);
            var sut = GetEndpoint();

            // Act
            var actual = sut.GetCanStop(_serviceName);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_GetDisplayName()
        {
            // Arrange
            var expected = Tests.Random<string>();

            _controller.Setup(m => m.DisplayName).Returns(expected);
            var sut = GetEndpoint();

            // Act
            var actual = sut.GetDisplayName(_serviceName);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_GetIsInstalled()
        {
            // Arrange
            var expected = Tests.Random<bool>();

            _controller.Setup(m => m.IsInstalled).Returns(expected);
            var sut = GetEndpoint();

            // Act
            var actual = sut.GetIsInstalled(_serviceName);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_GetServices()
        {
            // Arrange
            var gotServices = false;
            var expected = new[] {_serviceData};

            _reader.Setup(m => m.GetServices())
                   .Returns(expected)
                   .Callback(() => { gotServices = true; });
            var sut = GetEndpoint();

            // Act
            var actual = sut.GetServices();

            // Assert
            Assert.IsTrue(gotServices);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_GetStatus()
        {
            // Arrange
            var expectedState = Tests.Random<ServiceState>();
            var expected = PollResult.GetInstance(_serviceName, expectedState, new List<Exception>());

            _controller.Setup(m => m.Status).Returns(expectedState);
            var sut = GetEndpoint();

            // Act
            var actual = sut.GetStatus(_serviceName);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Refresh()
        {
            // Arrange
            var refreshed = false;

            _controller.Setup(m => m.Refresh(It.IsAny<string>(), It.IsAny<TrackingList>()))
                       .Callback(() => { refreshed = true; });
            var sut = GetEndpoint();

            // Act
            sut.Refresh(_serviceData);

            // Assert
            Assert.IsTrue(refreshed);
        }

        [Test]
        public void Test_Shutdown()
        {
            // Arrange
            var sut = GetEndpoint();

            // Act
            sut.Shutdown(null, null);

            // Assert
            Assert.IsNull(sut.State);
        }

        [Test]
        public void Test_Start()
        {
            // Arrange
            var started = false;

            _controller.Setup(m => m.Start(ref _state))
                       .Callback(() => { started = true; });
            var sut = GetEndpoint();

            // Act
            sut.Start(_serviceData);

            // Assert
            Assert.IsTrue(started);
        }

        [Test]
        public void Test_Startup()
        {
            // Arrange
            var started = false;
            var count = 0;
            var expected = _handler.Object.ReadConfigFile().Services.Items.Count;

            _controller.Setup(m => m.UpdateSubscription(It.IsAny<SubscriptionPacket>(), ref _state))
                       .Callback(() =>
                           {
                               started = true;
                               count++;
                           });
            var sut = GetEndpoint();

            // Act
            sut.Startup(null, null);

            // Assert
            Assert.IsTrue(started);
            Assert.AreEqual(expected, count);
        }

        [Test]
        public void Test_Stop()
        {
            // Arrange
            var stopped = false;

            _controller.Setup(m => m.Stop(ref _state))
                       .Callback(() => { stopped = true; });
            var sut = GetEndpoint();

            // Act
            sut.Stop(_serviceData);

            // Assert
            Assert.IsTrue(stopped);
        }

        [Test]
        public void Test_Subscribe()
        {
            // Arrange
            var subscribed = false;

            _controller.Setup(m => m.Subscribe(It.IsAny<TrackingList>()))
                       .Callback(() => { subscribed = true; });
            var sut = GetEndpoint();

            // Act
            sut.Subscribe(_serviceData);

            // Assert
            Assert.IsTrue(subscribed);
        }

        [Test]
        public void Test_Unsubscribe()
        {
            // Arrange
            var unsubscribed = false;

            _controller.Setup(m => m.Unsubscribe(ref _state))
                       .Callback(() => { unsubscribed = true; });
            var sut = GetEndpoint();

            // Act
            sut.Unsubscribe(_serviceName);

            // Assert
            Assert.IsTrue(unsubscribed);
        }

        [Test]
        public void Test_UpdateSubscription()
        {
            // Arrange
            var updated = false;

            _controller.Setup(m => m.UpdateSubscription(It.IsAny<SubscriptionPacket>(), ref _state))
                       .Callback(() => { updated = true; });
            var sut = GetEndpoint();

            // Act
            sut.UpdateSubscription(_serviceData);

            // Assert
            Assert.IsTrue(updated);
        }

        [Test]
        public void Test_WaitForStatus()
        {
            // Arrange
            var waited = false;

            var state = Tests.Random<ServiceState>();
            _controller.Setup(m => m.WaitForStatus(It.IsAny<ServiceState>(), It.IsAny<TimeSpan>()))
                       .Callback(() => { waited = true; });
            var sut = GetEndpoint();

            // Act
            sut.WaitForStatus(_serviceData, state);

            // Assert
            Assert.IsTrue(waited);
        }
    }
}