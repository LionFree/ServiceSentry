// -----------------------------------------------------------------------
//  <copyright file="FileListTests.cs" company="Curtis Kaler">
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

namespace ServiceSentry.Model.UnitTests.Model.DataClasses.Serializable
{
    [TestFixture]
    internal class FileListTests
    {
        private readonly Mock<FileSystem> _mFileSystem;
        private readonly Mock<Logger> _logger;

        public FileListTests()
        {
            _mFileSystem = new Mock<FileSystem>();
            _logger = new Mock<Logger>();
        }

        private static FileList CreateTestObject(ExternalFile item)
        {
            var output = FileList.Default;
            output.Items.Add(item);
            return output;
        }

        [Test]
        public void Test_FileList_CollectionChanged()
        {
            // Arrange
            var wasCalled = false;
            var sut = FileList.Default;

            _mFileSystem.Setup(m => m.FileExists()).Returns(true);

            var file = new Mock<ExternalFile>();
            //var file = ExternalFile.GetInstance(_mFileSystem.Object);
            sut.Items.CollectionChanged += (o, e) => { wasCalled = true; };


            // Act
            sut.Items.Add(file.Object);


            // Assert
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_FileList_Equals()
        {
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            var name = Tests.Random<string>();

            var file1 = ExternalFile.GetInstance(_logger.Object, path, name);
            var file2 = ExternalFile.GetInstance(_logger.Object, path, name);
            var file3 = ExternalFile.GetInstance(_logger.Object, path, name);
            var file4 = ExternalFile.GetInstance(_logger.Object, path, name);

            var item1 = CreateTestObject(file1);
            var item2 = CreateTestObject(file1);
            var item3 = CreateTestObject(file1);
            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            item1.Items.Add(file2);
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.Items.Add(file2);
            item3.Items.Add(file2);
            item2.Items.Add(file3);
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            item1.Items.Add(file3);
            item3.Items.Add(file3);
            item3.Items.Add(file4);
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item1.Items.Add(file4);
            item2.Items.Add(file4);
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_FileList_GetHashCode()
        {
            // Arrange
            var file = new Mock<ExternalFile>();
            var item1 = CreateTestObject(file.Object);

            // Act
            var item2 = item1;

            // Assert
            // If two distinct objects compare as equal, their hashcodes must be equal.
            Assert.That(item1 == item2);
            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_FileList_ItemPropertyChanged()
        {
            // Arrange
            var wasCalled = false;

            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            var name = Tests.Random<string>();
            var file = ExternalFile.GetInstance(_logger.Object, path, name);

            var sut = FileList.Default;
            
            var integer1 = (new Random()).Next(10000);
            var integer2 = Tests.GetNewInteger(integer1);

            var oldValue = path + integer1;
            var newValue = path + integer2;

            sut.Items.Add(file);
            sut.Items[0].FullPath = oldValue;

            sut.Items[0].PropertyChanged += (s, e) => { wasCalled = true; };

            // Act / Assert
            Assert.That(oldValue != newValue);

            sut.Items[0].FullPath = oldValue;
            Assert.IsFalse(wasCalled);

            sut.Items[0].FullPath = newValue;
            Assert.IsTrue(wasCalled);
        }
    }
}