// -----------------------------------------------------------------------
//  <copyright file="ManagedInstaller.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Configuration.Install;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    internal abstract class ManagedInstaller
    {
        internal static ManagedInstaller Default
        {
            get { return new ManagedInstallerImplementation(); }
        }

        public abstract void InstallHelper(string[] args);

        private sealed class ManagedInstallerImplementation : ManagedInstaller
        {
            public override void InstallHelper(string[] args)
            {
                ManagedInstallerClass.InstallHelper(args);
            }
        }
    }
}