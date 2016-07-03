// -----------------------------------------------------------------------
//  <copyright file="ConsoleHarnessTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using NUnit.Framework;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.ServiceFramework
{
    [TestFixture]
    internal class ConsoleHarnessTests
    {
        private readonly Mock<AssemblyWrapper> _assembly;
        private readonly Mock<ConsoleWrapper> _console;
        private readonly ConsoleColor _color;
        private readonly string _text;

        public ConsoleHarnessTests()
        {
            _assembly = new Mock<AssemblyWrapper>();
            _console = new Mock<ConsoleWrapper>();
            _color = Tests.Random<ConsoleColor>();
            _text = Tests.Random<string>();
        }
        
        [Test]
        public void Test_WriteLine_WithColor()
        {
            // Arrange
            var wasCalled = false;
            var color = Tests.Random<ConsoleColor>();
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Callback(() => wasCalled = true);

            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            sut.WriteLine(color, string.Empty, new object[] {string.Empty});

            // Assert
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_WriteToConsole_WithColor()
        {
            // Arrange
            var wasCalled = false;
            var color = Tests.Random<ConsoleColor>();
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Callback(() => wasCalled = true);

            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            sut.WriteToConsole(color, string.Empty, new object[] { string.Empty });

            // Assert
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void ManualTest_Write_WithColor()
        {
            // Arrange
            var wasCalled = false;
            var color = Tests.Random<ConsoleColor>();
            _console.Setup(m => m.Write(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Callback(() => wasCalled = true);

            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            sut.Write(color, string.Empty, new object[] { string.Empty });

            // Assert
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_ReadKey_DisplayOnConsole()
        {
            // Arrange
            var expected = new ConsoleKeyInfo(Tests.Random<char>(), Tests.Random<ConsoleKey>(),
                Tests.Random<bool>(), Tests.Random<bool>(), Tests.Random<bool>());

            _console.Setup(m => m.ReadKey(It.IsAny<bool>())).Returns(expected);
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            var actual = sut.ReadKey(true);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ManualTest_ReadKey_DoNot_DisplayOnConsole()
        {
            // Arrange
            var expected = new ConsoleKeyInfo(Tests.Random<char>(), Tests.Random<ConsoleKey>(),
                Tests.Random<bool>(), Tests.Random<bool>(), Tests.Random<bool>());

            _console.Setup(m => m.ReadKey(It.IsAny<bool>())).Returns(expected);
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            var actual = sut.ReadKey(false);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ReadLine()
        {
            // Arrange
            var expected = Tests.Random<string>();
            _console.Setup(m => m.ReadLine()).Returns(expected);
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            var actual = sut.ReadLine();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Run_StartStopShutdown()
        {
            // Arrange
            var started = false;
            var stopped = false;
            var shutdown = false;
            var mock = new Mock<WindowsService>();

            _console.Setup(m => m.ReadKey(It.IsAny<bool>())).Returns(new ConsoleKeyInfo('Q', ConsoleKey.Q, false, false, false));

            // The service must start, stop, and shutdown
            mock.Setup(m => m.OnStart(It.IsAny<string[]>()))
                .Callback(() => { started = true; });
            mock.Setup(m => m.OnStop()).Callback(() => { stopped = true; });
            mock.Setup(m => m.OnShutdown()).Callback(() => { shutdown = true; });

            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            // Act
            sut.Run(null, mock.Object);

            // Assert
            Assert.IsTrue(started);
            Assert.IsTrue(stopped);
            Assert.IsTrue(shutdown);
        }

        [Test]
        public void Test_WriteLine_BlankLine()
        {
            var text = string.Empty;
            
            var wasCalled = false;
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);
            
            sut.WriteLine(text);

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_WriteLine_Int()
        {
            var wasCalled = false;
            var integer = Tests.Random<int>();
            
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback(() => { wasCalled = true; });
            
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.WriteLine(integer);

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_WriteLine_Text()
        {
            var text = Tests.Random<string>();

            var called = false;
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { called = true; });
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);
            
            sut.WriteLine(text);

            Assert.IsTrue(called);
        }

        [Test]
        public void Test_WriteLine_TextAndFinalColor()
        {
            var wasCalled = false;
            var colorSet = false;
            var originalColor = _console.Object.ForegroundColor;
            var colors = new List<ConsoleColor> {originalColor};
            
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            _console.Setup(m => m.ForegroundColor).Callback(() =>
                {
                    colorSet = true;
                });
            _console.SetupSet(m => m.ForegroundColor = It.IsAny<ConsoleColor>())
                    .Callback((ConsoleColor color) => colors.Add(color));

            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.WriteLine(_color, _text);

            Assert.IsTrue(wasCalled);
            Assert.IsTrue(colorSet);
            Assert.AreEqual(3, colors.Count);
            Assert.AreEqual(_color, colors[1]);
        }

        [Test]
        public void Test_WriteToConsole_Text()
        {
            var wasCalled = false;
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.WriteToConsole(_text);

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_WriteToConsole_TextAndFinalColor()
        {
            var wasCalled = false;
            var colorSet = false;
            var originalColor = _console.Object.ForegroundColor;
            var colors = new List<ConsoleColor> { originalColor };
            
            _console.Setup(m => m.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            _console.Setup(m => m.ForegroundColor).Callback(() =>
            {
                colorSet = true;
            });
            _console.SetupSet(m => m.ForegroundColor = It.IsAny<ConsoleColor>())
                    .Callback((ConsoleColor color) => colors.Add(color));
            
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.WriteToConsole(_color, _text);

            Assert.IsTrue(wasCalled);
            Assert.IsTrue(colorSet);
            Assert.AreEqual(3, colors.Count);
            Assert.AreEqual(_color, colors[1]);
        }

        [Test]
        public void Test_Write_Int()
        {
            var expected = Tests.Random<int>().ToString(CultureInfo.InvariantCulture);
            var wasCalled = false;
            _console.Setup(m => m.Write(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.Write(expected);

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_Write_Text()
        {
            var wasCalled = false;
            _console.Setup(m => m.Write(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.Write(_text);

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Test_Write_TextAndFinalColor()
        {
            var wasCalled = false;
            var colorSet = false;
            var originalColor = _console.Object.ForegroundColor;
            var colors = new List<ConsoleColor> { originalColor };
            
            _console.Setup(m => m.Write(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => { wasCalled = true; });
            _console.Setup(m => m.ForegroundColor).Callback(() =>
            {
                colorSet = true;
            });
            _console.SetupSet(m => m.ForegroundColor = It.IsAny<ConsoleColor>())
                    .Callback((ConsoleColor color) => colors.Add(color));
            
            var sut = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);

            sut.Write(_color,_text);

            Assert.IsTrue(wasCalled);
            Assert.IsTrue(colorSet);
            Assert.AreEqual(3, colors.Count);
            Assert.AreEqual(_color, colors[1]);
        }
    }
}