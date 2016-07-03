// -----------------------------------------------------------------------
//  <copyright file="ExternalFileTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.ExternalFiles;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses.Serializable
{
    [TestFixture]
    internal class ExternalFileTests
    {
        [SetUp]
        public void SetUp()
        {
            _mFileSystem = new Mock<FileSystem>();
            _mFileBehavior = new Mock<ExternalFileBehavior>();
            _logger = new Mock<Logger>();
        }

        private Mock<FileSystem> _mFileSystem;
        private Mock<ExternalFileBehavior> _mFileBehavior;
        private Mock<Logger> _logger;


        private void Validate_GetStringSize(long minSize, string suffix)
        {
            // Arrange
            var size = Tests.Random<long>(minSize, minSize*1024);
            _mFileSystem.Setup(m => m.FileExists()).Returns(true);
            _mFileSystem.Setup(m => m.FileLength()).Returns(size);
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);

            var length = (double) size;
            var expVal = length/minSize;
            var expected = string.Format("{0:0.##} {1}", expVal, suffix);

            // Act
            var actual = sut.FileSize;

            // Assert
            Trace.WriteLine("Given size (" + size + "),\r\nExpecting: " + expected + "\r\nActual: " + actual + "\r\n");
            Assert.AreEqual(expected, actual);
        }


        private ExternalFile CreateTestObject(string logName, string fullPath, bool showParentDirectory)
        {
            var output = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            output.CommonName = logName;
            output.FullPath = fullPath;
            output.ShowParentDirectory = showParentDirectory;
            return output;
        }


        [Test]
        public void Test_ExternalFile_Equals()
        {
            // Arrange
            var name = Path.GetRandomFileName();
            var path = Environment.CurrentDirectory + "\\" + name;
            var spd = Tests.Random<bool>();

            var item1 = CreateTestObject(name, path, spd);
            var item2 = CreateTestObject(name, path, spd);
            var item3 = CreateTestObject(name, path, spd);


            // Act / Assert
            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            item2.FullPath = path.Substring(path.Length/2);
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item1.FullPath = item2.FullPath;
            item3.FullPath = item2.FullPath;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_ExternalFile_Exists_EmptyPath()
        {
            // Arrange
            _mFileSystem.Setup(fs => fs.FileExists()).Returns(false);
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);

            // Act
            sut.FullPath = string.Empty;

            // Assert
            Assert.IsFalse(sut.Exists);
        }

        [Test]
        public void Test_ExternalFile_Exists_GoodPath()
        {
            // Arrange
            var path = Tests.RandomFilePath();
            _mFileSystem.Setup(fs => fs.FileExists()).Returns(true);

            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);

            // Act
            sut.FullPath = path;

            // Assert
            Assert.That(sut.Exists);
        }

        [Test]
        public void Test_ExternalFile_Exists_NullPath()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);

            // Act
            sut.FullPath = null;

            // Assert
            Assert.IsFalse(sut.Exists);
        }

        [Test]
        public void Test_ExternalFile_GetHashCode()
        {
            // Arrange
            var logName = Tests.Random<string>();
            var fullPath = Tests.RandomFilePath();

            var showParentDirectory = Tests.Random<bool>();

            // Act
            var item1 = CreateTestObject(logName, fullPath, showParentDirectory);
            var item2 = CreateTestObject(logName, fullPath, showParentDirectory);

            // Assert
            Assert.That(item1 == item2);

            // If two distinct objects compare as equal, their hashcodes must be equal.
            Tests.TestGetHashCode(item1, item2);
        }


        [Test]
        public void Test_ExternalFile_Get_LastModified()
        {
            // Arrange
            var path = Path.GetRandomFileName();
            var expectedValue = DateTime.Now;
            var expected = expectedValue.ToString(CultureInfo.CurrentCulture);

            _mFileSystem.Setup(m => m.FileExists()).Returns(true);
            _mFileSystem.Setup(m => m.LastWriteTime()).Returns(expectedValue);

            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);


            // Act
            sut.FullPath = path;
            var actual = sut.LastModified;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ExternalFile_Get_ParsedName()
        {
            // Arrange
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            _mFileSystem.Setup(m => m.FileExists()).Returns(true);
            var expected = Tests.Random<string>();
            _mFileBehavior.Setup(m => m.ParseFileName(It.IsAny<string>())).Returns(expected);
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            sut.FullPath = path;
            
            // Act
            var actual = sut.ParsedName;

            // Assert
            Assert.AreEqual(expected, actual,
                            "Expected: {0}{1}But was:{2}",
                            expected, Environment.NewLine, actual);
        }

        [Test]
        public void Test_ExternalFile_Get_ShortParsedName()
        {
            // Arrange
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            var expected = Path.GetFileName(path);
            _mFileBehavior.Setup(m => m.ParseFileName(It.IsAny<string>())).Returns(expected);
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            sut.FullPath = path;

            // Act
            var actual = sut.ShortParsedName;            

            // Assert
            Trace.WriteLine("Expected: " + expected);
            Assert.AreEqual(expected, actual,
                            "Expected: {0}{1}But was: {2}",
                            expected, Environment.NewLine, actual);
        }

        [Test]
        public void Test_ExternalFile_OnFileChanged()
        {
            // Arrange
            var wasCalled = false;
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            sut.FileChanged += (o, e) => { wasCalled = true; };


            // Act
            sut.OnFileChanged(new FileChangedEventArgs());


            //Assert
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_ExternalFile_PropertyChanged_Directory()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            var oldValue = sut.Directory;
            var newValue = Path.GetRandomFileName();

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.Directory).When(s => s.Directory = oldValue);
            sut.ShouldNotifyOn(s => s.Directory).When(s => s.Directory = newValue);
            Assert.That(sut.Directory == newValue);
        }

        [Test]
        public void Test_ExternalFile_PropertyChanged_DisplayName()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            var oldValue = sut.DisplayName;
            var newValue = Path.GetRandomFileName();

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.DisplayName).When(s => s.DisplayName = oldValue);
            sut.ShouldNotifyOn(s => s.DisplayName).When(s => s.DisplayName = newValue);
            Assert.That(sut.DisplayName == newValue);
        }

        [Test]
        public void Test_ExternalFile_PropertyChanged_FileName()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            var oldValue = sut.FileName;
            var newValue = Path.GetRandomFileName();

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.FileName).When(s => s.FileName = oldValue);
            sut.ShouldNotifyOn(s => s.FileName).When(s => s.FileName = newValue);
            Assert.That(sut.FileName == newValue);
        }

        [Test]
        public void Test_ExternalFile_PropertyChanged_FullPath()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            var oldValue = sut.FullPath;
            var newValue = Path.GetRandomFileName();

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.FullPath).When(s => s.FullPath = oldValue);
            sut.ShouldNotifyOn(s => s.FullPath).When(s => s.FullPath = newValue);
            Assert.That(sut.FullPath == newValue);
        }

        [Test]
        public void Test_ExternalFile_PropertyChanged_CommonName()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            var oldValue = sut.CommonName;
            var newValue = Path.GetRandomFileName();

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.CommonName).When(s => s.CommonName = oldValue);
            sut.ShouldNotifyOn(s => s.CommonName).When(s => s.CommonName = newValue);
            Assert.That(sut.CommonName == newValue);
        }

        [Test]
        public void Test_ExternalFile_PropertyChanged_ShowParentDirectory()
        {
            // Arrange
            var sut = ExternalFile.GetInstance(_logger.Object, _mFileSystem.Object, _mFileBehavior.Object, "", "", 0);
            var oldValue = sut.ShowParentDirectory;
            var newValue = !oldValue;

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.ShowParentDirectory).When(s => s.ShowParentDirectory = oldValue);
            sut.ShouldNotifyOn(s => s.ShowParentDirectory).When(s => s.ShowParentDirectory = newValue);
            Assert.That(sut.ShowParentDirectory == newValue);
        }

        [Test]
        public void Test_ExternalFile_GetStringSize()
        {
            Validate_GetStringSize(1, "B");
            Validate_GetStringSize(1024, "KB");
            Validate_GetStringSize(1024*1024, "MB");
            Validate_GetStringSize(1024*1024*1024, "GB");
        }
    }
}