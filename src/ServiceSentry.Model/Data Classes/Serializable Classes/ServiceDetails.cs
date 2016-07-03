// -----------------------------------------------------------------------
//  <copyright file="ServiceDetails.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ImplementedServiceDetails))]
    public abstract class ServiceDetails : EquatableBase
    {
        [DataMember(IsRequired = false)]
        public abstract int Timeout { get; set; }

        [DataMember(IsRequired = false)]
        public abstract int StopOrder { get; set; }

        [DataMember(IsRequired = false)]
        public abstract int StartOrder { get; set; }

        [DataMember(IsRequired = true)]
        public abstract bool NotifyOnUnexpectedStop { get; set; }

        //[DataMember(IsRequired = true)]
        //public abstract string IPAddress { get; set; }

        public static ServiceDetails Default
        {
            get { return new ImplementedServiceDetails(); }
        }

        #region Equality Comparer

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (ServiceDetails) obj;

            var sameTimeout = (Timeout == p.Timeout);
            var sameStop = (StopOrder == p.StopOrder);
            var sameStart = (StartOrder == p.StartOrder);
            var sameWarning = (NotifyOnUnexpectedStop == p.NotifyOnUnexpectedStop);
            //var sameIPAddress = (IPAddress == p.IPAddress);

            var same = (sameTimeout && sameStart && sameStop && sameWarning);

            return same;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 7;
                var hash = 17;

                // be sure to check for nullity, etc.
                hash *= seed + Timeout.GetHashCode();
                hash *= seed + StartOrder.GetHashCode();
                hash *= seed + StopOrder.GetHashCode();
                //hash *= seed + (IPAddress != null ? IPAddress.GetHashCode() : 0);
                hash *= seed + NotifyOnUnexpectedStop.GetHashCode();
                return hash;
            }
        }

        #endregion

        public static ServiceDetails GetInstance()
        {
            return new ImplementedServiceDetails();
        }

        [DataContract]
        private sealed class ImplementedServiceDetails : ServiceDetails
        {
            private bool _notifyOnUnexpectedStop;
            private int _startOrder;
            private int _stopOrder;
            private int _timeout;
            
            internal ImplementedServiceDetails()
            {
                _notifyOnUnexpectedStop = false;
            }

            public override int Timeout
            {
                get { return _timeout; }
                set
                {
                    if (_timeout == value) return;
                    _timeout = value;
                    OnPropertyChanged();
                }
            }

            public override int StopOrder
            {
                get { return _stopOrder; }
                set
                {
                    if (_stopOrder == value) return;
                    _stopOrder = value;
                    OnPropertyChanged();
                }
            }

            public override int StartOrder
            {
                get { return _startOrder; }
                set
                {
                    if (_startOrder == value) return;
                    _startOrder = value;
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