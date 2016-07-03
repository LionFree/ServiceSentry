// -----------------------------------------------------------------------
//  <copyright file="ClientMediator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;

#endregion

namespace ServiceSentry.Model.Client
{
    public abstract class ClientMediator : IDisposable
    {
        public static ClientMediator GetInstance(Logger logger, ClientList clients, SubscriptionPacket serviceData)
        {
            return GetInstance( logger, clients, serviceData, ModelClassFactory.GetInstance(logger));
        }

        internal static ClientMediator GetInstance(Logger logger,
                                                   ClientList clients,
                                                   SubscriptionPacket serviceData,
                                                   ModelClassFactory factory)
        {
            //Contract.Requires(serviceData != null);
            //Contract.Requires(factory != null);

            return new ClientLocalMediator(serviceData, clients, factory, logger);
        }

        internal abstract void OnMonitorError(PollResult pollResult);
        public abstract void DisplayMonitorExceptions();

        #region Abstract Members 

        public abstract bool CanStop { get; }

        public abstract string DisplayName { get; }

        public abstract bool IsInstalled { get; }

        public abstract ServiceState Status { get; }

        public abstract string ServiceName { get; }

        public abstract string MachineName { get; }

        public abstract void Start();

        public abstract void Stop(MonitorServiceWatchdog monitor);

        public abstract void Refresh(MonitorServiceWatchdog monitor);

        public abstract void WaitForStatus(ServiceState desiredStatus);

        public abstract void Unsubscribe(MonitorServiceWatchdog monitor);
        
        public abstract event MonitorErrorEventHandler MonitorError;

        public abstract void UpdateSubscription(MonitorServiceWatchdog monitor, SubscriptionPacket packet);

        #endregion

        #region Disposer

        // Tracks whether Dispose has been called or not.
        private bool _isDisposed;

        /// <summary>
        ///     Tracks whether
        ///     <see cref="M:ServiceSentry.Model.Client.ClientMediator.Dispose" />
        ///     has been called or not.
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
        ///     <see
        ///         cref="M:ServiceSentry.Model.Client.ClientMediator.Dispose(System.Boolean)" />
        ///     .
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     This destructor will run only if the
        ///     <see
        ///         cref="M:ServiceSentry.Model.Client.ClientMediator.Dispose" />
        ///     method does not get called. This gives this base class the
        ///     opportunity to finalize.
        ///     <para>
        ///         Important: Do not provide destructors in types derived from
        ///         this class.
        ///     </para>
        /// </summary>
        ~ClientMediator()
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
        ///     Check the
        ///     <see cref="P:ServiceSentry.Model.Client.ClientMediator.IsDisposed" />
        ///     property to determine whether
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