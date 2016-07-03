// -----------------------------------------------------------------------
//  <copyright file="ServerMediatorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using Moq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Monitor.Mediator;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Monitor.UnitTests.Model
{
    [TestFixture]
    internal class ServerMediatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _harness = new Mock<ConsoleHarness>();
            _logger = new Mock<Logger>();
        }

        private Mock<ConsoleHarness> _harness;
        private Mock<Logger> _logger;


        [Test]
        public void Test_SM_GetInstance_Name()
        {
            var name = Tests.Random<string>();
            var factory = ModelClassFactory.GetInstance(_logger.Object);

            var sut = ServerMediator.GetInstance(_logger.Object, name, factory, _harness.Object);

            Assert.That(sut is ServerLocalMediator);
        }

        [Test]
        public void Test_SM_GetInstance_Packet()
        {
            var packet = new SubscriptionPacket
                {
                    ServiceName = Tests.Random<string>(),
                };
            var factory = ModelClassFactory.GetInstance(_logger.Object);

            var sut = ServerMediator.GetInstance(_logger.Object, packet, factory, _harness.Object);

            Assert.That(sut is ServerLocalMediator);
        }
    }
}