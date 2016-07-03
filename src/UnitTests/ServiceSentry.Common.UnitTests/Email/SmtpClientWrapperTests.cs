using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using NUnit.Framework;
using ServiceSentry.Common.Email;
using ServiceSentry.UnitTests.Common;

namespace ServiceSentry.Common.UnitTests.Email
{
    [TestFixture]
    internal class SmtpClientWrapperTests
    {

        // SMTP Server FQDN
        private const string RealHostName = "";

        // a SMTP Server FQDN that shouldn't work
        private const string FakeHostName = "";
        
        private const string SpecifiedPickupDirectory
            = @"C:\Files\Debug\TempEmailDirectory";

        private readonly MailMessage _message;
        private readonly SMTPInfo _info;

        [SetUp]
        public void SetUp()
        {
            if (!Directory.Exists(SpecifiedPickupDirectory))
                Directory.CreateDirectory(SpecifiedPickupDirectory);

            foreach (var file in Directory.GetFiles(SpecifiedPickupDirectory, "*.eml"))
            {
                File.Delete(file);
            }
        }

        public SmtpClientWrapperTests()
        {
            _message = Tests.GenerateMailMessage();

            //_info = EmailInfo.Default;
            _info = SMTPInfo.Default;

            _info.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            _info.PickupDirectoryLocation = SpecifiedPickupDirectory;
            _info.UseDefaultCredentials = true;
        }

        [Test]
        public void Test_SmtpClientWrapper_Constructor()
        {
            //
            var expected = Tests.RandomSmtpInfo();

            // Arrange / Act
            var sut = SmtpClientWrapper.GetInstance(expected);

            // Assert
            Assert.That(sut.Host == expected.HostName);
            Assert.That(sut.Port == expected.Port);
            Assert.That(sut.UseDefaultCredentials == expected.UseDefaultCredentials);
            Assert.That(sut.DeliveryMethod == expected.DeliveryMethod);
            Assert.That(sut.EnableSsl == expected.EnableSsl);

            if (expected.UseDefaultCredentials)
            {
                Assert.That(sut.Credentials == CredentialCache.DefaultNetworkCredentials);
            }
            else
            {
                Assert.That(sut.Credentials == expected.Credentials);
            }
        }

        [Test, Explicit]
        public void Test_SmtpClientWrapper_SendAsync_Real()
        {
            // Arrange
            var wasCalled = false;
            _info.HostName = RealHostName;
            var sut = SmtpClientWrapper.GetInstance(_info);
            sut.SendCompleted += ((sender, e) => { wasCalled = true; });

            // Act
            sut.SendAsync(_message);
            Thread.Sleep(500);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.That(Directory.GetFiles(SpecifiedPickupDirectory, "*.eml").Length, Is.EqualTo(1));
        }

        [Test, Explicit]
        public void Test_SmtpClientWrapper_SendAsync_BadServerName()
        {
            // Arrange
            var wasCalled = false;
            _info.HostName = FakeHostName;
            var sut = SmtpClientWrapper.GetInstance(_info);
            sut.SendCompleted += ((sender, e) => { wasCalled = true; });

            // Act
            sut.SendAsync(_message);
            Thread.Sleep(500);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.That(Directory.GetFiles(SpecifiedPickupDirectory, "*.eml").Length, Is.EqualTo(1));
        }
    }
}