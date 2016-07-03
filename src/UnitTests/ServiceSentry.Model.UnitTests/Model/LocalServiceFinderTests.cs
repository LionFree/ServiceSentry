// -----------------------------------------------------------------------
//  <copyright file="ServiceReaderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Linq;
using System.ServiceProcess;
using NUnit.Framework;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model
{
    [TestFixture]
    internal class LocalServiceFinderTests
    {
        private const string RealService = "W32Time";
        private readonly string _fakeService = Tests.Random<string>();

        [Test]
        public void Test_LocalServiceFinder_GetServices()
        {
            var realServices = ServiceController.GetServices();
            var testServices = LocalServiceFinder.Default.GetServices();

            var expectedList = realServices.Select(controller => controller.ServiceName).ToArray();
            var actualList = testServices.Select(serviceData => serviceData.ServiceName).ToArray();

            Assert.AreEqual(expectedList, actualList);
        }

        [Test]
        public void Test_LocalServiceFinder_IsInstalled_Yes()
        {
            // Arrange
            var sut = LocalServiceFinder.Default;

            // Act
            var actual = sut.IsInstalled(RealService);


            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void Test_LocalServiceFinder_IsInstalled_No()
        {
            // Arrange
            var sut = LocalServiceFinder.Default;

            // Act
            var actual = sut.IsInstalled(_fakeService);
            
            // Assert
            Assert.IsFalse(actual);
        }
    }
}