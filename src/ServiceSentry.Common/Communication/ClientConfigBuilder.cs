// -----------------------------------------------------------------------
//  <copyright file="BindingBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceModel.Channels;

#endregion

namespace ServiceSentry.Common.Communication
{
    public abstract class ClientConfigBuilder
    {
        /// <summary>
        ///     Gets a new instance of the <see cref="ClientConfigBuilder" /> class.
        /// </summary>
        public static ClientConfigBuilder GetInstance(ServiceHostType serviceHostType)
        {
            return GetInstance(EndpointBuilder.GetInstance(serviceHostType),
                BindingBuilder.GetInstance(serviceHostType));
        }

        internal static ClientConfigBuilder GetInstance(EndpointBuilder endpointBuilder, BindingBuilder bindingBuilder)
        {
            return new ConfigBuilderImplementation(endpointBuilder, bindingBuilder);
        }

        /// <summary>
        ///     Gets a new <see cref="Binding" />.
        /// </summary>
        public abstract Binding GetBinding();

        /// <summary>
        ///     Gets a new endpoint base address with the given servername and port.
        /// </summary>
        public abstract string GetEndpoint(string serverName, string endpointSuffix, int port);


        private sealed class ConfigBuilderImplementation : ClientConfigBuilder
        {
            private readonly BindingBuilder _bindingBuilder;
            private readonly EndpointBuilder _endpointBuilder;

            internal ConfigBuilderImplementation(EndpointBuilder endpointBuilder, BindingBuilder bindingBuilder)
            {
                _endpointBuilder = endpointBuilder;
                _bindingBuilder = bindingBuilder;
            }

            public override Binding GetBinding()
            {
                return _bindingBuilder.GetBinding();
            }

            public override string GetEndpoint(string serverName, string endpointSuffix, int port)
            {
                return _endpointBuilder.GetEndpoint(serverName, endpointSuffix, port);
            }
        }
    }
}