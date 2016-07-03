// -----------------------------------------------------------------------
//  <copyright file="WindowsServiceInstallerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceProcess;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Common.Testing;
using ServiceSentry.ServiceFramework;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    internal class WindowsServiceInstallerTests
    {
        private readonly WindowsServiceAttribute _configuration;
        private readonly ServiceInstaller _expectedServiceInstaller;
        private readonly ServiceProcessInstaller _expectedProcessInstaller;

        public WindowsServiceInstallerTests()
        {
            var service = TestWindowsService.GetInstance();
            _configuration = service.GetType().GetAttribute<WindowsServiceAttribute>();

            _expectedServiceInstaller = new ServiceInstaller
                {
                    ServiceName = _configuration.ServiceName,
                    DisplayName = _configuration.DisplayName,
                    Description = _configuration.Description,
                    StartType = _configuration.StartMode
                };


            _expectedProcessInstaller = new ServiceProcessInstaller();
            if (string.IsNullOrEmpty(_configuration.UserName))
            {
                _expectedProcessInstaller.Account = ServiceAccount.LocalSystem;
                _expectedProcessInstaller.Username = null;
                _expectedProcessInstaller.Password = null;
            }
            else
            {
                _expectedProcessInstaller.Account = ServiceAccount.User;
                _expectedProcessInstaller.Username = _configuration.UserName;
                _expectedProcessInstaller.Password = _configuration.Password;
            }
        }

        [Test]
        public void Test_WSI_Constructor_by_Instance()
        {
            // Arrange
            var service = TestWindowsService.GetInstance();
            WindowsServiceInstaller sut = null;

            // Act
            Assert.DoesNotThrow(() => { sut = WindowsServiceInstaller.GetInstance(service); });

            // Assert
            Assert.IsNotNull(sut);
        }

        [Test]
        public void Test_WSI_Constructor_by_Type()
        {
            // Arrange
            WindowsServiceInstaller sut = null;

            // Act
            Assert.DoesNotThrow(() => { sut = WindowsServiceInstaller.GetInstance<TestWindowsService>(); });

            // Assert
            Assert.IsNotNull(sut);
        }

        [Test]
        public void Test_WSI__Get_ProcessInstaller()
        {
            // Arrange
            var expected = _expectedProcessInstaller;
            var sut = WindowsServiceInstaller.GetInstance<TestWindowsService>();

            // Act
            var actual = sut.ProcessInstaller;

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Account, actual.Account);
            Assert.AreEqual(expected.Username, actual.Username);
            Assert.AreEqual(expected.Password, actual.Password);
        }

        [Test]
        public void Test_WSI__Get_ServiceInstaller()
        {
            // Arrange
            var expected = _expectedServiceInstaller;
            var sut = WindowsServiceInstaller.GetInstance<TestWindowsService>();

            // Act
            var actual = sut.ServiceInstaller;

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ServiceName, actual.ServiceName);
            Assert.AreEqual(expected.DisplayName, actual.DisplayName);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.StartType, actual.StartType);
        }
    }
}