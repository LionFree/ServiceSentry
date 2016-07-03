// -----------------------------------------------------------------------
//  <copyright file="ImportedServicesListTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.PluginInfrastructure
{
    [TestFixture]
    internal class ImportedServicesListTests
    {
        [Test]
        public void Test_Defaults()
        {
            // Arrange
            var name = Path.GetRandomFileName();
            var canEx = Tests.Random<bool>();

            var service = new Mock<Service>();
            var services = new List<Service> {service.Object};

            var file = new Mock<ExternalFile>();
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            file.Setup(m => m.FullPath).Returns(path);
            var files = new List<ExternalFile> {file.Object};

            var sle = new Mock<ServiceListExtension>();
            sle.Setup(m => m.ExtensionName).Returns(name);
            sle.Setup(m => m.CanExecute).Returns(canEx);
            sle.Setup(m => m.OtherFiles).Returns(files);
            sle.Setup(m => m.Services).Returns(services);

            // Act
            var sut = new ImportedServicesList(Logger.Null, sle.Object);

            // Assert
            Assert.That(sut.ExtensionName == name);
            Assert.That(sut.CanExecute == canEx);
            Assert.That(sut.OtherFiles == files);
            Assert.That(sut.Services == services);
        }
    }
}