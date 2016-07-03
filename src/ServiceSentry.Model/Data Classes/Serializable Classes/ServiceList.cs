// -----------------------------------------------------------------------
//  <copyright file="ServiceList.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;
using ServiceSentry.Model.Events;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ImplementedServiceList))]
    public abstract class ServiceList : EquatableBase
    {
        public static ServiceList Default
        {
            get { return GetInstance(); }
        }

        internal static ServiceList GetInstance()
        {
            return new ImplementedServiceList();
        }

        #region Abstract Members

        [DataMember(Name = "Services", IsRequired = true)]
        public abstract ObservableCollection<Service> Items { get; set; }

        public abstract LoggingDetails LogDetails { get; set; }

        protected abstract void OnStatusChanged(StatusChangedEventArgs e);
        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        protected abstract void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public abstract event StatusChangedEventHandler StatusChanged;

        #endregion

        #region Special Method Overrides

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (ServiceList) obj;

            var sameCount = (Items.Count == p.Items.Count);

            var sameItems = true;
            if (sameCount)
            {
                for (var i = 0; i < p.Items.Count; i++)
                {
                    var itemA = p.Items[i];
                    var itemB = Items[i];
                    var sameItem = (itemA.Equals(itemB));
                    if (sameItem == false) sameItems = false;
                }
            }

            var sameDetails = LogDetails.Equals(p.LogDetails);
            var same = (sameCount && sameItems && sameDetails);

            return same;
        }

        public override int GetHashCode()
        {
            return Items.GetHashCode();
        }

        #endregion

        [DataContract]
        private sealed class ImplementedServiceList : ServiceList
        {
            #region Fields

            private ObservableCollection<Service> _items;

            private LoggingDetails _logDetails = LoggingDetails.GetInstance();

            #endregion

            public ImplementedServiceList()
            {
                Items = new ObservableCollection<Service>();
                Items.CollectionChanged += OnCollectionChanged;
            }

            #region Properties

            public override ObservableCollection<Service> Items
            {
                get { return _items; }
                set
                {
                    if (_items == value) return;
                    _items = value;
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
                    OnPropertyChanged();
                }
            }

            #endregion

            // Invoke the StatusChanged event; called whenever list changes
            protected override void OnStatusChanged(StatusChangedEventArgs e)
            {
                var handler = StatusChanged;
                if (handler == null) return;
                
                handler(this, e);
            }

            protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (Service item in e.NewItems)
                    {
                        //_modifiedItems.Add(item);
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (Service item in e.OldItems)
                {
                    //_modifiedItems.Add(item);
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }

            public override event StatusChangedEventHandler StatusChanged;
        }
    }
}