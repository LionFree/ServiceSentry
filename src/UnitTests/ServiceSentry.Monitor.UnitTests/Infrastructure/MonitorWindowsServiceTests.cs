// -----------------------------------------------------------------------
//  <copyright file="MonitorWindowsServiceTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceModel;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.Communication;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Monitor.Infrastructure;
using ServiceSentry.Monitor.Mediator;
using ServiceSentry.ServiceFramework;

#endregion

namespace ServiceSentry.Monitor.UnitTests.Infrastructure
{
    [TestFixture]
    public class MonitorServiceTests
    {
        private readonly Mock<MonitorClassFactory> _factory;
        private readonly ConsoleHarness _testHarness;
        private TrackingList _testState;
        private readonly MonitorEndpoint _testEndpoint;
        private readonly ServiceHost _testHost;
        private readonly string _testEPValue;
        private readonly Mock<Logger> _logger;

        public MonitorServiceTests()
        {
            _logger = new Mock<Logger>();
            _testHarness = ConsoleHarness.Default;
            _testState = TrackingList.Default;
            _testEndpoint = MonitorEndpoint.GetInstance(_logger.Object, _testHarness, ref _testState);

            var config = (MonitorService.GetInstance(Logger.Null).GetType()).GetAttribute<WindowsServiceAttribute>();

            var description = new WindowsServiceDescription
            {
                ServiceObject = _testEndpoint,
                Contract = typeof (IMonitorService),
                EndpointSuffix = config.ServiceName,
            };
            _testHost = HostBuilder.GetInstance(ServiceHostType.NetTcp).BuildHost(description);
            _testEPValue = _testHost.Description.Endpoints[0].Address.ToString();

            _factory = new Mock<MonitorClassFactory>();
            _factory.Setup(m => m.GetHarness()).Returns(_testHarness);
            _factory.Setup(m => m.GetState()).Returns(_testState);
            _factory.Setup(m => m.GetEndpoint(It.IsAny<ConsoleHarness>(), ref _testState))
                .Returns(_testEndpoint);
            _factory.Setup(m => m.GetHost(It.IsAny<WindowsServiceDescription>())).Returns(_testHost);
        }

        [Test]
        public void Test_Endpoint()
        {
            // Arrange
            MonitorService sut = MonitorService.GetInstance(_logger.Object, _factory.Object);
            string expected = _testEPValue;

            // Act
            string actual = sut.Endpoint;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Harness()
        {
            // Arrange
            MonitorService sut = MonitorService.GetInstance(_logger.Object, _factory.Object);
            ConsoleHarness expected = _testHarness;

            // Act
            ConsoleHarness actual = sut.Harness;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Time consuming: Requires starting a service.")]
        public void Test_OnStart()
        {
            // Arrange
            bool started = false;
            MonitorService sut = MonitorService.GetInstance(_logger.Object, _factory.Object);
            sut.Started += (o, e) => { started = true; };

            // Act
            sut.OnStart(null);

            // Assert
            Assert.IsTrue(started);
        }

        [Test, Explicit("Time consuming: Requires stopping a service.")]
        public void Test_OnStop()
        {
            // Arrange
            bool stopped = false;
            MonitorService sut = MonitorService.GetInstance(_logger.Object, _factory.Object);
            sut.Stopped += (o, e) => { stopped = true; };

            // Act
            sut.OnStop();

            // Assert
            Assert.IsTrue(stopped);
        }

        [Test]
        public void Test_ServiceName()
        {
            // Arrange
            MonitorService sut = MonitorService.GetInstance(_logger.Object, _factory.Object);
            var attribute = sut.GetType().GetAttribute<WindowsServiceAttribute>();
            string expected = attribute.ServiceName;

            // Act
            string actual = sut.ServiceName;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}