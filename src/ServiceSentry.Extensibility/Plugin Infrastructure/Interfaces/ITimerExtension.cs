// -----------------------------------------------------------------------
//  <copyright file="ITimerExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Threading;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Interfaces
{
    /// <summary>
    ///     Provides a generalized mechanism for exporting a
    ///     DispatcherTimer and its events as an extension.
    /// </summary>
    public interface ITimerExtension : IExtensionClass
    {
        /// <summary>
        ///     Gets the DispatcherTimer.
        /// </summary>
        DispatcherTimer Timer(Logger logger);
    }
}