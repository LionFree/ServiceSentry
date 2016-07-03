// -----------------------------------------------------------------------
//  <copyright file="ExternalFileBehaviorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.Model.ExternalFiles;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Behavior
{
    [TestFixture]
    public class ExternalFileBehaviorTests
    {
        private FileInfoWrapper[] _array;
        private readonly Mock<FileSystem> _mFileSystem;

        public ExternalFileBehaviorTests()
        {
            _mFileSystem = new Mock<FileSystem>();
        }


        private ExternalFileBehavior GenerateBehavior(string folder, string filename, string searchPattern = null)
        {
            if (searchPattern == null) searchPattern = Tests.InjectAsteriskIntoString(filename);

            var expected = folder + "\\" + filename;

            _array = new FileInfoWrapper[1];
            var mFile = new Mock<FileInfoWrapper>();

            mFile.Setup(m => m.FullPath).Returns(expected);
            _array[0] = mFile.Object;

            _mFileSystem.Setup(m => m.DirectoryExists()).Returns(true);
            _mFileSystem.Setup(m => m.GetFiles(searchPattern)).Returns(_array);

            return ExternalFileBehavior.GetInstance(_mFileSystem.Object);
        }

        [Test]
        public void Test_GoodFilename_where_No_Files_Match()
        {
            // Arrange
            var path = Environment.CurrentDirectory + "\\" + Tests.InjectAsteriskIntoString(Path.GetRandomFileName());

            _mFileSystem.Setup(fs => fs.DirectoryExists()).Returns(true);
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act
            var result = behavior.GoodFileName(path);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_GoodFilename_where_Path_Includes_EnvironmentVariable()
        {
            // Arrange
            var specialFolder = Tests.RandomSpecialFolder();
            var variable = Tests.SpecialFolderToVariable(specialFolder);
            var filename = Path.GetRandomFileName();

            var path = variable + "\\" + filename;
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);


            // Act
            var result = behavior.GoodFileName(path);


            // Assert
            Trace.WriteLine("Should be a \"Good\" filename: \"" + path + "\"");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_GoodFilename_where_Path_String_IsEmpty()
        {
            // Arrange
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act:
            Trace.WriteLine("Testing: Path is string.Empty.");
            var result = behavior.GoodFileName(string.Empty);

            // Assert:
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_GoodFilename_where_Path_String_IsNull()
        {
            // Arrange
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act:
            Trace.WriteLine("Testing: Path is null.");
            var result1 = behavior.GoodFileName(null);

            // Assert:
            Assert.IsFalse(result1);
        }

        [Test]
        public void Test_GoodFilename_where_Path_is_a_valid_Filename()
        {
            // Arrange
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act:
            var result = behavior.GoodFileName(Path.GetRandomFileName());

            // Assert:
            Assert.IsTrue(result);
        }


        [Test]
        public void Test_ParseFileName_GoldenPath_NormalPath()
        {
            var specialFolder = Tests.RandomSpecialFolder();
            var expected = specialFolder + "\\" + Path.GetRandomFileName();

            var mBehavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            var testPath = expected;
            Trace.WriteLine("Testing: \"" + testPath + "\"");
            var actual = mBehavior.ParseFileName(testPath);
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ParseFileName_GoldenPath_SpecialFolder()
        {
            // Arrange
            var specialFolder = Tests.RandomSpecialFolder();
            var variable = Tests.SpecialFolderToVariable(specialFolder);
            var randomFileName = Path.GetRandomFileName();
            var expected = Environment.GetFolderPath(specialFolder) + "\\" + randomFileName;
            var testPath = (variable + "\\" + randomFileName);
            var mBehavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);


            // Act
            var actual = mBehavior.ParseFileName(testPath);


            // Assert
            Trace.WriteLine("Testing: \"" + testPath + "\"");
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ParseFileName_GoldenPath_WildCards()
        {
            // Arrange
            var specialFolder = Tests.RandomSpecialFolder();
            var folder = Environment.GetFolderPath(specialFolder);
            var randomFileName = Path.GetRandomFileName();
            var searchPattern = Tests.InjectAsteriskIntoString(randomFileName);
            var mBehavior = GenerateBehavior(folder, randomFileName, searchPattern);

            var expected = folder + "\\" + randomFileName;
            var testPath = folder + "\\" + searchPattern;

            // Act            
            var actual = mBehavior.ParseFileName(testPath);

            // Assert
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ParseFileName_GoldenPath_with_Asterisk_and_EnvironmentVariable()
        {
            // Arrange
            var specialFolder = Tests.RandomSpecialFolder();
            var randomFileName = Path.GetRandomFileName();
            var searchPattern = Tests.InjectAsteriskIntoString(randomFileName);
            var mBehavior = GenerateBehavior(Environment.GetFolderPath(specialFolder), randomFileName, searchPattern);

            var expected = Environment.GetFolderPath(specialFolder) + "\\" + randomFileName;
            var testPath = Tests.SpecialFolderToVariable(specialFolder) + "\\" + searchPattern;

            // Act            
            var actual = mBehavior.ParseFileName(testPath);

            // Assert
            Trace.WriteLine("Testing: \"" + testPath + "\"");
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ParseFileName_where_File_Doesnt_Exist()
        {
            // Make sure that an invalid filename (a path with an 
            // asterisk where no such file exists) returns an empty
            // string.
            var folder = Tests.SpecialFolderToVariable(Tests.RandomSpecialFolder());
            var randomFilename = Path.GetRandomFileName();
            var searchPattern = Tests.InjectAsteriskIntoString(randomFilename);
            var testPath = folder + "\\" + searchPattern;

            var array = new FileInfoWrapper[0];
            _mFileSystem.Setup(m => m.GetFiles(searchPattern)).Returns(array);

            var mBehavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act
            var actual = mBehavior.ParseFileName(testPath);

            // Assert
            Trace.WriteLine("Testing: \"" + testPath + "\"");
            Assert.That(actual == string.Empty);
        }

        [Test]
        public void Test_ParseFileName_where_Path_String_IsEmpty()
        {
            // Arrange
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act:
            Trace.WriteLine("Testing: Path is string.Empty.");
            var actual = behavior.ParseFileName(string.Empty);

            // Assert:
            Assert.That(actual == string.Empty);
        }

        [Test]
        public void Test_ParseFileName_where_Path_String_IsNull()
        {
            // Arrange
            var behavior = ExternalFileBehavior.GetInstance(_mFileSystem.Object);

            // Act:
            Trace.WriteLine("Testing: Path is null.");
            var actual = behavior.ParseFileName(null);

            // Assert:
            Assert.That(actual == string.Empty);
        }

        [Test]
        public void Test_ParseFileName_where_Path_includes_EnvironmentVariable()
        {
            // Arrange
            var specialFolder = Tests.RandomSpecialFolder();
            var randomFileName = Path.GetRandomFileName();
            var mBehavior = GenerateBehavior(Environment.GetFolderPath(specialFolder), randomFileName);

            var expected = Environment.GetFolderPath(specialFolder) + "\\" + randomFileName;
            var testPath = Tests.SpecialFolderToVariable(specialFolder) + "\\" + randomFileName;

            // Act            
            var actual = mBehavior.ParseFileName(testPath);

            // Assert
            Trace.WriteLine("Testing: \"" + testPath + "\"");
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ParseFileName_with_only_a_WildCard()
        {
            // Arrange
            var folder = Environment.CurrentDirectory;
            var filename = Path.GetRandomFileName();
            var expected = folder + "\\" + filename;
            var mBehavior = GenerateBehavior(folder, filename, "*");

            // Act
            Trace.WriteLine("Testing: \"*\"");
            var actual = mBehavior.ParseFileName("*");

            // Assert
            Assert.That(actual == expected);
        }
    }
}