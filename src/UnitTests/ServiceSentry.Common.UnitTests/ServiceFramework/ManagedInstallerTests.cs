// -----------------------------------------------------------------------
//  <copyright file="ManagedInstallerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    internal class ManagedInstallerTests
    {
        private const string ReleaseFolder =
            @"C:\Users\curtis.kaler\Documents\Visual Studio 2012\Projects\ServiceSentry\Source\Build\Release\";
        private const string ServiceName = "ServiceSentry.Monitor.exe";

        [Test, Explicit("Installs service")]
        public void Test_ManagedInstaller_Install()
        {
            var args = new []
                {
                    ReleaseFolder + ServiceName
                };


            var sut = ManagedInstaller.Default;
            sut.InstallHelper(args);
        }


        [Test, Explicit("Installs service")]
        public void Test_ManagedInstaller_Uninstall()
        {
            var args = new[]
                {
                    "/uninstall",
                    ReleaseFolder + ServiceName
                };


            var sut = ManagedInstaller.Default;
            sut.InstallHelper(args);
        }
    }
}