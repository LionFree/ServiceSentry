// -----------------------------------------------------------------------
//  <copyright file="RandomFolder.cs" company="Curtis Kaler">
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
        public static string RandomFolder()
        {
            var root = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            var subDirs = root.GetDirectories();

            var directory = Randomizer.Next(subDirs.Length);
            var randomDirectory = subDirs[directory];

            return randomDirectory.FullName;
        }
    }
}