// -----------------------------------------------------------------------
//  <copyright file="MonitorErrorHandler.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using ServiceSentry.Client.UNTESTED.Framework.Notification;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Events;
using WPFNotifyIcon.Enums;

#endregion

namespace ServiceSentry.Client.UNTESTED.Model
{
    internal abstract class MonitorErrorHandler
    {
        internal abstract void OnMonitorError(object sender, MonitorErrorEventArgs args);

        internal static MonitorErrorHandler GetInstance(NotificationArea notificationArea, Logger logger)
        {
            return new MonitorErrorHandlerImplementation(notificationArea, logger);
        }

        private sealed class MonitorErrorHandlerImplementation : MonitorErrorHandler
        {
            private readonly Logger _logger;
            private readonly NotificationArea _notificationArea;

            internal MonitorErrorHandlerImplementation(NotificationArea notificationArea, Logger logger)
            {
                _logger = logger;
                _notificationArea = notificationArea;
            }

            internal override void OnMonitorError(object sender, MonitorErrorEventArgs e)
            {
                if (sender == null) throw new ArgumentNullException("sender");
                if (e == null) throw new ArgumentNullException("e");

                _logger.Warn(Strings.Error_MonitorServiceError,
                             e.Exceptions.Length == 1
                                 ? "an"
                                 : e.Exceptions.Length.ToString(CultureInfo.InvariantCulture),
                             e.Exceptions.Length == 1 ? "" : "s");

                foreach (var item in e.Exceptions)
                {
                    _logger.ErrorException(item, Strings.Header_MonitorServiceError);
                }

                _notificationArea.ShowBalloonTip(Strings._ApplicationName,
                                                 Strings.Header_MonitorServiceError,
                                                 BalloonIcon.Warning);
            }
        }
    }
}