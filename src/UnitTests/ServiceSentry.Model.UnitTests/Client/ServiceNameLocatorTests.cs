// -----------------------------------------------------------------------
//  <copyright file="ServiceNameLocatorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Client
{
    [TestFixture]
    internal class ServiceNameLocatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _reader = new Mock<LocalServiceFinder>();
            _service = new Mock<Service>();
        }

        private Mock<LocalServiceFinder> _reader;
        private Mock<Service> _service;

        private static SubscriptionPacket PickOneService(IReadOnlyList<string> possibleServices)
        {
            var chosenIndex = Tests.Randomizer.Next(0, possibleServices.Count);
            return CreateServiceData(possibleServices[chosenIndex]);
        }

        private static SubscriptionPacket CreateServiceData(string name)
        {
            return new SubscriptionPacket{ServiceName = name, MachineName = Environment.MachineName, DisplayName = name};
        }

        [Test]
        public void Test_GetServiceNameFromList()
        {
            var possibleServices = Tests.RandomList<string>(1, 100);
            var expectedService = PickOneService(possibleServices);
            var expected = expectedService.ServiceName;
            _reader.Setup(m => m.GetServices()).Returns(new[] { expectedService });
            var sut = ServiceNameLocator.GetInstance(_reader.Object);

            // Act
            var actual = sut.GetServiceNameFromList(possibleServices);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceNameLocator_IsValidService()
        {
            var expected = Tests.Random<bool>();
            _service.Setup(m => m.IsInstalled).Returns(expected);
            var sut = ServiceNameLocator.GetInstance();

            var actual = sut.IsValidService(_service.Object);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceNameLocator_IsValidService_nullService()
        {
            var sut = ServiceNameLocator.GetInstance();

            var actual = sut.IsValidService(null);

            Assert.IsFalse(actual);
        }
    }
}