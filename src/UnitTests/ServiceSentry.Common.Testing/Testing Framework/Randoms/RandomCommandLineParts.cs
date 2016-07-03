// -----------------------------------------------------------------------
//  <copyright file="RandomCommandLineParts.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Linq;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        private static readonly string[] ParamSymbols = { " ", "=", ":" };
        private static readonly string[] SwitchSymbols = { "-", "/", "--" };
        private static readonly string[] QuoteSymbols = { "\"", "'" };

        public static string RandomParamSymbol()
        {
            return ParamSymbols[Randomizer.Next(0, ParamSymbols.Count())];
        }

        public static string RandomSwitch()
        {
            return SwitchSymbols[Randomizer.Next(0, SwitchSymbols.Count())];
        }

        public static string RandomQuote()
        {
            return QuoteSymbols[Randomizer.Next(0, QuoteSymbols.Count())];
        }
    }
}