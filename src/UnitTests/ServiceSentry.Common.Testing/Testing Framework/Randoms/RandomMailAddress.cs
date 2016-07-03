// -----------------------------------------------------------------------
//  <copyright file="RandomMailAddress.cs" company="Curtis Kaler">
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
        public static MailAddress RandomMailAddress()
        {
            //var array = Enum.GetValues(typeof (Encoding));
            //var random = new Random((int) DateTime.Now.Ticks);
            //var encoding = (Encoding) array.GetValue(random.Next(array.Length));


            return new MailAddress(RandomAddress(), Random<string>(), Encoding.UTF8);
        }
    }
}