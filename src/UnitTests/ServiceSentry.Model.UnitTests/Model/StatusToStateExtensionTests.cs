// -----------------------------------------------------------------------
//  <copyright file="StatusToStateExtensionTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceProcess;
using NUnit.Framework;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model.UnitTests.Model
{
    [TestFixture]
    internal class StatusToStateExtensionTests
    {
        [Test]
        public void Test_ToState()
        {
            var stoppedState = ServiceControllerStatus.Stopped.ToState();
            var runningState = ServiceControllerStatus.Running.ToState();
            var pausedState = ServiceControllerStatus.Paused.ToState();
            var pausePendingState = ServiceControllerStatus.PausePending.ToState();
            var startPendingState = ServiceControllerStatus.StartPending.ToState();
            var stopPendingState = ServiceControllerStatus.StopPending.ToState();
            var continuePendingState = ServiceControllerStatus.ContinuePending.ToState();

            Assert.AreEqual(ServiceState.Stopped, stoppedState);
            Assert.AreEqual(ServiceState.Running, runningState);
            Assert.AreEqual(ServiceState.Paused, pausedState);
            Assert.AreEqual(ServiceState.PausePending, pausePendingState);
            Assert.AreEqual(ServiceState.StartPending, startPendingState);
            Assert.AreEqual(ServiceState.StopPending, stopPendingState);
            Assert.AreEqual(ServiceState.ContinuePending, continuePendingState);
        }

        [Test]
        public void Test_ToStatus()
        {
            var stoppedState = ServiceState.Stopped.ToStatus();
            var runningState = ServiceState.Running.ToStatus();
            var pausedState = ServiceState.Paused.ToStatus();
            var pausePendingState = ServiceState.PausePending.ToStatus();
            var startPendingState = ServiceState.StartPending.ToStatus();
            var stopPendingState = ServiceState.StopPending.ToStatus();
            var continuePendingState = ServiceState.ContinuePending.ToStatus();

            Assert.AreEqual(ServiceControllerStatus.Stopped, stoppedState);
            Assert.AreEqual(ServiceControllerStatus.Running, runningState);
            Assert.AreEqual(ServiceControllerStatus.Paused, pausedState);
            Assert.AreEqual(ServiceControllerStatus.PausePending, pausePendingState);
            Assert.AreEqual(ServiceControllerStatus.StartPending, startPendingState);
            Assert.AreEqual(ServiceControllerStatus.StopPending, stopPendingState);
            Assert.AreEqual(ServiceControllerStatus.ContinuePending, continuePendingState);
        }
    }
}