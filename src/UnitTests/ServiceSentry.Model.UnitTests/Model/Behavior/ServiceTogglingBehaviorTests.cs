// -----------------------------------------------------------------------
//  <copyright file="ServiceTogglingBehaviorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Behavior
{
    [TestFixture]
    public class ServiceTogglingBehaviorTests
    {
        [SetUp]
        public void SetUp()
        {
            _monitor = new Mock<MonitorServiceWatchdog>();
            _logger = new Mock<Logger>();
            _sut = ServiceTogglingBehavior.GetInstance(_monitor.Object,_logger.Object);

            var deets = ServiceDetails.GetInstance();
            deets.NotifyOnUnexpectedStop = true;

            _service = new Mock<Service>();
            _service.Setup(m => m.CanStop).Returns(true);
            _service.Setup(m => m.Details).Returns(deets);
        }

        private Mock<MonitorServiceWatchdog> _monitor;
        private ServiceTogglingBehavior _sut;
        private Mock<Service> _service;
        private Mock<Logger> _logger;

        private void Test_RefreshStatus(bool statusChange, bool fallOver)
        {
            // Arrange
            var refreshed = false;
            var statusChanged = false;
            var fellOver = false;

            var expectedArgs = new StatusChangedEventArgs
                {
                    OldStatus = ServiceState.Running,
                    NewStatus =
                        (statusChange || fallOver) ? ServiceState.Stopped : ServiceState.Running
                };
            var args = expectedArgs;

            _service.Object.Details.NotifyOnUnexpectedStop = fallOver;
            _service.Setup(m => m.Status).Returns(ServiceState.Running);

            _service.Setup(m => m.OnExternalStatusChange(It.IsAny<StatusChangedEventArgs>()))
                    .Callback<StatusChangedEventArgs>(e =>
                        {
                            statusChanged = true;
                            args = e;
                        });

            _service.Setup(m => m.OnServiceFellOver(It.IsAny<StatusChangedEventArgs>()))
                    .Callback<StatusChangedEventArgs>(e =>
                        {
                            fellOver = true;
                            args = e;
                        });
            _service.Setup(m => m.Refresh(_monitor.Object)).Callback(() =>
                {
                    refreshed = true;
                    if (statusChange || fallOver)
                    {
                        _service.Setup(m => m.Status).Returns(ServiceState.Stopped);
                    }
                });

            

            // Act
            _sut.Refresh(_service.Object);

            // Assert
            Assert.AreEqual(true, refreshed, "Was not refreshed.");
            
            Assert.AreEqual(fallOver, fellOver,
                            "Fellover: expected {0}, was {1}.",
                            fallOver, fellOver);

            Assert.AreEqual(statusChange, statusChanged,
                            "statusChanged: expected {0}, was {1}.",
                            statusChange, statusChanged);

            Assert.AreEqual(expectedArgs.OldStatus, args.OldStatus,
                            "OldStatus: expected {0}, was {1}.",
                            expectedArgs.OldStatus, args.OldStatus);

            Assert.AreEqual(expectedArgs.NewStatus, args.NewStatus,
                            "NewStatus: expected {0}, was {1}.",
                            expectedArgs.NewStatus, args.NewStatus);
        }

        [Test]
        public void Test_ServiceBehavior_RefreshStatus_FellOver()
        {
            Test_RefreshStatus(false, true);
        }

        [Test]
        public void Test_ServiceBehavior_RefreshStatus_NoChange()
        {
            Test_RefreshStatus(false, false);
        }

        [Test]
        public void Test_ServiceBehavior_RefreshStatus_StatusChange()
        {
            Test_RefreshStatus(true, false);
        }


        [Test]
        public void Test_ServiceBehavior_Start_a_Running_Service()
        {
            // Arrange
            var wasCalled = false;
            const ServiceState expected = ServiceState.Running;
            _service.Setup(m => m.Status).Returns(ServiceState.Running);
            _service.Setup(m => m.Start()).Callback(() =>
                {
                    wasCalled = true;
                    _service.Setup(m => m.Status).Returns(expected);
                });

            // Act
            var actual = _sut.Start(_service.Object);

            // Assert
            Assert.IsFalse(wasCalled);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceBehavior_Start_a_Stopped_Service()
        {
            // Arrange
            var wasCalled = false;
            const ServiceState expected = ServiceState.Running;
            _service.Setup(m => m.Status).Returns(ServiceState.Stopped);
            _service.Setup(m => m.Start()).Callback(() =>
                {
                    wasCalled = true;
                    _service.Setup(m => m.Status).Returns(expected);
                });

            // Act
            var actual = _sut.Start(_service.Object);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceBehavior_Stop_a_Service_that_cannot_Stop()
        {
            // Arrange
            var wasCalled = false;
            const ServiceState expected = ServiceState.Running;
            _service.Setup(m => m.CanStop).Returns(false);
            _service.Setup(m => m.Status).Returns(expected);
            _service.Setup(m => m.Stop(It.IsAny<MonitorServiceWatchdog>())).Callback(() => { wasCalled = true; });

            // Act
            var actual = _sut.Stop(_service.Object);

            // Assert
            Assert.IsFalse(wasCalled);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceBehavior_Stop_a_running_Service()
        {
            // Arrange
            var wasCalled = false;
            const ServiceState expected = ServiceState.Stopped;
            _service.Setup(m => m.Status).Returns(ServiceState.Running);
            _service.Setup(m => m.Stop(It.IsAny<MonitorServiceWatchdog>())).Callback(() =>
                {
                    wasCalled = true;
                    _service.Setup(m => m.Status).Returns(expected);
                });


            // Act
            var actual = _sut.Stop(_service.Object);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceBehavior_Stop_a_stopped_Service()
        {
            // Arrange
            var wasCalled = false;
            const ServiceState expected = ServiceState.Stopped;
            _service.Setup(m => m.Status).Returns(ServiceState.Stopped);
            _service.Setup(m => m.Stop(It.IsAny<MonitorServiceWatchdog>())).Callback(() =>
                {
                    wasCalled = true;
                    _service.Setup(m => m.Status).Returns(expected);
                });


            // Act
            var actual = _sut.Stop(_service.Object);

            // Assert
            Assert.IsFalse(wasCalled);
            Assert.AreEqual(expected, actual);
        }

        //TODO : ServiceBehavior: Add tests to check for different Timeout values.
    }
}