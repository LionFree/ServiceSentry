// -----------------------------------------------------------------------
//  <copyright file="Service.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
//using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.Events;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ServiceImplementation))]
    public abstract class Service : DisposableEquatableBase
    {
        public static Service GetInstance(string serviceName, string machineName, Logger logger)
        {
            // MachineName = Dns.GetHostEntry("LocalMachine").HostName
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException("serviceName");
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            var serviceData = new SubscriptionPacket {ServiceName = serviceName, MachineName = machineName};
            return GetInstance(serviceData, logger);
        }

        internal static Service GetInstance(SubscriptionPacket packet, Logger logger)
        {
            //Contract.Requires(packet != null);
            return new ServiceImplementation(packet, ModelClassFactory.GetInstance(logger))
                {
                    ServiceGroup = Strings.DefaultServiceGroupName
                };
        }

        
        #region Abstract Members

        public abstract void UpdateParameters(MonitorServiceWatchdog monitor, NotificationSettings settings, WardenServerDetails wardenDetails);


        [DataMember]
        public abstract string CommonName { get; set; }

        /// <summary>
        ///     Gets or sets the name that identifies the service that this instance references.
        /// </summary>
        /// <returns>
        ///     The name that identifies the service that this
        ///     <see
        ///         cref="T:ServiceSentry.Model.Service" />
        ///     instance references. The default is an empty string ("").
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     The <see cref="P:ServiceSentry.Model.Service.ServiceName" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The syntax of the <see cref="P:ServiceSentry.Model.Service.ServiceName" /> property is invalid.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">The service was not found.</exception>
        [DataMember]
        public abstract string ServiceName { get; set; }

        [DataMember]
        public abstract string ServiceGroup { get; set; }

        [DataMember]
        public abstract ServiceDetails Details { get; set; }

        [DataMember]
        public abstract ObservableCollection<ExternalFile> ConfigFiles { get; set; }

        [DataMember]
        public abstract ObservableCollection<ExternalFile> LogFiles { get; set; }

        [DataMember]
        public abstract string MachineName { get; set; }

        [DataMember]
        public abstract bool ParametersUnlocked { get; set; }

        public abstract bool CanToggle { get; set; }
        public abstract Guid Guid { get; }
        public abstract bool IsRestarting { get; set; }
        public abstract bool IsReceivingInternalUpdate { get; set; }
        public abstract bool IsInstalled { get; }

        /// <summary>
        ///     Gets a value indicating whether the service can be stopped after it has started.
        /// </summary>
        /// <returns>
        ///     true if the service can be stopped; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when accessing a system API. </exception>
        /// <exception cref="T:System.InvalidOperationException">The service was not found.</exception>
        public abstract bool CanStop { get; }

        /// <summary>
        ///     Gets a friendly name for the service.
        /// </summary>
        /// <returns>
        ///     The friendly name of the service, which can be used to identify the service.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     The <see cref="P:ServiceSentry.Model.Service.DisplayName" /> is null.
        /// </exception>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when accessing a system API. </exception>
        /// <exception cref="T:System.InvalidOperationException">The service was not found.</exception>
        public abstract string DisplayName { get; }

        /// <summary>
        ///     Gets the status of the service that is referenced by this instance.
        /// </summary>
        /// <returns>
        ///     One of the <see cref="T:ServiceSentry.Model.Enumerations.ServiceState" /> values that indicates whether the
        ///     service is running, stopped, or paused, or whether a start, stop, pause, or continue command is pending.
        /// </returns>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when accessing a system API. </exception>
        /// <exception cref="T:System.InvalidOperationException">The service was not found.</exception>
        public abstract ServiceState Status { get; }


        public abstract SubscriptionPacket Packet { get; }
        internal abstract bool MediatorIsValid { get; }

        #region Methods

        /// <summary>
        ///     Starts the service, passing no arguments.
        /// </summary>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when accessing a system API. </exception>
        /// <exception cref="T:System.InvalidOperationException">The service was not found.</exception>
        public abstract void Start();

        /// <summary>
        ///     Stops this service and any services that are dependent on this service.
        /// </summary>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when accessing a system API. </exception>
        /// <exception cref="T:System.InvalidOperationException">The service was not found. </exception>
        public abstract void Stop(MonitorServiceWatchdog monitor);

        /// <summary>
        ///     Infinitely waits for the service to reach the specified status.
        /// </summary>
        /// <param name="desiredStatus">The status to wait for. </param>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
        ///     The <paramref name="desiredStatus" /> parameter is not any of the values defined in the
        ///     <see
        ///         cref="T:System.ServiceProcess.ServiceControllerStatus" />
        ///     enumeration.
        /// </exception>
        public abstract void WaitForStatus(ServiceState desiredStatus);

        /// <summary>
        ///     Refreshes property values by resetting the properties to their current values.
        /// </summary>
        public abstract void Refresh(MonitorServiceWatchdog monitor);

        /// <summary>
        ///     Tells the Monitor Service to stop watching this service.
        /// </summary>
        public abstract void Unsubscribe(MonitorServiceWatchdog monitor);


        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        protected abstract void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e);
        public abstract void OnExternalStatusChange(StatusChangedEventArgs e);
        public abstract void OnServiceFellOver(StatusChangedEventArgs e);
        public abstract void OnMonitorError(object sender, MonitorErrorEventArgs args);

        /// <summary>
        ///     Attach a <see cref="ClientMediator" /> to this <see cref="Service" /> before use.
        /// </summary>
        /// <param name="mediator">
        ///     The <see cref="ClientMediator" /> to attach.
        /// </param>
        public abstract void AttachMediator(ClientMediator mediator);

        public abstract Service Clone(Logger logger);

        public abstract event EventHandler<StatusChangedEventArgs> ExternalStatusChanged;
        public abstract event EventHandler<StatusChangedEventArgs> ServiceFellOver;
        public abstract event MonitorErrorEventHandler MonitorError;

        /// <summary>
        ///     Determines whether the specifies <see cref="Object" /> is equal to the current <see cref="Service" />.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="Object" /> to compare with the current <see cref="Service" />.
        /// </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (Service) obj;
            return (ServiceName == p.ServiceName);
        }

        /// <summary>
        ///     Returns the hashcode for this <see cref="Service" />.
        /// </summary>
        public override int GetHashCode()
        {
            const int seed = 7;
            var hash = 13;
            hash *= seed + (ServiceName != null ? ServiceName.GetHashCode() : 0);
            return hash;
        }

        /// <summary>
        ///     Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ServiceName;
        }

        #endregion

        #endregion

        [DataContract]
        private sealed class ServiceImplementation : Service
        {
            #region privates

            //private readonly ModelClassFactory _factory;
            private readonly SubscriptionPacket _packet;

            private bool _canToggle;
            private string _commonName;
            private ObservableCollection<ExternalFile> _configFiles = new ObservableCollection<ExternalFile>();
            private ServiceDetails _details = ServiceDetails.GetInstance();
            private bool _disposed;
            private string _group;
            private Guid _guid;
            private bool _isReceivingInternalUpdate;
            private bool _isRestarting;
            private ObservableCollection<ExternalFile> _logFiles = new ObservableCollection<ExternalFile>();
            private string _machineName;
            private ClientMediator _mediator;
            private bool _parametersUnlocked;
            private string _serviceName;

            #endregion

            internal ServiceImplementation(SubscriptionPacket serviceData, ModelClassFactory factory)
            {
                //Contract.Requires(serviceData != null);
                _packet = serviceData;
                _guid = Guid.NewGuid();
                _parametersUnlocked = true;

                ServiceName = serviceData.ServiceName;
                MachineName = serviceData.MachineName;
                //_factory = factory;

                LogFiles.CollectionChanged += OnCollectionChanged;
                ConfigFiles.CollectionChanged += OnCollectionChanged;
            }

            public override SubscriptionPacket Packet
            {
                get
                {
                    return _packet ??
                           new SubscriptionPacket
                               {
                                   ServiceName = ServiceName,
                                   MachineName = MachineName,
                                   CommonName = CommonName,
                                   NotifyOnUnexpectedStop = Details.NotifyOnUnexpectedStop,
                                   Timeout = Details.Timeout == 0
                                                 ? TimeSpan.MaxValue
                                                 : new TimeSpan(0, 0, 0, Details.Timeout)
                               };
                }
            }

            public override string ServiceName
            {
                get { return _serviceName; }
                set
                {
                    if (_serviceName == value) return;
                    _serviceName = value;
                    OnPropertyChanged();
                }
            }

            public override string ServiceGroup
            {
                get { return _group; }
                set
                {
                    if (_group == value) return;
                    _group = value;
                    OnPropertyChanged();
                }
            }

            public override ServiceDetails Details
            {
                get { return _details; }
                set
                {
                    if (_details == value) return;
                    _details = value;
                    OnPropertyChanged();
                }
            }

            public override ObservableCollection<ExternalFile> ConfigFiles
            {
                get { return _configFiles; }
                set
                {
                    if (_configFiles == value) return;
                    _configFiles = value;
                    OnPropertyChanged();
                }
            }

            public override ObservableCollection<ExternalFile> LogFiles
            {
                get { return _logFiles; }
                set
                {
                    if (_logFiles == value) return;
                    _logFiles = value;
                    OnPropertyChanged();
                }
            }

            public override string MachineName
            {
                get { return _machineName; }
                set
                {
                    if (_machineName == value) return;
                    _machineName = value;
                    OnPropertyChanged();
                }
            }

            public override bool ParametersUnlocked
            {
                get { return _parametersUnlocked; }
                set
                {
                    if (_parametersUnlocked == value) return;
                    _parametersUnlocked = value;
                    OnPropertyChanged();
                }
            }

            public override string CommonName
            {
                get { return _commonName; }
                set
                {
                    if (_commonName == value) return;
                    _commonName = value;
                    OnPropertyChanged();
                }
            }

            public override bool CanToggle
            {
                get { return _canToggle; }
                set
                {
                    if (_canToggle == value) return;
                    _canToggle = value;
                    OnPropertyChanged();
                }
            }

            public override Guid Guid
            {
                get { return _guid; }
            }

            public override bool IsRestarting
            {
                get { return _isRestarting; }
                set
                {
                    if (_isRestarting == value) return;
                    _isRestarting = value;
                    OnPropertyChanged();
                }
            }

            public override bool IsReceivingInternalUpdate
            {
                get { return _isReceivingInternalUpdate; }
                set
                {
                    if (_isReceivingInternalUpdate == value) return;
                    _isReceivingInternalUpdate = value;
                    OnPropertyChanged();
                }
            }

            public override Service Clone(Logger logger)
            {
                var clone = GetInstance(ServiceName, MachineName, logger);
                clone.CommonName = CommonName;
                clone.ConfigFiles = ConfigFiles;
                clone.LogFiles = LogFiles;
                clone.ParametersUnlocked = true;
                clone.Details = Details;
                clone.ServiceGroup = ServiceGroup;
                clone.AttachMediator(_mediator);
                return clone;
            }

            protected override void Dispose(bool disposing)
            {
                // Without a using-statement, it is necessary to call 
                // Close() or Dispose() on the ServiceController, because
                // it uses unmanaged resources that need to be released. 
                // (Otherwise you have a memory leak.)

                if (_disposed) return;

                if (disposing)
                {
                    if (_mediator != null)
                    {
                        _mediator.MonitorError -= OnMonitorError;
                        _mediator.Dispose();
                    }
                }
                _mediator = null;

                base.Dispose(disposing);

                _disposed = true;
            }

            public override void OnExternalStatusChange(StatusChangedEventArgs e)
            {
                var handler = ExternalStatusChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public override void OnServiceFellOver(StatusChangedEventArgs e)
            {
                var handler = ServiceFellOver;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public override void OnMonitorError(object sender, MonitorErrorEventArgs args)
            {
                var handler = MonitorError;
                if (handler != null)
                {
                    handler(this, args);
                }
            }

            protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (ExternalFile item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (ExternalFile item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }

            public override event EventHandler<StatusChangedEventArgs> ExternalStatusChanged;
            public override event EventHandler<StatusChangedEventArgs> ServiceFellOver;
            public override event MonitorErrorEventHandler MonitorError;

            public override void UpdateParameters(MonitorServiceWatchdog monitor, NotificationSettings emailSettings, WardenServerDetails wardenDetails)
            {
                var packet = new SubscriptionPacket
                {
                    ServiceName = ServiceName,
                    MachineName = MachineName,
                    CommonName = CommonName,
                    DisplayName = DisplayName,
                    NotifyOnUnexpectedStop = Details.NotifyOnUnexpectedStop,
                    EmailInfo = emailSettings.EmailInfo,
                    SMTPInfo = emailSettings.SMTPInfo,
                    LogFiles = LogFiles,
                    WardenHost = wardenDetails.HostName,
                    WardenPort = wardenDetails.Port,
                };

                _mediator.UpdateSubscription(monitor, packet);
            }

            public override void AttachMediator(ClientMediator mediator)
            {
                _mediator = mediator;
                _mediator.MonitorError += OnMonitorError;
                _guid = Guid.NewGuid();
                _canToggle = true;
            }

            #region Mediator Methods

            public override bool IsInstalled
            {
                get { return (MediatorIsValid) && _mediator.IsInstalled; }
            }

            internal override bool MediatorIsValid
            {
                get
                {
                    return (_mediator != null);
                }
            }

            public override ServiceState Status
            {
                get { return MediatorIsValid ? _mediator.Status : ServiceState.Error; }
            }

            public override string DisplayName
            {
                get { return MediatorIsValid ? _mediator.DisplayName : "Mediator is Null"; }
            }

            public override bool CanStop
            {
                get { return MediatorIsValid && _mediator.CanStop; }
            }

            public override void Refresh(MonitorServiceWatchdog monitor)
            {
                if (MediatorIsValid) _mediator.Refresh(monitor);
            }

            public override void Start()
            {
                if (MediatorIsValid) _mediator.Start();
            }

            public override void Stop(MonitorServiceWatchdog monitor)
            {
                if (MediatorIsValid) _mediator.Stop(monitor);
            }

            public override void Unsubscribe(MonitorServiceWatchdog monitor)
            {
                if (MediatorIsValid) _mediator.Unsubscribe(monitor);
            }

            public override void WaitForStatus(ServiceState desiredStatus)
            {
                if (MediatorIsValid) _mediator.WaitForStatus(desiredStatus);
            }

            #endregion
        }
    }
}