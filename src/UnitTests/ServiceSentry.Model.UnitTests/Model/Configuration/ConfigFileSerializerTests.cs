// -----------------------------------------------------------------------
//  <copyright file="ConfigFileSerializerTests.cs" company="Curtis Kaler">
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
    internal class ConfigFileSerializerTests
    {
        internal ConfigFileSerializerTests()
        {
            _logger = new Mock<Logger>();
        }
        [SetUp]
        public void Setup()
        {
            _workFolder = Environment.CurrentDirectory + "\\" +
                          Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            (new DirectoryInfo(_workFolder)).Create();
            _serializer = ConfigFileSerializer.GetInstance(_logger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            (new DirectoryInfo(_workFolder)).Delete(true);
        }

        private string _workFolder;
        private static ConfigFileSerializer _serializer;
        private readonly Mock<Logger> _logger;
        
        internal static void WriteFile()
        {
            var behavior = ConfigFileHandler.ConfigFileHelper.GetInstance(_serializer);

            var file = Tests.CreateTestFile();
            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();


            //file.Services.Items.Add();


            Trace.WriteLine("Creating new file: " + path);
            behavior.WriteFile(file, path);

            // Open the folder.
            Process.Start(Environment.CurrentDirectory);
        }


        [Test, Explicit]
        public void Test_Serializer_ReadWrite()
        {
            // Arrange
            var path = _workFolder + "\\" + Path.GetRandomFileName();
            var expected = Tests.CreateTestFile();
            expected.FilePath = path;
            ConfigFile actual = null;

            // Act
            try
            {
                _serializer.WriteFile(expected, path);
                actual = _serializer.ReadFile(path);
            } // Assert
            catch (Exception)
            {
                Assert.Fail();
            }

            Assert.That(actual != null);
            Assert.That(actual == expected);
        }


        [Test, Explicit]
        public void Test_WriteFile()
        {
            WriteFile();
        }
    }
}