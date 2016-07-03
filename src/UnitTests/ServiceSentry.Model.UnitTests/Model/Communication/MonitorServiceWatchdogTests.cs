// -----------------------------------------------------------------------
//  <copyright file="MonitorServiceWatchdogTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using Moq;
using NUnit.Framework;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Services;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Communication
{
    [TestFixture]
    internal class MonitorServiceWatchdogTests
    {
        private readonly int _pollInterval;

        public MonitorServiceWatchdogTests()
        {
            _pollInterval = 2;
        }

        private Mock<ModelClassFactory> BuildFactory(bool isInstalled, bool isRunning)
        {
            var factory = new Mock<ModelClassFactory>();
            var lsf = new Mock<LocalServiceFinder>();
            lsf.Setup(m => m.IsInstalled(Extensibility.Strings._HostServiceName)).Returns(isInstalled);
            factory.Setup(m => m.GetLocalServiceFinder()).Returns(lsf.Object);

            var expectedState = isRunning ? ServiceState.Running : ServiceState.Stopped;
            var lsc = new Mock<ServiceWrapper>();
            lsc.Setup(m => m.Status).Returns(expectedState);
            factory.Setup(m => m.GetLocalServiceController(Extensibility.Strings._HostServiceName)).Returns(lsc.Object);

            return factory;
        }

        [Test]
        public void Test_CheckInstalled()
        {
            // Arrange
            var expectedInstalled = Tests.Random<bool>();
            var expectedRunning = Tests.Random<bool>();

            var factory = BuildFactory(expectedInstalled, expectedRunning);
            var sut = MonitorServiceWatchdog.GetInstance(factory.Object, _pollInterval);

            // Act
            var actual = sut.IsInstalled;

            // Assert
            Assert.AreEqual(expectedInstalled, actual);
        }

        [Test]
        public void Test_CheckIsAvailable()
        {
            // Arrange
            var expectedInstalled = Tests.Random<bool>();
            var expectedRunning = Tests.Random<bool>();

            var factory = BuildFactory(expectedInstalled, expectedRunning);
            var sut = MonitorServiceWatchdog.GetInstance(factory.Object, _pollInterval);

            // Act
            var actual = sut.IsAvailable;

            // Assert
            Assert.AreEqual(expectedRunning, actual);
        }
    }
}