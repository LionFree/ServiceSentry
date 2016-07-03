// -----------------------------------------------------------------------
//  <copyright file="InitializeNotificationAreaIcon.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;
using System.Windows.Controls;

#endregion

namespace WPFNotifyIcon
{
    public partial class Utilities
    {
        /// <summary>
        ///     Inits the component that displays status information in the
        ///     notification area.
        /// </summary>
        public static TaskbarIcon InitializeNotificationAreaIcon(object context)
        {
            var contextMenu = (ContextMenu)Application.Current.TryFindResource("NotificationAreaContextMenu");
            var tbIcon = (TaskbarIcon) Application.Current.TryFindResource("TrayIcon");
            if (tbIcon == null) return null;

            contextMenu.DataContext = context;
            tbIcon.ContextMenu = contextMenu;
            tbIcon.DataContext = context;
            return tbIcon;
        }
    }
}