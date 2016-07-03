// -----------------------------------------------------------------------
//  <copyright file="ConfigFile.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Events;

#endregion

namespace ServiceSentry.Model
{
    [DataContract(Name = "ServiceSentry.ConfigFile")]
    public abstract class ConfigFile : EquatableBase
    {
        public static ConfigFile Default
        {
            get { return GetInstance(); }
        }

        internal static ConfigFile GetInstance()
        {
            return new ImplementedConfigFile();
        }

        public abstract bool Contains(ExternalFile logFile);
        public abstract void Add(Service service);
        public abstract void Add(ExternalFile logFile);

        #region Special Method Overrides

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (ConfigFile) obj;

            var samePath = (FilePath == p.FilePath);
            var sameNotification = (NotificationDetails == p.NotificationDetails);
            var sameArchive = (LogDetails == p.LogDetails);
            var sameServices = (Services == p.Services);
            var sameLogs = (LogList == p.LogList);
            var sameWarden = (WardenDetails == p.WardenDetails);

            var same = (samePath && sameNotification && sameArchive && sameServices && sameLogs && sameWarden);

            return same;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 11;
                var hash = 19;

                // be sure to check for nullity, etc.
                hash *= seed + (FilePath != null ? FilePath.GetHashCode() : 0);
                hash *= seed + (NotificationDetails != null ? NotificationDetails.GetHashCode() : 0);
                hash *= seed + (DebugLogConfiguration != null ? DebugLogConfiguration.GetHashCode() : 0);
                hash *= seed + (LogDetails != null ? LogDetails.GetHashCode() : 0);
                hash *= seed + (Services != null ? Services.GetHashCode() : 0);
                hash *= seed + (LogList != null ? LogList.GetHashCode() : 0);

                return hash;
            }
        }

        #endregion

        #region Abstract Members

        public abstract string FilePath { get; set; }

        [DataMember(Name="WardenServerDetails",EmitDefaultValue = true,IsRequired = false)]
        public abstract WardenServerDetails WardenDetails { get; set; }

        [DataMember(Name = "NotificationSettings", EmitDefaultValue = true, IsRequired = true)]
        public abstract NotificationSettings NotificationDetails { get; set; }
        
        [DataMember(EmitDefaultValue = true)]
        public abstract LogConfiguration DebugLogConfiguration { get; set; }

        [DataMember(Name = "LogSettings", EmitDefaultValue = true, IsRequired = true)]
        public abstract LoggingDetails LogDetails { get; set; }

        [DataMember(Name = "ServiceList", EmitDefaultValue = true, IsRequired = true)]
        public abstract ServiceList Services { get; set; }

        [DataMember(Name = "OtherLogs", EmitDefaultValue = true, IsRequired = true)]
        public abstract FileList LogList { get; set; }



        protected abstract void OnServiceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        protected abstract void OnLogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        protected abstract void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e);
        protected abstract void OnMonitorError(object sender, MonitorErrorEventArgs e);
        public event MonitorErrorEventHandler MonitorError;

        public abstract bool Contains(Service service);

        #endregion

        [DataContract(Name = "ServiceSentry.ConfigFile")]
        internal sealed class ImplementedConfigFile : ConfigFile
        {
            #region Fields

            private string _filePath;
            private LoggingDetails _logDetails;
            private FileList _logList;
            private LogConfiguration _loggingConfiguration;

            private NotificationSettings _notificationDetails;

            private ServiceList _serviceList;
            private WardenServerDetails _wardenDetails;

            #endregion

            internal ImplementedConfigFile()
            {
                LogDetails = LoggingDetails.Default;
                Services = ServiceList.Default;
                LogList = FileList.Default;
                NotificationDetails = NotificationSettings.Default;
                DebugLogConfiguration = LogConfiguration.Default;
                WardenDetails = WardenServerDetails.Default;

                Services.Items.CollectionChanged += OnServiceCollectionChanged;
                LogList.Items.CollectionChanged += OnLogCollectionChanged;
            }

            #region Properties

            public override string FilePath
            {
                get { return _filePath; }
                set
                {
                    if (_filePath == value) return;
                    _filePath = value;
                    OnPropertyChanged();
                }
            }

            public override WardenServerDetails WardenDetails 
            {
                get { return _wardenDetails; }
                set
                {
                    if (_wardenDetails == value) return;
                    _wardenDetails = value;
                    OnPropertyChanged();
                }
            }

            public override NotificationSettings NotificationDetails
            {
                get { return _notificationDetails; }
                set
                {
                    if (_notificationDetails == value) return;
                    _notificationDetails = value;
                    OnPropertyChanged();
                }
            }

            public override LogConfiguration DebugLogConfiguration
            {
                get { return _loggingConfiguration; }
                set
                {
                    if (_loggingConfiguration == value) return;
                    _loggingConfiguration = value;
                    OnPropertyChanged();
                }
            }

            public override LoggingDetails LogDetails
            {
                get { return _logDetails; }
                set
                {
                    if (_logDetails == value) return;
                    _logDetails = value;

                    if (Services != null)
                        Services.LogDetails = _logDetails;
                    else
                    {
                        Services = ServiceList.GetInstance();
                        Services.LogDetails = _logDetails;
                    }

                    OnPropertyChanged();
                }
            }

            public override ServiceList Services
            {
                get { return _serviceList; }
                set
                {
                    if (_serviceList == value) return;
                    _serviceList = value;
                    OnPropertyChanged();
                }
            }

            public override FileList LogList
            {
                get { return _logList; }
                set
                {
                    if (_logList == value) return;
                    _logList = value;
                    OnPropertyChanged();
                }
            }

            #endregion

            #region Events

            protected override void OnServiceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (Service item in e.NewItems)
                    {
                        //_modifiedItems.Add(item);
                        item.PropertyChanged += OnItemPropertyChanged;
                        item.MonitorError += OnMonitorError;
                    }
                }

                if (e.OldItems == null) return;
                foreach (Service item in e.OldItems)
                {
                    //_modifiedItems.Add(item);
                    item.PropertyChanged -= OnItemPropertyChanged;
                    item.MonitorError += OnMonitorError;
                }
            }

            protected override void OnLogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (ExternalFile item in e.NewItems)
                    {
                        //_modifiedItems.Add(item);
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (ExternalFile item in e.OldItems)
                {
                    //_modifiedItems.Add(item);
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }

            protected override void OnMonitorError(object sender, MonitorErrorEventArgs e)
            {
                Trace.WriteLine("Config file received monitor errors.  Passing them along.");
                var handler = MonitorError;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public override bool Contains(Service service)
            {
                foreach (var existingItem in Services.Items)
                {
                    if (service == existingItem) return true;
                }
                return false;
            }

            public override bool Contains(ExternalFile logFile)
            {
                foreach (var existingItem in LogList.Items)
                {
                    if (logFile == existingItem) return true;
                }
                return false;
            }

            public override void Add(Service service)
            {
                if (!Contains(service))
                {
                    Services.Items.Add(service);
                }
            }

            public override void Add(ExternalFile logFile)
            {
                if (!Contains(logFile))
                {

                    LogList.Items.Add(logFile);
                }
            }

            #endregion
        }
    }
}