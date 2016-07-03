// -----------------------------------------------------------------------
//  <copyright file="EmailInfoTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mail;
using NUnit.Framework;
using ServiceSentry.Common.Email;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.Email
{
    [TestFixture]
    internal class EmailInfoTests
    {
        [Test]
        public void Test_EmailInfo_Equals()
        {
            // Arrange
            var cc = new ObservableCollection<string> {Tests.RandomAddress()};
            var from = Tests.RandomAddress();
            var isBodyHtml = Tests.Random<bool>();
            var priority = Tests.Random<MailPriority>();
            var replyTo = Tests.RandomAddress();
            var sender = Tests.RandomAddress();
            var to = new ObservableCollection<string> {Tests.RandomAddress()};

            var item1 = Tests.CreateTestObject(cc, from, isBodyHtml, priority, replyTo, sender, to);
            var item2 = Tests.CreateTestObject(cc, from, isBodyHtml, priority, replyTo, sender, to);
            var item3 = Tests.CreateTestObject(cc, from, isBodyHtml, priority, replyTo, sender, to);

            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            var newCCValue = new ObservableCollection<string> {Tests.RandomAddress()};
            item1.CC = newCCValue;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            var newFromValue = Tests.RandomAddress();
            item2.CC = newCCValue;
            item3.CC = newCCValue;
            item2.From = newFromValue;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item1));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);


            var newIsBodyHtml = !isBodyHtml;
            item1.From = newFromValue;
            item3.From = newFromValue;
            item3.IsBodyHtml = newIsBodyHtml;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item1));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);


            var newPriority = Tests.GetDifferentMailPriority(item1.Priority);
            item1.IsBodyHtml = newIsBodyHtml;
            item2.IsBodyHtml = newIsBodyHtml;
            item1.Priority = newPriority;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);


            var newReplyTo = Tests.RandomAddress();
            item2.Priority = newPriority;
            item3.Priority = newPriority;
            item2.ReplyTo = newReplyTo;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item1));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            
            var newToValue = new ObservableCollection<string> {Tests.RandomAddress()};
            item1.ReplyTo = newReplyTo;
            item3.ReplyTo = newReplyTo;
            item1.To = newToValue;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item2.To = newToValue;
            item3.To = newToValue;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item3.Equals(item2));
            Assert.IsTrue(item3.Equals(item1));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_EmailInfo_GetHashCode()
        {
            // If two distinct objects compare as equal, their hashcodes must be equal.

            // Arrange / Act
            var item1 = EmailInfo.Default;
            var item2 = EmailInfo.Default;
            Assert.That(item1 == item2);

            // Assert
            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_EmailInfo_GetInstance()
        {
            // Arrange / Act
            var sut = EmailInfo.Default;

            // Assert
            Assert.That(sut != null);
        }

        [Test]
        public void Test_EmailInfo_PropertyChanged_SenderAddress()
        {
            var sut = EmailInfo.Default;
            var oldValue = sut.SenderAddress;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.SenderAddress).When(s => s.SenderAddress = oldValue);
            sut.ShouldNotifyOn(s => s.SenderAddress).When(s => s.SenderAddress = newValue);
            Assert.That(sut.SenderAddress == newValue);
        }
    }
}