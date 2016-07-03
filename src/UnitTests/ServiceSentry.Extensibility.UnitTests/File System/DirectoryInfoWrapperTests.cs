// -----------------------------------------------------------------------
//  <copyright file="DirectoryInfoTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.File_System
{
    [TestFixture, Explicit("Ignored because it touches the real file system.")]
    internal class DirectoryInfoTests
    {
        [Test]
        public void Test_DirectoryInfo_Equals()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var differentFolder = Environment.CurrentDirectory;

            var item1 = DirectoryInfoWrapper.GetInstance(folder);
            var item2 = DirectoryInfoWrapper.GetInstance(folder);
            var item3 = DirectoryInfoWrapper.GetInstance(folder);


            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            item1 = DirectoryInfoWrapper.GetInstance(differentFolder);
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2 = DirectoryInfoWrapper.GetInstance(differentFolder);
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item1));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item3 = DirectoryInfoWrapper.GetInstance(differentFolder);
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_DirectoryInfo_Exists()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;

            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);

            // Assert
            Assert.IsTrue(sut.Exists);


            // Arrange
            folder = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();

            // Act
            sut = DirectoryInfoWrapper.GetInstance(folder);


            // Assert
            Assert.IsFalse(sut.Exists);
        }

        [Test]
        public void Test_DirectoryInfo_FullPath()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;
            var expected = new DirectoryInfo(folder).FullName;

            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);

            // Assert
            Assert.That(sut.FullPath == expected);
        }

        [Test]
        public void Test_DirectoryInfo_GetDirectories()
        {
            // Arrange
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var expected = new DirectoryInfo(folder).GetDirectories();

            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);
            var actual = sut.GetDirectories();


            // Assert
            Assert.That(actual.Length == expected.Length);
            for (var i = 0; i < actual.Length; i++)
            {
                Assert.That(actual[i].FullPath == expected[i].FullName);
            }
        }

        [Test]
        public void Test_DirectoryInfo_GetFiles()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;
            var expected = new DirectoryInfo(folder).GetFiles();

            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);
            var actual = sut.GetFiles();


            // Assert
            Assert.That(actual.Length == expected.Length);
            for (var i = 0; i < actual.Length; i++)
            {
                Assert.That(actual[i].FullPath == expected[i].FullName);
                Assert.That(actual[i].Length == expected[i].Length);
                Assert.That(actual[i].Exists == expected[i].Exists);
                Assert.That(actual[i].LastWriteTime == expected[i].LastWriteTime);
            }
        }

        [Test]
        public void Test_DirectoryInfo_GetHashCode()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            // If two distinct objects compare as equal, their hashcodes must be equal.
            var item1 = DirectoryInfoWrapper.GetInstance(folder);
            var item2 = DirectoryInfoWrapper.GetInstance(folder);
            Assert.That(item1 == item2);

            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_DirectoryInfo_Name()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;
            var expected = new DirectoryInfo(folder).Name;

            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);

            // Assert
            Assert.That(sut.Name == expected);
        }

        [Test]
        public void Test_DirectoryInfo_Parent()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;
            var parent = Path.GetDirectoryName(folder);
            var expected = DirectoryInfoWrapper.GetInstance(parent);


            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);


            // Assert
            Assert.That(sut.Parent == expected);
        }

        [Test]
        public void Test_DirectoryInfo_Root()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;
            var root = Path.GetPathRoot(folder);
            var expected = DirectoryInfoWrapper.GetInstance(root);


            // Act
            var sut = DirectoryInfoWrapper.GetInstance(folder);


            // Assert
            Assert.That(sut.Root == expected);
        }


        [Test]
        public void Test_DirectoryInfo_Delete()
        {
            // Arrange
            var newFolder = Path.GetRandomFileName();
            newFolder = newFolder.Substring(0, 8);
            var folder = Environment.CurrentDirectory + "\\" + newFolder;
            
            Directory.CreateDirectory(folder);
            Assert.IsTrue((new DirectoryInfo(folder)).Exists);
            var sut = DirectoryInfoWrapper.GetInstance(folder);

            // Act
            sut.Delete();

            // Assert
            Assert.IsFalse((new DirectoryInfo(folder)).Exists);
        }

    }
}