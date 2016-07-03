// -----------------------------------------------------------------------
//  <copyright file="SMTPClientInfoTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using NUnit.Framework;
using ServiceSentry.Common.Email;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.Email
{
    [TestFixture]
    internal class SMTPClientInfoTests
    {
        private readonly string _host = Tests.Random<string>();
        private readonly int _port = (new Random()).Next(10000);
        private readonly bool _defaultCredentials = Tests.Random<bool>();
        private readonly SmtpDeliveryMethod _method = Tests.Random<SmtpDeliveryMethod>();
        private readonly bool _enableSsl = Tests.Random<bool>();

        private readonly ICredentialsByHost _credentials = new NetworkCredential(Tests.Random<string>(),
                                                                                 Tests.Random<string>(),
                                                                                 Tests.Random<string>());

        private static SMTPInfo CreateTestObject(string host, int port, bool defaultCredentials,
                                                       SmtpDeliveryMethod method, bool enableSsl,
                                                       ICredentialsByHost credentials)
        {
            var item1 = SMTPInfo.Default;
            item1.HostName = host;
            item1.Port = port;
            item1.Credentials = credentials;
            item1.DeliveryMethod = method;
            item1.EnableSsl = enableSsl;
            item1.UseDefaultCredentials = defaultCredentials;

            return item1;
        }

        [Test]
        public void Test_SMTPClientInfo_Equals()
        {
            var newHost = _host + Tests.Random<string>();
            var newPort = (_port + Tests.Randomizer.Next(10000))%64000;
            var newDefaultCredentials = !_defaultCredentials;
            var newMethod = Tests.NewRandom(_method);
            var newEnableSsl = !_enableSsl;
            var newCredentials = new NetworkCredential(Tests.Random<string>(), Tests.Random<string>(), Tests.Random<string>());

            var newDay = Tests.Random<int>();
            var newMinute = Tests.Random<int>();


            var item1 = CreateTestObject(_host, _port,  _defaultCredentials, _method, _enableSsl, _credentials);
            var item2 = CreateTestObject(_host, _port,  _defaultCredentials, _method, _enableSsl, _credentials);
            var item3 = CreateTestObject(_host, _port, _defaultCredentials, _method, _enableSsl, _credentials);

            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            item1.HostName = newHost;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.HostName = newHost;
            item3.HostName = newHost;
            item2.Port = newPort;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            item1.Port = newPort;
            item3.Port = newPort;
            item3.UseDefaultCredentials = newDefaultCredentials;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
            
            item1.UseDefaultCredentials = newDefaultCredentials;
            item2.UseDefaultCredentials = newDefaultCredentials;
            item1.DeliveryMethod = newMethod;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.DeliveryMethod = newMethod;
            item3.DeliveryMethod = newMethod;
            item2.EnableSsl = newEnableSsl;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item1));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item1.EnableSsl = newEnableSsl;
            item3.EnableSsl = newEnableSsl;
            item3.Credentials = newCredentials;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item1));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            item1.Credentials = newCredentials;
            item2.Credentials = newCredentials;
            item1.MaxMailsPerDay = newDay;
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item3.MaxMailsPerDay = newDay;
            item2.MaxMailsPerDay = newDay;
            item2.MaxMailsPerMinute = newMinute;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item1));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item1.MaxMailsPerMinute = newMinute;
            item3.MaxMailsPerMinute = newMinute;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            
        }

        [Test]
        public void Test_SMTPClientInfo_GetHashCode()
        {
            // If two distinct objects compare as equal, their hashcodes must be equal.
            var item1 = CreateTestObject(_host, _port,  _defaultCredentials, _method, _enableSsl, _credentials);
            var item2 = CreateTestObject(_host, _port,  _defaultCredentials, _method, _enableSsl, _credentials);

            Assert.That(item1 == item2);

            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_SMTPClientInfo_PropertyChanged_HostName()
        {
            var sut = SMTPInfo.Default;
            var oldValue = sut.HostName;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.HostName).When(s => s.HostName = oldValue);
            sut.ShouldNotifyOn(s => s.HostName).When(s => s.HostName = newValue);
            Assert.That(sut.HostName == newValue);
        }

        [Test]
        public void Test_SMTPClientInfo_PropertyChanged_Port()
        {
            var sut = SMTPInfo.Default;
            var oldValue = sut.Port;
            var newValue = Tests.GetNewInteger(oldValue);

            sut.ShouldNotNotifyOn(s => s.Port).When(s => s.Port = oldValue);
            sut.ShouldNotifyOn(s => s.Port).When(s => s.Port = newValue);
            Assert.That(sut.Port == newValue);
        }

        

        [Test]
        public void Test_SMTPClientInfo_PropertyChanged_UseDefaultCredentials()
        {
            var sut = SMTPInfo.Default;
            var oldValue = sut.UseDefaultCredentials;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.UseDefaultCredentials).When(s => s.UseDefaultCredentials = oldValue);
            sut.ShouldNotifyOn(s => s.UseDefaultCredentials).When(s => s.UseDefaultCredentials = newValue);
            Assert.That(sut.UseDefaultCredentials == newValue);
        }

        [Test]
        public void Test_SMTPClientInfo_PropertyChanged_DeliveryMethod()
        {
            var sut = SMTPInfo.Default;
            var oldValue = sut.DeliveryMethod;
            var newValue = Tests.NewRandom(oldValue);

            sut.ShouldNotNotifyOn(s => s.DeliveryMethod).When(s => s.DeliveryMethod = oldValue);
            sut.ShouldNotifyOn(s => s.DeliveryMethod).When(s => s.DeliveryMethod = newValue);
            Assert.That(sut.DeliveryMethod == newValue);
        }

        [Test]
        public void Test_SMTPClientInfo_PropertyChanged_EnableSsl()
        {
            var sut = SMTPInfo.Default;
            var oldValue = sut.EnableSsl;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.EnableSsl).When(s => s.EnableSsl = oldValue);
            sut.ShouldNotifyOn(s => s.EnableSsl).When(s => s.EnableSsl = newValue);
            Assert.That(sut.EnableSsl == newValue);
        }

        [Test]
        public void Test_SMTPClientInfo_PropertyChanged_Credentials()
        {
            var sut = SMTPInfo.Default;
            var oldValue = sut.Credentials;
            var newValue = new NetworkCredential(Tests.Random<string>(), Tests.Random<string>(), Tests.Random<string>());

            sut.ShouldNotNotifyOn(s => s.Credentials).When(s => s.Credentials = oldValue);
            sut.ShouldNotifyOn(s => s.Credentials).When(s => s.Credentials = newValue);
            Assert.That(sut.Credentials == newValue);
        }
    }
}