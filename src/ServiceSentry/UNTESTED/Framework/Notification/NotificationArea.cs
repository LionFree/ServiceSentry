// -----------------------------------------------------------------------
//  <copyright file="NotificationArea.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls.Primitives;
using ServiceSentry.Client.UNTESTED.Model;
using WPFNotifyIcon;
using WPFNotifyIcon.Enums;

#endregion

namespace ServiceSentry.Client.UNTESTED.Framework.Notification
{
    internal abstract class NotificationArea
    {
        internal static NotificationArea GetInstance(ViewController controller)
        {
            return new NotificationAreaImplementation(controller);
        }

        #region Abstract Members

        /// <summary>
        ///     Closes the current <see cref="TaskbarIcon.CustomBalloon" />, if the
        ///     property is set.
        /// </summary>
        internal abstract void CloseBalloon();

        /// <summary>
        ///     Resets the closing timeout, which effectively
        ///     keeps a displayed balloon message open until
        ///     it is either closed programmatically through
        ///     <see cref="CloseBalloon" /> or due to a new
        ///     message being displayed.
        /// </summary>
        internal abstract void ResetBalloonCloseTimer();

        /// <summary>
        ///     Shows a custom control as a tooltip in the tray location.
        /// </summary>
        /// <param name="balloon"></param>
        /// <param name="animation">An optional animation for the popup.</param>
        /// <param name="timeout">
        ///     The time after which the popup is being closed.
        ///     Submit null in order to keep the balloon open inde
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="balloon" />
        ///     is a null reference.
        /// </exception>
        internal abstract void ShowCustomBalloon(UIElement balloon, PopupAnimation animation, int? timeout);

        /// <summary>
        ///     Hides a balloon ToolTip, if any is displayed.
        /// </summary>
        internal abstract void HideBalloonTip();

        /// <summary>
        ///     Displays a balloon tip with the specified title,
        ///     text, and a custom icon in the taskbar for the specified time period.
        /// </summary>
        /// <param name="title">The title to display on the balloon tip.</param>
        /// <param name="message">The text to display on the balloon tip.</param>
        /// <param name="customIcon">A custom icon.</param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="customIcon" />
        ///     is a null reference.
        /// </exception>
        internal abstract void ShowBalloonTip(string title, string message, Icon customIcon);
        
        /// <summary>
        ///     Displays a balloon tip with the specified title,
        ///     text, and icon in the taskbar for the specified time period.
        /// </summary>
        /// <param name="title">The title to display on the balloon tip.</param>
        /// <param name="message">The text to display on the balloon tip.</param>
        /// <param name="symbol">A symbol that indicates the severity.</param>
        internal abstract void ShowBalloonTip(string title, string message, BalloonIcon symbol);

        #endregion

        private sealed class NotificationAreaImplementation : NotificationArea
        {
            private readonly ViewController _controller;

            internal NotificationAreaImplementation(ViewController controller)
            {
                _controller = controller;
            }

            internal override void ShowBalloonTip(string title, string message, Icon customIcon)
            {
                ////Contract.Requires(customIcon != null);
                _controller.TaskbarIcon.ShowBalloonTip(title, message, customIcon);
            }

            internal override void ShowBalloonTip(string title, string message, BalloonIcon symbol)
            {
                _controller.TaskbarIcon.ShowBalloonTip(title, message, symbol);
            }


            internal override void HideBalloonTip()
            {
                _controller.TaskbarIcon.HideBalloonTip();
            }


            internal override void ShowCustomBalloon(UIElement balloon, PopupAnimation animation, int? timeout)
            {
                if (balloon == null) throw new ArgumentNullException("balloon");

                _controller.TaskbarIcon.ShowCustomBalloon(balloon, animation, timeout);
            }


            internal override void ResetBalloonCloseTimer()
            {
                _controller.TaskbarIcon.ResetBalloonCloseTimer();
            }


            internal override void CloseBalloon()
            {
                _controller.TaskbarIcon.CloseBalloon();
            }
        }
    }
}