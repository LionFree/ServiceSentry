// -----------------------------------------------------------------------
//  <copyright file="ApplicationState.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ServiceSentry.Common.Email;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.Model
{
    public abstract class ApplicationState : PropertyChangedBase
    {
        public abstract TrackingList State { get; set; }
        internal abstract ObservableCollection<ServiceGroup> ServiceGroups { get; set; }
        public abstract ServiceList Services { get; set; }
        public abstract ConfigFile LocalConfigs { get; set; }
        public abstract EmailInfo SMTPDetails { get; set; }
        public abstract FileList LogList { get; set; }
        public abstract ObservableCollection<ServiceInfo> ServiceInfos { get; set; }
        public abstract NotificationSettings NotificationDetails { get; set; }

        internal static ApplicationState GetInstance()
        {
            return new StateImplementation(FileAttacher.GetInstance()); 
        }

        public abstract void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e);
        public abstract void ServiceInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        public abstract void OnServicesChanged();

        public event EventHandler UpdateContextMenu;

        public abstract void AttachLoggerToFiles(Logger logger);

        private sealed class StateImplementation : ApplicationState
        {
            private ConfigFile _localConfigs;
            private FileList _logList;
            private NotificationSettings _notificationDetails;
            private ObservableCollection<ServiceGroup> _serviceGroups;
            private ObservableCollection<ServiceInfo> _serviceInfos;
            private ServiceList _services;
            private EmailInfo _smtpDetails;
            private TrackingList _state;
            private readonly FileAttacher _attacher;

            public StateImplementation(FileAttacher attacher)
            {
                _attacher = attacher;
                LogList = FileList.Default;
                SMTPDetails = EmailInfo.Default;
                NotificationDetails = NotificationSettings.Default;
                ServiceGroups = new ObservableCollection<ServiceGroup>();
                ServiceInfos = new ObservableCollection<ServiceInfo>();
                LocalConfigs = ConfigFile.Default;
                LogList = FileList.Default;
                SMTPDetails = EmailInfo.Default;
                ServiceInfos = new ObservableCollection<ServiceInfo>();
                NotificationDetails = NotificationSettings.GetInstance();

                ServiceInfos.CollectionChanged += ServiceInfos_CollectionChanged;

                Services = ServiceList.Default;
                State = TrackingList.Default;

                ServiceGroups.CollectionChanged += OnCollectionChanged;
                ServiceInfos.CollectionChanged += OnCollectionChanged;
            }

            public override TrackingList State
            {
                get { return _state; }
                set
                {
                    if (_state == value) return;
                    _state = value;
                    OnPropertyChanged();
                }
            }

            internal override ObservableCollection<ServiceGroup> ServiceGroups
            {
                get { return _serviceGroups; }
                set
                {
                    if (_serviceGroups == value) return;
                    _serviceGroups = value;
                    OnPropertyChanged();
                }
            }

            public override ServiceList Services
            {
                get { return _services; }
                set
                {
                    if (_services == value) return;
                    _services = value;
                    OnPropertyChanged();
                }
            }

            public override ConfigFile LocalConfigs
            {
                get { return _localConfigs; }
                set
                {
                    if (_localConfigs == value) return;
                    _localConfigs = value;
                    OnPropertyChanged();
                }
            }

            public override EmailInfo SMTPDetails
            {
                get { return _smtpDetails; }
                set
                {
                    if (_smtpDetails == value) return;
                    _smtpDetails = value;
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

            public override ObservableCollection<ServiceInfo> ServiceInfos
            {
                get { return _serviceInfos; }
                set
                {
                    if (_serviceInfos == value) return;
                    _serviceInfos = value;
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

            public override void ServiceInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (PropertyChangedBase item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (PropertyChangedBase item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            public override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }

            protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (PropertyChangedBase item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (PropertyChangedBase item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            public override void OnServicesChanged()
            {
                var handler = UpdateContextMenu;
                if (handler == null) return;
                handler(this, EventArgs.Empty);
            }

            public override void AttachLoggerToFiles(Logger logger)
            {
                foreach (var service in LocalConfigs.Services.Items)
                {
                    _attacher.AttachToList(logger, service.ConfigFiles);
                    _attacher.AttachToList(logger, service.LogFiles);
                }
                _attacher.AttachToList(logger, LocalConfigs.LogList.Items);
                
                foreach (var service in Services.Items)
                {
                    _attacher.AttachToList(logger, service.ConfigFiles);
                    _attacher.AttachToList(logger, service.LogFiles);
                }
                _attacher.AttachToList(logger, LogList.Items);
            }
        }
    }


    internal abstract class FileAttacher
    {
        internal static FileAttacher GetInstance()
        {
            return new FileAttacherImp();
        }

        internal abstract void AttachToList(Logger logger, IList<ExternalFile> list);

        private sealed class FileAttacherImp : FileAttacher
        {
            internal override void AttachToList(Logger logger, IList<ExternalFile> list)
            {
                foreach (var file in list)
                {
                    file.AttachLogger(logger);
                }

                foreach (var file in list)
                {
                    file.CheckForDuplicateNames(list);
                }
            }
        }
    }
}