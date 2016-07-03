// -----------------------------------------------------------------------
//  <copyright file="RandomSmtpInfo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Net;
using System.Net.Mail;
using ServiceSentry.Common.Email;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static SMTPInfo RandomSmtpInfo()
        {
            var output = SMTPInfo.Default;
            output.HostName = "exch1-colo.Curtis Kaler.net";
            output.HostName = RandomAddress();
            output.DeliveryMethod = Random<SmtpDeliveryMethod>();
            output.PickupDirectoryLocation = Environment.CurrentDirectory;
            output.UseDefaultCredentials = Random<bool>();
            output.Port = RandomInt(1, 10000);

            if (!output.UseDefaultCredentials)
            {
                output.Credentials = new NetworkCredential(Random<string>(), Random<string>());
            }
            return output;
        }
    }
}