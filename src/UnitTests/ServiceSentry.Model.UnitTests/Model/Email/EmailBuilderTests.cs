// -----------------------------------------------------------------------
//  <copyright file="EmailBuilderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
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
    internal class EmailBuilderTests
    {
        private static string CreateNewAttachment()
        {
            var fileName = Path.GetRandomFileName();
            var path = Path.Combine(Environment.CurrentDirectory, fileName);

            using (var outStream = new StreamWriter(fileName))
            {
                outStream.WriteLine("Hello World");
                outStream.Close();
            }

            return path;
        }

        private static void DeleteFiles(IEnumerable<string> filenames)
        {
            foreach (var item in filenames)
            {
                File.Delete(item);
            }

            Trace.WriteLine("Dummy files deleted.");
        }

        private List<string> GetFileNames(ObservableCollection<ExternalFile> files)
        {
            var output = new List<string>();
            for (var i = 0; i < files.Count; i++)
            {
                output.Add(Path.GetFileName(files[i].ParsedName));
            }
            return output;
        }

        private ObservableCollection<ExternalFile> GetExternalFiles(bool shouldFilesExist)
        {
            var numFiles = Tests.Random<int>(1, 5);
            var logFiles = new ObservableCollection<ExternalFile>();
            for (var i = 0; i < numFiles; i++)
            {
                var fullPath = shouldFilesExist ? CreateNewAttachment() : Path.GetRandomFileName();
                var file = new Mock<ExternalFile>();
                file.Setup(m => m.ParsedName).Returns(fullPath);
                file.Setup(m => m.Exists).Returns(shouldFilesExist);
                logFiles.Add(file.Object);
            }
            return logFiles;
        }


        [Test]
        public void Test_EB_AddCCRecipient()
        {
            // Arrange
            var newAddress = Tests.RandomAddress();
            var info = EmailInfo.Default;
            var expectedCount = info.CC.Count + 1;
            var sut = EmailBuilder.Default;

            // Act
            sut.AddCCRecipient(info, newAddress);

            // Assert
            Assert.AreEqual(expectedCount, info.CC.Count);
            Assert.AreEqual(newAddress, info.CC[info.CC.Count - 1]);
        }

        [Test]
        public void Test_EB_AddToRecipient()
        {
            // Arrange
            var newAddress = Tests.RandomAddress();
            var info = EmailInfo.Default;
            var expectedCount = info.To.Count + 1;
            var sut = EmailBuilder.Default;

            // Act
            sut.AddToRecipient(info, newAddress);

            // Assert
            Assert.AreEqual(expectedCount, info.To.Count);
            Assert.AreEqual(newAddress, info.To[info.To.Count - 1]);
        }

        [Test]
        public void Test_EB_AttachLogs()
        {
            var message = new MailMessage();
            var sut = EmailBuilder.Default;
            var logFiles = GetExternalFiles(true);
            var fileNames = GetFileNames(logFiles);

            var packet = new SubscriptionPacket {LogFiles = logFiles};

            // Act
            try
            {
                sut.AttachLogs(ref message, packet);

                var actual = message.Attachments;
                foreach (var item in actual)
                {
                    Assert.That(fileNames.Contains(item.Name));
                }
            }
            finally
            {
                // The files are still being used by the attachments.
                // "Dispose of them."
                message.Dispose();
                DeleteFiles(fileNames);
            }
        }

        [Test, Explicit("Tests email formatting.")]
        public void Test_EB_GetServiceFailureMessageBody()
        {
            Assert.Fail("Run the program and send an email.  Compare against desired result.");
        }

        [Test]
        public void Test_EB_NewMailMessage()
        {
            //var from = new MailAddress("NoReply@Curtis Kaler.ServiceSentry.com", "Service Sentry", Encoding.UTF8);

            // Arrange
            var numRecips = Tests.Random<int>(0, 10);
            var recipients = new MailAddress[numRecips];
            for (var i = 0; i < numRecips; i++)
            {
                recipients[i] = new MailAddress(Tests.RandomAddress());
            }
            var appName = Strings._ApplicationName;

            var emailInfo = EmailInfo.Default;
            var senderAddress = Tests.RandomAddress();
            var encoding = Encoding.UTF8;
            var sender = new MailAddress(senderAddress, appName, encoding);
            var isBodyHtml = Tests.Random<bool>();
            var priority = Tests.Random<MailPriority>();
            var fromAddress = Tests.RandomAddress();
            var from = new MailAddress(fromAddress, appName, encoding);

            emailInfo.IsBodyHtml = isBodyHtml;
            emailInfo.Priority = priority;
            emailInfo.From = fromAddress;
            emailInfo.BodyEncoding = encoding.WebName;
            foreach (var item in recipients)
            {
                emailInfo.To.Add(item.Address);
            }
            emailInfo.SenderAddress = senderAddress;
            
            var sut = EmailBuilder.Default;

            // Act
            var actual = sut.NewMailMessage(emailInfo);

            // Assert
            Assert.AreEqual(isBodyHtml, actual.IsBodyHtml,
                            "IsBodyHtml: expected {0}, was {1}.",
                            isBodyHtml, actual.IsBodyHtml);

            Assert.AreEqual(sender, actual.Sender,
                            "Sender: expected {0}, was {1}.",
                            sender, actual.Sender);

            Assert.AreEqual(priority, actual.Priority,
                            "Priority: expected {0}, was {1}.",
                            priority, actual.Priority);

            Assert.AreEqual(from, actual.From,
                            "From: expected {0}, was {1}.",
                            from, actual.From);

            Assert.AreEqual(encoding, actual.BodyEncoding,
                            "BodyEncoding: expected {0}, was {1}.",
                            encoding, actual.BodyEncoding);

            Assert.AreEqual(recipients, actual.To,
                            "To: expected {0}, was {1}.",
                            recipients, actual.To);

        }
    }
}