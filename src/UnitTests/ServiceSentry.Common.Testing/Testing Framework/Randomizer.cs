// -----------------------------------------------------------------------
//  <copyright file="RandomNumberGenerator.cs" company="Curtis Kaler">
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
        public static readonly Random Randomizer = new Random((int) DateTime.Now.Ticks);
    }
}