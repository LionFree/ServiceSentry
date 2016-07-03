// -----------------------------------------------------------------------
//  <copyright file="GetNewInteger.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static int GetNewInteger(int oldValue)
        {
            var newValue = (new Random()).Next(10000);
            if (newValue == oldValue)
            {
                if (newValue == 0)
                {
                    newValue = 1;
                }
                else
                {
                    newValue = -newValue;
                }
            }
            return newValue;
        }
    }
}