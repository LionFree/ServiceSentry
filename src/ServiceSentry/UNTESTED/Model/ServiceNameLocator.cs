// -----------------------------------------------------------------------
//  <copyright file="ServiceNameLocator.cs" company="Accelrys, Inc.">
//      Copyright (c) 2014 Accelrys, Inc.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using ServiceSentry.Model.DataClasses;
using ServiceSentry.Model.Wrappers;

#endregion

namespace ServiceSentry.Shit.Model
{
    public abstract class ServiceNameLocator
    {
        /// <summary>
        ///     Given a list of potential service names,
        ///     searches the installed Windows services,
        ///     and returns the first match.
        /// </summary>
        /// <param name="listOfPossibleServiceNames">A list of service names to search for.</param>
        /// <returns>
        ///     The <see cref="System.ServiceProcess.ServiceController.ServiceName" /> of a matching existing service.
        /// </returns>
        public abstract string GetServiceNameFromList(IEnumerable<string> listOfPossibleServiceNames);

        /// <summary>
        ///     Returns a value indicating whether the indicated
        ///     <see cref="Service" /> is valid.
        /// </summary>
        /// <param name="service">
        ///     The <see cref="ServiceSentry.Model.DataClasses.Service" /> to check.
        /// </param>
        /// <returns>True if valid, otherwise false.</returns>
        public abstract bool IsValidService(Service service);

        public static ServiceNameLocator GetInstance(Service service)
        {
            return new Implementation(service);
        }

        private sealed class Implementation : ServiceNameLocator
        {
            private readonly Service _service;

            public Implementation(Service service)
            {
                _service = service;
            }

            /// <summary>
            ///     Given a list of potential service names,
            ///     searches the installed Windows services,
            ///     and returns the first match.
            /// </summary>
            /// <param name="listOfPossibleServiceNames">A list of service names to search for.</param>
            /// <returns>
            ///     The <see cref="System.ServiceProcess.ServiceController.ServiceName" /> of a matching existing service.
            /// </returns>
            public override string GetServiceNameFromList(IEnumerable<string> listOfPossibleServiceNames)
            {
                if (listOfPossibleServiceNames == null) throw new ArgumentNullException("listOfPossibleServiceNames");

                var reader = ServiceListReader.GetInstance();

                var services = reader.GetServices();

                foreach (var serviceName in listOfPossibleServiceNames)
                {
                    Service existingService = null;
                    foreach (var s in services)
                    {
                        if (!s.ServiceName.Contains(serviceName)) continue;

                        existingService = s;
                        break;
                    }
                    if (existingService != null) return existingService.ServiceName;
                }
                return "";
            }

            /// <summary>
            ///     Returns a value indicating whether the indicated
            ///     <see cref="Service" /> is valid.
            /// </summary>
            /// <param name="service">
            ///     The <see cref="ServiceSentry.Model.DataClasses.Service" /> to check.
            /// </param>
            /// <returns>True if valid, otherwise false.</returns>
            public override bool IsValidService(Service service)
            {
                var mediator = ServiceMediator.GetInstance(service.ServiceName, service.MachineName);
                return mediator.IsInstalled;
            }
        }
    }
}