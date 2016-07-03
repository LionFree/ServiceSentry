// -----------------------------------------------------------------------
//  <copyright file="ServiceInfo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    public abstract class ServiceInfo : PropertyChangedBase
    {
        public abstract string ServiceName { get; set; }
        public abstract string DisplayName { get; set; }
        public abstract bool Selected { get; set; }
        public abstract string ServiceGroup { get; set; }
        public abstract string CommonName { get; set; }
        public abstract ObservableCollection<string> LogFileNames { get; set; }
        public abstract ObservableCollection<string> ConfigFileNames { get; set; }
        public abstract int? StartOrder { get; set; }
        public abstract int? StopOrder { get; set; }
        public abstract int? Timeout { get; set; }
        public abstract bool NotifyOnUnexpectedStop { get; set; }


        public static ServiceInfo GetInstance()
        {
            return new ServiceInfoImplementation();
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (ServiceInfo) obj;

            var same1 = (ServiceName == p.ServiceName);
            var same2 = (DisplayName == p.DisplayName);
            var same3 = (Selected == p.Selected);

            var same = (same1 && same2 && same3);

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
                hash *= seed + (ServiceName != null ? ServiceName.GetHashCode() : 0);
                hash *= seed + (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hash *= seed + Selected.GetHashCode();
                return hash;
            }
        }


        private sealed class ServiceInfoImplementation : ServiceInfo
        {
            private string _commonName;
            private ObservableCollection<string> _configFileNames = new ObservableCollection<string>();
            private string _displayName;
            private ObservableCollection<string> _logFileNames = new ObservableCollection<string>();
            private bool _notifyOnUnexpectedStop;
            private bool _selected;
            private string _serviceGroup;
            private string _serviceName;
            private int? _startOrder;
            private int? _stopOrder;
            private int? _timeout;

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

            public override string DisplayName
            {
                get { return _displayName; }
                set
                {
                    if (_displayName == value) return;
                    _displayName = value;
                    OnPropertyChanged();
                }
            }

            public override bool Selected
            {
                get { return _selected; }
                set
                {
                    if (_selected == value) return;
                    _selected = value;
                    OnPropertyChanged();
                }
            }

            public override string ServiceGroup
            {
                get { return _serviceGroup; }
                set
                {
                    if (_serviceGroup == value) return;
                    _serviceGroup = value;
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

            public override ObservableCollection<string> LogFileNames
            {
                get { return _logFileNames; }
                set
                {
                    if (_logFileNames == value) return;
                    _logFileNames = value;
                    OnPropertyChanged();
                }
            }

            public override ObservableCollection<string> ConfigFileNames
            {
                get { return _configFileNames; }
                set
                {
                    if (_configFileNames == value) return;
                    _configFileNames = value;
                    OnPropertyChanged();
                }
            }

            public override int? StartOrder
            {
                get { return _startOrder; }
                set
                {
                    if (_startOrder == value) return;
                    _startOrder = value;
                    OnPropertyChanged();
                }
            }

            public override int? StopOrder
            {
                get { return _stopOrder; }
                set
                {
                    if (_stopOrder == value) return;
                    _stopOrder = value;
                    OnPropertyChanged();
                }
            }

            public override int? Timeout
            {
                get { return _timeout; }
                set
                {
                    if (_timeout == value) return;
                    _timeout = value;
                    OnPropertyChanged();
                }
            }

            public override bool NotifyOnUnexpectedStop
            {
                get { return _notifyOnUnexpectedStop; }
                set
                {
                    if (_notifyOnUnexpectedStop == value) return;
                    _notifyOnUnexpectedStop = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}