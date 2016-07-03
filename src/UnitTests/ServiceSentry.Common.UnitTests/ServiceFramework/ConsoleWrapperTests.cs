// -----------------------------------------------------------------------
//  <copyright file="ConsoleWrapperTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    public class ConsoleWrapperTests
    {
        [TearDown]
        public void TearDown()
        {
            // Reset the standard output stream.
            var standardOut = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
            Console.SetOut(standardOut);
        }

        private readonly ConsoleWrapper _sut;


        public ConsoleWrapperTests()
        {
            _sut = ConsoleWrapper.Default;
        }

        [Test]
        public void Test_ReadLine()
        {
            var expected = Tests.Random<string>();

            using (var sr = new StringReader(expected))
            {
                Console.SetIn(sr);

                // Act
                var actual = _sut.ReadLine();

                // Assert
                Assert.AreEqual(expected, actual);
            }
        }

        //[Test]
        //public void Test_ConsoleWrapper_ForegroundColor()
        //{

        //}

        [Test]
        public void Test_Write()
        {
            using (var sw = new StringWriter())
            {
                var expectedText = Tests.Random<int>().ToString(CultureInfo.InvariantCulture);

                // Arrange
                Console.SetOut(sw);

                // Act
                _sut.Write(expectedText);

                // Assert
                var actualText = sw.ToString();
                Assert.AreEqual(expectedText, actualText);
            }
        }

        [Test]
        public void Test_WriteLine()
        {
            var text = Tests.Random<string>();
            var expected = text + Environment.NewLine;

            using (var sw = new StringWriter())
            {
                // Arrange
                Console.SetOut(sw);

                // Act
                _sut.WriteLine(text);

                // Assert
                var actual = sw.ToString();
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Test_WriteLine_Text()
        {
            var text = Tests.Random<string>();
            var expected = text + Environment.NewLine;

            using (var sw = new StringWriter())
            {
                // Arrange
                Console.SetOut(sw);

                // Act
                _sut.WriteLine(text);

                // Assert
                var actual = sw.ToString();
                Assert.AreEqual(expected, actual);
            }
        }
    }
}