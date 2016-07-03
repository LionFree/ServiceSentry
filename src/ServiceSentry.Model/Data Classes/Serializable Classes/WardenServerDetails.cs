// -----------------------------------------------------------------------
//  <copyright file="WardenServerDetails.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Runtime.Serialization;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof(WardenServerDetailsImplementation))]
    public abstract class WardenServerDetails : EquatableBase
    {
        public static WardenServerDetails Default
        {
            get { return GetInstance(string.Empty, 0); }
        }

        internal static WardenServerDetails GetInstance(string hostName, int port)
        {
            return new WardenServerDetailsImplementation(hostName, port);
        }
        
        #region Abstract Members

        [DataMember]
        public abstract string HostName { get; set; }

        [DataMember]
        public abstract int Port { get; set; }

        
        #endregion

        #region Special Methods

        /// <summary>
        ///     Converts the value of this <see cref="WardenServerDetails" /> to its equivalent string representation.
        /// </summary>
        public override string ToString()
        {
            return HostName + ":" + Port;
        }

        /// <summary>
        ///     Determines whether the specifies <see cref="object" /> is equal to the current <see cref="WardenServerDetails" />.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="object" /> to compare with the current <see cref="WardenServerDetails" />.
        /// </param>
        public override bool Equals(object obj)
        {
            var item = obj as WardenServerDetails;
            if (item == null)
            {
                return false;
            }

            var same = (HostName == item.HostName) && (Port == item.Port);
            return same;
        }

        /// <summary>
        ///     Returns the hashcode for this <see cref="ExternalFile" />.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 3;
                var hash = 29;

                // be sure to check for nullity, etc.
                hash *= seed + (!string.IsNullOrEmpty(HostName) ? HostName.GetHashCode() : 0);
                hash *= seed + Port.GetHashCode();

                return hash;
            }
        }

        #endregion

        [DataContract]
        private sealed class WardenServerDetailsImplementation : WardenServerDetails
        {
            private int _port;
            private string _hostName;

            internal WardenServerDetailsImplementation(string hostName, int port)
            {
                _hostName = hostName;
                _port = port;
            }

            #region Properties

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

            #endregion

            #region Events

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
            }

            #endregion
        }
    }
}