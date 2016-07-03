// -----------------------------------------------------------------------
//  <copyright file="Emailer.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.Diagnostics;
//using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Mail;
using ServiceSentry.Common.Email;

#endregion

namespace ServiceSentry.Model.Email
{
    public abstract class Emailer
    {
        internal static Emailer GetInstance(SmtpClientWrapper client, EmailBuilder builder)
        {
            //Contract.Requires(client != null);
            //Contract.Requires(builder != null);
            return new EmailerImplementation(client, builder);
        }

        public static Emailer GetInstance(SMTPInfo smtpInfo)
        {
            //Contract.Requires(smtpInfo != null);
            return new EmailerImplementation(SmtpClientWrapper.GetInstance(smtpInfo), EmailBuilder.Default);
        }

        public abstract bool SendServiceFailureNotification(TrackingObject state);
        public virtual event EmailEventHandler EmailCancelled;
        public virtual event EmailEventHandler EmailFailed;
        public virtual event EmailEventHandler EmailSent;
        internal abstract void OnSendCompleted(object sender, AsyncCompletedEventArgs e);

        private sealed class EmailerImplementation : Emailer
        {
            private readonly EmailBuilder _builder;
            private readonly SmtpClientWrapper _client;
            private readonly ICredentialsByHost _credential;
            private bool _result;

            internal EmailerImplementation(SmtpClientWrapper client, EmailBuilder builder)
            {
                //Contract.Requires(client != null && builder != null);
                _builder = builder;
                _client = client;
                _credential = _client.Credentials;
            }

            private void DisplayExceptionInConsole(Exception ex)
            {
                //Contract.Requires(ex != null);
                var exMessage = "  " + ex.Message;
                if (ex.InnerException != null)
                {
                    exMessage += Environment.NewLine;
                    exMessage += "  " + ex.InnerException.Message;
                }
                Console.WriteLine(exMessage);
            }

            private EmailEventHandler GetEmailHandler(AsyncCompletedEventArgs e)
            {
                //Contract.Requires(e != null);
                if (e.Cancelled) return EmailCancelled;
                return e.Error != null ? EmailFailed : EmailSent;
            }

            internal override void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine(Strings.Info_SendCompleted);
                _result = (!e.Cancelled && e.Error == null);

                var handler = GetEmailHandler(e);
                if (handler == null) return;

                if (e.Error != null) DisplayExceptionInConsole(e.Error);

                var args = new EmailEventArgs
                    {
                        Cancelled = e.Cancelled,
                        Message = (MailMessage) e.UserState,
                        Error = e.Error,
                        Client = _client,
                        Credential = _credential
                    };

                Trace.WriteLine(string.Format(Strings.Trace_RaisingEvent,
                                              e.Cancelled
                                                  ? "EmailCancelled"
                                                  : e.Error == null ? "EmailSent" : "EmailFailed"));
                handler(this, args);
            }

            public override bool SendServiceFailureNotification(TrackingObject state)
            {
                var packet = state.Packet;

                var emailInfo = packet.EmailInfo;

                // TODO: Replace this with a string

                if (string.IsNullOrEmpty(emailInfo.From))
                {
                    if (emailInfo.To != null && emailInfo.To.Count > 0)
                    {
                        emailInfo.From = Strings._ApplicationName + _builder.GetDomainFromEmail(emailInfo.To[0]);
                    }
                    throw new Exception("Email 'From' address is empty.");
                }

                var message = _builder.NewMailMessage(emailInfo);

                message.Subject = _builder.GetServiceFailureSubject(packet);

                var errorlist = _builder.AttachLogs(ref message, packet);

                var attachmentMessage = _builder.GetAttachmentMessage(message, packet);

                message.Body = _builder.GetServiceFailureMessageBody(packet,
                                                                     emailInfo.IsBodyHtml,
                                                                     attachmentMessage,
                                                                     errorlist);

                try
                {
                    _result = false;

                    _client.SendCompleted += OnSendCompleted;
                    _client.SendAsync(message);

                    return _result;
                }
                catch (Exception ex)
                {
                    state.Exceptions.Add(ex);
                    return false;
                }
            }
        }
    }

    // A delegate type for hooking up email event notifications.
    public delegate void EmailEventHandler(object sender, EmailEventArgs e);

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class EmailEventArgs : EventArgs
    {
        public SmtpClientWrapper Client { get; set; }
        public MailMessage Message { get; set; }
        public ICredentialsByHost Credential { get; set; }
        public bool Cancelled { get; set; }
        public Exception Error { get; set; }

        public new static EmailEventArgs Empty
        {
            get { return new EmailEventArgs(); }
        }
    }
}