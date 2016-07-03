// -----------------------------------------------------------------------
//  <copyright file="RandomString.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

#endregion

using System.Diagnostics;

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        [DebuggerStepThrough]
        private static string RandomString()
        {
            const int fixedLength = 10;
            return RandomString(fixedLength);
        }

        [DebuggerStepThrough]
        private static string RandomString(int length)
        {
            var buf = new char[length];
            for (var i = 0; i < length; i++)
            {
                var index = Randomizer.Next(AlphaNumeric.Length);
                buf[i] = AlphaNumeric[index];
            }

            return new string(buf);
        }
    }
}