// -----------------------------------------------------------------------
//  <copyright file="TypeExtensionTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Linq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;

#endregion

namespace ServiceSentry.ServiceFramework.UnitTests
{
    [TestFixture]
    internal class TypeExtensionTests
    {
        
        [WindowsService(ExpectedName)]
        private class TestClass
        {
        }

        private const string ExpectedName = "Expected Service Name";
        private readonly Type _type = typeof(TestClass);

        [Test]
        public void Test_GetAttribute()
        {
            // Act
            var attributes = _type.GetAttribute<WindowsServiceAttribute>();


            // Assert
            Assert.IsNotNull(attributes);
            var actual = attributes.ServiceName;

            Assert.AreEqual(ExpectedName, actual);
        }

        [Test]
        public void Test_GetAttributes()
        {
            // Act
            var attributes = _type.GetAttributes<WindowsServiceAttribute>();

            // Assert
            Assert.IsNotNull(attributes);

            var attribute = attributes.FirstOrDefault();
            Assert.IsNotNull(attribute);

            var actual = attribute.ServiceName;
            Assert.AreEqual(ExpectedName, actual);
        }
    }
}