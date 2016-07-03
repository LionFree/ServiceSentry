// -----------------------------------------------------------------------
//  <copyright file="ConfigFileTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.Email;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.ExternalFiles;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model
{
    [TestFixture]
    internal class ConfigFileTests
    {
        [Test]
        public void Test_ConfigFile_Equals()
        {
            var item1 = ConfigFile.GetInstance();
            var item2 = ConfigFile.GetInstance();
            var item3 = ConfigFile.GetInstance();

            Tests.TestEquality(item1, item2, item3);

            var newPath = item1.FilePath + Path.GetRandomFileName();

            var newDetails = LoggingDetails.GetInstance();
            newDetails.ArchiveLogs = !item1.LogDetails.ArchiveLogs;

            var newLogs = FileList.Default;
            newLogs.Items.Add(ExternalFile.GetInstance(new Mock<Logger>().Object,
                                                       new Mock<FileSystem>().Object,
                                                       new Mock<ExternalFileBehavior>().Object,
                                                       string.Empty,
                                                       string.Empty, 0));

            var newNotification = NotificationSettings.GetInstance();
            newNotification.ShouldEmail = !item1.NotificationDetails.ShouldEmail;

            var newSMTP = SMTPInfo.Default;
            newSMTP.Port = Tests.GetNewInteger(item1.NotificationDetails.SMTPInfo.Port);

            var newEmail = EmailInfo.Default;
            newEmail.Priority = Tests.GetDifferentMailPriority(item1.NotificationDetails.EmailInfo.Priority);

            var mock = new Mock<Service>();
            mock.Setup(m => m.Equals(It.IsAny<object>())).Returns(true);

            var newServices = ServiceList.GetInstance();
            newServices.Items = new ObservableCollection<Service> {mock.Object};


            // set them different by one item property.
            //==========================================

            item1.FilePath = newPath;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.FilePath = newPath;
            item3.FilePath = newPath;
            item2.LogDetails = newDetails;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            item1.LogDetails = newDetails;
            item3.LogDetails = newDetails;
            item3.LogList = newLogs;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item1.LogList = newLogs;
            item2.LogList = newLogs;
            item1.NotificationDetails = newNotification;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item2.Equals(item1));
            Assert.IsFalse(item3.Equals(item1));
            Tests.TestEquality(item1, item2, item3);

            item2.NotificationDetails = newNotification;
            item3.NotificationDetails = newNotification;
            item2.Services = newServices;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item1.Services = newServices;
            item3.Services = newServices;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_ConfigFile_GetHashCode()
        {
            // Arrange
            var item1 = ConfigFile.GetInstance();

            // Act
            var item2 = item1;

            // Assert
            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_ConfigFile_LogDetails()
        {
            // Arrange
            var sut = ConfigFile.GetInstance();
            var oldValue = sut.LogDetails;
            var newValue = LoggingDetails.GetInstance();
            newValue.ArchivePath = Path.GetRandomFileName();
            newValue.ArchiveLogs = !oldValue.ArchiveLogs;

            // Act / Assert
            sut.ShouldNotNotifyOn(m => m.LogDetails).When(m => m.LogDetails = oldValue);
            sut.ShouldNotifyOn(m => m.LogDetails).When(m => m.LogDetails = newValue);
            Assert.That(sut.LogDetails == newValue);
        }

        [Test]
        public void Test_ConfigFile_LogList()
        {
            // Arrange
            var sut = ConfigFile.GetInstance();
            var oldValue = sut.LogList;
            var newValue = FileList.Default;
            newValue.Items.Add(ExternalFile.GetInstance(new Mock<Logger>().Object,
                                                       new Mock<FileSystem>().Object,
                                                       new Mock<ExternalFileBehavior>().Object,
                                                       string.Empty,
                                                       string.Empty, 0));

            // Act / Assert
            sut.ShouldNotNotifyOn(m => m.LogList).When(m => m.LogList = oldValue);
            sut.ShouldNotifyOn(m => m.LogList).When(m => m.LogList = newValue);
            Assert.That(sut.LogList == newValue);
        }

        [Test]
        public void Test_ConfigFile_NotificationDetails()
        {
            // Arrange
            var sut = ConfigFile.GetInstance();
            var oldValue = sut.NotificationDetails;
            var newValue = NotificationSettings.GetInstance();
            newValue.ShouldEmail = !oldValue.ShouldEmail;

            // Act / Assert
            sut.ShouldNotNotifyOn(m => m.NotificationDetails).When(m => m.NotificationDetails = oldValue);
            sut.ShouldNotifyOn(m => m.NotificationDetails).When(m => m.NotificationDetails = newValue);
            Assert.That(sut.NotificationDetails == newValue);
        }

        [Test]
        public void Test_ConfigFile_OnLogCollectionChanged()
        {
            // Arrange
            var name = Path.GetRandomFileName();
            var file = ExternalFile.GetInstance(new Mock<Logger>().Object,
                                                       new Mock<FileSystem>().Object,
                                                       new Mock<ExternalFileBehavior>().Object,
                                                       string.Empty,
                                                       string.Empty, 0);
            file.FullPath = name;
            var sut = ConfigFile.GetInstance();

            // Act
            sut.LogList.Items.Add(file);

            // Assert
            sut.ShouldNotNotifyOn(m => m.LogList.Items[0].FullPath).When(m => m.LogList.Items[0].FullPath = name);
            sut.ShouldNotifyOn(m => m.LogList.Items[0].FullPath).When(m => m.LogList.Items[0].FullPath = name + "1");
            // This also tests OnItemPropertyChanged.
        }

        [Test]
        public void Test_ConfigFile_OnServiceCollectionChanged()
        {
            // Arrange
            var wasCalled = false;
            var mock = new Mock<Service>();
            var sut = ConfigFile.GetInstance();
            sut.Services.Items.CollectionChanged += (s, e) => { wasCalled = true; };

            // Act
            sut.Services.Items.Add(mock.Object);

            // Assert
            Assert.IsTrue(wasCalled);
        }
        
        [Test]
        public void Test_ConfigFile_Services()
        {
            // Arrange
            var sut = ConfigFile.GetInstance();
            var oldValue = sut.Services;
            var newValue = ServiceList.GetInstance();
            newValue.LogDetails.ArchiveLogs = !oldValue.LogDetails.ArchiveLogs;

            // Act / Assert
            sut.ShouldNotNotifyOn(m => m.Services).When(m => m.Services = oldValue);
            sut.ShouldNotifyOn(m => m.Services).When(m => m.Services = newValue);
            Assert.That(sut.Services == newValue);
        }
    }
}