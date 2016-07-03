// -----------------------------------------------------------------------
//  <copyright file="WindowsServiceHarnessTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Common.Testing;
using ServiceSentry.ServiceFramework;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    internal class WindowsServiceHarnessTests
    {
        [SetUp]
        public void SetUp()
        {
            // Arrange
            _service = TestWindowsService.GetInstance();
        }

        private const bool ExpectedAutoLog = true;
        private const bool ExpectedCanHandlePowerEvent = false;
        private const bool ExpectedCanHandleSessionChangeEvent = false;

        private readonly bool _expectedCanStop;
        private readonly bool _expectedCanPauseAndContinue;
        private readonly bool _expectedCanShutdown;

        private TestWindowsService _service;

        public WindowsServiceHarnessTests()
        {
            using (var dummyService = TestWindowsService.GetInstance())
            {
                var attribute = dummyService.GetType().GetAttribute<WindowsServiceAttribute>();
                Assert.IsNotNull(attribute, "WindowsServiceAttribute is null.");
                _expectedCanStop = attribute.CanStop;
                _expectedCanPauseAndContinue = attribute.CanPauseAndContinue;
                _expectedCanShutdown = attribute.CanShutdown;
            }
        }

        [Test]
        public void Test_Harness_Constructor_NoServiceAttribute()
        {
            var tempService = new Mock<WindowsService>();

            // Assert
            Assert.Throws<InvalidOperationException>(
                () => WindowsServiceHarness.GetInstance(tempService.Object));
        }

        [Test]
        public void Test_Harness_Get_AutoLog()
        {
            // Act
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Assert
            Assert.AreEqual(ExpectedAutoLog, sut.AutoLog,
                            "AutoLog should be {0}, but is {1}.",
                            ExpectedAutoLog, sut.AutoLog);
        }

        [Test]
        public void Test_Harness_Get_CanHandlePowerEvent()
        {
            // Act
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Assert
            Assert.AreEqual(ExpectedCanHandlePowerEvent, sut.CanHandlePowerEvent,
                            "CanHandlePowerEvent should be {0}, but is {1}.",
                            ExpectedCanHandlePowerEvent, sut.CanHandlePowerEvent);
        }

        [Test]
        public void Test_Harness_Get_CanHandleSessionChangeEvent()
        {
            // Act
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Assert
            Assert.AreEqual(ExpectedCanHandleSessionChangeEvent, sut.CanHandleSessionChangeEvent,
                            "CanHandleSessionChangeEvent should be {0}, but is {1}.",
                            ExpectedCanHandleSessionChangeEvent, sut.CanHandleSessionChangeEvent);
        }

        [Test]
        public void Test_Harness_Get_CanPauseAndContinue()
        {
            // Act
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Assert
            Assert.AreEqual(_expectedCanPauseAndContinue, sut.CanPauseAndContinue,
                            "CanPauseAndContinue should be {0}, but is {1}.",
                            _expectedCanPauseAndContinue, sut.CanPauseAndContinue);
        }

        [Test]
        public void Test_Harness_Get_CanStop()
        {
            // Act
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Assert
            Assert.AreEqual(_expectedCanStop, sut.CanStop,
                            "CanStop should be {0}, but is {1}.",
                            _expectedCanStop, sut.CanStop);
        }

        [Test]
        public void Test_Harness_Get_Shutdown()
        {
            // Act
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Assert
            Assert.AreEqual(_expectedCanShutdown, sut.CanShutdown,
                            "CanShutdown should be {0}, but is {1}.",
                            _expectedCanShutdown, sut.CanShutdown);
        }

        [Test]
        public void Test_Harness_Set_AutoLog()
        {
            // Arrange 
            var sut = WindowsServiceHarness.GetInstance(_service);
            var expected = !sut.AutoLog;

            // Act
            sut.AutoLog = expected;

            // Assert
            Assert.AreEqual(expected, sut.AutoLog,
                            "AutoLog should be {0}, but is {1}.",
                            expected, sut.AutoLog);
        }

        [Test]
        public void Test_Harness_Set_CanHandlePowerEvent()
        {
            // Arrange
            var sut = WindowsServiceHarness.GetInstance(_service);
            var expected = !sut.CanHandlePowerEvent;

            // Act
            sut.CanHandlePowerEvent = expected;

            // Assert
            Assert.AreEqual(expected, sut.CanHandlePowerEvent,
                            "CanHandlePowerEvent should be {0}, but is {1}.",
                            expected, sut.CanHandlePowerEvent);
        }

        [Test]
        public void Test_Harness_Set_CanHandleSessionChangeEvent()
        {
            // Arrange
            var sut = WindowsServiceHarness.GetInstance(_service);
            var expected = !sut.CanHandleSessionChangeEvent;

            // Act
            sut.CanHandleSessionChangeEvent = expected;

            // Assert
            Assert.AreEqual(expected, sut.CanHandleSessionChangeEvent,
                            "CanHandleSessionChangeEvent should be {0}, but is {1}.",
                            expected, sut.CanHandleSessionChangeEvent);
        }

        [Test]
        public void Test_Harness_Set_CanPauseAndContinue()
        {
            // Arrange
            var sut = WindowsServiceHarness.GetInstance(_service);
            var expected = !sut.CanPauseAndContinue;

            // Act
            sut.CanPauseAndContinue = expected;

            // Assert
            Assert.AreEqual(expected, sut.CanPauseAndContinue,
                            "CanPauseAndContinue should be {0}, but is {1}.",
                            expected, sut.CanPauseAndContinue);
        }

        [Test]
        public void Test_Harness_Set_CanShutdown()
        {
            // Arrange 
            var sut = WindowsServiceHarness.GetInstance(_service);
            var expected = !sut.CanShutdown;

            // Act
            sut.CanShutdown = expected;

            // Assert
            Assert.AreEqual(expected, sut.CanShutdown,
                            "CanShutdown should be {0}, but is {1}.",
                            expected, sut.CanShutdown);
        }

        [Test]
        public void Test_Harness_Set_CanStop()
        {
            // Arrange 
            var sut = WindowsServiceHarness.GetInstance(_service);
            var expected = !sut.CanStop;

            // Act
            sut.CanStop = expected;

            // Assert
            Assert.AreEqual(expected, sut.CanStop,
                            "CanStop should be {0}, but is {1}.",
                            expected, sut.CanStop);
        }


        [Test,
         Explicit(
             "If this test throws a 'Cannot start service from the command line or a debugger' error, then it passes.")]
        public void Test_Run()
        {
            // Arrange 
            var sut = WindowsServiceHarness.GetInstance(_service);

            // Act
            sut.Run();
        }
    }
}