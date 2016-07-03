// -----------------------------------------------------------------------
//  <copyright file="EndpointBuilderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Globalization;
using NUnit.Framework;
using ServiceSentry.Common.Communication;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.Communication
{
    [TestFixture]
    internal class EndpointBuilderTests
    {
        [Test]
        public void Test_EndpointBuilder_GetEndpoint()
        {
            // Arrange
            var server = Tests.Random<string>();
            var suffix = Tests.Random<string>();
            var expected = string.Format(CultureInfo.InvariantCulture, "net.tcp://{0}/{1}", server, suffix);
            var sut = EndpointBuilder.GetInstance(ServiceHostType.NetTcp);

            // Act
            var actual = sut.GetEndpoint(server, suffix, -1);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_EndpointBuilder_GetEndpointBase()
        {
            // Arrange
            var server = Tests.Random<string>();
            var expected = string.Format(CultureInfo.InvariantCulture, "net.tcp://{0}", server);
            var sut = EndpointBuilder.GetInstance(ServiceHostType.NetTcp);

            // Act
            var actual = sut.GetEndpointBase(server, -1);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}