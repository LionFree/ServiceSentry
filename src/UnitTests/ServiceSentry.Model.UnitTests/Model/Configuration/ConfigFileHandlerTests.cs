// -----------------------------------------------------------------------
//  <copyright file="ConfigFileHandlerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model
{
    [TestFixture]
    internal class ConfigFileHandlerTests
    {
        private readonly Mock<ConfigFileHandler.ConfigFileHelper> _behavior;
        private readonly Mock<Logger> _logger;

        public ConfigFileHandlerTests()
        {
            _logger = new Mock<Logger>();
            _behavior = new Mock<ConfigFileHandler.ConfigFileHelper>();
        }

        [Test]
        public void Test_ConfigHandler_ReadConfigFile_no_Path()
        {
            // Arrange
            var read = false;
            _behavior.Setup(m => m.ConfigFileExists(It.IsAny<string>())).Returns(true);
            _behavior.Setup(m => m.ReadFile(It.IsAny<string>()))
                     .Callback(() => { read = true; });
            var sut = ConfigFileHandler.GetInstance(_behavior.Object, _logger.Object);

            // Act 
            sut.ReadConfigFile();

            // Assert
            Assert.IsTrue(read);
        }

        [Test]
        public void Test_ConfigHandler_ReadConfigFile_with_Path()
        {
            // Arrange
            var path = Tests.Random<string>();
            var read = false;
            _behavior.Setup(m => m.ConfigFileExists(It.IsAny<string>())).Returns(true);
            _behavior.Setup(m => m.ReadFile(It.IsAny<string>()))
                     .Callback(() => { read = true; });
            var sut = ConfigFileHandler.GetInstance(_behavior.Object, _logger.Object);

            // Act 
            sut.ReadConfigFile(path);

            // Assert
            Assert.IsTrue(read);
        }

        [Test]
        public void Test_ConfigHandler_WriteConfigFile()
        {
            // Arrange
            var wrote = false;
            var file = new Mock<ConfigFile>();
            _behavior.Setup(m => m.WriteFile(It.IsAny<ConfigFile>(), It.IsAny<string>()))
                     .Callback(() => { wrote = true; });
            var sut = ConfigFileHandler.GetInstance(_behavior.Object, _logger.Object);

            // Act 
            sut.WriteConfigFile(file.Object);

            // Assert
            Assert.IsTrue(wrote);
        }
    }
}