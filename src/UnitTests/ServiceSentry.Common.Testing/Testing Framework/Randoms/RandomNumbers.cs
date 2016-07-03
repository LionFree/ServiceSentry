// -----------------------------------------------------------------------
//  <copyright file="RandomInt.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        
        private static int RandomInt(int min = 0, int max = 65536)
        {
            return Randomizer.Next(min, max);
        }

        private static long RandomLong(long min = long.MinValue, long max = long.MaxValue)
        {
            var buf = new byte[8];
            Randomizer.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        private static double RandomDouble(double min = double.MinValue, double max = double.MaxValue)
        {
            return Randomizer.NextDouble() * (max - min) + min;
        }
    }
}