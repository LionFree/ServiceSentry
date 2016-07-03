// -----------------------------------------------------------------------
//  <copyright file="HostBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using ServiceSentry.Common.Resources;

#endregion

namespace ServiceSentry.Common.Communication
{
    public abstract class HostBuilder
    {
        public static HostBuilder GetInstance(ServiceHostType serviceHostType)
        {
            return GetInstance(HostConfigBuilder.GetInstance(serviceHostType));
        }


        internal static HostBuilder GetInstance(HostConfigBuilder configBuilder)
        {
            return new HostBuilderImplementation(configBuilder);
        }

        public abstract ServiceHost BuildHost(WindowsServiceDescription serviceDescription);

        private class HostBuilderImplementation : HostBuilder
        {
            private readonly HostConfigBuilder _configBuilder;

            internal HostBuilderImplementation(HostConfigBuilder configBuilder)
            {
                _configBuilder = configBuilder;
            }

            public override ServiceHost BuildHost(WindowsServiceDescription description)
            {
                string endpointBase = _configBuilder.GetEndpointBase(Dns.GetHostEntry("LocalHost").HostName,
                    description.Port);
                var baseAddresses = new Uri(endpointBase);

                ServiceHost host = ManufactureServiceHostObject(baseAddresses, description);

                Binding binding = _configBuilder.GetBinding();
                ServiceEndpoint endpoint = host.AddServiceEndpoint(description.Contract, binding,
                    description.EndpointSuffix);

                IEndpointBehavior bindingBehavior = _configBuilder.GetBehavior();
                if (bindingBehavior != null) endpoint.Behaviors.Add(bindingBehavior);
                
                _configBuilder.ConfigureAuthorization(host);

                return host;
            }


            private ServiceHost ManufactureServiceHostObject(Uri baseAddresses, WindowsServiceDescription description)
            {
                if (description.ServiceType != null)
                {
                    return new ServiceHost(description.ServiceType, baseAddresses);
                }

                if (description.ServiceObject != null)
                {
                    return new ServiceHost(description.ServiceObject, baseAddresses);
                }

                throw new InvalidOperationException(CommonStrings.EXCEPTION_ServiceDescriptionNeedsTypeOrObject);
            }
        }
    }
}