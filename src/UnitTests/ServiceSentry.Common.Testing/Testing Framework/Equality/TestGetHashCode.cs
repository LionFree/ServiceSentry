// -----------------------------------------------------------------------
//  <copyright file="Test_GetHashCode.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using NUnit.Framework;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    [TestFixture]
    public partial class Tests
    {
        public static void TestGetHashCode(object item1, object item2)
        {
            // If two distinct objects compare as equal, their hashcodes must be equal.
            if (item1.Equals(item2))
                Assert.That(item1.GetHashCode() == item2.GetHashCode());
        }
    }
}