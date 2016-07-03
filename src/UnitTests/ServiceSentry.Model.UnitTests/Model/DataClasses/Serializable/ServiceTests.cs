// -----------------------------------------------------------------------
//  <copyright file="ServiceTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;
using ServiceSentry.UnitTests.Common;

#endregion

#pragma warning disable 1685

namespace ServiceSentry.Model.UnitTests.Model.DataClasses.Serializable
{
    [TestFixture, Explicit("Touches real services.")]
    internal class ServiceTests
    {
        private const string Time = "W32Time";

        private const string CanStop = "iphlpsvc";
        private const string Start = "AudioSrv";
        private const string Stop = "W3SVC";
        private const string RefreshService = "wuauserv";


        private const string RPC = "RpcSs";
        private readonly Mock<MonitorServiceWatchdog> _monitor;
        private readonly Mock<Logger> _logger;

        public ServiceTests()
        {
            _monitor = new Mock<MonitorServiceWatchdog>();
            _logger = new Mock<Logger>();
        }


        [Test]
        public void Test_Dispose()
        {
            // Arrange
            var sut = Service.GetInstance(Time, ".", _logger.Object);

            // Act
            sut.Dispose();

            // Assert
            Assert.That(sut.IsDisposed);
        }

        [Test]
        public void Test_Get_CanStop()
        {
            // Arrange
            var sut = Service.GetInstance(CanStop, ".", _logger.Object);
            var expected = new ServiceController(CanStop).CanStop;

            // Act 
            var actual = sut.CanStop;

            // Assert
            Assert.That(expected == actual);
        }

        [Test]
        public void Test_Get_DisplayName()
        {
            // Arrange
            var sut = Service.GetInstance(Time, ".", _logger.Object);
            var expected = new ServiceController(Time).DisplayName;

            // Act
            var actual = sut.DisplayName;

            // Assert
            Assert.That(expected == actual);
        }

        [Test]
        public void Test_Get_ServiceName()
        {
            // Arrange
            var sut = Service.GetInstance(Time, ".", _logger.Object);

            // Act
            var actual = sut.ServiceName;

            // Assert
            Assert.That(actual == Time);
        }

        [Test]
        public void Test_Get_Status()
        {
            // Arrange
            var sut = Service.GetInstance(Time, ".", _logger.Object);
            var expected = (ServiceState) new ServiceController(Time).Status;

            // Act
            var actual = sut.Status;

            // Assert
            Assert.That(expected == actual);
        }

        [Test]
        public void Test_LogFiles_CollectionChanged()
        {
            // Arrange
            var wasCalled = false;
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var name = Path.GetRandomFileName();
            var file = new Mock<ExternalFile>();
            file.Setup(m => m.CommonName).Returns(name);

            sut.LogFiles.CollectionChanged += (o, e) => { wasCalled = true; };

            // Act
            sut.LogFiles.Add(file.Object);

            // Assert
            sut.ShouldNotifyOn(m => m.LogFiles[0].CommonName).When(m => m.LogFiles[0].CommonName = name + "1");
            Assert.IsTrue(wasCalled);
            // This also tests OnItemPropertyChanged.
        }

        [Test]
        public void Test_OnExternalStatusChange()
        {
            // Arrange
            var wasCalled = false;
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            sut.ExternalStatusChanged += (s, e) => { wasCalled = true; };

            // Act
            sut.OnExternalStatusChange(new StatusChangedEventArgs());

            // Assert
            Assert.That(wasCalled);
        }

        [Test]
        public void Test_OnServiceFellOver()
        {
            // Arrange
            var wasCalled = false;
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            sut.ServiceFellOver += (s, e) => { wasCalled = true; };

            // Act
            sut.OnServiceFellOver(new StatusChangedEventArgs());

            // Assert
            Assert.That(wasCalled);
        }

        [Test, Explicit("Takes half a second.")]
        public void Test_Refresh()
        {
            // Arrange
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var sc = new ServiceController(RefreshService);
            if (sc.CanStop)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }

            Assert.That(sc.Status == ServiceControllerStatus.Stopped);
            sut.Start();
            sut.WaitForStatus(ServiceState.Running);
            Assert.That(sc.Status == ServiceControllerStatus.Stopped);


            // Act 
            sut.Refresh(_monitor.Object);
            var actual = sut.Status;


            // Assert
            Assert.That(actual == ServiceState.Running);
        }

