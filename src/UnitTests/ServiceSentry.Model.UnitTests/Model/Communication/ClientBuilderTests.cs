// -----------------------------------------------------------------------
//  <copyright file="ClientBuilderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Net;
using System.ServiceModel.Channels;
using NUnit.Framework;
using ServiceSentry.Common.Communication;
using ServiceSentry.Common.Testing;
using ServiceSentry.Model.Client;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Communication
{
    [TestFixture]
    internal class ClientBuilderTests
    {
        public ServiceClient<T> Test_ClientBuilder_GetClient<T>(string server) where T : class
        {
            var sut = ClientBuilder.GetInstance();
            var suffix = Tests.Random<string>();

            //Act
            var client = sut.GetClient<T>(server, suffix);

            // Assert
            Assert.IsNotNull(client);
            return client;
        }

        private void Test_ClientBuilder_GetClient_LocalHost(string server)
        {
            var client = Test_ClientBuilder_GetClient<ITestService>(server);
            Assert.IsTrue(client.IsLocal);
        }

        [Test]
        public void Test_ClientBuilder_GetClient_Dot()
        {
            Test_ClientBuilder_GetClient_LocalHost(".");
        }

        [Test]
        public void Test_ClientBuilder_GetClient_LocalHost()
        {
            Test_ClientBuilder_GetClient_LocalHost("localhost");
        }

        [Test]
        public void Test_ClientBuilder_GetClient_LocalHost_by_DNS()
        {
            Test_ClientBuilder_GetClient_LocalHost(Dns.GetHostEntry("LocalHost").HostName);
        }

        [Test]
        public void Test_ClientBuilder_GetClient_Random()
        {
            var client = Test_ClientBuilder_GetClient<ITestService>(Tests.Random<string>());
            Assert.IsFalse(client.IsLocal);
        }
    }


    [TestFixture]
    internal class ServiceClientTests
    {
        private readonly ChannelExposingClient<ITestService> _cet;
        private readonly bool _isLocal;

        public ServiceClientTests()
        {
            var suffix = Tests.Random<string>();
            var binding = BindingBuilder.GetInstance(ServiceHostType.NetTcp).GetBinding();
            var localEndpoint = EndpointBuilder.GetInstance(ServiceHostType.NetTcp)
                                               .GetEndpoint(Dns.GetHostEntry("localhost").HostName, suffix,-1);
            var remoteEndpoint = EndpointBuilder.GetInstance(ServiceHostType.NetTcp).GetEndpoint(Tests.Random<string>(), suffix,-1);
            _isLocal = Tests.Random<bool>();

            _cet = ChannelExposingClient<ITestService>.GetInstance(binding, _isLocal ? localEndpoint : remoteEndpoint);
        }

        [Test]
        public void Test_ServiceClient_Execute()
        {
            // Arrange
            var expectedParameter = Tests.Random<string>();
            var executed = false;
            object actualParameter = null;
            var sut = ServiceClient<ITestService>.GetInstance(_cet);

            // Act
            var done = sut.Execute(x =>
                {
                    executed = true;
                    actualParameter = x;
                }, expectedParameter);

            // Assert
            Assert.IsTrue(done);
            Assert.IsTrue(executed);
            Assert.AreEqual(expectedParameter, actualParameter);
        }

        [Test]
        public void Test_ServiceClient_Failback()
        {
            // Arrange
            var expectedParameter = Tests.Random<string>();
            var expectedException = new Exception(Tests.Random<string>());
            var executed = false;
            var failed = false;
            object actualParameter = null;
            Exception actualException = null;
            var sut = ServiceClient<ITestService>.GetInstance(_cet);

            // Act
            var done = sut.Execute(
                x =>
                    {
                        executed = true;
                        actualParameter = x;
                        throw expectedException;
                    },
                (x, ex) =>
                    {
                        failed = true;
                        actualException = ex;
                    },
                expectedParameter);

            // Assert
            Assert.IsTrue(done);
            Assert.IsTrue(executed);
            Assert.IsTrue(failed);
            Assert.AreEqual(expectedParameter, actualParameter);
            Assert.AreEqual(expectedException, actualException);
        }

        [Test]
        public void Test_ServiceClient_IsLocal()
        {
            // Arrange
            var expected = _isLocal;
            var sut = ServiceClient<ITestService>.GetInstance(_cet);

            // Act
            var actual = sut.IsLocal;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceClient_Service()
        {
            // Arrange
            var expectedType = typeof (ITestService);
            var sut = ServiceClient<ITestService>.GetInstance(_cet);

            // Act
            var actual = sut.Service;

            // Assert
            var actualType = actual.GetType();
            Assert.AreEqual(expectedType, actualType);
        }
    }

    [TestFixture]
    internal class ChannelExposingClientTests
    {
        private readonly Binding _binding;
        private readonly string _endpoint;

        public ChannelExposingClientTests()
        {
            _binding = BindingBuilder.GetInstance(ServiceHostType.NetTcp).GetBinding();

            var machine = Tests.Random<string>();
            var suffix = Tests.Random<string>();
            _endpoint = EndpointBuilder.GetInstance(ServiceHostType.NetTcp).GetEndpoint(machine, suffix, -1);
        }

        [Test]
        public void Test_CET_Service()
        {
            // Arrange
            var expectedType = typeof (ITestService);
            ChannelExposingClient<ITestService> sut = null;

            // Act
            Assert.DoesNotThrow(() => { sut = ChannelExposingClient<ITestService>.GetInstance(_binding, _endpoint); });

            // Assert
            Assert.IsNotNull(sut);
            var actual = sut.Service;
            Assert.IsNotNull(actual);
            var actualType = actual.GetType();
            Assert.AreEqual(expectedType, actualType);
        }
    }
}