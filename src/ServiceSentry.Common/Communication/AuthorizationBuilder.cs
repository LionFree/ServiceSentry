// -----------------------------------------------------------------------
//  <copyright file="BindingBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ServiceModel;
using System.ServiceModel.Description;

#endregion

namespace ServiceSentry.Common.Communication
{
    internal abstract class AuthorizationBuilder
    {
        /// <summary>
        ///     Gets a new instance of the <see cref="BehaviorBuilder" /> class.
        /// </summary>
        internal static AuthorizationBuilder GetInstance(ServiceHostType serviceHostType)
        {
            if (serviceHostType == ServiceHostType.NetTcp)
                return new NetTcpAuthorizationImplementation();

            if (serviceHostType == ServiceHostType.Http)
                return new WebHttpAuthorizationImplementation(HttpServiceAuthorizationManager.GetInstance());

            throw new InvalidOperationException("ServiceHostType must be either net.tcp or http.");
        }

        /// <summary>
        ///     Gets a new <see cref="IEndpointBehavior" />.
        /// </summary>
        internal abstract void ConfigureAuthorization(ServiceHost host);

        private sealed class NetTcpAuthorizationImplementation : AuthorizationBuilder
        {
            internal override void ConfigureAuthorization(ServiceHost host)
            {
            }
        }

        private sealed class WebHttpAuthorizationImplementation : AuthorizationBuilder
        {
            private readonly HttpServiceAuthorizationManager _manager;

            internal WebHttpAuthorizationImplementation(HttpServiceAuthorizationManager manager)
            {
                _manager = manager;
            }

            internal override void ConfigureAuthorization(ServiceHost host)
            {
                var serviceAuthorizationBehavior = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
                serviceAuthorizationBehavior.PrincipalPermissionMode = PrincipalPermissionMode.None;
                serviceAuthorizationBehavior.ServiceAuthorizationManager = _manager;

                var serviceDebugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                serviceDebugBehavior.HttpHelpPageEnabled = false;
                serviceDebugBehavior.IncludeExceptionDetailInFaults = true;
            }
        }
    }
}