// -----------------------------------------------------------------------
//  <copyright file="ApplicationStartPoint.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Monitor.Infrastructure
{
    internal static class ApplicationStartPoint
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            var logger = Logger.GetInstance();
            ServiceBootstrapper.GetInstance(MonitorService.GetInstance(logger), logger).Startup(args);
        }
    }
}