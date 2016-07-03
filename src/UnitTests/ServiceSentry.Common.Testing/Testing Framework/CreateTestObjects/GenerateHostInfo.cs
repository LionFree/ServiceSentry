// -----------------------------------------------------------------------
//  <copyright file="GenerateHostInfo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using ServiceSentry.Common.Email;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static SMTPInfo GenerateHostInfo()
        {
            var output = SMTPInfo.Default;
            output.HostName = "exch1-colo.Curtis Kaler.com";
            output.Port = (new Random((int) DateTime.Now.Ticks)).Next(10000);
            //output.SenderAddress = RandomAddress();
            return output;
        }
    }
}