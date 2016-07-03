// -----------------------------------------------------------------------
//  <copyright file="NotificationSettings.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Runtime.Serialization;
using ServiceSentry.Common.Email;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ImplementedNotificationSettings))]
    public abstract class NotificationSettings : EquatableBase
    {
        public static NotificationSettings Default
        {
            get { return new ImplementedNotificationSettings {ShouldEmail = false}; }
        }

        [DataMember(Name = "EmailOnUnexpectedStop", IsRequired = true)]
        public abstract bool ShouldEmail { get; set; }

        [DataMember]
        public abstract EmailInfo EmailInfo { get; set; }

        [DataMember]
        public abstract SMTPInfo SMTPInfo { get; set; }

        public static NotificationSettings GetInstance()
        {
            return Default;
        }

        #region Equality Comparer

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (NotificationSettings) obj;

            var sameNotify = (ShouldEmail == p.ShouldEmail);
            var sameEmail = (EmailInfo == p.EmailInfo);
            var sameSmtp = (SMTPInfo == p.SMTPInfo);

            var same = sameNotify && sameEmail && sameSmtp;

            return same;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 23;
                var hash = 3;

                // be sure to check for nullity, etc.
                hash *= seed + ShouldEmail.GetHashCode();
                hash *= seed + (EmailInfo != null ? EmailInfo.GetHashCode() : 0);
                hash *= seed + (SMTPInfo != null ? SMTPInfo.GetHashCode() : 0);
                return hash;
            }
        }

        public override string ToString()
        {
            return "ShouldEmail = " + ShouldEmail;
        }

        #endregion

        [DataContract]
        private class ImplementedNotificationSettings : NotificationSettings
        {
            private EmailInfo _emailInfo;
            private bool _shouldEmail;
            private SMTPInfo _smtpInfo;

            internal ImplementedNotificationSettings()
            {
                _emailInfo = EmailInfo.Default;
                _smtpInfo = SMTPInfo.Default;
            }

            public override bool ShouldEmail
            {
                get { return _shouldEmail; }
                set
                {
                    if (_shouldEmail == value) return;
                    _shouldEmail = value;
                    OnPropertyChanged();
                }
            }

            public override EmailInfo EmailInfo
            {
                get { return _emailInfo; }
                set
                {
                    if (_emailInfo == value) return;
                    _emailInfo = value;
                    OnPropertyChanged();
                }
            }

            public override SMTPInfo SMTPInfo
            {
                get { return _smtpInfo; }
                set
                {
                    if (_smtpInfo == value) return;
                    _smtpInfo = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}