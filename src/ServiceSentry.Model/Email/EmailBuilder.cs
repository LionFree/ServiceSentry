// -----------------------------------------------------------------------
//  <copyright file="EmailBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using ServiceSentry.Common.Email;
using ServiceSentry.Model.Communication;

#endregion

namespace ServiceSentry.Model.Email
{
    public abstract class EmailBuilder
    {
        public static EmailBuilder Default
        {
            get { return new EmailBuilderImplementation(); }
        }

        public abstract MailMessage NewMailMessage(EmailInfo smtpInfo);
        public abstract void AddToRecipient(EmailInfo smtpInfo, string recipient);
        public abstract void AddCCRecipient(EmailInfo smtpInfo, string recipient);


        public abstract ObservableCollection<string> AttachLogs(ref MailMessage message, SubscriptionPacket packet);

        public abstract string GetServiceFailureMessageBody(SubscriptionPacket packet, bool isBodyHtml,
                                                            string logsMessage, ObservableCollection<string> errorList);


        internal abstract string GetAttachmentMessage(MailMessage message, SubscriptionPacket packet);
        internal abstract string GetServiceStoppedMessage(SubscriptionPacket packet);
        internal abstract string GetServiceFailureSubject(SubscriptionPacket packet);
        internal abstract string GetDomainFromEmail(string emailAddress);

        private sealed class EmailBuilderImplementation : EmailBuilder
        {
            public override void AddToRecipient(EmailInfo smtpInfo, string recipient)
            {
                if (smtpInfo == null) throw new ArgumentNullException("info");
                if (recipient == null) throw new ArgumentNullException("recipient");

                smtpInfo.To.Add(recipient);
            }

            public override void AddCCRecipient(EmailInfo smtpInfo, string recipient)
            {
                if (smtpInfo == null) throw new ArgumentNullException("info");
                if (recipient == null) throw new ArgumentNullException("recipient");

                smtpInfo.CC.Add(recipient);
            }

            public override MailMessage NewMailMessage(EmailInfo emailInfo)
            {
                // From, Sender, ReplyTo
                if (emailInfo.From == null) throw new ArgumentException(Strings.EXCEPTION_FromCannotBeNull);

                var message = new MailMessage
                    {
                        From =
                            new MailAddress(emailInfo.From, Extensibility.Strings._ApplicationName,
                                            Encoding.GetEncoding(emailInfo.BodyEncoding)),
                        BodyEncoding = Encoding.GetEncoding(emailInfo.BodyEncoding),
                        IsBodyHtml = emailInfo.IsBodyHtml,
                        Priority = emailInfo.Priority,
                    };

                if (!string.IsNullOrEmpty(emailInfo.SenderAddress))
                    message.Sender = new MailAddress(emailInfo.SenderAddress, Extensibility.Strings._ApplicationName,
                                                     Encoding.GetEncoding(emailInfo.BodyEncoding));

                foreach (var recipient in emailInfo.To)
                {
                    message.To.Add(recipient);
                }

                return message;
            }

            public override ObservableCollection<string> AttachLogs(ref MailMessage message, SubscriptionPacket packet)
            {
                if (message == null) throw new ArgumentNullException("message");
                if (packet == null) throw new ArgumentNullException("service");

                var errorList = new ObservableCollection<string>();

                foreach (var logFile in packet.LogFiles)
                {
                    if (!logFile.Exists) continue;

                    try
                    {
                        var attachment = new Attachment(logFile.ParsedName);
                        message.Attachments.Add(attachment);
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof (IOException))
                        {
                            var errorMessage = string.Format(Strings.Error_CouldNotAttach1, logFile.ParsedName);

                            if (ex.Message.Contains("it is being used by another process"))
                            {
                                errorMessage += Strings.Error_CouldNotAttach2;
                            }

                            errorMessage += Strings.Noun_SentenceTerminator;
                            errorList.Add(errorMessage);
                        }
                        Trace.WriteLine(ex.Message);
                    }
                }
                return errorList;
            }

