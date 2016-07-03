// -----------------------------------------------------------------------
//  <copyright file="ServiceBootstrapperTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.CommandLine;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Common.Testing;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    internal class ServiceBootstrapperTests
    {
        private readonly Mock<Logger> _logger;

        internal ServiceBootstrapperTests()
        {
            _logger = new Mock<Logger>();
        }


        [Test]
        public void Test_GetInstance_UnmarkedWindowsService()
        {
            // Arrange
            var service = UnmarkedWindowsService.GetInstance();

            // Assert
            Assert.Throws<InvalidOperationException>(() => ServiceBootstrapper.GetInstance(service, _logger.Object));
        }

        [Test]
        public void Test_GetInstance_WindowsService()
        {
            // Arrange
            var service = TestWindowsService.GetInstance();

            // Act
            var sut = ServiceBootstrapper.GetInstance(service, _logger.Object);

            // Assert
            Assert.IsNotNull(sut);
        }


        [Test]
        public void Test_Startup_Interactive()
        {
            // Arrange
            var parsed = false;
            var service = TestWindowsService.GetInstance();
            var hness = WindowsServiceHarness.GetInstance(service);
            var parser = new Mock<CommandLineParser>();
            var attributes = new Mock<AssemblyWrapper>();

            parser.Setup(m => m.Length).Returns(0);
            parser.Setup(m => m.Parse(It.IsAny<string[]>()))
                  .Returns(parser.Object)
                  .Callback(() => { parsed = true; });

            var sut = ServiceBootstrapper.GetInstance(service, true, _logger.Object, hness, parser.Object,
                                                      attributes.Object);

            // Act
            sut.Startup(new[] {string.Empty});

            // Assert
            Assert.IsTrue(parsed);
        }

        [Test]
        public void Test_Startup_NonInteractive()
        {
            // Arrange
            var wasCalled = false;
            var service = TestWindowsService.GetInstance();
            var harness = new Mock<WindowsServiceHarness>();
            var parser = new Mock<CommandLineParser>();
            var attributes = new Mock<AssemblyWrapper>();

            harness.Setup(m => m.Run()).Callback(() => { wasCalled = true; });


            var sut = ServiceBootstrapper.GetInstance(service, false, _logger.Object, harness.Object,
                                                      parser.Object, attributes.Object);

            // Act
            sut.Startup(new[] {string.Empty});

            // Assert
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_Startup_withArgs_Interactive()
        {
            // Arrange
            var parsed = false;
            var service = TestWindowsService.GetInstance();
            var hness = WindowsServiceHarness.GetInstance(service);
            var parser = new Mock<CommandLineParser>();
            var attributes = new Mock<AssemblyWrapper>();

            parser.Setup(m => m.Length).Returns(0);
            parser.Setup(m => m.Parse(It.IsAny<string[]>()))
                  .Returns(parser.Object)
                  .Callback(() => { parsed = true; });

            var sut = ServiceBootstrapper.GetInstance(service, true, _logger.Object, hness, parser.Object,
                                                      attributes.Object);

            // Act
            sut.Startup(new[] {"-t"});

            // Assert
            Assert.IsTrue(parsed);
        }

        [Test]
        public void Test_Startup_withArgs_NonInteractive()
        {
            // Arrange
            var wasCalled = false;
            var service = TestWindowsService.GetInstance();
            var harness = new Mock<WindowsServiceHarness>();
            var parser = new Mock<CommandLineParser>();
            var attributes = new Mock<AssemblyWrapper>();

            harness.Setup(m => m.Run()).Callback(() => { wasCalled = true; });


            var sut = ServiceBootstrapper.GetInstance(service, false, _logger.Object, harness.Object, parser.Object,
                                                      attributes.Object);

            // Act
            sut.Startup(new[] {"-t"});

            // Assert
            Assert.IsTrue(wasCalled);
        }
    }
}