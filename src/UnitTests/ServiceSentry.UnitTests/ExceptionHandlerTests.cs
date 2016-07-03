// -----------------------------------------------------------------------
//  <copyright file="ExceptionHandlerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.Windows;
using Moq;
using NUnit.Framework;
using ServiceSentry.Client.Infrastructure;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.UnitTests
{
    [TestFixture]
    internal class ExceptionHandlerTests
    {
        [Test]
        public void Test_BindingErrorListener_GetInstance_with_action()
        {
            // Arrange
            var action = new Action<string>(s => Trace.WriteLine(s));

            // Act
            var sut = BindingErrorListener.GetInstance(action);

            // Assert
            var actual = sut.LogAction;
            Assert.AreEqual(action, actual);
        }

        [Test]
        public void Test_BindingErrorListener_GetInstance_without_action()
        {
            // Arrange
            // Act
            var sut = BindingErrorListener.GetInstance();

            // Assert
            Assert.IsNull(sut.LogAction);
        }

        [Test]
        public void Test_BindingErrorListener_Listen()
        {
            // Arrange
            var action = new Action<string>(s => Trace.WriteLine(s));
            var sut = BindingErrorListener.GetInstance();

            // Act
            sut.Listen(action);

            // Assert
            var actual = (BindingErrorListener) PresentationTraceSources.DataBindingSource.Listeners[1];
            Assert.AreEqual(action, actual.LogAction);
        }

        [Test, Explicit("It either works or it doesn't.")]
        public void Test_ExceptionHandler_AttachExceptionHandler()
        {
            Assert.Fail("Check that this is working.");
        }

        [Test, Explicit("Needs to be refactored into usefulness.")]
        public void Test_ExceptionHandler_LogException()
        {
            Assert.Fail("Needs to be refactored to usefulness.");
        }

        [Test]
        public void Test_ExceptionHandler_DefaultConstructor()
        {
            // Arrange
            ExceptionHandler sut = null;

            // Act // Assert
            Assert.DoesNotThrow(() => { sut = ExceptionHandler.GetInstance(); });
            Assert.IsNotNull(sut);
        }

        [Test]
        public void Test_ExceptionHandler_SpecialConstructor()
        {
            // Arrange
            ExceptionHandler sut = null;
            var listener = new Mock<BindingErrorListener>();
            var dialogs = new Mock<Dialogs>();

            // Act // Assert
            Assert.DoesNotThrow(() => { sut = ExceptionHandler.GetInstance(listener.Object, dialogs.Object); });
            Assert.IsNotNull(sut);
        }
        
        [Test, STAThread]
        public void Test_ExceptionHandler_ReportException()
        {
            // Arrange
            var errorShown = false;
            var listener = new Mock<BindingErrorListener>();
            var dialogs = new Mock<Dialogs>();
            var window = new Window();
            var exception = new ApplicationException(Tests.Random<string>());
            dialogs.Setup(m => m.ShowError(It.IsAny<string>(), window)).Callback(() => { errorShown = true; });
            var sut = ExceptionHandler.GetInstance(listener.Object, dialogs.Object);

            // Act
            sut.ReportException(exception, window);

            // Assert
            Assert.IsTrue(errorShown);
        }

        [Test]
        public void Test_PromoteDataBindingErrors()
        {
            // Arrange
            var wasCalled = false;
            var listener = new Mock<BindingErrorListener>();
            var dialogs = Dialogs.GetInstance();
            Action<string> expected = dialogs.ShowError;

            listener.Setup(m => m.Listen(expected)).Callback(() => { wasCalled = true; });
            var sut = ExceptionHandler.GetInstance(listener.Object, dialogs);

            // Act
            sut.PromoteDataBindingErrors();

            // Assert
            Assert.IsTrue(wasCalled);
        }
    }
}