// -----------------------------------------------------------------------
//  <copyright file="BindingBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

#endregion

namespace ServiceSentry.Common.Communication
{
    internal abstract class HostConfigBuilder
    {
        /// <summary>
        ///     Gets a new instance of the <see cref="HostConfigBuilder" /> class.
        /// </summary>
        internal static HostConfigBuilder GetInstance(ServiceHostType serviceHostType)
        {
            return GetInstance(EndpointBuilder.GetInstance(serviceHostType),
                BindingBuilder.GetInstance(serviceHostType),
                BehaviorBuilder.GetInstance(serviceHostType),
                AuthorizationBuilder.GetInstance(serviceHostType));
        }

        internal static HostConfigBuilder GetInstance(EndpointBuilder endpointBuilder, BindingBuilder bindingBuilder,
            BehaviorBuilder behaviorBuilder, AuthorizationBuilder authorizationBuilder)
        {
            return new ConfigBuilderImplementation(endpointBuilder, bindingBuilder, behaviorBuilder,
                authorizationBuilder);
        }

        /// <summary>
        ///     Gets a new <see cref="Binding" />.
        /// </summary>
        internal abstract Binding GetBinding();

        /// <summary>
        ///     Gets a new endpoint base address with the given servername and port.
        /// </summary>
        internal abstract string GetEndpointBase(string serverName, int port);

        /// <summary>
        ///     Gets a new <see cref="IEndpointBehavior" />.
        /// </summary>
        internal abstract IEndpointBehavior GetBehavior();

        /// <summary>
        ///     Configured the host.
        /// </summary>
        internal abstract void ConfigureAuthorization(ServiceHost host);

        private sealed class ConfigBuilderImplementation : HostConfigBuilder
        {
            private readonly AuthorizationBuilder _authorizationBuilder;
            private readonly BehaviorBuilder _behaviorBuilder;
            private readonly BindingBuilder _bindingBuilder;
            private readonly EndpointBuilder _endpointBuilder;

            internal ConfigBuilderImplementation(
                EndpointBuilder endpointBuilder,
                BindingBuilder bindingBuilder,
                BehaviorBuilder behaviorBuilder, AuthorizationBuilder authorizationBuilder)
            {
                _endpointBuilder = endpointBuilder;
                _bindingBuilder = bindingBuilder;
                _behaviorBuilder = behaviorBuilder;
                _authorizationBuilder = authorizationBuilder;
            }

            internal override Binding GetBinding()
            {
                return _bindingBuilder.GetBinding();
            }

            internal override string GetEndpointBase(string serverName, int port)
            {
                return _endpointBuilder.GetEndpointBase(serverName, port);
            }

            internal override IEndpointBehavior GetBehavior()
            {
                return _behaviorBuilder.GetBehavior();
            }

            internal override void ConfigureAuthorization(ServiceHost host)
            {
                _authorizationBuilder.ConfigureAuthorization(host);
            }
        }
    }
}