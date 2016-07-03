// -----------------------------------------------------------------------
//  <copyright file="NotificationAreaIcon.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Client.UNTESTED.Model;
using WPFNotifyIcon;

#endregion

namespace ServiceSentry.Client.UNTESTED.Framework.Notification
{
    internal abstract class NotificationIcon
    {
        internal abstract TaskbarIcon NotificationAreaIcon { get; set; }

        internal NotificationIcon GetInstance(ViewController viewController)
        {
            return new NotificationIconImplementation(viewController);
        }

        private sealed class NotificationIconImplementation : NotificationIcon
        {
            private readonly ViewController _viewController;

            internal NotificationIconImplementation(ViewController viewController)
            {
                _viewController = viewController;
            }

            internal override TaskbarIcon NotificationAreaIcon
            {
                get { return _viewController.TaskbarIcon; }
                set { _viewController.TaskbarIcon = value; }
            }
        }
    }
}