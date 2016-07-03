// -----------------------------------------------------------------------
//  <copyright file="ClientListTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Net;
using Moq;
using NUnit.Framework;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Communication;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Client
{
    [TestFixture]
    internal class ClientListTests
    {
        [Test]
        public void Test_GetClient_Existing()
        {
            // Arrange
            var count = Tests.Random<int>(25);
            var machines = new string[25];
            var mcb = new Mock<ClientBuilder>();

            var expected = new ServiceClient<IMonitorService>[25];
            var actual = new ServiceClient<IMonitorService>[25];

            for (var i = 0; i < count; i++)
            {
                machines[i] = Tests.Random<string>();
                expected[i] = ClientBuilder.GetInstance()
                                           .GetClient<IMonitorService>(machines[i], Extensibility.Strings._HostServiceName);
                mcb.Setup(m => m.GetClient<IMonitorService>(machines[i], It.IsAny<string>())).Returns(expected[i]);
            }

            expected = new ServiceClient<IMonitorService>[25];

            var sut = ClientList.GetInstance(mcb.Object);

            // Act
            for (var i = 0; i < count; i++)
            {
                expected[i] = sut.GetClient(machines[i]);
                actual[i] = sut.GetClient(machines[i]);
            }

            // Assert
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [Test]
        public void Test_GetClient_New()
        {
            // Arrange
            var machine = Dns.GetHostEntry("localhost").HostName;
            var expected = ClientBuilder.GetInstance().GetClient<IMonitorService>(machine, Extensibility.Strings._HostServiceName);
            ServiceClient<IMonitorService> actual = null;

            var mcb = new Mock<ClientBuilder>();
            mcb.Setup(m => m.GetClient<IMonitorService>(It.IsAny<string>(), It.IsAny<string>())).Returns(expected);

            var sut = ClientList.GetInstance(mcb.Object);

            // Act
            Assert.DoesNotThrow(() => { actual = sut.GetClient(machine); });

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}