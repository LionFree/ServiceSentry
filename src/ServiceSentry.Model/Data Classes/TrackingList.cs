// -----------------------------------------------------------------------
//  <copyright file="TrackingList.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model
{
    public abstract class TrackingList : IDictionary<string, TrackingObject>
    {
        public static TrackingList Default
        {
            get { return new TrackingListImplementation(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Abstract Members

        public abstract IEnumerator<KeyValuePair<string, TrackingObject>> GetEnumerator();
        public abstract void Add(string key, TrackingObject value);
        public abstract void Add(KeyValuePair<string, TrackingObject> item);

        public abstract bool Remove(string key);
        public abstract bool Remove(KeyValuePair<string, TrackingObject> item);

        public abstract void Clear();

        public abstract bool Contains(KeyValuePair<string, TrackingObject> item);
        public abstract void CopyTo(KeyValuePair<string, TrackingObject>[] array, int arrayIndex);
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool ContainsKey(string key);
        public abstract bool TryGetValue(string key, out TrackingObject value);
        public abstract TrackingObject this[string key] { get; set; }
        public abstract ICollection<string> Keys { get; }
        public abstract ICollection<TrackingObject> Values { get; }

        #endregion

        private sealed class TrackingListImplementation : TrackingList
        {
            private readonly Dictionary<string, TrackingObject> _watch;

            public TrackingListImplementation()
            {
                _watch = new Dictionary<string, TrackingObject>();
            }

            public override int Count
            {
                get { return _watch.Count; }
            }

            public override bool IsReadOnly
            {
                get { return ((IDictionary) _watch).IsReadOnly; }
            }

            public override TrackingObject this[string key]
            {
                get { return _watch[key]; }
                set
                {
                    Console.WriteLine(Strings.Trace_SettingTrackingObjectKey, key);
                    _watch[key] = value;
                }
            }

            public override ICollection<string> Keys
            {
                get { return _watch.Keys; }
            }

            public override ICollection<TrackingObject> Values
            {
                get { return _watch.Values; }
            }

            public override IEnumerator<KeyValuePair<string, TrackingObject>> GetEnumerator()
            {
                return _watch.GetEnumerator();
            }


            public override void Clear()
            {
                _watch.Clear();
            }

            [DebuggerStepThrough]
            public override bool Contains(KeyValuePair<string, TrackingObject> item)
            {
                return _watch.Contains(item);
            }

            public override void CopyTo(KeyValuePair<string, TrackingObject>[] array, int arrayIndex)
            {
                ((IDictionary) _watch).CopyTo(array, arrayIndex);
            }

            public override bool Remove(KeyValuePair<string, TrackingObject> item)
            {
                return _watch.Remove(item.Key);
            }

            [DebuggerStepThrough]
            public override bool ContainsKey(string key)
            {
                return _watch.ContainsKey(key);
            }

            public override void Add(KeyValuePair<string, TrackingObject> item)
            {
                Add(item.Key, item.Value);
            }

            public override void Add(string key, TrackingObject value)
            {
                if (!_watch.ContainsKey(key))
                {
                    _watch.Add(key, value);
                    return;
                }
                _watch[key] = value;
            }

            public override bool Remove(string key)
            {
                return _watch.Remove(key);
            }

            public override bool TryGetValue(string key, out TrackingObject value)
            {
                return _watch.TryGetValue(key, out value);
            }
        }
    }

    public sealed class TrackingObject
    {
        public List<Exception> Exceptions;
        public List<DateTime> FailDates;
        public bool IsStopped;
        public bool IsToggling;
        public ServiceState LastState;

        public SubscriptionPacket Packet;
        public Timer Timer;
        private string _serviceName;
        private bool _warn;

        public TrackingObject()
        {
            Exceptions = new List<Exception>();
            FailDates = new List<DateTime>();
            Timer = new Timer();
            Packet = new SubscriptionPacket {MachineName = "."};
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set
            {
                _serviceName = value;
                Packet.ServiceName = value;
            }
        }

        public bool NotifyOnUnexpectedStop
        {
            get { return _warn; }
            set
            {
                _warn = value;
                Packet.NotifyOnUnexpectedStop = value;
            }
        }
    }
}