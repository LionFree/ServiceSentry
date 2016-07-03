// -----------------------------------------------------------------------
//  <copyright file="StatusToStateExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ServiceProcess;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model
{
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Converts this <see cref="ServiceControllerStatus" /> to the corresponding <see cref="ServiceState" />.
        /// </summary>
        /// <returns>
        ///     The corresponding <see cref="ServiceState" />.
        /// </returns>
        public static ServiceState ToState(this ServiceControllerStatus status)
        {
            switch (status)
            {
                case ServiceControllerStatus.Running:
                    return ServiceState.Running;

                case ServiceControllerStatus.Stopped:
                    return ServiceState.Stopped;

                case ServiceControllerStatus.Paused:
                    return ServiceState.Paused;

                case ServiceControllerStatus.StartPending:
                    return ServiceState.StartPending;

                case ServiceControllerStatus.StopPending:
                    return ServiceState.StopPending;

                case ServiceControllerStatus.ContinuePending:
                    return ServiceState.ContinuePending;

                case ServiceControllerStatus.PausePending:
                    return ServiceState.PausePending;
            }
            
            throw new ArgumentException(Strings.EXCEPTION_InvalidServiceControllerStatus);
        }

        /// <summary>
        ///     Converts this <see cref="ServiceState" /> to the corresponding <see cref="ServiceControllerStatus" />.
        /// </summary>
        /// <returns>
        ///     The corresponding <see cref="ServiceControllerStatus" />.
        /// </returns>
        public static ServiceControllerStatus ToStatus(this ServiceState state)
        {
            switch (state)
            {
                case ServiceState.Running:
                    return ServiceControllerStatus.Running;

                case ServiceState.Stopped:
                    return ServiceControllerStatus.Stopped;

                case ServiceState.Paused:
                    return ServiceControllerStatus.Paused;

                case ServiceState.StartPending:
                    return ServiceControllerStatus.StartPending;

                case ServiceState.StopPending:
                    return ServiceControllerStatus.StopPending;

                case ServiceState.ContinuePending:
                    return ServiceControllerStatus.ContinuePending;

                case ServiceState.PausePending:
                    return ServiceControllerStatus.PausePending;
            }

            throw new ArgumentException(Strings.EXCEPTION_InvalidServiceState);
        }
    }
}