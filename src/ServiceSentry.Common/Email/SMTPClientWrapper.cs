// -----------------------------------------------------------------------
//  <copyright file="SMTPClientWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using ServiceSentry.Common.Resources;

#endregion

namespace ServiceSentry.Common.Email
{
    public abstract class SmtpClientWrapper
    {
        public abstract string PickupDirectoryLocation { get; set; }
        public abstract string Host { get; set; }
        public abstract SmtpDeliveryMethod DeliveryMethod { get; set; }
        public abstract bool UseDefaultCredentials { get; set; }
        public abstract int Port { get; set; }
        public abstract ICredentialsByHost Credentials { get; set; }
        public abstract bool EnableSsl { get; set; }
        public abstract void SendAsync(MailMessage message);

        public virtual event SendCompletedEventHandler SendCompleted;
        
        public static SmtpClientWrapper GetInstance(SMTPInfo info)
        {
            if (info==null) throw new ArgumentNullException("info");
            return new SmtpClientImplementation(info);
        }


        private sealed class SmtpClientImplementation : SmtpClientWrapper
        {
            private readonly SmtpClient _client;

            internal SmtpClientImplementation(SMTPInfo info)
            {
                if (info==null) throw new ArgumentNullException("info");
                _client = new SmtpClient
                    {
                        Host = info.HostName,
                        UseDefaultCredentials = info.UseDefaultCredentials,
                        DeliveryMethod = info.DeliveryMethod,
                        EnableSsl = info.EnableSsl
                    };

                if (!string.IsNullOrEmpty(info.PickupDirectoryLocation))
                    _client.PickupDirectoryLocation = info.PickupDirectoryLocation;

                if (info.Port != 0) _client.Port = info.Port;
                if (info.Credentials != null) _client.Credentials = info.Credentials;

                _client.SendCompleted += AsyncSendCompletedCallback;
            }

            private void AsyncSendCompletedCallback(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine(CommonStrings.INFO_EmailSent);
                
                var handler = SendCompleted;
                if (handler == null) return;

                handler(sender, e);
            }

            public override void SendAsync(MailMessage message)
            {
                // Some SMTP servers require that the client be authenticated before the 
                // server sends e-mail on its behalf. 
                // Set the UseDefaultCredentials property to true when this SmtpClient
                // object should, if requested by the server, authenticate using the
                // default credentials of the currently logged on user. For client
                // applications, this is the desired behavior in most scenarios.
                // Credentials information can also be specified using the
                // application and machine configuration files. For more information, see
                // http://msdn.microsoft.com/en-us/library/w355a94k(v=vs.110).aspx.
                // If the UseDefaultCredentials property is set to false, then the value set
                // in the Credentials property will be used for the credentials when
                // connecting to the server. If the UseDefaultCredentials property is set to
                // false and the Credentials property has not been set, then mail is sent to
                // the server anonymously.        
                // Caution:
                // If you provide credentials for basic authentication, they are sent to the
                // server in clear text. 
                // This can present a security issue because your credentials can be seen, 
                // and then used by others.

                _client.SendAsync(message, message);
            }

            #region Getters/Setters

            public override string PickupDirectoryLocation
            {
                get { return _client.PickupDirectoryLocation; }
                set
                {
                    // Add "~" (relative paths) support for pickupdirectories.
                    var root = AppDomain.CurrentDomain.BaseDirectory;
                    var pickupRoot = value.Replace("~/", root);
                    pickupRoot = pickupRoot.Replace("/", @"\");
                    _client.PickupDirectoryLocation = pickupRoot;
                }
            }

            public override string Host
            {
                get { return _client.Host; }
                set { _client.Host = value; }
            }

            public override SmtpDeliveryMethod DeliveryMethod
            {
                get { return _client.DeliveryMethod; }
                set { _client.DeliveryMethod = value; }
            }

            public override bool UseDefaultCredentials
            {
                get { return _client.UseDefaultCredentials; }
                set { _client.UseDefaultCredentials = value; }
            }

            public override int Port
            {
                get { return _client.Port; }
                set { if (value > 0) _client.Port = value; }
            }

            public override ICredentialsByHost Credentials
            {
                get { return _client.Credentials; }
                set { _client.Credentials = value; }
            }

            public override bool EnableSsl
            {
                get { return _client.EnableSsl; }
                set { _client.EnableSsl = value; }
            }

            #endregion
        }
    }
}