// -----------------------------------------------------------------------
//  <copyright file="FileInfoWrapperTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Common.UnitTests.File_System
{
    [TestFixture, Explicit("Ignored because it touches the real file system.")]
    internal class FileInfoWrapperTests
    {
        private string _folder;
        private string _fileName;
        private string _fullPath;


        [SetUp]
        public void SetUp()
        {
            _folder = Environment.CurrentDirectory;
            _fileName = Path.GetRandomFileName();
            _fullPath = _folder + "\\" + _fileName;
        }


        [Test]
        public void Test_FileInfoWrapper_Delete()
        {
            // Arrange
            File.Create(_fullPath).Close();
            Assert.IsTrue((new FileInfo(_fullPath)).Exists);
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Act
            sut.Delete();

            // Assert
            Assert.IsFalse((new FileInfo(_fullPath)).Exists);
        }

        [Test]
        public void Test_FileInfoWrapper_Directory()
        {
            // Arrange
            File.Create(_fullPath).Close();
            Assert.IsTrue((new FileInfo(_fullPath)).Exists);

            var directoryInfo = DirectoryInfoWrapper.GetInstance(_folder);

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.That(sut.Directory == directoryInfo);
            File.Delete(_fullPath);
        }

        [Test]
        public void Test_FileInfoWrapper_Exists_where_file_Exists()
        {
            // Arrange
            File.Create(_fullPath).Close();
            Assert.IsTrue((new FileInfo(_fullPath)).Exists);

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.IsTrue(sut.Exists);
            File.Delete(_fullPath);
        }

        [Test]
        public void Test_FileInfoWrapper_Exists_where_file_does_not_Exist()
        {
            // Arrange
            Assert.IsFalse((new FileInfo(_fullPath)).Exists);

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.IsFalse(sut.Exists);
        }


        [Test]
        public void Test_FileInfoWrapper_FullPath()
        {
            // Arrange
            File.Create(_fullPath).Close();
            Assert.IsTrue(new FileInfo(_fullPath).Exists);

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.AreEqual(sut.FullPath, _fullPath);
            File.Delete(_fullPath);
        }

        [Test]
        public void Test_FileInfoWrapper_LastWriteTime()
        {
            // Arrange
            File.WriteAllText(_fullPath, _fullPath);

            var realFI = new FileInfo(_fullPath);
            Assert.IsTrue(realFI.Exists);
            var expected = realFI.LastWriteTime;

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.That(sut.LastWriteTime == expected);
            File.Delete(_fullPath);
        }

        [Test]
        public void Test_FileInfoWrapper_Length()
        {
            // Arrange
            File.WriteAllText(_fullPath, _fullPath);

            var realFI = new FileInfo(_fullPath);
            Assert.IsTrue(realFI.Exists);
            var expected = realFI.Length;

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.That(sut.Length == expected);
            File.Delete(_fullPath);
        }

        [Test]
        public void Test_FileInfoWrapper_Name()
        {
            // Arrange
            File.Create(_fullPath).Close();
            Assert.IsTrue((new FileInfo(_fullPath)).Exists);

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.AreEqual(sut.Name, _fileName);
            File.Delete(_fullPath);
        }
    
        [Test]
        public void Test_FileInfoWrapper_ProductVersion()
        {
            // Arrange
            File.Create(_fullPath).Close();
            Assert.IsTrue((new FileInfo(_fullPath)).Exists);
            var expected = FileVersionInfo.GetVersionInfo(_fullPath).ProductVersion;

            // Act
            var sut = FileInfoWrapper.GetInstance(_fullPath);

            // Assert
            Assert.AreEqual(sut.ProductVersion, expected);
            File.Delete(_fullPath);

        }
    
    }
}