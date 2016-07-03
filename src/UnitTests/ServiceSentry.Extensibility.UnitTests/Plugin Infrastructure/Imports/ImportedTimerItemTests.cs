// -----------------------------------------------------------------------
//  <copyright file="ImportedTimerItemTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using System.Windows.Threading;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Imports
{
    [TestFixture]
    internal class ImportedTimerItemTests
    {
        [Test]
        public void Test_Defaults()
        {
            // Arrange
            var name = Path.GetRandomFileName();
            var canEx = Tests.Random<bool>();
            var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(new Random((int) DateTime.Now.Ticks).Next(10000))
                };

            var param = new Mock<TimerExtension>();
            param.Setup(m => m.ExtensionName).Returns(name);
            param.Setup(m => m.CanExecute).Returns(canEx);
            param.Setup(m => m.Timer(It.IsAny<Logger>())).Returns(timer);

            var logger = new Mock<Logger>();

            // Act
            var sut = new ImportedTimerItem(logger.Object, param.Object);

            // Assert
            Assert.That(sut.ExtensionName == name);
            Assert.That(sut.CanExecute == canEx);
            Assert.That(sut.Timer == timer);
        }
    }
}