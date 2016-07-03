// -----------------------------------------------------------------------
//  <copyright file="TrackingObjectTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses
{
    [TestFixture]
    internal class TrackingObjectTests
    {
        [Test]
        public void Test_TrackingObject_ServiceName()
        {
            // Arrange
            var expected = Tests.Random<string>();
            
            // Act
            var sut = new TrackingObject {ServiceName = expected};

            // Assert
            Assert.AreEqual(expected, sut.ServiceName,
                            "ServiceName: expected '{0}', was '{1}'.",
                            expected, sut.ServiceName);

            Assert.AreEqual(expected, sut.Packet.ServiceName,
                            "Packet.ServiceName: expected '{0}', was '{1}'.",
                            expected, sut.Packet.ServiceName);
        }

        [Test]
        public void Test_TrackingObject_NotifyOnUnexpectedStop()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            
            // Act
            var sut = new TrackingObject { NotifyOnUnexpectedStop = expected };

            // Assert
            Assert.AreEqual(expected, sut.NotifyOnUnexpectedStop,
                            "NotifyOnUnexpectedStop: expected '{0}', was '{1}'.",
                            expected, sut.NotifyOnUnexpectedStop);

            Assert.AreEqual(expected, sut.Packet.NotifyOnUnexpectedStop,
                            "Packet.NotifyOnUnexpectedStop: expected '{0}', was '{1}'.",
                            expected, sut.Packet.NotifyOnUnexpectedStop);
        }
    }
}