// -----------------------------------------------------------------------
//  <copyright file="ServiceMetadata.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace ServiceSentry.Common.ServiceFramework
{
    internal sealed class ServiceMetadata
    {
        internal string EventLogName { get; set; }
        internal string EventLogSource { get; set; }
        internal WindowsService Implementation { get; set; }
        internal string LongDescription { get; set; }
        internal bool Quiet { get; set; }
        internal string ServiceName { get; set; }
        internal string ShortDescription { get; set; }
        internal bool Silent { get; set; }
    }
}