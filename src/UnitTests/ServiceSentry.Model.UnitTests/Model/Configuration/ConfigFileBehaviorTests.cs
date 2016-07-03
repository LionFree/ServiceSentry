// -----------------------------------------------------------------------
//  <copyright file="ServiceFileBehaviorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model
{
    [TestFixture]
    internal class ConfigFileBehaviorTests
    {
        private readonly Mock<ConfigFileSerializer> _serializer;
        private readonly Mock<Logger> _logger;

        public ConfigFileBehaviorTests()
        {
            _serializer = new Mock<ConfigFileSerializer>();
            _logger = new Mock<Logger>();
        }

        private void Test_WriteFile(bool expected, ConfigFile file, string path = null)
        {
            if (path == null) path = file.FilePath;
            _serializer.Setup(m => m.WriteFile(file, path)).Returns(expected);
            var behavior = ConfigFileHandler.ConfigFileHelper.GetInstance(_serializer.Object);

            // Act
            Trace.WriteLine("Writing file: " + path);
            var success = (path == null) ? behavior.WriteFile(file) : behavior.WriteFile(file, path);

            // Assert
            Assert.That(success == expected);
        }

        [Test]
        public void Test_ServiceFileBehavior_CreateFile()
        {
            // Arrange
            var expected = ConfigFile.Default;
            
            // Act
            var actual = ConfigFileHandler.ConfigFileHelper.GetHelperInstance(_logger.Object)
                                          .NewConfigFile;

            // Assert
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ServiceFileBehavior_ReadFile()
        {
            // Arrange
            var path = Tests.RandomFilePath();
            var expected = ConfigFile.GetInstance();
            expected.FilePath = path;
            expected.LogDetails.IgnoreLogs = Tests.Random<bool>();
            expected.Services.LogDetails = expected.LogDetails;

            _serializer.Setup(m => m.ReadFile(path)).Returns(expected);

            var sfb = ConfigFileHandler.ConfigFileHelper.GetInstance(_serializer.Object);

            // Act
            var actual = sfb.ReadFile(path);

            // Assert
            Assert.That(actual == expected);
        }

        [Test]
        public void Test_ServiceFileBehavior_WriteFile()
        {
            // Arrange
            var file = ConfigFile.GetInstance();
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();

            // Act / Assert
            Test_WriteFile(true, file, path);
            Test_WriteFile(false, file, path);
        }

        [Test]
        public void Test_ServiceFileBehavior_WriteFile_get_path_from_File_Success()
        {
            // Arrange
            var file = ConfigFile.GetInstance();
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            file.FilePath = path;

            // Assert / Act
            Test_WriteFile(true, file);
            Test_WriteFile(false, file);
        }
    }
}