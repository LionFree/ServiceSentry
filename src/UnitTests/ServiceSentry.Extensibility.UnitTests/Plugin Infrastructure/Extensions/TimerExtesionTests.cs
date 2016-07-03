// -----------------------------------------------------------------------
//  <copyright file="TimerExtesionTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Windows.Threading;
using NUnit.Framework;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Extensions
{
    [TestFixture]
    internal class TimerExtesionTests
    {
        internal class FakeTimerExtension : TimerExtension
        {
            public override string ExtensionName
            {
                get { throw new NotImplementedException(); }
            }

            public override DispatcherTimer Timer(Logger logger)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Test_Defaults()
        {
            // Arrange
            var expected = TimeSpan.FromSeconds(.5);

            // Act
            var pdq = new FakeTimerExtension();

            // Assert
            Assert.That(pdq != null);
            Assert.IsFalse(pdq.CanExecute);
        }
    }
}