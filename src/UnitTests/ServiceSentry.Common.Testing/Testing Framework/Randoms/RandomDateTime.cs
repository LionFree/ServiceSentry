// -----------------------------------------------------------------------
//  <copyright file="RandomDateTime.cs" company="Curtis Kaler">
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
        private static DateTime RandomDateTime()
        {
            var start = new DateTime(1995, 1, 1);
            
            var range = (DateTime.Today - start).Days;
            
            var output = start.AddDays(Randomizer.Next(range));
            output = output.AddHours(Randomizer.Next(24));
            output = output.AddMinutes(Randomizer.Next(60));
            output = output.AddSeconds(Randomizer.Next(60));
            output = output.AddMilliseconds(Randomizer.Next(1000));

            return output;
        }
    }
}