// -----------------------------------------------------------------------
//  <copyright file="ServiceNameLocator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;

#endregion

namespace ServiceSentry.Model.Client
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
        ///     The <see cref="Service.ServiceName" /> of a matching existing service.
        /// </returns>
        public abstract string GetServiceNameFromList(IEnumerable<string> listOfPossibleServiceNames);

        /// <summary>
        ///     Returns a value indicating whether the indicated
        ///     <see cref="Service" /> is valid.
        /// </summary>
        /// <param name="service">
        ///     The <see cref="Service" /> to check.
        /// </param>
        /// <returns>True if valid, otherwise false.</returns>
        public abstract bool IsValidService(Service service);

        public static ServiceNameLocator GetInstance()
        {
            return new ServiceNameLocatorImplementation(LocalServiceFinder.Default);
        }

        public static ServiceNameLocator GetInstance(LocalServiceFinder reader)
        {
            return new ServiceNameLocatorImplementation(reader);
        }

        private sealed class ServiceNameLocatorImplementation : ServiceNameLocator
        {
            private readonly LocalServiceFinder _reader;

            public ServiceNameLocatorImplementation(LocalServiceFinder reader)
            {
                _reader = reader;
            }

            public override string GetServiceNameFromList(IEnumerable<string> listOfPossibleServiceNames)
            {
                if (listOfPossibleServiceNames == null) throw new ArgumentNullException("listOfPossibleServiceNames");

                var services = _reader.GetServices();
                var existingService = string.Empty;

                foreach (var serviceName in listOfPossibleServiceNames)
                {
                    foreach (var s in services)
                    {
                        if (!s.ServiceName.Contains(serviceName)) continue;
                        existingService = s.ServiceName;
                        break;
                    }
                }
                return existingService;
            }

            public override bool IsValidService(Service service)
            {
                return service != null && service.IsInstalled;
            }
        }
    }
}