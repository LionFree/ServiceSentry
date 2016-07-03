// -----------------------------------------------------------------------
//  <copyright file="BindingBuilderTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using NUnit.Framework;
using ServiceSentry.Common.Communication;

#endregion

namespace ServiceSentry.Common.UnitTests.Communication
{
    [TestFixture]
    internal class BindingBuilderTests
    {
        private const TcpClientCredentialType ExpectedCredentials = TcpClientCredentialType.Windows;
        private const ProtectionLevel ExpectedProtection = ProtectionLevel.EncryptAndSign;


        [Test]
        public void Test_BindingBuilder_GetBinding()
        {
            // Arrange
            Binding binding = null;
            var sut = BindingBuilder.GetInstance(ServiceHostType.NetTcp);

            // Act
            Assert.DoesNotThrow(() => { binding = sut.GetBinding(); });
            var tcpBinding = binding as NetTcpBinding;
            
            // Assert
            Assert.IsNotNull(tcpBinding);
            var actualCredentials = tcpBinding.Security.Transport.ClientCredentialType;
            var actualProtection = tcpBinding.Security.Transport.ProtectionLevel;

            Assert.AreEqual(ExpectedCredentials, actualCredentials);
            Assert.AreEqual(ExpectedProtection, actualProtection);
        }
    }
}