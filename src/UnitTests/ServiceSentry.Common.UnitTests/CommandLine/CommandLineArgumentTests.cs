// -----------------------------------------------------------------------
//  <copyright file="CommandLineArgumentTests.cs" company="Curtis Kaler">
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
    internal class CommandLineArgumentTests
    {
        private readonly string[] _quotes;
        private int _testsRun;

        public CommandLineArgumentTests()
        {
            _quotes = CommandLineArgument.QuoteCharacters;
            Array.Resize(ref _quotes, CommandLineArgument.QuoteCharacters.Length + 1);
            _quotes[CommandLineArgument.QuoteCharacters.Length] = string.Empty;
        }


        private void Test_Argument(string switchChar, string swtch)
        {
            Test_Argument(switchChar, swtch, string.Empty, string.Empty, string.Empty);
        }

        private void Test_Argument(string switchChar, string swtch, string paramChar, string param, string quoteChar)
        {
            // Arrange
            var isQuoted = (quoteChar != string.Empty);
            if (!isQuoted) param = param.Replace(" ", string.Empty);
            var arg = string.Format("{0}{1}{2}{3}{4}{3}", switchChar, swtch, paramChar, quoteChar, param);

            // Act
            //Trace.WriteLine(string.Format("Test_Argument: {0}", arg));
            var sut = CommandLineArgument.GetInstance(arg);

            // Assert
            Assert.AreEqual(isQuoted, sut.IsQuoted,
                            "The parameter should{0}have quotes, but does{1}.",
                            isQuoted ? " " : " not", isQuoted ? " not" : "");

            Assert.AreEqual(switchChar, sut.SwitchDelimiter,
                            "The switch delimiter should be '{0}', but is '{1}'.",
                            switchChar, sut.SwitchDelimiter);

            Assert.AreEqual(swtch, sut.Switch,
                            "The switch should be '{0}', but is '{1}'.",
                            swtch, sut.Switch);

            Assert.AreEqual(paramChar, sut.ParameterDelimiter,
                            "The parameter delimiter should be '{0}', but is '{1}'.",
                            paramChar, sut.ParameterDelimiter);


            Assert.AreEqual(param, sut.Parameter,
                            "The parameter should be '{0}', but is '{1}'.",
                            param, sut.Parameter);

            _testsRun++;
        }

        [Test]
        public void Test_CommandLineArgument()
        {
            foreach (var switchChar in CommandLineArgument.SwitchDelimiters)
            {
                Test_Argument(switchChar, Tests.Random<string>());

                foreach (var paramChar in CommandLineArgument.ParameterDelimiters)
                {
                    foreach (var quoteChar in _quotes)
                    {
                        Test_Argument(switchChar, Tests.Random<string>(), paramChar, Tests.RandomFilePath(), quoteChar);
                        Test_Argument(switchChar, Tests.Random<string>(), paramChar, Tests.Random<string>(), quoteChar);
                    }
                }
            }
            Trace.WriteLine(String.Format("All {0} tests passed.", _testsRun));
        }
    }
}