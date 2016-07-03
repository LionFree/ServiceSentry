// -----------------------------------------------------------------------
//  <copyright file="FileChangedEventArgs.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Model.ExternalFiles
{
    public sealed class FileChangedEventArgs : EventArgs
    {
        public string ServiceName { get; set; }
        public string LogFileName { get; set; }
        public string LogFilePath { get; set; }
    }
}