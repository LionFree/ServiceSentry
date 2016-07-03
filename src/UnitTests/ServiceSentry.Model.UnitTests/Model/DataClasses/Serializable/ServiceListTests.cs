// -----------------------------------------------------------------------
//  <copyright file="ServiceListTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using System.IO;
using Moq;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses.Serializable
{
    [TestFixture]
    internal class ServiceListTests
    {
        [Test]
        public void Test_ServiceList_Equals()
        {
            var item1 = ServiceList.GetInstance();
            var item2 = ServiceList.GetInstance();
            var item3 = ServiceList.GetInstance();

            var newLogs = LoggingDetails.GetInstance();
            newLogs.ArchivePath = Path.GetRandomFileName();
            newLogs.ClearLogs = Tests.Random<bool>();
            newLogs.ArchiveLogs = Tests.Random<bool>();

            var mock = new Mock<Service>();
            mock.Setup(m => m.Equals(It.IsAny<object>())).Returns(true);
            var newItems = new ObservableCollection<Service> {mock.Object};

            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================

            item1.LogDetails = newLogs;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.LogDetails = newLogs;
            item3.LogDetails = newLogs;
            item2.Items = newItems;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item1.Items = newItems;
            item3.Items = newItems;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_ServiceList_GetHashCode()
        {
            // Arrange
            var item1 = ServiceList.GetInstance();

            // Act
            var item2 = item1;

            // Assert
            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_ServiceList_Items_PropertyChanged()
        {
            // Arrange
            var sut = ServiceList.GetInstance();
            var oldValue = sut.Items;
            var newValue = new ObservableCollection<Service> {(new Mock<Service>()).Object};

            Assert.That(newValue != oldValue);

            // Act / Assert
            sut.ShouldNotNotifyOn(m => m.Items).When(m => m.Items = oldValue);
            sut.ShouldNotifyOn(m => m.Items).When(m => m.Items = newValue);
            Assert.That(sut.Items == newValue);
        }

        [Test]
        public void Test_ServiceList_LoggingDetails_PropertyChanged()
        {
            // Arrange
            var sut = ServiceList.GetInstance();
            var oldValue = sut.LogDetails;
            var newValue = LoggingDetails.GetInstance();
            newValue.ArchivePath = Path.GetRandomFileName();
            Assert.That(newValue != oldValue);

            // Act / Assert
            sut.ShouldNotNotifyOn(m => m.LogDetails).When(m => m.LogDetails = oldValue);
            sut.ShouldNotifyOn(m => m.LogDetails).When(m => m.LogDetails = newValue);
            Assert.That(sut.LogDetails == newValue);
        }

        [Test]
        public void Test_ServiceList_OnCollectionChanged()
        {
            // Arrange
            var wasCalled = false;
            var mock = new Mock<Service>();
            var sut = ServiceList.GetInstance();
            sut.Items.CollectionChanged += (s, e) => { wasCalled = true; };

            // Act
            sut.Items.Add(mock.Object);

            // Assert
            Assert.IsTrue(wasCalled);
        }

        //[Test]
        //public void Test_ServiceList_OnStatusChanged()
        //{
        //    // Arrange
        //    var wasCalled = false;
        //    var sut = ServiceList.GetInstance();
        //    sut.StatusChanged += (o, e) => { wasCalled = true; };

        //    // Act
        //    sut.OnStatusChanged(new StatusChangedEventArgs());

        //    // Assert
        //    Assert.IsTrue(wasCalled);
        //}
    }
}