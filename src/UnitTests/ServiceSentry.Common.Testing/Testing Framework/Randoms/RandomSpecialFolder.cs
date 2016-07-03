// -----------------------------------------------------------------------
//  <copyright file="GetRandomSpecialFolder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static Environment.SpecialFolder RandomSpecialFolder()
        {
            var validFolders = new List<Environment.SpecialFolder>
                {
                    Environment.SpecialFolder.ProgramFiles,
                    Environment.SpecialFolder.UserProfile,
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolder.Windows,
                };

            return validFolders[Randomizer.Next(validFolders.Count)];
        }
    }
}