using System;
using System.ServiceProcess;
using ServiceSentry.Model.Enumerations;

namespace ServiceSentry.Model.Services
{
    public abstract class ServiceWrapper : IDisposable
    {
        public static ServiceWrapper GetInstance(string serviceName)
        {
            return new LocalServiceController(serviceName, LocalServiceFinder.Default);
        }

        public static ServiceWrapper GetInstance(string serviceName, LocalServiceFinder finder)
        {
            return new LocalServiceController(serviceName, finder);
        }

        #region Abstract Members

        public abstract bool CanStop { get; }

        public abstract string DisplayName { get; }

        public abstract bool IsInstalled { get; }

        public abstract ServiceState Status { get; }

        public abstract string ServiceName { get; }

        public abstract string MachineName { get; }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Refresh();

        public abstract void WaitForStatus(ServiceState desiredStatus);
        public abstract void WaitForStatus(ServiceState desiredStatus, TimeSpan timeout);

        #endregion

        #region Disposer

        // Tracks whether Dispose has been called or not.
        private bool _isDisposed;

        /// <summary>
        ///     Tracks whether
        ///     <see cref="M:ServiceSentry.Model.Services.ServiceWrapper.Dispose" />
        ///     has been called or not.
        /// </summary>
        public virtual bool IsDisposed
        {
            get { return _isDisposed; }
            protected set { _isDisposed = value; }
        }

        /// <summary>
        ///     Disposes the object.
        /// </summary>
        /// <remarks>
        ///     This method is not virtual by design. Derived classes
        ///     should override
        ///     <see
        ///         cref="M:ServiceSentry.Model.Services.ServiceWrapper.Dispose(System.Boolean)" />
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
        ///         cref="M:ServiceSentry.Model.Services.ServiceWrapper.Dispose" />
        ///     method does not get called. This gives this base class the
        ///     opportunity to finalize.
        ///     <para>
        ///         Important: Do not provide destructors in types derived from
        ///         this class.
        ///     </para>
        /// </summary>
        ~ServiceWrapper()
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
        ///     <see cref="P:ServiceSentry.Model.Services.ServiceWrapper.IsDisposed" />
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

    internal sealed class LocalServiceController : ServiceWrapper
    {
        private readonly ServiceController _controller;
        private readonly LocalServiceFinder _finder;
        private readonly string _serviceName;

        internal LocalServiceController(string serviceName, LocalServiceFinder finder)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException(nameof(serviceName));

            _serviceName = serviceName;
            _controller = new ServiceController(_serviceName);
            _finder = finder;
        }

        public override bool CanStop => IsInstalled && _controller.CanStop;

        public override string DisplayName => IsInstalled ? _controller.DisplayName : string.Empty;

        public override bool IsInstalled => _finder.IsInstalled(_serviceName);

        public override ServiceState Status => IsInstalled ? _controller.Status.ToState() : ServiceState.None;

        public override string ServiceName => _serviceName;

        public override string MachineName => IsInstalled ? _controller.MachineName : string.Empty;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose all managed objects
                //_controller.Close();
                _controller.Dispose();
                //_controller = null;
            }
            // Release unmanaged resources
            IsDisposed = true;
        }

        public override void Start()
        {
            _controller.Start();
        }

        public override void Stop()
        {
            _controller.Stop();
        }

        public override void Refresh()
        {
            _controller.Refresh();
        }

        public override void WaitForStatus(ServiceState desiredStatus)
        {
            WaitForStatus(desiredStatus, TimeSpan.MaxValue);
        }

        public override void WaitForStatus(ServiceState desiredStatus, TimeSpan timeout)
        {
            _controller.WaitForStatus(desiredStatus.ToStatus(), timeout);
        }
    }
}