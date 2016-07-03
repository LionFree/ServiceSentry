// -----------------------------------------------------------------------
//  <copyright file="RandomChar.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static char RandomChar()
        {
            var chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();
            var r = new Random();
            var i = r.Next(chars.Length);
            return chars[i];
        }
    }
}