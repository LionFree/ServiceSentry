// -----------------------------------------------------------------------
//  <copyright file="WindowsServiceManagerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceProcess;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Common.Testing;
using ServiceSentry.Extensibility;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    internal class WindowsServiceManagerTests
    {
        private readonly Mock<ConsoleHarness> _harness;
        private readonly Mock<AssemblyWrapper> _assembly;
        private readonly Mock<TestAssembly> _testAssembly;
        private readonly Mock<ManagedInstaller> _installer;
        private readonly Mock<ControllerWrapper> _controller;
        private readonly ServiceMetadata _data;

        public WindowsServiceManagerTests()
        {
            _harness = new Mock<ConsoleHarness>();
            _installer = new Mock<ManagedInstaller>();

            _testAssembly = new Mock<TestAssembly>();
            _testAssembly.Setup(m => m.Location).Returns(Tests.Random<string>());

            _assembly = new Mock<AssemblyWrapper>();
            _assembly.Setup(m => m.GetEntryAssembly()).Returns(_testAssembly.Object);

            _controller = new Mock<ControllerWrapper>();

            _data = new ServiceMetadata {Silent = false, Quiet = false, ServiceName = Tests.Random<string>()};
        }

        [Test]
        public void Test__Get_Status()
        {
            //Arrange
            var expected = Tests.Random<ServiceControllerStatus>();
            _controller.Setup(m => m.Status).Returns(expected);
            ServiceControllerStatus? actual = ServiceControllerStatus.Stopped;
            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            Assert.DoesNotThrow(() => { actual = sut.Status; });

            // Assert
            Assert.AreEqual(expected, actual, "Service status should be {0}, but is {1}.", expected, actual);
        }

        [Test]
        public void Test_Install()
        {
            var finished = false;
            var installed = false;
            _installer.Setup(m => m.InstallHelper(It.IsAny<string[]>())).Callback(() => { installed = true; });
            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            Assert.DoesNotThrow(() => { finished = sut.Install(); });

            // Assert
            Assert.IsTrue(installed, "InstallHelper was not called.");
            Assert.IsTrue(finished, "Method did not finish as expected.");
        }

        [Test]
        public void Test_InstallAndStart()
        {
            var started = false;
            var installed = false;
            var finished = false;

            _controller.Setup(m => m.CanStop).Returns(true);
            _controller.Setup(m => m.Status).Returns(ServiceControllerStatus.Stopped);
            _controller.Setup(m => m.Start()).Callback(() => { started = true; });

            _installer.Setup(m => m.InstallHelper(It.IsAny<string[]>())).Callback(() => { installed = true; });
            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            Assert.DoesNotThrow(() => { finished = sut.InstallAndStart(); });

            // Assert
            Assert.IsTrue(installed, "InstallHelper was not called.");
            Assert.IsTrue(started, "Service was not started.");
            Assert.IsTrue(finished, "Method did not finish as expected.");
        }

        [Test]
        public void Test_ShowStatus()
        {
            var finished = false;
            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            Assert.DoesNotThrow(() => { finished = sut.ShowStatus(); });

            // Assert
            Assert.IsTrue(finished, "Method did not finish as expected.");
        }

        [Test]
        public void Test_StartService()
        {
            //Arrange
            var serviceStarted = false;
            var finished = false;

            _controller.Setup(m => m.CanStop).Returns(true);
            _controller.Setup(m => m.Status).Returns(ServiceControllerStatus.Stopped);
            _controller.Setup(m => m.Start()).Callback(() => { serviceStarted = true; });

            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            Assert.DoesNotThrow(() => { finished = sut.StartService(); });

            // Assert
            Assert.IsTrue(serviceStarted, "Service was not started.");
            Assert.IsTrue(finished, "Method did not finish as expected.");
        }

        [Test]
        public void Test_StartService_withArgs()
        {
            // Arrange
            var serviceStarted = false;
            var finished = false;

            var serviceArgs = new[] {string.Empty};
            _controller.Setup(m => m.CanStop).Returns(true);
            _controller.Setup(m => m.Status).Returns(ServiceControllerStatus.Stopped);
            _controller.Setup(m => m.Start(It.IsAny<string[]>())).Callback(() => { serviceStarted = true; });

            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            Assert.DoesNotThrow(() => { finished = sut.StartService(serviceArgs); });

            // Assert
            Assert.IsTrue(serviceStarted, "Start method was not called.");
            Assert.IsTrue(finished, "Method did not finish as expected.");
        }

        [Test]
        public void Test_Uninstall()
        {
            var uninstalled = false;
            _installer.Setup(m => m.InstallHelper(It.IsAny<string[]>())).Callback(() => { uninstalled = true; });
            var sut = WindowsServiceManager.GetInstance(_data,
                                                        _harness.Object,
                                                        _assembly.Object,
                                                        _installer.Object,
                                                        _controller.Object,
                                                        false);

            // Act
            var actual = sut.Uninstall();

            // Assert
            Assert.IsTrue(uninstalled, "InstallHelper was not called.");
            Assert.IsTrue(actual, "Method did not finish as expected.");
        }
    }

    [TestFixture]
    internal class ControllerWrapperTests
    {
        private const string ServiceName = "W32Time";

        private static void StopService(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Stopped) return;
                if (sc.CanStop)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    if (sc.Status == ServiceControllerStatus.Stopped) return;
                }
                Assert.Fail("Could not stop service.");
            }
        }

        private static void StartService(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Running) return;

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running);
                if (sc.Status == ServiceControllerStatus.Running) return;

                Assert.Fail("Could not stop service.");
            }
        }

        [Test, Explicit("Touches a real service.")]
        public void Test_ControllerWrapper_Start()
        {
            const ServiceControllerStatus expected = ServiceControllerStatus.Running;
            var actual = ServiceControllerStatus.Stopped;
            StopService(ServiceName);

            Assert.DoesNotThrow(() => ControllerWrapper.GetInstance(ServiceName).Start());
            Assert.DoesNotThrow(
                () => ControllerWrapper.GetInstance(ServiceName).WaitForStatus(ServiceControllerStatus.Running));


            using (var sc = new ServiceController(ServiceName))
            {
                actual = sc.Status;
            }
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Touches a real service.")]
        public void Test_ControllerWrapper_Stop()
        {
            const ServiceControllerStatus expected = ServiceControllerStatus.Stopped;
            var actual = ServiceControllerStatus.Running;
            StartService(ServiceName);

            Assert.DoesNotThrow(() => ControllerWrapper.GetInstance(ServiceName).Stop());
            Assert.DoesNotThrow(
                () => ControllerWrapper.GetInstance(ServiceName).WaitForStatus(ServiceControllerStatus.Stopped));

            using (var sc = new ServiceController(ServiceName))
            {
                actual = sc.Status;
            }
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ControllerWrapper__Get_CanStop()
        {
            bool expected;
            using (var sc = new ServiceController(ServiceName))
            {
                expected = sc.CanStop;
            }

            var actual = ControllerWrapper.GetInstance(ServiceName).CanStop;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ControllerWrapper__Get_Status()
        {
            ServiceControllerStatus expected;
            using (var sc = new ServiceController(ServiceName))
            {
                expected = sc.Status;
            }

            var actual = ControllerWrapper.GetInstance(ServiceName).Status;

            Assert.AreEqual(expected, actual);
        }
    }
}