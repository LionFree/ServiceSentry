// -----------------------------------------------------------------------
//  <copyright file="ServiceClassFactoryTests.cs" company="Curtis Kaler">
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
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Monitor.UnitTests.Model
{
    [TestFixture]
    internal class MonitorClassFactoryTests
    {
        private TrackingList _state;
        private readonly ConsoleHarness _harness;
        private readonly MonitorEndpoint _endpoint;
        private readonly ServiceHost _host;
        private readonly string _suffix;
        private readonly WindowsServiceDescription _description;
        private readonly Mock<Logger> _logger;

        public MonitorClassFactoryTests()
        {
            _logger = new Mock<Logger>();
            _state = TrackingList.Default;
            _harness = ConsoleHarness.Default;
            _endpoint = MonitorEndpoint.GetInstance(_logger.Object, _harness, ref _state);

            _suffix = Tests.Random<string>();
            _description = new WindowsServiceDescription
                {
                    ServiceObject = _endpoint,
                    Contract = typeof (IMonitorService),
                    EndpointSuffix = _suffix
                };

            _host = HostBuilder.GetInstance(ServiceHostType.NetTcp).BuildHost(_description);
            _logger=new Mock<Logger>();
        }


        [Test]
        public void Test_GetEndpoint()
        {
            // Arrange
            var sut = MonitorClassFactory.GetInstance(_logger.Object);
            var expected = _endpoint;
            var expectedType = expected.GetType();

            // Act
            var actual = sut.GetEndpoint(_harness, ref _state);

            // Assert
            Assert.IsAssignableFrom(expectedType, actual);
            Assert.AreEqual(expected.State, _state);
        }

        [Test]
        public void Test_GetHarness()
        {
            // Arrange
            var sut = MonitorClassFactory.GetInstance(_logger.Object);
            var expected = _harness;
            var expectedType = expected.GetType();

            // Act
            var actual = sut.GetHarness();

            // Assert
            Assert.IsAssignableFrom(expectedType, actual);
        }

        [Test]
        public void Test_GetHost()
        {
            // Arrange
            var sut = MonitorClassFactory.GetInstance(_logger.Object);
            var expected = _host;
            var expectedType = expected.GetType();

            // Act
            var actual = sut.GetHost(_description);

            // Assert
            Assert.IsAssignableFrom(expectedType, actual);
        }

        [Test]
        public void Test_GetState()
        {
            // Arrange
            var sut = MonitorClassFactory.GetInstance(_logger.Object);
            var expected = _state;
            var expectedType = _state.GetType();

            // Act
            var actual = sut.GetState();

            // Assert
            Assert.IsAssignableFrom(expectedType, actual);
            Assert.AreEqual(expected, actual);
        }
    }
}