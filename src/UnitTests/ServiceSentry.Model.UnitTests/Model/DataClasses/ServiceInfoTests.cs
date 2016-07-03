// -----------------------------------------------------------------------
//  <copyright file="ServiceInfoTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.IO;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses
{
    [TestFixture]
    internal class ServiceInfoTests
    {
        [Test]
        public void Test_ServiceInfo_DisplayName_PropertyChanged()
        {
            var sut = ServiceInfo.GetInstance();
            var oldValue = sut.DisplayName;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.DisplayName).When(s => s.DisplayName = oldValue);
            sut.ShouldNotifyOn(s => s.DisplayName).When(s => s.DisplayName = newValue);
            Assert.That(sut.DisplayName == newValue);
        }

        [Test]
        public void Test_ServiceInfo_Selected_PropertyChanged()
        {
            var sut = ServiceInfo.GetInstance();
            var oldValue = sut.Selected;
            var newValue = !oldValue;

            sut.ShouldNotNotifyOn(s => s.Selected).When(s => s.Selected = oldValue);
            sut.ShouldNotifyOn(s => s.Selected).When(s => s.Selected = newValue);
            Assert.That(sut.Selected == newValue);
        }

        [Test]
        public void Test_ServiceInfo_ServiceName_PropertyChanged()
        {
            var sut = ServiceInfo.GetInstance();
            var oldValue = sut.ServiceName;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.ServiceName).When(s => s.ServiceName = oldValue);
            sut.ShouldNotifyOn(s => s.ServiceName).When(s => s.ServiceName = newValue);
            Assert.That(sut.ServiceName == newValue);
        }
    }
}