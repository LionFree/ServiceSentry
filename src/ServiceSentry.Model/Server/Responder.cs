// -----------------------------------------------------------------------
//  <copyright file="Responder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Email;

#endregion

namespace ServiceSentry.Model.Server
{
    public abstract class Responder
    {
       
        public static Responder GetInstance(Logger logger)
        {
            return GetInstance(logger, ResponderHelper.GetInstance(logger)); 
        }

        internal static Responder GetInstance(Logger logger, ResponderHelper helper)
        {
            return new ResponderImplementation(helper, logger);
        }

        #region Abstract Members
        
        public abstract List<Exception> Exceptions { get; }
        public abstract void HandleFailure(TrackingObject state);

        #endregion

        private sealed class ResponderImplementation : Responder
        {
            private readonly List<Exception> _exceptions;
            private readonly ResponderHelper _helper;
            private readonly Logger _logger;

            public ResponderImplementation(ResponderHelper helper, Logger logger)
            {
                _exceptions = new List<Exception>();
                _logger = logger;
                _helper = helper;
                _helper.ExceptionsUpdated += OnExceptionsUpdated;
            }

            private void OnExceptionsUpdated(object sender, EventArgs e)
            {
                _exceptions.Clear();
                _exceptions.AddRange(_helper.Exceptions);
            }

            public override List<Exception> Exceptions
            {
                get { return _exceptions; }
            }

            public override void HandleFailure(TrackingObject state)
            {
                Console.WriteLine(Strings.Debug_HandlingStop, state.ServiceName);
                _logger.Debug(Strings.Debug_HandlingStop, state.ServiceName);
                _helper.AddFailureToList(ref state);

                if (!state.NotifyOnUnexpectedStop || !_helper.CanEmail(state)) return;

                Console.WriteLine(Strings.Info_RespondingToStoppedService, state.ServiceName);
                _logger.Info(Strings.Info_RespondingToStoppedService, state.ServiceName);
                _helper.SendEmail(state);

            }
        }
    }

    public abstract class ResponderHelper
    {
        public abstract List<Exception> Exceptions { get; }

        internal static ResponderHelper GetInstance(Logger logger)
        {
            return GetInstance(ModelClassFactory.GetInstance(logger), logger); 
        }

        internal static ResponderHelper GetInstance(ModelClassFactory factory, Logger logger)
        {
            return new ResponderHelperImplementation(factory, logger);
        }

        #region Abstract Members
        
        public abstract event EventHandler ExceptionsUpdated;

        protected abstract void OnEmailFailed(object sender, EmailEventArgs e);

        internal abstract void AddFailureToList(ref TrackingObject state);
        internal abstract bool EmailOverLimit(int actualEvents, int maxEvents);
        internal abstract bool CanEmail(TrackingObject state);

        internal abstract void SendEmail(TrackingObject state);
        #endregion

        private sealed class ResponderHelperImplementation : ResponderHelper
        {
            private readonly List<Exception> _exceptions;
            private readonly ModelClassFactory _factory;
            private readonly Logger _logger;

            internal ResponderHelperImplementation(ModelClassFactory factory, Logger logger)
            {
                _exceptions = new List<Exception>();
                _factory = factory;
                _logger = logger;
            }

            public override List<Exception> Exceptions
            {
                get { return _exceptions; }
            }

            internal override void SendEmail(TrackingObject state)
            {
                var list = state.Packet.EmailInfo.To;
                var str = "";
                if (list.Count == 0)
                {
                    Trace.WriteLine("No email addresses to notify.");
                    _logger.Error("No email addresses to notify.");
                    return;
                }

                for (var i = 0; i < list.Count; i++)
                {
                    if (string.IsNullOrEmpty(list[i]))
                    {
                        // SHOULD NEVER HAPPEN.
                        // MAKE IT SO.
                        Trace.WriteLine("Email (To) address is an empty string.");
                        _logger.Error("Email (To) address is an empty string.");
                        return;
                    }
                    str += list[i];
                    if (i < list.Count - 1) str += ", ";
                }

                Trace.WriteLine(String.Format(Strings.Info_SendingEmail, str));
                Console.WriteLine(String.Format(Strings.Info_SendingEmail, str));
                _logger.Info(Strings.Info_SendingEmail, str);

                var emailer = _factory.GetEmailer(state.Packet.SMTPInfo);
                emailer.EmailFailed += OnEmailFailed;
                emailer.SendServiceFailureNotification(state);

                if (state.Exceptions.Count <= 0) return;

                _exceptions.AddRange(state.Exceptions);
                OnExceptionsUpdated();
            }

            private void OnExceptionsUpdated()
            {
                var handler = ExceptionsUpdated;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }


            internal override void AddFailureToList(ref TrackingObject state)
            {
                state.FailDates.Add(DateTime.UtcNow);

                if (state.FailDates.Count > 5)
                {
                    state.FailDates.RemoveAt(0);
                }
            }

            internal override bool CanEmail(TrackingObject state)
            {
                var reference = DateTime.UtcNow;

                var eventsInThisMinute = 0;
                var eventsToday = 0;

                var list = state.FailDates;
                foreach (var date in list)
                {
                    var minutesBetween = (reference - date).TotalMinutes;
                    var daysBetween = (reference - date).TotalDays;
                    if (minutesBetween < 1)
                        eventsInThisMinute += 1;

                    if (daysBetween < 1)
                        eventsToday += 1;
                }

                if (EmailOverLimit(eventsInThisMinute, state.Packet.SMTPInfo.MaxMailsPerMinute)) return false;
                if (EmailOverLimit(eventsToday, state.Packet.SMTPInfo.MaxMailsPerDay)) return false;

                return true;
            }

            internal override bool EmailOverLimit(int actualEvents, int maxEvents)
            {
                if (maxEvents > 0)
                {
                    if (actualEvents > maxEvents) return true;
                }
                return false;
            }

            public override event EventHandler ExceptionsUpdated;

            protected override void OnEmailFailed(object sender, EmailEventArgs e)
            {
                _exceptions.Add(e.Error);
            }
        }
    }
}