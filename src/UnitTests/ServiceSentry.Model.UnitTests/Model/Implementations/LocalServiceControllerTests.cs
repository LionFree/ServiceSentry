// -----------------------------------------------------------------------
//  <copyright file="LocalServiceControllerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using Moq;
using NUnit.Framework;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Implementations
{
    [TestFixture]
    internal class LocalServiceControllerTests
    {
        private const string RealService = "W32Time";
        private readonly Mock<LocalServiceFinder> _finder;

        public LocalServiceControllerTests()
        {
            _finder = new Mock<LocalServiceFinder>();
        }

        private void StopService(string serviceName)
        {
            const ServiceControllerStatus desiredStatus = ServiceControllerStatus.Stopped;
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == desiredStatus) return;
                if (!sc.CanStop) Assert.Fail("Service cannot Stop.");

                sc.Stop();
                sc.WaitForStatus(desiredStatus);
            }
        }

        private void StartService(string serviceName)
        {
            const ServiceControllerStatus desiredStatus = ServiceControllerStatus.Running;
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == desiredStatus) return;

                sc.Start();
                sc.WaitForStatus(desiredStatus);
            }
        }

        public bool Test_LSC_Wait_with_Timeout(TimeSpan timeout, int timeToTake)
        {
            var finished = false;
            ServiceState expectedState;
            var sut = new LocalServiceController(RealService, _finder.Object);
            using (var sc = new ServiceController(RealService))
            {
                sc.Refresh();
                var currentStatus = sc.Status;
                expectedState = (currentStatus == ServiceControllerStatus.Running)
                                    ? ServiceState.Stopped
                                    : ServiceState.Running;
            }

            var bg = new BackgroundWorker();
            bg.DoWork += (s, e) => sut.WaitForStatus(expectedState, timeout);
            bg.RunWorkerCompleted += (s, e) =>
                {
                    sut.Refresh();
                    if (sut.Status == expectedState) finished = true;
                };

            // Act
            bg.RunWorkerAsync();

            Thread.Sleep(TimeSpan.FromSeconds(timeToTake));

            if (expectedState == ServiceState.Running)
            {
                StartService(RealService);
            }
            else
            {
                StopService(RealService);
            }

            return finished;
        }

        [Test]
        public void Test_LSC_CanStop()
        {
            // Arrange
            var expected = (new ServiceController(RealService)).CanStop;
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            var actual = sut.CanStop;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSC_DisplayName()
        {
            // Arrange
            var expected = (new ServiceController(RealService)).DisplayName;
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            var actual = sut.DisplayName;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSC_Dispose()
        {
            // Arrange
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            sut.Dispose();

            // Assert
            Assert.IsTrue(sut.IsDisposed);
        }

        [Test]
        public void Test_LSC_IsInstalled()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _finder.Setup(m => m.IsInstalled(RealService)).Returns(expected);
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            var actual = sut.IsInstalled;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_LSC_MachineName()
        {
            // Arrange
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            var actual = sut.MachineName;

            // Assert
            Assert.AreEqual(".", actual);
        }

        [Test]
        public void Test_LSC_Refresh()
        {
            // Arrange
            ServiceState expectedState;
            var sut = new LocalServiceController(RealService, _finder.Object);
            using (var sc = new ServiceController(RealService))
            {
                sc.Refresh();
                var currentStatus = sc.Status;
                expectedState = (currentStatus == ServiceControllerStatus.Running)
                                    ? ServiceState.Stopped
                                    : ServiceState.Running;
            }

            // Act
            var actual = sut.Status;
            if (expectedState == ServiceState.Running)
            {
                StartService(RealService);
            }
            else
            {
                StopService(RealService);
            }
            var actualAfterToggle = sut.Status;
            sut.Refresh();
            var actualAfterRefresh = sut.Status;

            // Assert
            Assert.AreEqual(actualAfterToggle, actual);
            Assert.AreEqual(actualAfterRefresh, expectedState);
        }

        [Test]
        public void Test_LSC_ServiceName()
        {
            // Arrange
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            var actual = sut.ServiceName;

            // Assert
            Assert.AreEqual(RealService, actual);
        }

        [Test, Explicit("Slow: touches a real service.")]
        public void Test_LSC_Start()
        {
            // Arrange
            const ServiceState expected = ServiceState.Running;
            var sut = new LocalServiceController(RealService, _finder.Object);
            StopService(RealService);

            // Act
            sut.Start();
            sut.WaitForStatus(expected);

            // Assert
            Assert.AreEqual(expected, sut.Status);
        }

        [Test]
        public void Test_LSC_Status()
        {
            // Arrange
            var expected = (new ServiceController(RealService)).Status.ToState();
            var sut = new LocalServiceController(RealService, _finder.Object);

            // Act
            var actual = sut.Status;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Slow: touches a real service.")]
        public void Test_LSC_Stop()
        {
            // Arrange
            const ServiceState expected = ServiceState.Stopped;
            var sut = new LocalServiceController(RealService, _finder.Object);
            StartService(RealService);

            // Act
            sut.Stop();
            sut.WaitForStatus(expected);

            // Assert
            Assert.AreEqual(expected, sut.Status);
        }


        [Test, Explicit("Slow: touches a real service.")]
        public void Test_LSC_WaitForStatus()
        {
            // Arrange
            var finished = false;
            ServiceState expectedState;
            var sut = new LocalServiceController(RealService, _finder.Object);
            using (var sc = new ServiceController(RealService))
            {
                sc.Refresh();
                var currentStatus = sc.Status;
                expectedState = (currentStatus == ServiceControllerStatus.Running)
                                    ? ServiceState.Stopped
                                    : ServiceState.Running;
            }

            var bg = new BackgroundWorker();
            bg.DoWork += (s, e) => sut.WaitForStatus(expectedState);
            bg.RunWorkerCompleted += (s, e) =>
                {
                    sut.Refresh();
                    if (sut.Status == expectedState) finished = true;
                };

            // Act
            bg.RunWorkerAsync();

            if (expectedState == ServiceState.Running)
            {
                StartService(RealService);
            }
            else
            {
                StopService(RealService);
            }

            sut.Refresh();
            var actualAfterRefresh = sut.Status;

            // Assert
            Assert.IsTrue(finished);
            Assert.AreEqual(actualAfterRefresh, expectedState);
        }

        [Test, Explicit("Slow: touches a real service.")]
        public void Test_LSC_WaitForStatus_Fail()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(Tests.Random<int>(2));
            var time = timeout.Seconds + 1;

            // Act
            var finished = Test_LSC_Wait_with_Timeout(timeout, time);

            // Assert
            Assert.IsFalse(finished);
        }

        [Test, Explicit("Slow: touches a real service.")]
        public void Test_LSC_WaitForStatus_Success()
        {
            // Arrange
            var time = Tests.Random<int>(2);
            var timeout = TimeSpan.FromSeconds(time + 1);

            // Act
            var finished = Test_LSC_Wait_with_Timeout(timeout, time);

            // Assert
            Assert.IsTrue(finished);
        }

        [Test]
        public void Test_LocalServiceController_Constructor_Name_is_Real()
        {
            LocalServiceController sut = null;
            Assert.DoesNotThrow(() => { sut = new LocalServiceController(RealService, _finder.Object); });
            Assert.IsNotNull(sut);
        }

        [Test]
        public void Test_LocalServiceController_Constructor_Name_is_null()
        {
            LocalServiceController sut = null;
            Assert.Throws<ArgumentNullException>(() => { sut = new LocalServiceController(null, _finder.Object); });
            Assert.IsNull(sut);
        }
    }
}