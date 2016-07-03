// -----------------------------------------------------------------------
//  <copyright file="NotificationSettingsTests.cs" company="Curtis Kaler">
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
    internal class NotificationSettingsTests
    {
        private static NotificationSettings CreateTestObject(bool shouldEmail)
        {
            var output = NotificationSettings.GetInstance();
            output.ShouldEmail = shouldEmail;
            return output;
        }

        [Test]
        public void Test_NotificationSettings_Equals()
        {
            const bool shouldEmail = true;

            // Set them equal:
            var item1 = CreateTestObject(shouldEmail);
            var item2 = CreateTestObject(shouldEmail);
            var item3 = CreateTestObject(shouldEmail);
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));

            Tests.TestEquality(item1, item2, item3);

            // Set them different by one property.
            item1.ShouldEmail = false;
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));

            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_NotificationSettings_GetHashCode()
        {
            var shouldEmail = (new Random()).Next(2) == 1;

            // If two distinct objects compare as equal, their hashcodes must be equal.
            var item1 = CreateTestObject(shouldEmail);
            var item2 = CreateTestObject(shouldEmail);
            Assert.That(item1 == item2);

            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_NotificationSettings_PropertyChanged_Warn()
        {
            var sut = NotificationSettings.GetInstance();
            var currentValue = sut.ShouldEmail;
            var newValue = !currentValue;

            sut.ShouldNotNotifyOn(s => s.ShouldEmail).When(s => s.ShouldEmail = currentValue);
            sut.ShouldNotifyOn(s => s.ShouldEmail).When(s => s.ShouldEmail = newValue);
            Assert.That(sut.ShouldEmail == newValue);
        }

        [Test]
        public void Test_NotificationSettings_ToString()
        {
            var item = NotificationSettings.GetInstance();
            var value = item.ShouldEmail;
            Assert.IsTrue(item.ToString() == "ShouldEmail = " + value);

            // Make sure it wasn't a fluke.
            var newValue = !value;
            item.ShouldEmail = newValue;
            Assert.IsTrue(item.ToString() == "ShouldEmail = " + newValue);
        }
    }
}