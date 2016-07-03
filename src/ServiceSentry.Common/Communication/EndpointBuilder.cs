// -----------------------------------------------------------------------
//  <copyright file="EndpointBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;

#endregion

namespace ServiceSentry.Common.Communication
{
    internal abstract class EndpointBuilder
    {
        /// <summary>
        ///     Gets a new instance of the <see cref="EndpointBuilder" /> class.
        /// </summary>
        internal static EndpointBuilder GetInstance(ServiceHostType serviceHostType)
        {
            if (serviceHostType == ServiceHostType.NetTcp)
                return new NetTcpEndpointImplementation();

            if (serviceHostType == ServiceHostType.Http)
                return new WebHttpEndpointImplementation();

            throw new InvalidOperationException("ServiceHostType must be either net.tcp or http.");
        }

        /// <summary>
        ///     Gets a net.tcp endpoint with the given servername and
        ///     endpoint suffix.
        /// </summary>
        internal abstract string GetEndpoint(string server, string endpointSuffix, int port);

        /// <summary>
        ///     Gets a net.tcp endpoint base with the given servername.
        /// </summary>
        internal abstract string GetEndpointBase(string serverName, int port);

        private sealed class NetTcpEndpointImplementation : EndpointBuilder
        {
            internal override string GetEndpoint(string serverName, string endpointSuffix, int port)
            {
                return string.Format(CultureInfo.InvariantCulture, "net.tcp://{0}/{1}", serverName, endpointSuffix);
            }

            internal override string GetEndpointBase(string serverName, int port)
            {
                return String.Format(CultureInfo.InvariantCulture, "net.tcp://{0}", serverName);
            }
        }

        private sealed class WebHttpEndpointImplementation : EndpointBuilder
        {
            internal override string GetEndpoint(string serverName, string endpointSuffix, int port)
            {
                return string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/{2}", serverName, port,
                    endpointSuffix);
            }

            internal override string GetEndpointBase(string serverName, int port)
            {
                return String.Format(CultureInfo.InvariantCulture, "http://{0}:{1}", serverName, port);
            }
        }
    }
}