// -----------------------------------------------------------------------
//  <copyright file="InjectAsteriskIntoString.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using NUnit.Framework;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static string InjectAsteriskIntoString(string filename)
        {
            var extension = Path.GetExtension(filename);
            var shortFilename = Path.GetFileNameWithoutExtension(filename);
            if (shortFilename == null) Assert.Fail();

            var randIndex = (new Random()).Next(shortFilename.Length);
            return shortFilename.Substring(0, randIndex) + "*" +
                   shortFilename.Substring(randIndex + 1) + extension;
        }
    }
}