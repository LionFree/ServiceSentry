// -----------------------------------------------------------------------
//  <copyright file="LocalServerMediatorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;
using ServiceSentry.Monitor.Mediator;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Monitor.UnitTests.Model
{
    [TestFixture]
    internal class ServerLocalMediatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _name = Tests.Random<string>();
            _controller = new Mock<ServiceWrapper>();
            _packet = new SubscriptionPacket {ServiceName = _name};
            _factory = new Mock<ModelClassFactory>();
            _responder = new Mock<Responder>();
            _harness = new Mock<ConsoleHarness>();
            _logger=new Mock<Logger>();

            _state = TrackingList.Default;
            _state.Add(_name, new TrackingObject {ServiceName = _name});
        }

        [TearDown]
        public void TearDown()
        {
            if (!_state.ContainsKey(_name)) return;

            if (_state[_name].Timer.Enabled)
            {
                _state[_name].Timer.Enabled = false;
            }
            _state[_name].Timer.Dispose();
        }

        private string _name;
        private Mock<ServiceWrapper> _controller;
        private SubscriptionPacket _packet;
        private Mock<ModelClassFactory> _factory;
        private TrackingList _state;
        private Mock<Responder> _responder;
        private Mock<ConsoleHarness> _harness;
        private Mock<Logger> _logger;

        private const int TestingTimeout = 250;

        public void Test_LSM_Refresh(ServiceState oldState, ServiceState newState, bool isToggling)
        {
            var failureHandled = false;
            var expectedHandled = (newState == ServiceState.Stopped);
            var expectedStopped = (newState != ServiceState.Running);
            _state[_name].IsToggling = isToggling;
            _state[_name].LastState = oldState;

            _responder.Setup(m => m.HandleFailure(It.IsAny<TrackingObject>()))
                      .Callback(() => { failureHandled = true; });

            _controller.Setup(m => m.Status).Returns(newState);
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            _factory.Setup(m => m.GetResponder()).Returns(_responder.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.Refresh(_name, _state);

            // Assert
            if (oldState == newState || isToggling) return;
            Assert.AreEqual(expectedStopped, _state[_name].IsStopped);
            Assert.AreEqual(expectedHandled, failureHandled);
        }

        [Test]
        public void Test_LSM_CanStop()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _controller.Setup(m => m.CanStop).Returns(expected);
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            var actual = sut.CanStop;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSM_Constructor()
        {
            // Act
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Assert
            Assert.IsNotNull(sut);
        }

        [Test]
        public void Test_LSM_DisplayName()
        {
            // Arrange
            var expected = Tests.Random<string>();
            _controller.Setup(m => m.DisplayName).Returns(expected);
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            var actual = sut.DisplayName;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSM_Dispose()
        {
            // Arrange
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.Dispose();

            // Assert
            Assert.IsTrue(sut.IsDisposed);
        }

        [Test]
        public void Test_LSM_IsInstalled()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _controller.Setup(m => m.IsInstalled).Returns(expected);
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            var actual = sut.IsInstalled;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSM_MachineName()
        {
            // Arrange
            var expected = Environment.MachineName;
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            var actual = sut.MachineName;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSM_Refresh_FellOver()
        {
            Test_LSM_Refresh(ServiceState.Running, ServiceState.Stopped, false);
        }

        [Test]
        public void Test_LSM_Refresh_Still_Running()
        {
            Test_LSM_Refresh(ServiceState.Running, ServiceState.Running, false);
        }

        [Test]
        public void Test_LSM_Refresh_Still_Stopped()
        {
            Test_LSM_Refresh(ServiceState.Stopped, ServiceState.Stopped, false);
        }

        [Test]
        public void Test_LSM_Refresh_Toggled_To_Running()
        {
            Test_LSM_Refresh(ServiceState.Stopped, ServiceState.Running, true);
        }

        [Test]
        public void Test_LSM_Refresh_Toggled_To_Stopped()
        {
            Test_LSM_Refresh(ServiceState.Running, ServiceState.Stopped, true);
        }

        [Test]
        public void Test_LSM_ServiceName()
        {
            // Arrange
            var expected = _packet.ServiceName;
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            var actual = sut.ServiceName;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSM_Start()
        {
            // Arrange

            var started = false;
            var refreshed = false;

            _controller.Setup(m => m.Start()).Callback(() => { started = true; });
            _controller.Setup(m => m.Refresh()).Callback(() => { refreshed = true; });

            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.Start(ref _state);

            // Assert
            Assert.IsTrue(started);
            Assert.IsTrue(refreshed);
            var failures = _state[_name].FailDates;
            Assert.That(failures.Count == 0);
            Assert.IsFalse(_state[_name].IsStopped);
            Assert.IsFalse(_state[_name].IsToggling);
        }

        [Test]
        public void Test_LSM_Status()
        {
            // Arrange
            var expected = Tests.Random<ServiceState>();
            _controller.Setup(m => m.Status).Returns(expected);
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            var actual = sut.Status;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSM_Stop()
        {
            // Arrange
            var stopped = false;
            var waited = false;
            var refreshed = false;

            _controller.Setup(m => m.Stop())
                       .Callback(() => { stopped = true; });
            _controller.Setup(m => m.Refresh())
                       .Callback(() => { refreshed = true; });
            _controller.Setup(m => m.WaitForStatus(It.IsAny<ServiceState>(), It.IsAny<TimeSpan>()))
                       .Callback(() => { waited = true; });

            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.Stop(ref _state);

            // Assert
            Assert.IsTrue(stopped);
            Assert.IsTrue(waited);
            Assert.IsTrue(refreshed);
            var failures = _state[_name].FailDates;
            Assert.That(failures.Count == 0);
        }

        [Test]
        public void Test_LSM_Subscribe()
        {
            // Arrange
            var elapsed = false;
            _state.Clear();
            var initialCount = _state.Count;
            var expectedCount = initialCount + 1;
            var expectedWarn = _packet.NotifyOnUnexpectedStop;
            
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.Subscribe(_state);
            _state[_name].Timer.Elapsed += (s, e) => { elapsed = true; };
            Thread.Sleep(TestingTimeout);

            // Assert
            Assert.AreEqual(expectedCount, _state.Count,
                            "Expected {0} tracking objects, but found {1}.",
                            expectedCount, _state.Count);

            Assert.AreEqual(expectedWarn, _state[_name].Timer.Enabled,
                            "Expected Timer.Enabled={0}, but was not.",
                            expectedWarn);

            Assert.AreEqual(expectedWarn, _state[_name].NotifyOnUnexpectedStop,
                            "Expected Warn={0}, but was not.", expectedWarn);
            
            Assert.AreEqual(expectedWarn, elapsed,
                            "Expected Timer {0}to elapse, but did{1}.",
                            expectedWarn ? "" : "not ", 
                            expectedWarn ? " not" : "");
        }

        [Test]
        public void Test_LSM_WaitForStatus()
        {
            // Arrange
            var waited = false;
            var expectedState = Tests.Random<ServiceState>();
            var expectedTimeout = new TimeSpan(Tests.Random<int>(),
                                               Tests.Random<int>(),
                                               Tests.Random<int>(),
                                               Tests.Random<int>());

            _controller.Setup(m => m.WaitForStatus(expectedState, expectedTimeout))
                       .Callback(() => { waited = true; });

            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.WaitForStatus(expectedState, expectedTimeout);

            // Assert
            Assert.IsTrue(waited);
        }


        [Test]
        public void Test_LSM_Unsubscribe()
        {
            // Arrange
            var elapsed = false;
            var expectedCount = _state.Count;
            _state[_name].Timer.Elapsed += (s, e) => { elapsed = true; };
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            // Act
            sut.Unsubscribe(ref _state);
            Thread.Sleep(TestingTimeout);
            
            // Assert
            Assert.AreEqual(expectedCount, _state.Count,
                            "Expected {0} tracking objects, but found {1}.",
                            expectedCount, _state.Count);

            Assert.IsFalse(_state[_name].Timer.Enabled,
                           "Expected Timer to be disabled, but was not.");

            Assert.IsFalse(elapsed,
                           "Expected Timer NOT to elapse, but did.");
        }

        [Test]
        public void Test_LSM_UpdateSubscription()
        {
            // Arrange
            _factory.Setup(m => m.GetLocalServiceController(_name)).Returns(_controller.Object);
            var sut = new ServerLocalMediator(_logger.Object, _packet, _factory.Object, _harness.Object, TestingTimeout);

            var expected = !_state[_name].NotifyOnUnexpectedStop;
            _packet.NotifyOnUnexpectedStop = expected;

            // Act
            sut.UpdateSubscription(_packet, ref _state);
            var actualEnabled = _state[_name].Timer.Enabled;
            var actualWarn = _state[_name].NotifyOnUnexpectedStop;

            // Assert
            Assert.AreEqual(expected, actualWarn);
            Assert.AreEqual(expected, actualEnabled);
        }
    }
}