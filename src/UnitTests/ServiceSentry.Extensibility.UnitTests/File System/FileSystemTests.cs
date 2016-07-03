// -----------------------------------------------------------------------
//  <copyright file="FileSystemTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.File_System
{
    [TestFixture]
    internal class FileSystemTests
    {
        [SetUp]
        public void SetUp()
        {
            _directoryInfo = new Mock<DirectoryInfoWrapper>();
            _fileInfo = new Mock<FileInfoWrapper>();
            _fileSystem = FileSystem.GetUnloggedInstance(null, _fileInfo.Object, _directoryInfo.Object,
                                                         FileSystemFactory.GetFactory());
        }

        private Mock<DirectoryInfoWrapper> _directoryInfo;
        private Mock<FileInfoWrapper> _fileInfo;
        private FileSystem _fileSystem;


        [Test]
        public void Test_FileSystem_DeleteFolder_with_no_path()
        {
            // Arrange
            var wasCalled = false;

            var path = Tests.RandomFilePath();
            _directoryInfo.Setup(m => m.Delete(It.IsAny<bool>())).Callback(() => { wasCalled = true; });
            
            _fileSystem.SetFocus(path, _fileInfo.Object, _directoryInfo.Object);

            // Act
            _fileSystem.DeleteFolder();

            // Assert
            Assert.That(_fileSystem.DirectoryInfo != null);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_FileSystem_DirectoryExists()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _directoryInfo.Setup(m => m.Exists).Returns(expected);
            _fileSystem.DirectoryInfo = _directoryInfo.Object;

            // Act
            var actual = _fileSystem.DirectoryExists();

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void Test_FileSystem_DirectoryName()
        {
            // Arrange
            var expected = Tests.Random<string>();
            _directoryInfo.Setup(m => m.Name).Returns(expected);
            _fileSystem.DirectoryInfo = _directoryInfo.Object;

            // Act
            var actual = _fileSystem.DirectoryName();

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void Test_FileSystem_FileExists()
        {
            // Arrange
            var expected = Tests.Random<bool>();
            _fileInfo.Setup(m => m.Exists).Returns(expected);
            _fileSystem.FileInfoWrapper = _fileInfo.Object;

            // Act
            var actual = _fileSystem.FileExists();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_FileSystem_FileLength()
        {
            // Arrange
            var expected = Tests.Random<long>();
            _fileInfo.Setup(m => m.Length).Returns(expected);
            _fileSystem.FileInfoWrapper = _fileInfo.Object;

            // Act
            var actual = _fileSystem.FileLength();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_FileSystem_GetDirectories()
        {
            // Arrange
            var expected = new[] {(new Mock<DirectoryInfoWrapper>()).Object};
            _directoryInfo.Setup(m => m.GetDirectories(It.IsAny<string>())).Returns(expected);
            _fileSystem.DirectoryInfo = _directoryInfo.Object;

            // Act
            var actual = _fileSystem.GetDirectories(Path.GetRandomFileName());

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_FileSystem_GetFiles()
        {
            // Arrange
            var expected = new[] {(new Mock<FileInfoWrapper>()).Object};
            _directoryInfo.Setup(m => m.GetFiles(It.IsAny<string>())).Returns(expected);
            _fileSystem.DirectoryInfo = _directoryInfo.Object;

            // Act
            var actual = _fileSystem.GetFiles(Tests.Random<string>());

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_FileSystem_LastWriteTime()
        {
            // Arrange
            var expected = Tests.Random<DateTime>();
            _fileInfo.Setup(m => m.LastWriteTime).Returns(expected);
            _fileSystem.FileInfoWrapper = _fileInfo.Object;

            // Act
            var actual = _fileSystem.LastWriteTime();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_FileSystem_OpenPath_File_DoesNotExist()
        {
            // Arrange
            _fileInfo.Setup(m => m.Exists).Returns(false);
            _fileSystem.FileInfoWrapper = _fileInfo.Object;

            // Act
            var actual = _fileSystem.OpenPath(Tests.RandomFilePath());

            // Assert
            Assert.AreEqual("File does not exist.", actual);
        }

        [Test, Explicit("Touches the file system.")]
        public void Test_FileSystem_OpenPath_File_GoldenPath()
        {
            // Arrange

            // Act
            var actual = _fileSystem.OpenPath("C:\\Windows\\win.ini");

            // Assert
            Assert.AreEqual("", actual);
        }

        [Test]
        public void Test_FileSystem_OpenPath_Folder_DoesNotExist()
        {
            // Arrange
            _directoryInfo.Setup(m => m.Exists).Returns(false);
            _fileSystem.DirectoryInfo = _directoryInfo.Object;

            // Act
            var actual = _fileSystem.OpenPath(Tests.RandomFilePath(), true);

            // Assert
            Assert.AreEqual("Folder does not exist.", actual);
        }

        [Test]
        public void Test_FileSystem_OpenPath_with_invalid_URI()
        {
            Assert.That(_fileSystem.OpenPath(" ") == "URI is invalid.");
        }

        [Test]
        public void Test_FileSystem_OpenPath_with_null_or_empty_uriString()
        {
            Assert.Throws<ArgumentNullException>(() => _fileSystem.OpenPath(null));
            Assert.Throws<ArgumentNullException>(() => _fileSystem.OpenPath(string.Empty));
        }

        [Test]
        public void Test_FileSystem_ProductVersion()
        {
            // Arrange
            var expected = Path.GetRandomFileName();
            _fileInfo.Setup(m => m.ProductVersion).Returns(expected);
            var sut = FileSystem.GetInstance(Logger.Null);
            sut.FileInfoWrapper = _fileInfo.Object;

            // Act
            var actual = sut.ProductVersion;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_FileSystem_ShiftFocus()
        {
            // Arrange
            var sut = FileSystem.GetUnloggedInstance();
            var oldValue = sut.BasePath;
            var expected = Tests.RandomFilePath();

            // Act
            sut.SetFocus(expected);

            // Assert
            Assert.AreNotEqual(expected, oldValue);
            Assert.AreEqual(expected, sut.BasePath);
            Assert.IsNotNull(sut.DirectoryInfo);
            Assert.IsNotNull(sut.FileInfoWrapper);
            Assert.AreEqual(expected, sut.DirectoryInfo.FullPath);
            Assert.AreEqual(expected, sut.FileInfoWrapper.FullPath);
        }
    }
}