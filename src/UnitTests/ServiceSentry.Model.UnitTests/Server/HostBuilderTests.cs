// -----------------------------------------------------------------------
//  <copyright file="HostBuilderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Net;
using System.ServiceModel;
using NUnit.Framework;
using ServiceSentry.Common.Communication;
using ServiceSentry.Common.Testing;
using ServiceSentry.Model.Server;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Server
{
    [TestFixture]
    internal class HostBuilderTests
    {
        [Test]
        public void Test_HostBuilder_BuildHost()
        {
            ServiceHost actual = null;
            var server = Dns.GetHostEntry("localhost").HostName;
            var suffix = Tests.Random<string>();
            var expectedEndpoint = EndpointBuilder.GetInstance(ServiceHostType.NetTcp).GetEndpoint(server, suffix, -1);

            var service = new WindowsServiceDescription
                {
                    ServiceObject = TestWindowsService.GetInstance(),
                    Contract = typeof (ITestService),
                    EndpointSuffix = suffix,
                };

            var sut = HostBuilder.GetInstance(ServiceHostType.NetTcp);

            Assert.DoesNotThrow(() => { actual = sut.BuildHost(service); });
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedEndpoint, actual.Description.Endpoints[0].Address.ToString());
        }
    }
}