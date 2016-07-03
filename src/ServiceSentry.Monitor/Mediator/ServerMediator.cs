// -----------------------------------------------------------------------
//  <copyright file="ServerMediator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Monitor.Mediator
{
    public abstract class ServerMediator : IDisposable
    {
        private const int DefaultTrackingInterval = 2000;

        public static ServerMediator GetInstance(Logger logger, string serviceName, ModelClassFactory factory,
                                                 ConsoleHarness harness)
        {
            return GetInstance(logger, new SubscriptionPacket {ServiceName = serviceName}, factory, harness);
        }

        public static ServerMediator GetInstance(Logger logger, SubscriptionPacket packet, ModelClassFactory factory,
                                                 ConsoleHarness harness)
        {
            return new ServerLocalMediator(logger, packet, factory, harness, DefaultTrackingInterval);
        }

        #region Abstract Members

        public abstract bool CanStop { get; }

        public abstract string DisplayName { get; }

        public abstract bool IsInstalled { get; }

        public abstract ServiceState Status { get; }

        public abstract string ServiceName { get; }

        public abstract string MachineName { get; }
        public abstract List<Exception> Exceptions { get; }

        public abstract void Start(ref TrackingList state);

        public abstract void Stop(ref TrackingList state);

        public abstract void Refresh(string serviceName, TrackingList state);

        public abstract void WaitForStatus(ServiceState desiredStatus, TimeSpan timeout);

        public abstract void Subscribe(TrackingList trackingList);
        public abstract void Unsubscribe(ref TrackingList state);
        public abstract void UpdateSubscription(SubscriptionPacket packet, ref TrackingList state);

        #endregion

        #region Disposer

        /// <summary>
        ///     Tracks whether <see cref="M:ServiceSentry.Monitor.Mediator.ServerMediator.Dispose" /> has been called or not.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        ///     Tracks whether <see cref="M:ServiceSentry.Monitor.Mediator.ServerMediator.Dispose" /> has been called or not.
        /// </summary>
        public virtual bool IsDisposed
        {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }

        /// <summary>
        ///     Disposes the object.
        /// </summary>
        /// <remarks>
        ///     This method is not virtual by design. Derived classes
        ///     should override
        ///     <see cref="M:ServiceSentry.Monitor.Mediator.ServerMediator.Dispose(System.Boolean)" />
        ///     .
        /// </remarks>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     This destructor will run only if the
        ///     <see
        ///         cref="M:ServiceSentry.Monitor.Mediator.ServerMediator.Dispose" />
        ///     method does not get called. This gives this base class the
        ///     opportunity to finalize.
        ///     <para>
        ///         Important: Do not provide destructors in types derived from
        ///         this class.
        ///     </para>
        /// </summary>
        ~ServerMediator()
        {
            Dispose(false);
        }

        /// <summary>
        ///     <c>Dispose(bool disposing)</c> executes in two distinct scenarios.
        ///     If disposing equals <c>true</c>, the method has been called directly
        ///     or indirectly by a user's code. Managed and unmanaged resources
        ///     can be disposed.
        /// </summary>
        /// <param name="disposing">
        ///     If disposing equals <c>false</c>, the method
        ///     has been called by the runtime from inside the finalizer and you
        ///     should not reference other objects. Only unmanaged resources can
        ///     be disposed.
        /// </param>
        /// <remarks>
        ///     Check the <see cref="P:ServiceSentry.Monitor.Mediator.ServerMediator.IsDisposed" /> property to determine whether
        ///     the method has already been called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
                IsDisposed = true;
            else
                _isDisposed = true;
        }

        #endregion
    }
}