            internal override string GetAttachmentMessage(MailMessage message, SubscriptionPacket packet)
            {
                if (message == null) throw new ArgumentNullException("message");
                if (packet == null) throw new ArgumentNullException("packet");

                if (packet.LogFiles.Count == 0) return Strings.Info_NoLogFilesSpecified;

                if (packet.LogFiles.Count != 0 && message.Attachments.Count == 0)
                    return Strings.Info_LogsDoNotExist;

                if (message.Attachments.Count == packet.LogFiles.Count)
                    return Strings.Info_LogFilesAttached;

                return Strings.Info_ExistingLogFilesAttached;
            }

            internal override string GetServiceStoppedMessage(SubscriptionPacket packet)
            {
                var name = string.IsNullOrEmpty(packet.CommonName) ? packet.DisplayName : packet.CommonName;
                var actualName = string.IsNullOrEmpty(packet.CommonName)
                                     ? string.Empty
                                     : String.Format(Strings.Info_ServiceStoppedUnexpectedly1, packet.DisplayName);

                return (String.Format(Strings.Info_ServiceStoppedUnexpectedly, name, actualName));
            }

            internal override string GetServiceFailureSubject(SubscriptionPacket packet)
            {
                var name = string.IsNullOrEmpty(packet.CommonName) ? packet.DisplayName : packet.CommonName;
                return string.Format(Strings.Info_ServiceStoppedUnexpectedlyOnMachine, name, Dns.GetHostEntry("LocalHost").HostName);
            }

            internal override string GetDomainFromEmail(string emailAddress)
            {
                var parts = emailAddress.Split('@');
                if (parts.Length<2) throw new ArgumentException("parameter is not a valid email address.");
                return parts[1];
            }

            public override string GetServiceFailureMessageBody(SubscriptionPacket packet, bool isBodyHtml,
                                                                string logsMessage,
                                                                ObservableCollection<string> errorList)
            {
                var format = isBodyHtml ? TableFormatting.Html : TableFormatting.PlainText;

                var serviceStopped = GetServiceStoppedMessage(packet);

                var timeStamp = format.RowStart + Strings.Header_TimeStamp + format.CellDivider
                                + DateTime.Now + format.RowEnd;

                var machineName = format.RowStart + Strings.Header_MachineName + format.CellDivider
                                  + Environment.MachineName + format.RowEnd;

                var serviceName = format.RowStart + Strings.Header_ServiceName + format.CellDivider
                                  + packet.ServiceName + format.RowEnd;

                var displayName = format.RowStart + Strings.Header_DisplayName + format.CellDivider
                                  + packet.DisplayName + format.RowEnd;

                var output = serviceStopped
                             + format.LineBreak + format.LineBreak
                             + logsMessage
                             + format.LineBreak + format.LineBreak;
                if (errorList.Count > 0)
                {
                    foreach (var item in errorList)
                    {
                        output += item + format.LineBreak;
                    }

                    output += format.LineBreak + format.LineBreak;
                }
                output += format.TableStart + timeStamp + format.RowDivider + machineName + format.TableEnd
                          + format.RowDivider + format.LineBreak
                          + format.TableStart + serviceName + format.RowDivider + displayName + format.TableEnd
                          + format.LineBreak + format.LineBreak;

                return output;
            }
        }
    }

    internal sealed class TableFormatting
    {
        internal string TableStart { get; private set; }
        internal string TableEnd { get; private set; }
        internal string RowStart { get; private set; }
        internal string RowEnd { get; private set; }
        internal string CellDivider { get; private set; }
        internal string RowDivider { get; private set; }
        internal string LineBreak { get; private set; }

        internal static TableFormatting Html
        {
            get
            {
                return new TableFormatting
                    {
                        TableStart = "<table width=\"700\" border=\"1\" cellspacing=\"0\" cellpadding=\"5\">",
                        TableEnd = "</table>",
                        RowStart = "<tr><td>",
                        RowEnd = "</td></tr>",
                        CellDivider = "</td><td>",
                        RowDivider = string.Empty,
                        LineBreak = "<br />"
                    };
            }
        }

        internal static TableFormatting PlainText
        {
            get
            {
                return new TableFormatting
                    {
                        TableStart = string.Empty,
                        TableEnd = string.Empty,
                        RowStart = string.Empty,
                        RowEnd = string.Empty,
                        CellDivider = string.Empty,
                        RowDivider = Environment.NewLine,
                        LineBreak = Environment.NewLine,
                    };
            }
        }
    }
}