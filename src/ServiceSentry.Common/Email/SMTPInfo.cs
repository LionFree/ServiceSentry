// -----------------------------------------------------------------------
//  <copyright file="EmailInfo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Common.Email
{
    [DataContract, KnownType(typeof (SMTPInfoImplementation))]
    public abstract class SMTPInfo : EquatableBase
    {
        /// <summary>
        ///     Creates an instance of the <see cref="SMTPInfo" />
        ///     class, populated with the default values.
        /// </summary>
        public static SMTPInfo Default { get { return new SMTPInfoImplementation { HostName = "", Port = 0 }; } }

        #region Abstract Members
        
        [DataMember(IsRequired = false)]
        public abstract string HostName { get; set; }

        [DataMember(IsRequired = false)]
        public abstract string PickupDirectoryLocation { get; set; }

        [DataMember(IsRequired = false)]
        public abstract int Port { get; set; }

        [DataMember(IsRequired = false)]
        public abstract bool UseDefaultCredentials { get; set; }

        [DataMember(IsRequired = false)]
        public abstract SmtpDeliveryMethod DeliveryMethod { get; set; }

        [DataMember(IsRequired = false)]
        public abstract bool EnableSsl { get; set; }

        [DataMember(IsRequired = false)]
        public abstract ICredentialsByHost Credentials { get; set; }

        [DataMember(IsRequired = false)]
        public abstract int MaxMailsPerMinute { get; set; }

        [DataMember(IsRequired = false)]
        public abstract int MaxMailsPerDay { get; set; }

        #endregion

        #region Equality Comparer

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (SMTPInfo)obj;
            
            var sameHost = (HostName == p.HostName);
            var samePort = (Port == p.Port);
            var sameDefault = (UseDefaultCredentials == p.UseDefaultCredentials);
            var sameDelivery = (DeliveryMethod == p.DeliveryMethod);
            var sameSsl = (EnableSsl == p.EnableSsl);
            var sameCredentials = (Credentials == p.Credentials);
            var sameDay = (MaxMailsPerDay == p.MaxMailsPerDay);
            var sameMinute = (MaxMailsPerMinute == p.MaxMailsPerMinute);
            
            var same = (sameHost && samePort && sameDefault && sameDelivery &&
                        sameSsl && sameCredentials && sameMinute && sameDay);

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
                hash *= seed + (HostName != null ? HostName.GetHashCode() : 0);
                hash *= seed + Port.GetHashCode();
                hash *= seed + UseDefaultCredentials.GetHashCode();
                hash *= seed + EnableSsl.GetHashCode();
                hash *= seed + DeliveryMethod.GetHashCode();
                hash *= seed + (Credentials != null ? Credentials.GetHashCode() : 0);
                hash *= seed + MaxMailsPerMinute.GetHashCode();
                hash *= seed + MaxMailsPerDay.GetHashCode();
                
                return hash;
            }
        }

        #endregion

        [DataContract]
        private sealed class SMTPInfoImplementation : SMTPInfo
        {
            private string _hostName = "";
            private int _port;
            private SmtpDeliveryMethod _deliveryMethod;
            private string _pickupLocation = "";
            private ICredentialsByHost _credentials;
            private bool _useDefaultCredentials;
            private bool _enableSsl;
            private int _maxMailsPerMinute;
            private int _maxMailsPerDay;
            
            internal SMTPInfoImplementation()
            {
                _hostName = string.Empty;
                _port = 0;
                _credentials = null;
                _pickupLocation = string.Empty;
            }

            public override string HostName
            {
                get { return _hostName; }
                set
                {
                    if (_hostName == value) return;
                    _hostName = value;
                    OnPropertyChanged();
                }
            }

            public override string PickupDirectoryLocation
            {
                get { return _pickupLocation; }
                set
                {
                    if (_pickupLocation == value) return;
                    _pickupLocation = value;
                    OnPropertyChanged();
                }
            }

            public override int Port
            {
                get { return _port; }
                set
                {
                    if (_port == value) return;
                    _port = value;
                    OnPropertyChanged();
                }
            }
            
            public override bool UseDefaultCredentials
            {
                get { return _useDefaultCredentials; }
                set
                {
                    if (_useDefaultCredentials == value) return;
                    _useDefaultCredentials = value;
                    OnPropertyChanged();
                }
            }

            public override SmtpDeliveryMethod DeliveryMethod
            {
                get { return _deliveryMethod; }
                set
                {
                    if (_deliveryMethod == value) return;
                    _deliveryMethod = value;
                    OnPropertyChanged();
                }
            }

            public override bool EnableSsl
            {
                get { return _enableSsl; }
                set
                {
                    if (_enableSsl == value) return;
                    _enableSsl = value;
                    OnPropertyChanged();
                }
            }

            public override ICredentialsByHost Credentials
            {
                get { return _credentials; }
                set
                {
                    if (_credentials == value) return;
                    _credentials = value;
                    OnPropertyChanged();
                }
            }
            
            public override int MaxMailsPerMinute
            {
                get { return _maxMailsPerMinute; }
                set
                {
                    if (_maxMailsPerMinute == value) return;
                    _maxMailsPerMinute = value;
                    OnPropertyChanged();
                }
            }

            public override int MaxMailsPerDay
            {
                get { return _maxMailsPerDay; }
                set
                {
                    if (_maxMailsPerDay == value) return;
                    _maxMailsPerDay = value;
                    OnPropertyChanged();
                }
            }
           

            [OnDeserializing]
            private void BeforeDeserialization(StreamingContext context)
            {
                // Note to self: 
                // Deserialization does not normally respect default values.
                // In order to force nontrivial default values, this method 
                // (with the [OnDeserializing] attribute) is called at the 
                // beginning of the deserialization process.  
                // This is where we can initialize members with their nontrivial
                // default values.  
                _hostName = string.Empty;
                _port = 0;
                _credentials = null;
                _pickupLocation = string.Empty;
            }
        }
    }
}