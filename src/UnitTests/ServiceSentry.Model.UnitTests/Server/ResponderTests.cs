// -----------------------------------------------------------------------
//  <copyright file="ResponderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.Email;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Email;
using ServiceSentry.Model.Server;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Server
{
    [TestFixture]
    internal class ResponderTests
    {

        private readonly Mock<ModelClassFactory> _factory;
        private readonly Mock<Logger> _logger;

        internal ResponderTests()
        {
            _logger = new Mock<Logger>();
            _factory=new Mock<ModelClassFactory>();
        }

        internal List<DateTime> GetDates(int totalIntervalSeconds)
        {
            var events = Tests.Random<int>(1, 10);
            var interval = totalIntervalSeconds/(events + 2);

            var start = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(totalIntervalSeconds - 10));
            var dates = new List<DateTime> {start};

            for (var i = 0; i < events; i++)
            {
                dates.Add(dates[i].AddSeconds(interval));
            }

            Trace.WriteLine(String.Format("Number of dates: {0}", dates.Count));
            return dates;
        }

        internal List<Exception> GetExceptions()
        {
            var events = Tests.Random<int>(1, 10);

            var exceptions = new List<Exception>();

            for (var i = 0; i < events; i++)
            {
                exceptions.Add(Tests.Random<Exception>());
            }

            Trace.WriteLine(String.Format("Number of exceptions: {0}", exceptions.Count));
            return exceptions;
        }

        public void Test_Responder_HandleFailure(bool notifyOnStop, bool canEmail)
        {
            // Arrange
            var failureAdded = false;
            var emailSent = false;
            var shouldEmail = notifyOnStop && canEmail;
            var state = new TrackingObject {NotifyOnUnexpectedStop = notifyOnStop};
            var helper = new Mock<ResponderHelper>();
            helper.Setup(m => m.CanEmail(It.IsAny<TrackingObject>())).Returns(canEmail);
            helper.Setup(m => m.AddFailureToList(ref state)).Callback(() => { failureAdded = true; });
            helper.Setup(m => m.SendEmail(state)).Callback(() => { emailSent = true; });

            var sut = Responder.GetInstance(_logger.Object, helper.Object);

            // Act
            sut.HandleFailure(state);

            // Assert
            Assert.IsTrue(failureAdded);
            Assert.AreEqual(shouldEmail, emailSent);
        }

        [Test]
        public void Test_Responder_Exceptions()
        {
            // Arrange
            var expected = GetExceptions();
            var helper = new Mock<ResponderHelper>();
            helper.Setup(m => m.Exceptions).Returns(expected);
            var sut = Responder.GetInstance(_logger.Object, helper.Object);

            // Act
            helper.Raise(m => m.ExceptionsUpdated += null, new EventArgs());
            
            // Assert
            Assert.AreEqual(expected, sut.Exceptions);
        }

        [Test]
        public void Test_ResponderHelper_AddFailureToList()
        {
            var state = new TrackingObject {ServiceName = Tests.Random<string>()};
            var sut = ResponderHelper.GetInstance(_logger.Object);

            // Add between 10 and 20 failures.
            var count = Tests.Random<int>(10, 20);
            var expected = new List<DateTime>();

            // Act
            for (var i = 0; i < count; i++)
            {
                sut.AddFailureToList(ref state);
                if ((count - i) < 6) expected.Add(state.FailDates[state.FailDates.Count - 1]);
            }

            // Assert
            //  The LAST 5 should remain, in order.
            Assert.That(state.FailDates.Count == 5);
            Assert.AreEqual(expected, state.FailDates);
        }

        [Test]
        public void Test_ResponderHelper_CanEmail_NoErrors()
        {
            // Arrange
            var state = new TrackingObject();
            var sut = ResponderHelper.GetInstance(_factory.Object,_logger.Object);

            // Act
            var actual = sut.CanEmail(state);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void Test_ResponderHelper_CanEmail_NoLimit()
        {
            // Arrange
            var dates = GetDates(60);
            var smtpInfo = SMTPInfo.Default;
            smtpInfo.MaxMailsPerMinute = 0;
            smtpInfo.MaxMailsPerDay = 0;
            var packet = new SubscriptionPacket {SMTPInfo = smtpInfo};
            var state = new TrackingObject {FailDates = dates, Packet = packet};
            var sut = ResponderHelper.GetInstance(_factory.Object, _logger.Object);

            // Act
            var actual = sut.CanEmail(state);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void Test_ResponderHelper_CanEmail_TooManyPerDay()
        {
            // Arrange
            var dates = GetDates(60*60*24);

            var smtpInfo = SMTPInfo.Default;
            smtpInfo.MaxMailsPerMinute = Tests.Random<int>(1, 5); // Who cares.
            smtpInfo.MaxMailsPerDay = dates.Count - 1;
            Trace.WriteLine(String.Format("Max/minute: {0},  Max/day: {1}", smtpInfo.MaxMailsPerMinute,
                                          smtpInfo.MaxMailsPerDay));

            var packet = new SubscriptionPacket {SMTPInfo = smtpInfo};
            var state = new TrackingObject {FailDates = dates, Packet = packet};
            var sut = ResponderHelper.GetInstance(_factory.Object, _logger.Object); 

            // Act
            var actual = sut.CanEmail(state);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void Test_ResponderHelper_CanEmail_TooManyPerMinute()
        {
            // Arrange
            var dates = GetDates(60);
            var smtpInfo = SMTPInfo.Default;
            smtpInfo.MaxMailsPerMinute = dates.Count - 1;
            smtpInfo.MaxMailsPerDay = dates.Count + 1;
            Trace.WriteLine(String.Format("Max/minute: {0},  Max/day: {1}", smtpInfo.MaxMailsPerMinute,
                                          smtpInfo.MaxMailsPerDay));

            var packet = new SubscriptionPacket {SMTPInfo = smtpInfo};
            var state = new TrackingObject {FailDates = dates, Packet = packet};
            var sut = ResponderHelper.GetInstance(_factory.Object,_logger.Object);

            // Act
            var actual = sut.CanEmail(state);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void Test_ResponderHelper_EmailOverLimit_False()
        {
            // Arrange
            var sut = ResponderHelper.GetInstance(_factory.Object, _logger.Object);

            // Act
            var actual = sut.EmailOverLimit(2, 3);

            //Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void Test_ResponderHelper_EmailOverLimit_NoMax()
        {
            // Arrange
            var sut = ResponderHelper.GetInstance(_factory.Object, _logger.Object);

            // Act
            var actual = sut.EmailOverLimit(0, 3);

            //Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void Test_ResponderHelper_EmailOverLimit_True()
        {
            // Arrange
            var sut = ResponderHelper.GetInstance(_factory.Object, _logger.Object);

            // Act
            var actual = sut.EmailOverLimit(5, 3);

            //Assert
            Assert.IsTrue(actual);
        }


        [Test]
        public void Test_ResponderHelper_SendEmail_Fail_NoOtherExceptions()
        {
            // Arrange
            var wasCalled = false;
            var exceptionsUpdated = false;
            var failure = Tests.Random<Exception>();
            var factory = new Mock<ModelClassFactory>();
            var emailer = new Mock<Emailer>();
            var expected = new List<Exception> {failure};
            
            var state = new TrackingObject();
            emailer.Setup(m => m.SendServiceFailureNotification(It.IsAny<TrackingObject>()))
                   .Callback(() =>
                   {
                       wasCalled = true;
                       emailer.Raise(m => m.EmailFailed += null, new EmailEventArgs { Error = failure });
                   });
            
            factory.Setup(m => m.GetEmailer(It.IsAny<SMTPInfo>())).Returns(emailer.Object);
            var sut = ResponderHelper.GetInstance(factory.Object,_logger.Object);
            sut.ExceptionsUpdated += (s, e) => { exceptionsUpdated = true; };

            // Act
            sut.SendEmail(state);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.IsFalse(exceptionsUpdated);
            Assert.AreEqual(expected, sut.Exceptions);
        }

        [Test]
        public void Test_ResponderHelper_SendEmail_Succeed_NoExceptions()
        {
            // Arrange
            var wasCalled = false;
            var exceptionsUpdated = false;
            var exceptions = new List<Exception>();
            var state = new TrackingObject { Exceptions = exceptions };
            var emailer = new Mock<Emailer>();
            emailer.Setup(m => m.SendServiceFailureNotification(It.IsAny<TrackingObject>()))
                   .Callback(() => { wasCalled = true; });
            var factory = new Mock<ModelClassFactory>();
            factory.Setup(m => m.GetEmailer(It.IsAny<SMTPInfo>())).Returns(emailer.Object);
            var sut = ResponderHelper.GetInstance(factory.Object, _logger.Object);
            sut.ExceptionsUpdated += (s, e) => { exceptionsUpdated = true; };

            // Act
            sut.SendEmail(state);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.IsFalse(exceptionsUpdated);
            Assert.AreEqual(exceptions, sut.Exceptions);
        }

        [Test]
        public void Test_ResponderHelper_SendEmail_Fail()
        {
            // Arrange
            var wasCalled = false;
            var exceptionsUpdated = false;
            var exceptions = GetExceptions();
            var failure = Tests.Random<Exception>();
            var factory = new Mock<ModelClassFactory>();
            var emailer = new Mock<Emailer>();


            var expected = new List<Exception> {failure};
            foreach (var item in exceptions)
            {
                expected.Add(item);
            }

            var state = new TrackingObject {Exceptions = exceptions};
            emailer.Setup(m => m.SendServiceFailureNotification(It.IsAny<TrackingObject>()))
                   .Callback(() =>
                       {
                           wasCalled = true;
                           emailer.Raise(m => m.EmailFailed += null, new EmailEventArgs {Error = failure});
                       });


            factory.Setup(m => m.GetEmailer(It.IsAny<SMTPInfo>())).Returns(emailer.Object);
            var sut = ResponderHelper.GetInstance(factory.Object, _logger.Object);
            sut.ExceptionsUpdated += (s, e) => { exceptionsUpdated = true; };

            // Act
            sut.SendEmail(state);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(exceptionsUpdated);
            Assert.AreEqual(expected, sut.Exceptions);
        }

        [Test]
        public void Test_ResponderHelper_SendEmail_Succeed()
        {
            // Arrange
            var wasCalled = false;
            var exceptionsUpdated = false;
            var exceptions = GetExceptions();
            var state = new TrackingObject {Exceptions = exceptions};
            var emailer = new Mock<Emailer>();
            emailer.Setup(m => m.SendServiceFailureNotification(It.IsAny<TrackingObject>()))
                   .Callback(() => { wasCalled = true; });
            var factory = new Mock<ModelClassFactory>();
            factory.Setup(m => m.GetEmailer(It.IsAny<SMTPInfo>())).Returns(emailer.Object);
            var sut = ResponderHelper.GetInstance(factory.Object, _logger.Object);
            sut.ExceptionsUpdated += (s, e) => { exceptionsUpdated = true; };

            // Act
            sut.SendEmail(state);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(exceptionsUpdated);
            Assert.AreEqual(exceptions, sut.Exceptions);
        }

        [Test]
        public void Test_Responder_HandleFailure_CanEmail_and_DoNotNotify()
        {
            Test_Responder_HandleFailure(false, true);
        }

        [Test]
        public void Test_Responder_HandleFailure_CanEmail_and_NotifyOnStop()
        {
            Test_Responder_HandleFailure(true, true);
        }

        [Test]
        public void Test_Responder_HandleFailure_CannotEmail_and_DoNotNotify()
        {
            Test_Responder_HandleFailure(false, false);
        }

        [Test]
        public void Test_Responder_HandleFailure_CannotEmail_and_NotifyOnStop()
        {
            Test_Responder_HandleFailure(true, false);
        }
    }
}