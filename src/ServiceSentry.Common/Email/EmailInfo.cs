// -----------------------------------------------------------------------
//  <copyright file="EmailInfo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Common.Email
{
    [DataContract, KnownType(typeof (EmailInfoImplementation))]
    public abstract class EmailInfo : EquatableBase
    {
        /// <summary>
        ///     Creates an instance of the <see cref="EmailInfo" />
        ///     class, populated with the default values.
        /// </summary>
        public static EmailInfo Default
        {
            get { return new EmailInfoImplementation(); }
        }

        #region Abstract Members
        
        [DataMember(IsRequired = false)]
        public abstract bool IsBodyHtml { get; set; }

        [DataMember(IsRequired = false)]
        public abstract string From { get; set; }

        [DataMember(IsRequired = false)]
        public abstract string ReplyTo { get; set; }

        [DataMember(IsRequired = false)]
        public abstract ObservableCollection<string> To { get; set; }

        [DataMember(IsRequired = false)]
        public abstract ObservableCollection<string> CC { get; set; }

        [DataMember(IsRequired = false)]
        public abstract string BodyEncoding { get; set; }

        [DataMember(IsRequired = false)]
        public abstract MailPriority Priority { get; set; }
        
        [DataMember(IsRequired = false)]
        public abstract string SenderAddress { get; set; }

        #endregion

        #region Equality Comparer

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (EmailInfo) obj;

            // Verify that the entries in To are equal
            var sameTo = (To.Count == p.To.Count);
            if (sameTo)
            {
                for (var i = 0; i < To.Count; i++)
                {
                    if (To[i] != p.To[i])
                    {
                        return false;
                    }
                }
            }

            // Verify that the entries in CC are equal
            var sameCC = (CC.Count == p.CC.Count);
            if (sameCC)
            {
                for (var i = 0; i < CC.Count; i++)
                {
                    if (CC[i] != p.CC[i])
                    {
                        return false;
                    }
                }
            }

            var sameFrom = (From == p.From);
            var sameReplyTo = (ReplyTo == p.ReplyTo);
            var sameHTML = (IsBodyHtml == p.IsBodyHtml);
            var samePriority = (Priority == p.Priority);
            var sameEncoding = (BodyEncoding == p.BodyEncoding);
            var sameSender = (SenderAddress == p.SenderAddress);


            var same = (sameTo && sameCC && sameFrom && sameSender
                && sameReplyTo && sameHTML && samePriority && sameEncoding);

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
                hash *= seed + (From != null ? From.GetHashCode() : 0);
                hash *= seed + (ReplyTo != null ? ReplyTo.GetHashCode() : 0);
                hash *= seed + IsBodyHtml.GetHashCode();
                hash *= seed + Priority.GetHashCode();
                hash *= seed + (SenderAddress != null ? SenderAddress.GetHashCode() : 0);

                hash *= seed + (BodyEncoding != null ? BodyEncoding.GetHashCode() : 0);

                // Hash the contents of To
                foreach (var item in To)
                {
                    hash *= seed + (item != null ? item.GetHashCode() : 0);
                }

                // Hash the contents of CC
                foreach (var item in CC)
                {
                    hash *= seed + (item != null ? item.GetHashCode() : 0);
                }

                return hash;
            }
        }

        #endregion

        [DataContract]
        private sealed class EmailInfoImplementation : EmailInfo
        {
            private Encoding _bodyEncoding = Encoding.UTF8;
            private ObservableCollection<string> _cc = new ObservableCollection<string>();
            private string _from = "noreply@ServiceSentry.Curtis Kaler.com";
            private bool _isBodyHtml;
            private MailPriority _priority;
            private string _replyTo = "";
            private ObservableCollection<string> _to = new ObservableCollection<string>();
            private string _senderAddress;

            internal EmailInfoImplementation()
            {
                _priority = MailPriority.High;
                _replyTo = string.Empty;
                _from = string.Empty;
                _bodyEncoding = Encoding.GetEncoding(Encoding.UTF8.WebName);
                _to = new ObservableCollection<string>();
                _cc = new ObservableCollection<string>();
                _senderAddress = string.Empty;
            }

            public override bool IsBodyHtml
            {
                get { return _isBodyHtml; }
                set
                {
                    if (_isBodyHtml == value) return;
                    _isBodyHtml = value;
                    OnPropertyChanged();
                }
            }

            public override string From
            {
                get { return _from; }
                set
                {
                    if (_from == value) return;
                    _from = value;
                    OnPropertyChanged();
                }
            }

            public override string ReplyTo
            {
                get { return _replyTo; }
                set
                {
                    if (_replyTo == value) return;
                    _replyTo = value;
                    OnPropertyChanged();
                }
            }

            public override ObservableCollection<string> To
            {
                get { return _to; }
                set
                {
                    if (_to == value) return;
                    _to = value;
                    OnPropertyChanged();
                }
            }

            public override ObservableCollection<string> CC
            {
                get { return _cc; }
                set
                {
                    if (_cc == value) return;
                    _cc = value;
                    OnPropertyChanged();
                }
            }

            public override string BodyEncoding
            {
                get { return _bodyEncoding.WebName; }
                set
                {
                    if (value == null) return;
                    _bodyEncoding = Encoding.GetEncoding(value);
                }
            }

            public override MailPriority Priority
            {
                get { return _priority; }
                set
                {
                    if (_priority == value) return;
                    _priority = value;
                    OnPropertyChanged();
                }
            }

            public override string SenderAddress
            {
                get { return _senderAddress; }
                set
                {
                    if (_senderAddress == value) return;
                    _senderAddress = value;
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
                _replyTo = string.Empty;
                _from = string.Empty;
                _priority = MailPriority.High;
                _senderAddress = string.Empty;
                _bodyEncoding = Encoding.UTF8;
                _to = new ObservableCollection<string>();
                _cc = new ObservableCollection<string>();
            }
        }
    }
}