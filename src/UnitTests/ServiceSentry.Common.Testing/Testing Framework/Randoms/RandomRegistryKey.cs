// -----------------------------------------------------------------------
//  <copyright file="RandomRegistryKey.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using Microsoft.Win32;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static RegistryKey RandomRegistryKey()
        {
            var list = new List<RegistryKey>
                {
                    Registry.ClassesRoot,
                    Registry.CurrentConfig,
                    Registry.CurrentUser,
                    Registry.LocalMachine,
                    Registry.PerformanceData,
                    Registry.Users
                };

            return list[Randomizer.Next(list.Count)];
        }
    }
}