        [Test]
        public void Test_Service_CollectionChanged_ConfigFiles()
        {
            var wasCalled = false;

            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var name = Path.GetRandomFileName();
            var file = new Mock<ExternalFile>();
            file.Setup(m => m.CommonName).Returns(name);

            sut.ConfigFiles.CollectionChanged += (o, e) => { wasCalled = true; };

            sut.ConfigFiles.Add(file.Object);
            sut.ShouldNotifyOn(m => m.ConfigFiles[0].CommonName).When(m => m.ConfigFiles[0].CommonName = name + "1");

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_Service_Equals()
        {
            var item1 = Service.GetInstance(RefreshService, ".", _logger.Object);
            var item2 = item1;
            var item3 = Service.GetInstance(RefreshService, ".", _logger.Object);

            Assert.IsFalse(item1 == null);
            Assert.That(item1 == item1);

            Assert.That((item1 == item2) == (item2 == item1));

            if ((item1 == item2) && (item2 == item3))
                Assert.That(item1 == item3);

            // This is VALUE equality, not instance equality.
            // If two distinct Services refer to the same ServiceName,
            // then the two Services are equal.
            Assert.That(item1 == item3);
        }

        [Test]
        public void Test_Service_GetHashCode()
        {
            var item1 = Service.GetInstance(RefreshService, ".", _logger.Object);
            var item2 = item1;

            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_Service_GetSet_CanToggle()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.CanToggle;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.CanToggle).When(s => s.CanToggle = oldValue);
            sut.ShouldNotifyOn(s => s.CanToggle).When(s => s.CanToggle = newValue);
            Assert.That(sut.CanToggle == newValue);
        }

