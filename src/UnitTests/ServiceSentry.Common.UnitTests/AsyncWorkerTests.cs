// -----------------------------------------------------------------------
//  <copyright file="AsyncWorkerTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.Asynchrony
{
    [TestFixture, Explicit("Slow running tests.")]
    internal class AsyncWorkerTests
    {
        [SetUp]
        public void SetUp()
        {
            _worker = AsyncWorker.Default;
        }

        private AsyncWorker _worker;

        [Test]
        public void Test_ReportProgress_without_ProgressChanging()
        {
            // Arrange
            var arguments = new List<object> {Tests.Random<string>(), Tests.Random<int>(), Tests.Random<bool>()};
            var percentage = Tests.Random<int>(0, 100);


            // Act
            _worker.Run(
                (s, e) =>
                    {
                        Assert.Throws<InvalidOperationException>(() => _worker.ReportProgress(percentage),
                                                                 "FAIL: Failed to throw the expected exception.");
                        Trace.WriteLine("PASS: Threw the expected exception.");
                    },
                (s, e) =>
                    {
                        // OnComplete
                    },
                arguments);


            // Assert
            Thread.Sleep(200);
        }

        [Test]
        public void Test_Run_DoWork_noArgs()
        {
            // Arrange
            var didWork = false;
            var goodArgs = false;
            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == null);
                    });

            // Assert
            Thread.Sleep(200);
            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");


            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");
        }

        [Test]
        public void Test_Run_DoWork_onComplete_noArgs()
        {
            // Arrange
            var didWork = false;
            var completed = false;
            var goodArgs = false;

            // Act
            _worker.Run((s, e) =>
                {
                    // DoWork
                    didWork = true;
                    goodArgs = (e.Argument == null);
                }, (s, e) =>
                    {
                        // OnComplete
                        completed = true;
                    });

            // Assert
            Thread.Sleep(200);
            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(completed);
            Trace.WriteLine("Completion reported.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");
        }

        [Test]
        public void Test_Run_DoWork_onComplete_progress_noArgs_noState()
        {
            // Arrange

            var didWork = false;
            var completed = false;
            var goodArgs = false;
            var progress = false;
            var goodState = false;
            var goodPercentage = false;
            var percentage = Tests.Random<int>(0, 100);

            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == null);
                        _worker.ReportProgress(percentage);
                    },
                (s, e) =>
                    {
                        // OnComplete
                        completed = true;
                    },
                (s, e) =>
                    {
                        // OnProgressChanged
                        progress = true;
                        goodPercentage = (e.ProgressPercentage == percentage);
                        goodState = (e.UserState == null);
                    });


            // Assert
            Thread.Sleep(200);
            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(completed);
            Trace.WriteLine("Completion reported.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");

            Assert.IsTrue(progress);
            Trace.WriteLine("Progress reported.");

            Assert.IsTrue(goodPercentage);
            Trace.WriteLine("Percentage good.");

            Assert.IsTrue(goodState);
            Trace.WriteLine("State good.");
        }

        [Test]
        public void Test_Run_DoWork_onComplete_progress_noArgs_withState()
        {
            // Arrange

            var didWork = false;
            var completed = false;
            var goodArgs = false;
            var progress = false;
            var goodState = false;
            var goodPercentage = false;
            var state = Tests.Random<string>();
            var percentage = Tests.Random<int>(0, 100);

            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == null);
                        _worker.ReportProgress(percentage, state);
                    },
                (s, e) =>
                    {
                        // OnComplete
                        completed = true;
                    },
                (s, e) =>
                    {
                        // OnProgressChanged
                        progress = true;
                        goodPercentage = (e.ProgressPercentage == percentage);
                        goodState = (e.UserState.ToString() == state);
                    });


            // Assert
            Thread.Sleep(200);
            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(completed);
            Trace.WriteLine("Completion reported.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");

            Assert.IsTrue(progress);
            Trace.WriteLine("Progress reported.");

            Assert.IsTrue(goodPercentage);
            Trace.WriteLine("Percentage good.");

            Assert.IsTrue(goodState);
            Trace.WriteLine("State good.");
        }

        [Test]
        public void Test_Run_DoWork_onComplete_progress_withArgs_noState()
        {
            // Arrange

            var didWork = false;
            var completed = false;
            var goodArgs = false;
            var progress = false;
            var goodState = false;
            var goodPercentage = false;
            var arguments = new List<object> {Tests.Random<string>(), Tests.Random<int>(), Tests.Random<bool>()};
            var percentage = Tests.Random<int>(0, 100);


            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == arguments);
                        _worker.ReportProgress(percentage);
                    },
                (s, e) =>
                    {
                        // OnComplete
                        completed = true;
                    },
                (s, e) =>
                    {
                        // OnProgressChanged
                        progress = true;
                        goodPercentage = (e.ProgressPercentage == percentage);
                        goodState = (e.UserState == null);
                    },
                arguments);


            // Assert
            Thread.Sleep(200);

            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(completed);
            Trace.WriteLine("Completion reported.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");

            Assert.IsTrue(progress);
            Trace.WriteLine("Progress reported.");

            Assert.IsTrue(goodPercentage);
            Trace.WriteLine("Percentage good.");

            Assert.IsTrue(goodState);
            Trace.WriteLine("State good.");
        }

        [Test]
        public void Test_Run_DoWork_onComplete_progress_withArgs_withState()
        {
            // Arrange

            var didWork = false;
            var completed = false;
            var goodArgs = false;
            var progress = false;
            var goodState = false;
            var goodPercentage = false;
            var arguments = new List<object> {Tests.Random<string>(), Tests.Random<int>(), Tests.Random<bool>()};
            var state = Tests.Random<string>();
            var percentage = Tests.Random<int>(0, 100);


            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == arguments);
                        _worker.ReportProgress(percentage, state);
                    },
                (s, e) =>
                    {
                        // OnComplete
                        completed = true;
                    },
                (s, e) =>
                    {
                        // OnProgressChanged
                        progress = true;
                        goodPercentage = (e.ProgressPercentage == percentage);
                        goodState = (e.UserState.ToString() == state);
                    },
                arguments);


            // Assert
            Thread.Sleep(200);

            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(completed);
            Trace.WriteLine("Completion reported.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");

            Assert.IsTrue(progress);
            Trace.WriteLine("Progress reported.");

            Assert.IsTrue(goodPercentage);
            Trace.WriteLine("Percentage good.");

            Assert.IsTrue(goodState);
            Trace.WriteLine("State good.");
        }

        [Test]
        public void Test_Run_DoWork_onComplete_withArgs()
        {
            // Arrange
            var didWork = false;
            var completed = false;
            var goodArgs = false;
            var arguments = new List<object> {Tests.Random<string>(), Tests.Random<int>(), Tests.Random<bool>()};


            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == arguments);
                    },
                (s, e) =>
                    {
                        // OnComplete
                        completed = true;
                    },
                arguments);


            // Assert
            Thread.Sleep(200);
            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(completed);
            Trace.WriteLine("Completion reported.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");
        }

        [Test]
        public void Test_Run_DoWork_withArgs()
        {
            // Arrange
            var didWork = false;
            var goodArgs = false;
            // Create the argument list.
            var arguments = new List<object> {Tests.Random<string>(), Tests.Random<int>(), Tests.Random<bool>()};


            // Act
            _worker.Run(
                (s, e) =>
                    {
                        // DoWork
                        didWork = true;
                        goodArgs = (e.Argument == arguments);
                    },
                arguments);

            // Assert
            Thread.Sleep(200);
            Assert.IsTrue(didWork);
            Trace.WriteLine("Did work.");

            Assert.IsTrue(goodArgs);
            Trace.WriteLine("Args are good.");
        }
    }
}