// -----------------------------------------------------------------------
//  <copyright file="StatusChangeEventHandler.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model.Events
{
    // A delegate type for hooking up change notifications.
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);


    public class StatusChangedEventArgs : EventArgs
    {
        public ServiceState OldStatus { get; set; }
        public ServiceState NewStatus { get; set; }
    }
}