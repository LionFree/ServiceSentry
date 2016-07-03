using System;
using System.Timers;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Enumerations;

namespace ServiceSentry.Model.Client
{
    public abstract class MonitorServiceWatchdog : DisposableBase
    {
        private const int DefaultPollInterval = 3000;

        public static MonitorServiceWatchdog GetInstance(Logger logger)
        {
            return GetInstance(ModelClassFactory.GetInstance(logger), DefaultPollInterval);
        }

        internal static MonitorServiceWatchdog GetInstance(ModelClassFactory factory, int pollInterval)
        {
            return new WatchdogImplementation(factory, pollInterval);
        }

        #region Abstract Members

        public abstract bool IsAvailable { get; protected set; }
        public abstract bool IsInstalled { get; protected set; }
        protected abstract void OnAvailabilityChanged(bool value);
        public abstract event EventHandler<AvailabilityChangedEventArgs> AvailabilityChanged;

        #endregion

        private sealed class WatchdogImplementation : MonitorServiceWatchdog
        {
            private readonly ModelClassFactory _factory;
            private bool _available;
            private bool _installed;

            internal WatchdogImplementation(ModelClassFactory factory, int pollInterval)
            {
                //Contract.Requires(factory != null);
                _factory = factory;
                PerformChecks();

                var timer = new Timer();
                timer.Elapsed += (s, e) => PerformChecks();
                timer.Interval = pollInterval;
                timer.Enabled = true;
            }

            public override bool IsAvailable
            {
                get { return _available; }
                protected set
                {
                    if (_available == value) return;
                    _available = value;
                    OnAvailabilityChanged(value);
                    OnPropertyChanged();
                }
            }

            public override bool IsInstalled
            {
                get { return _installed; }
                protected set
                {
                    if (_installed == value) return;
                    _installed = value;
                    OnAvailabilityChanged(value);
                    OnPropertyChanged();
                }
            }

            protected override void OnAvailabilityChanged(bool value)
            {
                var handler = AvailabilityChanged;
                handler?.Invoke(this, new AvailabilityChangedEventArgs {Availability = value});
            }

            public override event EventHandler<AvailabilityChangedEventArgs> AvailabilityChanged;

            private void PerformChecks()
            {
                CheckInstalled();
                CheckAvailable();
            }

            private void CheckInstalled()
            {
                try
                {
                    IsInstalled = _factory.GetLocalServiceFinder().IsInstalled(Extensibility.Strings._HostServiceName);

                    if (!IsInstalled)
                    {
                        IsAvailable = false;
                    }
                }
                catch (Exception)
                {
                    IsInstalled = false;
                    IsAvailable = false;
                }
            }

            private void CheckAvailable()
            {
                try
                {
                    using (var sc = _factory.GetLocalServiceController(Extensibility.Strings._HostServiceName))
                    {
                        IsAvailable = (sc.Status == ServiceState.Running);
                    }
                }
                catch (Exception)
                {
                    IsAvailable = false;
                }
            }
        }
    }


    public class AvailabilityChangedEventArgs : EventArgs
    {
        public bool Availability { get; set; }
    }
}