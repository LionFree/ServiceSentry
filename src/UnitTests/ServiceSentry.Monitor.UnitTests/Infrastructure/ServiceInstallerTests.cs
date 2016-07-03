// -----------------------------------------------------------------------
//  <copyright file="ServiceInstallerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using NUnit.Framework;
using ServiceSentry.Monitor.Infrastructure;

#endregion

namespace ServiceSentry.Monitor.UnitTests.Infrastructure
{
    [TestFixture]
    internal class MonitorServiceInstallerTests
    {
        [Test]
        public void Test_MonitorServiceInstaller()
        {
            var sut = new MonitorServiceInstaller();
            Assert.IsNotNull(sut.Installers);
            Assert.AreEqual(2, sut.Installers.Count);
        }
    }
}