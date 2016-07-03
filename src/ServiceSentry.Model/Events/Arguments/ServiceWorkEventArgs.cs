// -----------------------------------------------------------------------
//  <copyright file="ServiceWorkEventArgs.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using ServiceSentry.Model.Client;

#endregion

namespace ServiceSentry.Model.Events.Arguments
{
    public class ServiceWorkEventArgs
    {
        public ObservableCollection<Service> Collection { get; set; }
        public ServiceAction Action { get; set; }
    }
}