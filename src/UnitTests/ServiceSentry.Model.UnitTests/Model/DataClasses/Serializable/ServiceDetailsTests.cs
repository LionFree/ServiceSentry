// -----------------------------------------------------------------------
//  <copyright file="ServiceDetailsTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses.Serializable
{
    [TestFixture]
    internal class ServiceDetailsTests
    {
        private static ServiceDetails CreateTestObject(int start, int stop, int timeout, bool notify)
        {
            var item1 = ServiceDetails.GetInstance();
            item1.StartOrder = start;
            item1.StopOrder = stop;
            item1.Timeout = timeout;
            item1.NotifyOnUnexpectedStop = notify;

            return item1;
        }

        [Test]
        public void Test_ServiceDetails_Equals()
        {
            var start = (new Random()).Next(10000);
            var stop = (new Random()).Next(10000);
            var timeout = (new Random()).Next(10000);
            var notify = (new Random()).Next(2) == 1;

            var item1 = CreateTestObject(start, stop, timeout, notify);
            var item2 = CreateTestObject(start, stop, timeout, notify);
            var item3 = CreateTestObject(start, stop, timeout, notify);

            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            item1.StartOrder = -start;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.StartOrder = -start;
            item3.StartOrder = -start;
            item2.StopOrder = -stop;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            item1.StopOrder = -stop;
            item3.StopOrder = -stop;
            item3.Timeout = -timeout;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);


            item1.Timeout = -timeout;
            item2.Timeout = -timeout;
            item1.NotifyOnUnexpectedStop = !notify;
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);


            // Set them equal.
            item2.NotifyOnUnexpectedStop = !notify;
            item3.NotifyOnUnexpectedStop = !notify;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_ServiceDetails_GetHashCode()
        {
            var start = (new Random()).Next(10000);
            var stop = (new Random()).Next(10000);
            var timeout = (new Random()).Next(10000);
            var notify = (new Random()).Next(2) == 1;

            // If two distinct objects compare as equal, their hashcodes must be equal.
            var item1 = CreateTestObject(start, stop, timeout, notify);
            var item2 = CreateTestObject(start, stop, timeout, notify);
            Assert.That(item1 == item2);

            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_ServiceDetails_PropertyChanged_StartOrder()
        {
            var sut = ServiceDetails.GetInstance();
            var oldValue = sut.StartOrder;
            var newValue = Tests.GetNewInteger(oldValue);

            sut.ShouldNotNotifyOn(s => s.StartOrder).When(s => s.StartOrder = oldValue);
            sut.ShouldNotifyOn(s => s.StartOrder).When(s => s.StartOrder = newValue);
            Assert.That(sut.StartOrder == newValue);
        }

        [Test]
        public void Test_ServiceDetails_PropertyChanged_StopOrder()
        {
            var sut = ServiceDetails.GetInstance();
            var oldValue = sut.StopOrder;
            var newValue = Tests.GetNewInteger(oldValue);

            sut.ShouldNotNotifyOn(s => s.StopOrder).When(s => s.StopOrder = oldValue);
            sut.ShouldNotifyOn(s => s.StopOrder).When(s => s.StopOrder = newValue);
            Assert.That(sut.StopOrder == newValue);
        }

        [Test]
        public void Test_ServiceDetails_PropertyChanged_Timout()
        {
            var sut = ServiceDetails.GetInstance();
            var oldValue = sut.Timeout;
            var newValue = Tests.GetNewInteger(oldValue);

            sut.ShouldNotNotifyOn(s => s.Timeout).When(s => s.Timeout = oldValue);
            sut.ShouldNotifyOn(s => s.Timeout).When(s => s.Timeout = newValue);
            Assert.That(sut.Timeout == newValue);
        }

        [Test]
        public void Test_ServiceDetails_PropertyChanged_Warn()
        {
            var sut = ServiceDetails.GetInstance();
            var oldValue = sut.NotifyOnUnexpectedStop;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.NotifyOnUnexpectedStop).When(s => s.NotifyOnUnexpectedStop = oldValue);
            sut.ShouldNotifyOn(s => s.NotifyOnUnexpectedStop).When(s => s.NotifyOnUnexpectedStop = newValue);
            Assert.That(sut.NotifyOnUnexpectedStop == newValue);
        }
    }
}