        [Test]
        public void Test_Service_GetSet_CommonName()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.CommonName;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.CommonName).When(s => s.CommonName = oldValue);
            sut.ShouldNotifyOn(s => s.CommonName).When(s => s.CommonName = newValue);

            Assert.That(sut.CommonName == newValue);
        }

        [Test]
        public void Test_Service_GetSet_ConfigFiles()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.ConfigFiles;
            var file = new Mock<ExternalFile>();
            var newValue = new ObservableCollection<ExternalFile> {file.Object};

            sut.ShouldNotNotifyOn(s => s.ConfigFiles).When(s => s.ConfigFiles = oldValue);
            sut.ShouldNotifyOn(s => s.ConfigFiles).When(s => s.ConfigFiles = newValue);
            Assert.That(sut.ConfigFiles == newValue);
        }

        [Test]
        public void Test_Service_GetSet_Details()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.Details;
            var newValue = ServiceDetails.GetInstance();
            newValue.NotifyOnUnexpectedStop = !newValue.NotifyOnUnexpectedStop;

            sut.ShouldNotNotifyOn(s => s.Details).When(s => s.Details = oldValue);
            sut.ShouldNotifyOn(s => s.Details).When(s => s.Details = newValue);
            Assert.That(sut.Details == newValue);
        }

        [Test]
        public void Test_Service_GetSet_IsReceivingInternalUpdate()
        {
            // Arrange
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.IsReceivingInternalUpdate;
            var newValue = !oldValue;

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.IsReceivingInternalUpdate).When(s => s.IsReceivingInternalUpdate = oldValue);
            sut.ShouldNotifyOn(s => s.IsReceivingInternalUpdate).When(s => s.IsReceivingInternalUpdate = newValue);
            Assert.IsTrue(sut.IsReceivingInternalUpdate == newValue);
        }

        [Test]
        public void Test_Service_GetSet_IsRestarting()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.IsRestarting;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.IsRestarting).When(s => s.IsRestarting = oldValue);

            sut.ShouldNotifyOn(s => s.IsRestarting).When(s => s.IsRestarting = newValue);

            Assert.That(sut.IsRestarting == newValue);
        }

        [Test]
        public void Test_Service_GetSet_LogFiles()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.LogFiles;
            var newFile = new Mock<ExternalFile>();
            var newValue = new ObservableCollection<ExternalFile> {newFile.Object};

            sut.ShouldNotNotifyOn(s => s.LogFiles).When(s => s.LogFiles = oldValue);
            sut.ShouldNotifyOn(s => s.LogFiles).When(s => s.LogFiles = newValue);
            Assert.That(sut.LogFiles == newValue);
        }

        [Test]
        public void Test_Service_GetSet_ParametersLocked()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.ParametersUnlocked;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.ParametersUnlocked).When(s => s.ParametersUnlocked = oldValue);
            sut.ShouldNotifyOn(s => s.ParametersUnlocked).When(s => s.ParametersUnlocked = newValue);
            Assert.That(sut.ParametersUnlocked == newValue);
        }

        [Test]
        public void Test_Service_GetSet_ServiceGroup()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.ServiceGroup;
            var newValue = oldValue + Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.ServiceGroup).When(s => s.ServiceGroup = oldValue);
            sut.ShouldNotifyOn(s => s.ServiceGroup).When(s => s.ServiceGroup = newValue);
            Assert.IsTrue(sut.ServiceGroup == newValue);
        }

        [Test]
        public void Test_Service_GetSet_ServiceName()
        {
            // Arrange
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.ServiceName;
            const string newValue = Start;

            // Act
            sut.ShouldNotNotifyOn(s => s.ServiceName).When(s => s.ServiceName = oldValue);
            sut.ShouldNotifyOn(s => s.ServiceName).When(s => s.ServiceName = newValue);

            // Assert
            Assert.IsTrue(sut.ServiceName == newValue);
            Assert.That(sut.CommonName == sut.DisplayName);
            Assert.That(sut.Guid != null);
            Assert.That(sut.Guid != Guid.Empty);
        }

        [Test]
        public void Test_Service_Get_DisplayName()
        {
            // Arrange
            var sut = Service.GetInstance(Time, ".", _logger.Object);
            var expected = new ServiceController(Time).DisplayName;

            // Act
            var actual = sut.DisplayName;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Service_ItemPropertyChanged_ConfigFiles()
        {
            // Arrange
            var wasCalled = false;

            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);

            var file = new Mock<ExternalFile>();

            var path = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();

            var integer1 = Tests.Random<int>();
            var integer2 = Tests.GetNewInteger(integer1);

            var oldValue = path + integer1;
            var newValue = path + integer2;
            Assert.That(oldValue != newValue);

            file.Setup(m => m.FullPath).Returns(oldValue);
            sut.ConfigFiles.Add(file.Object);

            sut.ConfigFiles[0].PropertyChanged += (s, e) => { wasCalled = true; };

            // Act
            sut.ConfigFiles[0].FullPath = oldValue;

            // Assert
            Assert.IsFalse(wasCalled);

            sut.ConfigFiles[0].FullPath = newValue;
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_Service_ItemPropertyChanged_LogFiles()
        {
            var wasCalled = false;

            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);

            var integer1 = Tests.Random<int>();
            var integer2 = Tests.GetNewInteger(integer1);

            var oldValue = Tests.RandomFilePath() + integer1;
            var newValue = Tests.RandomFilePath() + integer2;
            Assert.That(oldValue != newValue);

            var file = new Mock<ExternalFile>();
            file.Setup(m => m.FullPath).Returns(oldValue);

            sut.LogFiles.Add(file.Object);

            sut.LogFiles[0].PropertyChanged += (s, e) => { wasCalled = true; };

            sut.LogFiles[0].FullPath = oldValue;
            Assert.IsFalse(wasCalled);

            sut.LogFiles[0].FullPath = newValue;
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_Service_ServiceGroup_Default()
        {
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            Assert.That(sut.ServiceGroup == Strings.DefaultServiceGroupName);
        }

        [Test]
        public void Test_Service_ServiceName_Changes_CommonName()
        {
            // Assert
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldValue = sut.ServiceName;
            const string newValue = Stop;

            // Act / Assert
            sut.ShouldNotNotifyOn(s => s.CommonName).When(s => s.ServiceName = oldValue);
            sut.ShouldNotifyOn(s => s.CommonName).When(s => s.ServiceName = newValue);
            Assert.IsTrue(sut.CommonName == sut.DisplayName);
        }

        [Test]
        public void Test_Service_ServiceName_Changes_Guid()
        {
            // Arrange
            var sut = Service.GetInstance(RefreshService, ".", _logger.Object);
            var oldGuid = sut.Guid;

            // Act
            sut.ServiceName = Stop;

            // Assert
            Assert.IsFalse(sut.Guid == oldGuid);
        }

        [Test] //, Explicit("Ignored because it touches a real service, and may slow the test run.")
        public void Test_Service_Set_ServiceName()
        {
            // Arrange
            var sut = Service.GetInstance(Time, ".", _logger.Object);

            // Act
            sut.ServiceName = RPC;

            // Assert
            var actual = sut.ServiceName;
            var displayName = sut.DisplayName;
            var expected = new ServiceController(RPC).DisplayName;

            Assert.That(actual == RPC);
            Trace.WriteLine(displayName == expected);
        }

        [Test, Explicit("Touches a real service.")]
        public void Test_Service_Start()
        {
            // Arrange
            var sut = Service.GetInstance(Start, ".", _logger.Object);
            var sc = new ServiceController(Start);
            if (sc.CanStop)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }

            Assert.That(sc.Status == ServiceControllerStatus.Stopped);

            // Act 
            sut.Start();
            sut.WaitForStatus(ServiceState.Running);

            sc.Refresh();
            var actual = sc.Status;


            // Assert
            Assert.That(actual == ServiceControllerStatus.Running);
        }

        [Test, Explicit("Touches a real service.")]
        public void Test_Service_Stop()
        {
            // Arrange
            var monitor = new Mock<MonitorServiceWatchdog>();
            var sut = Service.GetInstance(Stop, ".", _logger.Object);
            var sc = new ServiceController(Stop);

            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running);
            }

            Assert.That(sc.Status == ServiceControllerStatus.Running);

            // Act 
            sut.Stop(monitor.Object);
            sut.WaitForStatus(ServiceState.Stopped);

            sc.Refresh();
            var actual = sc.Status;


            // Assert
            Assert.That(actual == ServiceControllerStatus.Stopped);
        }
    }
}