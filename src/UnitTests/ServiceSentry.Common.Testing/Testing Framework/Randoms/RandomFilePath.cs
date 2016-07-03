// -----------------------------------------------------------------------
//  <copyright file="GetRandomFilePath.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static string RandomFilePath()
        {
            return Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
        }
    }
}