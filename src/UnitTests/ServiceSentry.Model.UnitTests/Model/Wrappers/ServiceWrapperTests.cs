// -----------------------------------------------------------------------
//  <copyright file="ServiceWrapperTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using NUnit.Framework;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Wrappers
{
    internal class ServiceWrapperTests
    {
        private const string RealService = "W32Time";

        [Test]
        public void Test_GetInstance_Runtime()
        {
            // Act
            var sut = ServiceWrapper.GetInstance(RealService);

            // Assert
            Assert.IsNotNull(sut);
            Assert.That(sut is LocalServiceController);
        }

        [Test]
        public void Test_GetInstance_Mockable()
        {
            // Act
            var sut = ServiceWrapper.GetInstance(RealService, LocalServiceFinder.Default);

            // Assert
            Assert.IsNotNull(sut);
            Assert.That(sut is LocalServiceController);
        }

        [Test]
        public void Test_ServiceWrapper_Dispose()
        {
            var sut = ServiceWrapper.GetInstance(RealService, LocalServiceFinder.Default);

            sut.Dispose();

            Assert.That(sut.IsDisposed);
        }
    }
}