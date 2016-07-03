// -----------------------------------------------------------------------
//  <copyright file="EmailerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.Email;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Email;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Email
{
    [TestFixture]
    internal class EmailerTests
    {
        [SetUp]
        public void SetUp()
        {
            _client = new Mock<SmtpClientWrapper>();
            _info = EmailInfo.Default;
            _info.From = Tests.RandomAddress();
            _info.SenderAddress = Tests.RandomAddress();
            _info.To.Add(Tests.RandomAddress());


            _packet = new SubscriptionPacket
                {
                    ServiceName = Tests.Random<string>(),
                    CommonName = Tests.Random<string>(),
                    DisplayName = Tests.Random<string>(),
                    EmailInfo = _info,
                };

            _state = new TrackingObject
                {
                    ServiceName = _packet.ServiceName,
                    Packet = _packet,
                };

            _message = new MailMessage();
        }

        private MailMessage _message;
        private Mock<SmtpClientWrapper> _client;
        private SubscriptionPacket _packet;
        private EmailInfo _info;
        private TrackingObject _state;

        public void Test_Emailer_OnSendCompleted(bool cancelled, Exception error)
        {
            // Arrange
            var emailSent = false;
            var emailFailed = false;
            var emailCancelled = false;

            var actual = EmailEventArgs.Empty;

            var args = new AsyncCompletedEventArgs(error, cancelled, _message);

            var sut = Emailer.GetInstance(_client.Object, EmailBuilder.Default);
            sut.EmailSent += (o, e) =>
                {
                    emailSent = true;
                    actual = e;
                };
            sut.EmailFailed += (o, e) =>
                {
                    emailFailed = true;
                    actual = e;
                };
            sut.EmailCancelled += (o, e) =>
                {
                    emailCancelled = true;
                    actual = e;
                };

            // Act
            sut.OnSendCompleted(null, args);

            // Assert
            Assert.AreEqual(cancelled, emailCancelled,
                            "EmailCancelled raised: expected '{0}', was '{1}'.",
                            cancelled, emailCancelled);

            Assert.AreEqual(error != null, emailFailed,
                            "EmailFailed raised: expected '{0}', was '{1}'.",
                            error != null, emailFailed);

            Assert.AreEqual(error == null && !cancelled, emailSent,
                            "EmailSent raised: expected '{0}', was '{1}'.",
                            error == null && !cancelled, emailSent);

            Assert.AreEqual(_client.Object, actual.Client,
                            "Client: expected '{0}', was '{1}'.",
                            _client.Object, actual.Client);

            Assert.AreEqual(_message, actual.Message,
                            "Message: expected '{0}', was '{1}'.",
                            _message, actual.Message);

            Assert.AreEqual(null, actual.Credential,
                            "Credential: expected '{0}', was '{1}'.",
                            null, actual.Credential);

            Assert.AreEqual(cancelled, actual.Cancelled,
                            "Cancelled: expected {0}, was {1}.",
                            cancelled, actual.Cancelled);

            Assert.AreEqual(error, actual.Error,
                            "Error: expected {0}, was {1}.",
                            error, actual.Error);
        }

        public void Test_Emailer_SendFailureNotification(bool expectedResult)
        {
            // Arrange
            var emailSent = false;
            var emailFailed = false;
            var args = new AsyncCompletedEventArgs(expectedResult ? null : new Exception(Tests.Random<string>()),
                                                   false,
                                                   _message);

            _client.Setup(m => m.SendAsync(It.IsAny<MailMessage>()))
                   .Raises(m => m.SendCompleted += null, args);

            var sut = Emailer.GetInstance(_client.Object, EmailBuilder.Default);
            sut.EmailSent += (o, e) => { emailSent = true; };
            sut.EmailFailed += (o, e) => { emailFailed = true; };

            // Act
            var actual = sut.SendServiceFailureNotification(_state);

            // Assert
            Assert.AreEqual(expectedResult, actual);
            Assert.AreEqual(expectedResult, emailSent);
            Assert.AreEqual(!expectedResult, emailFailed);
        }

        [Test]
        public void Test_Emailer_OnSendCompleted_Cancelled()
        {
            Test_Emailer_OnSendCompleted(true, null);
        }

        [Test]
        public void Test_Emailer_OnSendCompleted_Failed()
        {
            Test_Emailer_OnSendCompleted(false, new Exception(Tests.Random<string>()));
        }

        [Test]
        public void Test_Emailer_OnSendCompleted_Success()
        {
            Test_Emailer_OnSendCompleted(false, null);
        }

        [Test]
        public void Test_Emailer_SendFailureNotification_Fail()
        {
            Test_Emailer_SendFailureNotification(false);
        }

        [Test]
        public void Test_Emailer_SendFailureNotification_Success()
        {
            Test_Emailer_SendFailureNotification(true);
        }
    }
}