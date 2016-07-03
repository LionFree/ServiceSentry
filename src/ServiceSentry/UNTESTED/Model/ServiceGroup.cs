// -----------------------------------------------------------------------
//  <copyright file="ServiceGroup.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ServiceSentry.Extensibility;
using ServiceSentry.Model;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Client.UNTESTED.Model
{
    internal abstract class ServiceGroup : ItemsCollection<Service>
    {
        public abstract string GroupName { get; set; }
        public abstract string Status { get; set; }

        internal static ServiceGroup GetInstance(string name, ObservableCollection<Service> collection)
        {
            return new ServiceGroupImplementation(name, collection);
        }

        private sealed class ServiceGroupImplementation : ServiceGroup
        {
            private string _groupName;
            private string _status;

            internal ServiceGroupImplementation(string name, IEnumerable<Service> collection)
            {
                GroupName = name;

                foreach (var service in collection.Where(service => service.ServiceGroup == GroupName))
                {
                    Items.Add(service);
                }
            }

            public override string GroupName
            {
                get { return _groupName; }
                set
                {
                    if (_groupName == value) return;
                    _groupName = value;
                    OnPropertyChanged("GroupName");
                }
            }

            public override string Status
            {
                get
                {
                    foreach (var item in Items.Where(item => item.Status != ServiceState.Running))
                    {
                        return item.Status.ToString();
                    }
                    return "Running";
                }
                set
                {
                    if (_status == value) return;
                    _status = value;
                    OnPropertyChanged();
                }
            }

            public override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
                OnPropertyChanged("Status");
            }
        }
    }
}