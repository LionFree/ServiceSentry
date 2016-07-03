// -----------------------------------------------------------------------
//  <copyright file="LogArchiverTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.ExternalFiles;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.LogArchival
{
    [TestFixture]
    internal class LogArchiverTests
    {
        [SetUp]
        public void SetUp()
        {
            _zipper = new Mock<Zipper>();
            _fileInfo = new Mock<FileInfoWrapper>();
            _fileInfo.Setup(m => m.Exists).Returns(true);

            _details = LoggingDetails.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
        }

        private Mock<Zipper> _zipper;
        private Mock<FileInfoWrapper> _fileInfo;
        private LoggingDetails _details;

        [Test]
        public void Test_LogArchiver_ArchiveLogs()
        {
            // Arrange
            var path = "";
            IEnumerable<FileInfoWrapper> files = new List<FileInfoWrapper>();

            var testPath = Tests.RandomFilePath();
            _details.DisplayName = testPath;
            _details.ArchiveLogs = true;

            var expectedPath = Path.ChangeExtension(testPath, ".zip");

            _fileInfo.Setup(m => m.Exists).Returns(true);
            var expectedFiles = new List<FileInfoWrapper> {_fileInfo.Object};


            _zipper.Setup(m => m.ZipFiles(
                It.IsAny<string>(), It.IsAny<IEnumerable<FileInfoWrapper>>()
                                   )).Callback<string, IEnumerable<FileInfoWrapper>>((a, b) =>
                                       {
                                           path = a;
                                           files = b;
                                       });

            var sut = LogArchiver.GetInstance(_zipper.Object, Logger.Null);

            // Act
            sut.ArchiveAndClearLogs(expectedFiles, _details);

            // Assert
            Assert.AreEqual(expectedFiles, files);
            Assert.AreEqual(expectedPath, path);
        }

        [Test]
        public void Test_LogArchiver_ClearLogs()
        {
            // Arrange
            var wasCalled = false;

            _details.ArchiveLogs = true;
            _details.ClearLogs = true;

            _fileInfo.Setup(m => m.Delete()).Callback(() => { wasCalled = true; });
            var expectedFiles = new List<FileInfoWrapper> {_fileInfo.Object};

            _zipper.Setup(m => m.ZipFiles(
                It.IsAny<string>(), It.IsAny<IEnumerable<FileInfoWrapper>>()
                                   )).Returns(true);

            var sut = LogArchiver.GetInstance(_zipper.Object, Logger.Null);

            // Act
            sut.ArchiveAndClearLogs(expectedFiles, _details);

            // Assert
            Assert.AreEqual(true, wasCalled);
        }

        [Test]
        public void Test_LogArchiver_Logs_not_cleared_when_Archival_fails()
        {
            // Arrange
            var wasCalled = false;

            _details.ArchiveLogs = true;
            _details.ClearLogs = true;

            _fileInfo.Setup(m => m.Delete()).Callback(() => { wasCalled = true; });
            var expectedFiles = new List<FileInfoWrapper> {_fileInfo.Object};

            _zipper.Setup(m => m.ZipFiles(
                It.IsAny<string>(), It.IsAny<IEnumerable<FileInfoWrapper>>()
                                   )).Returns(false);

            var sut = LogArchiver.GetInstance(_zipper.Object, Logger.Null);

            // Act
            sut.ArchiveAndClearLogs(expectedFiles, _details);

            // Assert
            Assert.AreEqual(false, wasCalled);
        }
    }
}