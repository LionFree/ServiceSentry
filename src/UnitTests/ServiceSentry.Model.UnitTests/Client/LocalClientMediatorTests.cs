// -----------------------------------------------------------------------
//  <copyright file="LocalClientMediatorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Services;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Client
{
    [TestFixture]
    internal class LocalClientMediatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _serviceController = new Mock<ServiceWrapper>();
        }


        private readonly string _serviceName;
        private readonly string _machineName;
        private Mock<ServiceWrapper> _serviceController;
        private readonly Mock<MonitorServiceWatchdog> _monitor;
        private readonly Mock<Logger> _logger;

        public LocalClientMediatorTests()
        {
            _serviceName = Tests.Random<string>();
            _machineName = Tests.Random<string>();
            _monitor = new Mock<MonitorServiceWatchdog>();
            _logger = new Mock<Logger>();
        }

        private ClientLocalMediator Get_LCM(ServiceWrapper wrapper, ServiceState toggleResult=ServiceState.Stopped,
                                            bool local = true)
        {
            var service = new Mock<IMonitorService>();

            var factory = new Mock<ModelClassFactory>();
            factory.Setup(m => m.GetLocalServiceController(_serviceName)).Returns(wrapper);

            var packet = new SubscriptionPacket {ServiceName = _serviceName, MachineName = _machineName};
            var clients = new Mock<ClientList>();
            var client = new Mock<ServiceClient<IMonitorService>>();
            client.Setup(m => m.Service).Returns(service.Object);

            var result = PollResult.GetInstance(_serviceName, toggleResult, new List<Exception>());

            client.Setup(m => m.Service.GetStatus(It.IsAny<string>())).Returns(result);

            client.Setup(
                m =>
                m.Execute(It.IsAny<Action<SubscriptionPacket>>(),
                          It.IsAny<SubscriptionPacket>()))
                  .Returns(!local);

            client.Setup(m => m.Execute(
                It.IsAny<Action<SubscriptionPacket>>(),
                It.IsAny<Action<SubscriptionPacket, Exception>>(),
                It.IsAny<SubscriptionPacket>()))
                  .Returns(true);

            clients.Setup(m => m.GetClient(It.IsAny<string>())).Returns(client.Object);

            var sut = new ClientLocalMediator(packet, clients.Object, factory.Object, _logger.Object);
            return sut;
        }

        [Test]
        public void Test_CanStop()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _serviceController.Setup(m => m.CanStop).Returns(expected);

            var sut = Get_LCM(_serviceController.Object);

            // Act
            var actual = sut.CanStop;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_DisplayName()
        {
            // Arrange
            var expected = Tests.Random<string>();
            _serviceController.Setup(m => m.DisplayName).Returns(expected);

            var sut = Get_LCM(_serviceController.Object);

            // Act
            var actual = sut.DisplayName;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_IsInstalled()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _serviceController.Setup(m => m.IsInstalled).Returns(expected);

            var sut = Get_LCM(_serviceController.Object);

            // Act
            var actual = sut.IsInstalled;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LCM_Constructor()
        {
            // Arrange

            // Act
            var sut = Get_LCM(_serviceController.Object);

            // Act
            Assert.IsNotNull(sut);
        }

        [Test]
        public void Test_MachineName()
        {
            // Arrange
            var sut = Get_LCM(_serviceController.Object);

            // Act
            var actual = sut.MachineName;

            // Assert
            Assert.AreEqual(_machineName, actual);
        }

        [Test]
        public void Test_Refresh()
        {
            // Arrange
            var refreshed = false;
            _serviceController.Setup(m => m.Refresh())
                              .Callback(() => { refreshed = true; });

            var sut = Get_LCM(_serviceController.Object);

            // Act
            sut.Refresh(_monitor.Object);

            // Assert
            Assert.IsTrue(refreshed);
        }

        [Test]
        public void Test_ServiceName()
        {
            // Arrange
            var sut = Get_LCM(_serviceController.Object);

            // Act
            var actual = sut.ServiceName;

            // Assert
            Assert.AreEqual(_serviceName, actual);
        }

        [Test]
        public void Test_Start()
        {
            // Arrange
            var started = false;
            _serviceController.Setup(m => m.Start())
                              .Callback(() => { started = true; });

            var sut = Get_LCM(_serviceController.Object);

            // Act
            sut.Start();

            // Assert
            Assert.IsTrue(started);
        }

        [Test]
        public void Test_Status()
        {
            // Arrange
            var expected = Tests.Random<ServiceState>();
            _serviceController.Setup(m => m.Status).Returns(expected);

            var sut = Get_LCM(_serviceController.Object);

            // Act
            var actual = sut.Status;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        public void Test_Stop(bool callLocal)
        {
            // Arrange
            var ranLocal = false;
            _monitor.Setup(m => m.IsAvailable).Returns(!callLocal);

            _serviceController.Setup(m => m.Stop())
                              .Callback(() => { ranLocal = true; });
            
            var sut = Get_LCM(_serviceController.Object, ServiceState.Stopped, callLocal);

            // Act
            sut.Stop(_monitor.Object);

            // Assert
            Assert.AreEqual(callLocal, ranLocal);
        }

        [Test]
        public void Test_Stop_Locally()
        {
            Test_Stop(true);
        }

        [Test]
        public void Test_Stop_Monitor()
        {
            Test_Stop(false);
        }


        [Test]
        public void Test_WaitForStatus()
        {
            // Arrange
            var waited = false;
            var expected = Tests.Random<ServiceState>();
            _serviceController.Setup(m => m.WaitForStatus(expected, It.IsAny<TimeSpan>()))
                              .Callback(() => { waited = true; });

            var sut = Get_LCM(_serviceController.Object);

            // Act
            sut.WaitForStatus(expected);

            // Assert
            Assert.IsTrue(waited);
        }
    }

    internal class TestMonitorService : IMonitorService
    {
        public bool GetCanStop(string serviceName)
        {
            throw new NotImplementedException();
        }

        public string GetDisplayName(string serviceName)
        {
            throw new NotImplementedException();
        }

        public bool GetIsInstalled(string serviceName)
        {
            throw new NotImplementedException();
        }

        public PollResult GetStatus(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void Start(SubscriptionPacket serviceData)
        {
            throw new NotImplementedException();
        }

        public void Stop(SubscriptionPacket serviceData)
        {
            throw new NotImplementedException();
        }

        public void Refresh(SubscriptionPacket serviceData)
        {
            throw new NotImplementedException();
        }

        public void WaitForStatus(SubscriptionPacket serviceData, ServiceState desiredStatus)
        {
            throw new NotImplementedException();
        }

        public SubscriptionPacket[] GetServices()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(SubscriptionPacket serviceData)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void UpdateSubscription(SubscriptionPacket serviceData)
        {
            throw new NotImplementedException();
        }
    }
}