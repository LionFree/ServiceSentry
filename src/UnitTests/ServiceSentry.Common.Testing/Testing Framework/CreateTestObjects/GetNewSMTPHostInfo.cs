// -----------------------------------------------------------------------
//  <copyright file="GetNewSMTPHostInfo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using ServiceSentry.Common.Email;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static SMTPInfo GetNewSMTPHost()
        {
            var output = SMTPInfo.Default;
            output.HostName = Path.GetRandomFileName();
            output.Port = (new Random()).Next(10000);
            return output;
        }
    }
}