// -----------------------------------------------------------------------
//  <copyright file="BindingBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;

#endregion

namespace ServiceSentry.Common.Communication
{
    internal abstract class BindingBuilder
    {
        /// <summary>
        ///     Gets a new instance of the
        ///     <see>
        ///         <cref>BindingBuilder2</cref>
        ///     </see>
        ///     class.
        /// </summary>
        internal static BindingBuilder GetInstance(ServiceHostType serviceHostType)
        {
            if (serviceHostType == ServiceHostType.NetTcp)
                return new NetTcpBindingImplementation();

            if (serviceHostType == ServiceHostType.Http)
                return new WebHttpBindingImplementation();

            throw new InvalidOperationException("ServiceHostType must be either net.tcp or http.");
        }

        /// <summary>
        ///     Gets a new <see cref="Binding" />.
        /// </summary>
        internal abstract Binding GetBinding();

        private sealed class NetTcpBindingImplementation : BindingBuilder
        {
            internal override Binding GetBinding()
            {
                var binding = new NetTcpBinding(SecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                binding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                binding.PortSharingEnabled = true;
                return binding;
            }
        }

        private sealed class WebHttpBindingImplementation : BindingBuilder
        {
            internal override Binding GetBinding()
            {
                return new WebHttpBinding();
            }
        }
    }
}