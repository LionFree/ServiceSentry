// -----------------------------------------------------------------------
//  <copyright file="ThreadSafeObservableCollection.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

#endregion

namespace ServiceSentry.Extensibility
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
                foreach (NotifyCollectionChangedEventHandler nh in collectionChanged.GetInvocationList())
                {
                    var dispObj = nh.Target as DispatcherObject;
                    if (dispObj != null)
                    {
                        var dispatcher = dispObj.Dispatcher;
                        if (dispatcher != null && !dispatcher.CheckAccess())
                        {
                            var notificationHandler = nh;
                            dispatcher.BeginInvoke(
                                (Action)(() => notificationHandler.Invoke(this,
                                                          new NotifyCollectionChangedEventArgs(
                                                              NotifyCollectionChangedAction.Reset))),
                                DispatcherPriority.DataBind);
                            continue;
                        }
                    }
                    nh.Invoke(this, e);
                }
        }
    }
}