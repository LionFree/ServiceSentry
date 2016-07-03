// -----------------------------------------------------------------------
//  <copyright file="NotificationOptionsViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Net.Mail;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class NotificationOptionsViewModel : PropertyChangedBase
    {
        public abstract string ApplicationName { get; }
        public abstract string ViewTitle { get; }
        public abstract bool ShouldEmail { get; set; }
        public abstract SmtpDeliveryMethod DeliveryMethod { get; set; }
        public abstract bool EnableSsl { get; set; }
        public abstract string SMTPHostName { get; set; }
        public abstract int SMTPHostPort { get; set; }
        public abstract string EmailSenderAddress { get; set; }
        public abstract int MaxEmailsPerMinute { get; set; }
        public abstract int MaxEmailsPerDay { get; set; }
        public abstract string AdministratorEmail { get; set; }

        internal static NotificationOptionsViewModel GetInstance(NotificationSettings settings, Logger logger)
        {
            return new NOVMImplementation(settings);
        }

        internal abstract NotificationSettings Commit(NotificationSettings notificationSettings);
        internal abstract void RefreshSettings(NotificationSettings notificationSettings);

        private sealed class NOVMImplementation : NotificationOptionsViewModel
        {
            private SmtpDeliveryMethod _deliveryMethod;
            private string _emailSenderAddress;
            private bool _enableSsl;
            private int _maxEmailsPerDay;
            private int _maxEmailsPerMinute;
            private bool _shouldEmail;
            private string _smtpHostName;
            private int _smtpHostPort;
            private string _administratorEmail;

            internal NOVMImplementation(NotificationSettings settings)
            {
                RefreshSettings(settings);
            }

            public override string ApplicationName
            {
                get { return Extensibility.Strings._ApplicationName; }
            }

            public override string ViewTitle
            {
                get { return Extensibility.Strings._ApplicationName; }
            }

            public override string AdministratorEmail
            {
                get { return _administratorEmail; }
                set
                {
                    if (_administratorEmail == value) return;
                    _administratorEmail = value;
                    OnPropertyChanged();
                }
            }

            public override bool ShouldEmail
            {
                get { return _shouldEmail; }
                set
                {
                    if (_shouldEmail == value) return;
                    _shouldEmail = value;
                    OnPropertyChanged();
                }
            }

            public override SmtpDeliveryMethod DeliveryMethod
            {
                get { return _deliveryMethod; }
                set
                {
                    if (_deliveryMethod == value) return;
                    _deliveryMethod = value;
                    OnPropertyChanged();
                }
            }

            public override bool EnableSsl
            {
                get { return _enableSsl; }
                set
                {
                    if (_enableSsl == value) return;
                    _enableSsl = value;
                    OnPropertyChanged();
                }
            }


            public override string SMTPHostName
            {
                get { return _smtpHostName; }
                set
                {
                    if (_smtpHostName == value) return;
                    _smtpHostName = value;
                    OnPropertyChanged();
                }
            }

            public override int SMTPHostPort
            {
                get { return _smtpHostPort; }
                set
                {
                    if (_smtpHostPort == value) return;
                    _smtpHostPort = value;
                    OnPropertyChanged();
                }
            }

            public override string EmailSenderAddress
            {
                get { return _emailSenderAddress; }
                set
                {
                    if (_emailSenderAddress == value) return;
                    _emailSenderAddress = value;
                    OnPropertyChanged();
                }
            }

            public override int MaxEmailsPerMinute
            {
                get { return _maxEmailsPerMinute; }
                set
                {
                    if (_maxEmailsPerMinute == value) return;
                    _maxEmailsPerMinute = value;
                    OnPropertyChanged();
                }
            }

            public override int MaxEmailsPerDay
            {
                get { return _maxEmailsPerDay; }
                set
                {
                    if (_maxEmailsPerDay == value) return;
                    _maxEmailsPerDay = value;
                    OnPropertyChanged();
                }
            }

            internal override NotificationSettings Commit(NotificationSettings state)
            {
                state.ShouldEmail = ShouldEmail;
                state.SMTPInfo.MaxMailsPerMinute = MaxEmailsPerMinute;
                state.SMTPInfo.MaxMailsPerDay = MaxEmailsPerDay;
                
                state.EmailInfo.To.Clear();
                var items = AdministratorEmail.Split(',');
                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.Trim())) continue;
                    state.EmailInfo.To.Add(item.Trim());
                }

                state.SMTPInfo.HostName = SMTPHostName;
                state.SMTPInfo.Port = SMTPHostPort;
                state.SMTPInfo.EnableSsl = EnableSsl;
                state.SMTPInfo.DeliveryMethod = DeliveryMethod;
                state.EmailInfo.SenderAddress = EmailSenderAddress;
                return state;
            }

            internal override void RefreshSettings(NotificationSettings state)
            {
                var adminEmail = "";
                var emails = state.EmailInfo.To;
                for (var i = 0; i < emails.Count;i++)
                {
                    adminEmail += emails[i];
                    if (i < emails.Count - 1) adminEmail += ", ";
                }
                AdministratorEmail = adminEmail;

                ShouldEmail = state.ShouldEmail;
                MaxEmailsPerMinute = state.SMTPInfo.MaxMailsPerMinute;
                MaxEmailsPerDay = state.SMTPInfo.MaxMailsPerDay;
                SMTPHostName = state.SMTPInfo.HostName;
                SMTPHostPort = state.SMTPInfo.Port;
                EnableSsl = state.SMTPInfo.EnableSsl;
                DeliveryMethod = state.SMTPInfo.DeliveryMethod;
                EmailSenderAddress = state.EmailInfo.SenderAddress;
            }
        }
    }
}