// -----------------------------------------------------------------------
//  <copyright file="PollResult.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model.Communication
{
    [DataContract, KnownType(typeof (PollResultImplementation))]
    [KnownType(typeof (SmtpException))]
    [KnownType(typeof (WebException))]
    public abstract class PollResult : EquatableBase
    {
        /// <summary>
        ///     A list of exceptions sent by the Monitor Service to the Client.
        /// </summary>
        [DataMember]
        public abstract Exception[] Exceptions { get; set; }

        /// <summary>
        ///     The name of the service being queried.
        /// </summary>
        [DataMember]
        public abstract string ServiceName { get; set; }

        /// <summary>
        ///     The <see cref="ServiceState" /> of the service.
        /// </summary>
        [DataMember]
        public abstract ServiceState State { get; set; }

        /// <summary>
        ///     Creates a new instance of the <see cref="PollResult" /> class.
        /// </summary>
        /// <returns></returns>
        public static PollResult GetInstance()
        {
            return new PollResultImplementation();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="PollResult" /> class.
        /// </summary>
        /// <returns>
        ///     A new instance of the <see cref="PollResult" /> class, populating the
        ///     <see cref="ServiceName" />, <see cref="State" />, and <see cref="Exceptions" /> properties.
        ///     The <see cref="Exceptions" /> parameter must not be null.
        /// </returns>
        public static PollResult GetInstance(string serviceName, ServiceState status, List<Exception> exceptions)
        {
            //Contract.Requires(exceptions != null);
            return new PollResultImplementation
                {
                    ServiceName = serviceName,
                    State = status,
                    Exceptions = exceptions.ToArray()
                };
        }

        [DataContract]
        private sealed class PollResultImplementation : PollResult
        {
            public PollResultImplementation()
            {
                Exceptions = new Exception[0];
            }

            public override Exception[] Exceptions { get; set; }
            public override string ServiceName { get; set; }
            public override ServiceState State { get; set; }

            public override bool Equals(object obj)
            {
                // Check for null values and compare run-time types.
                if (obj == null || GetType() != obj.GetType())
                    return false;

                if (ReferenceEquals(this, obj)) return true;

                var p = (PollResult) obj;

                var sameName = (ServiceName == p.ServiceName);
                var sameState = (State == p.State);

                var sameExceptions = (Exceptions.Length == p.Exceptions.Length);
                if (sameExceptions)
                {
                    for (var i = 0; i < p.Exceptions.Length; i++)
                    {
                        var itemA = p.Exceptions[i];
                        var itemB = Exceptions[i];
                        if (!itemA.Equals(itemB)) sameExceptions = false;
                    }
                }

                var same = (sameName && sameState && sameExceptions);

                return same;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap.
                {
                    // pick two prime numbers
                    const int seed = 7;
                    var hash = 11;

                    // be sure to check for nullity, etc.
                    hash *= seed + (ServiceName != null ? ServiceName.GetHashCode() : 0);
                    hash *= seed + State.GetHashCode();
                    hash *= seed + Exceptions.GetHashCode();
                    return hash;
                }
            }
        }
    }
}