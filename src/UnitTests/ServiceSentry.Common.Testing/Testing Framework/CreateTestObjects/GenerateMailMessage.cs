// -----------------------------------------------------------------------
//  <copyright file="GenerateMailMessage.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Net.Mail;
using System.Text;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static MailMessage GenerateMailMessage()
        {
            var message = new MailMessage
                {
                    From = new MailAddress("NoReply@ServiceSentry.Curtis Kaler.com", "Service Sentry", Encoding.UTF8),
                    Sender = new MailAddress("curtis.kaler@Curtis Kaler.com", "Service Sentry", Encoding.UTF8),
                    Subject = "Test Email",
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = Random<bool>(),
                    Priority = MailPriority.High,
                };
            message.To.Add(new MailAddress("curtis.kaler@Curtis Kaler.com"));

            return message;
        }
    }
}