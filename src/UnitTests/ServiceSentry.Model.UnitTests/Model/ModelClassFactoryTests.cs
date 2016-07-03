// -----------------------------------------------------------------------
//  <copyright file="ModelClassFactoryTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Communication;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model
{
    [TestFixture]
    internal class ModelClassFactoryTests
    {
        private const string RealService = "W32Time";
        private readonly Mock<Logger> _logger;

        internal ModelClassFactoryTests()
        {
            _logger = new Mock<Logger>();
        }

        [Test]
        public void Test_MCF_CanMock()
        {
            var mock = new Mock<ModelClassFactory>();
            ModelClassFactory obj = null;

            Assert.DoesNotThrow(() => { obj = mock.Object; });
            Assert.IsNotNull(obj);
        }

        [Test, Explicit("Slow.")]
        public void Test_MCF_GetClientMediator()
        {
            // Arrange
            var packet = new SubscriptionPacket
                {
                    ServiceName = RealService,
                    ArchiveLogs = Tests.Random<bool>(),
                    ClearLogs = Tests.Random<bool>(),
                };
            var sut = ModelClassFactory.GetInstance(_logger.Object);
            var clients = new Mock<ClientList>();

            // Act
            var actual = sut.GetClientMediator(clients.Object, packet);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(packet.ServiceName, actual.ServiceName);
        }

        [Test]
        public void Test_MCF_GetLocalServiceController()
        {
            // Arrange
            var sut = ModelClassFactory.GetInstance(_logger.Object);

            // Act
            var actual = sut.GetLocalServiceController(RealService);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(RealService, actual.ServiceName);
        }

        [Test]
        public void Test_MCF_GetLocalServiceFinder()
        {
            // Arrange
            var sut = ModelClassFactory.GetInstance(_logger.Object);

            // Act
            var actual = sut.GetLocalServiceFinder();

            // Assert
            Assert.IsNotNull(actual);
        }
    }
}