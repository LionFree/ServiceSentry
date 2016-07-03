// -----------------------------------------------------------------------
//  <copyright file="ImportedFileListTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.PluginInfrastructure
{
    [TestFixture]
    internal class ImportedFileListTests
    {
        [Test]
        public void Test_Defaults()
        {
            // Arrange
            var name = Path.GetRandomFileName();
            var canEx = Tests.Random<bool>();

            var file = new Mock<ExternalFile>();
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            file.Setup(m => m.FullPath).Returns(path);
            var files = new List<ExternalFile> {file.Object};

            var fle = new Mock<FileListExtension>();
            fle.Setup(m => m.ExtensionName).Returns(name);
            fle.Setup(m => m.CanExecute).Returns(canEx);
            fle.Setup(m => m.Files).Returns(files);


            // Act
            var sut = new ImportedFileList(fle.Object);

            // Assert
            Assert.That(sut.ExtensionName == name);
            Assert.That(sut.CanExecute == canEx);
            Assert.That(sut.Items == files);
        }
    }
}