// -----------------------------------------------------------------------
//  <copyright file="CommandLineParserTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using NUnit.Framework;
using ServiceSentry.Common.CommandLine;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.CommandLine
{
    [TestFixture]
    public class CommandLineParserTests
    {
        private readonly string[] _quotes;
        
        public CommandLineParserTests()
        {
            _quotes = CommandLineArgument.QuoteCharacters;
            Array.Resize(ref _quotes, CommandLineArgument.QuoteCharacters.Length + 1);
            _quotes[CommandLineArgument.QuoteCharacters.Length] = string.Empty;
        }

        [Test]
        public void CommandLineParser_Count()
        {
            // Arrange
            var expected = Tests.Random<int>(5, 25);
            var args = new string[expected];
            var paramArray = new string[expected];
            var valueArray = new string[expected];

            for (var i = 0; i < expected; i++)
            {
                paramArray[i] = Tests.Random<string>();
                valueArray[i] = Tests.Random<string>();
                var quote = Tests.RandomQuote();
                args[i] = string.Format("{0}{1}{2}{3}{4}{3}", Tests.RandomSwitch(), paramArray[i],
                                        Tests.RandomParamSymbol(), quote, valueArray[i]);
                //Debug.WriteLine(args[i]);
            }

            // Act
            var sut = CommandLineParser.GetInstance().Parse(args);

            // Assert
            var actual = sut.Length;
            Assert.AreEqual(expected, actual, "Actual number of parameters discovered does not match expected value.");
            Trace.WriteLine(string.Format("The parser counts {0} arguments, as expected.", expected));
        }

        [Test]
        public void CommandLineParser_ParseAndExpose()
        {
            // Arrange
            var expected = Tests.Random<int>(5, 25);
            var args = new string[expected];
            var switchArray = new string[expected];
            var paramArray = new string[expected];

            for (var i = 0; i < expected; i++)
            {
                switchArray[i] = Tests.Random<string>();
                paramArray[i] = Tests.Random<string>();
                var quote = Tests.RandomQuote();
                args[i] = string.Format("{0}{1}{2}{3}{4}{3}", Tests.RandomSwitch(), switchArray[i],
                                        Tests.RandomParamSymbol(), quote, paramArray[i]);
                //Debug.WriteLine(args[i]);
            }

            // Act
            var sut = CommandLineParser.GetInstance().Parse(args);
            
            // Assert
            for (int i = 0; i < expected; i++)
            {
                Assert.IsNotNull(sut[switchArray[i]],
                                 "The entry for switch '{0}' is null or empty.  It should be '{1}'.",
                                 switchArray[i], paramArray[i]);

                Assert.AreEqual(paramArray[i], sut[switchArray[i]],
                                "The parameter value for switch '{0}' should be '{1}', but is '{2}'.",
                                switchArray[i], paramArray[i], sut[switchArray[i]]);
            }

            Trace.WriteLine(string.Format("The parser parsed and exposed {0} arguments, as expected.", expected));
        }
    }
}