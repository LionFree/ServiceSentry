// -----------------------------------------------------------------------
//  <copyright file="GetDifferentMailPriority.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Net.Mail;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static MailPriority GetDifferentMailPriority(MailPriority current)
        {
            var array = Enum.GetValues(typeof (MailPriority));

            var num = (int) current;
            var newNum = (num + 1)%array.Length;

            return (MailPriority) array.GetValue(newNum);
        }
    }
}