// -----------------------------------------------------------------------
//  <copyright file="SubscriptionPacket.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using ServiceSentry.Common.Email;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model.Communication
{
    [DataContract]
    public class SubscriptionPacket
    {
        public SubscriptionPacket()
        {
            EmailRecipients = new List<string>();
            UnparsedConfigFileNames = new List<string>();
            UnparsedLogFileNames = new List<string>();
            LogFiles = new ObservableCollection<ExternalFile>();
            Timeout = TimeSpan.MaxValue;
            EmailInfo = EmailInfo.Default;
            SMTPInfo = SMTPInfo.Default;
        }

        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public ServiceState LastState { get; set; }

        [DataMember]
        public string CommonName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string MachineName { get; set; }

        [DataMember]
        public bool NotifyOnUnexpectedStop { get; set; }

        [DataMember]
        public List<string> EmailRecipients { get; set; }

        [DataMember]
        public TimeSpan Timeout { get; set; }

        [DataMember]
        public bool ArchiveLogs { get; set; }

        [DataMember]
        public bool ClearLogs { get; set; }
        
        [DataMember]
        public SMTPInfo SMTPInfo { get; set; }

        [DataMember]
        public EmailInfo EmailInfo { get; set; }

        [DataMember]
        public List<string> UnparsedLogFileNames { get; set; }

        [DataMember]
        public List<string> UnparsedConfigFileNames { get; set; }

        [DataMember]
        public ObservableCollection<ExternalFile> LogFiles { get; set; }

        [DataMember]
        public string WardenHost { get; set; }

        [DataMember]
        public int WardenPort { get; set; }


        public static SubscriptionPacket Empty()
        {
            return new SubscriptionPacket();
        }
    }
}