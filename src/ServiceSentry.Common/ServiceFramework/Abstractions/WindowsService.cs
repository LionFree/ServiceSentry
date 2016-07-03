// -----------------------------------------------------------------------
//  <copyright file="WindowsService.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    /// <summary>
    ///     The interface that any windows service should implement to be used
    ///     with ServiceSentry.ServiceFramework.
    /// </summary>
    public abstract class WindowsService : IDisposable
    {
        /// <summary>
        ///     A harness to which console output is directed.
        /// </summary>
        public abstract ConsoleHarness Harness { get; }

        /// <summary>
        ///     The name of the service.
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        ///     The endpoint address of the service.
        /// </summary>
        public abstract string Endpoint { get; }

        /// <summary>
        ///     Performs application-defined tasks associated with
        ///     freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     This method is called when the service gets a request to start.
        /// </summary>
        public abstract void OnStart(string[] args);

        /// <summary>
        ///     This method is called when the service gets a request to stop.
        /// </summary>
        public abstract void OnStop();

        /// <summary>
        ///     This method is called when the service gets a request to execute a custom command.
        /// </summary>
        public abstract void OnCustomCommand(int command);

        /// <summary>
        ///     This method is called when a service gets a request to pause,
        ///     but not stop completely.
        /// </summary>
        public virtual void OnPause()
        {
        }

        /// <summary>
        ///     This method is called when a service gets a request to resume
        /// </summary>
        public virtual void OnContinue()
        {
        }

        /// <summary>
        ///     This method is called when the machine the service is running on
        /// </summary>
        public virtual void OnShutdown()
        {
        }
    }
}