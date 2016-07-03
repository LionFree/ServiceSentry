// -----------------------------------------------------------------------
//  <copyright file="ShellWindowView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Windows;
using ServiceSentry.Client.Properties;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Client.UNTESTED.ViewModels.Commands;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views
{
    /// <summary>
    ///     Interaction logic for ShellWindowView.xaml
    /// </summary>
    public abstract partial class ShellWindowView
    {
        /// <summary>
        ///     Constructs a new ShellWindowView.
        /// </summary>
        internal static ShellWindowView GetInstance(ShellWindowViewModel viewModel)
        {
            return new ShellViewImplementation(viewModel);
        }

        #region Abstract Members

        /// <summary>
        ///     Stores the windows size, location, and window state.
        ///     Invoked if the size changes, and upon closing.
        /// </summary>
        /// <remarks>
        ///     Only caching on close is not enough - if the
        ///     window is maximized / minimized, the data is lost.
        /// </remarks>
        protected abstract void CacheWindowLocation(object sender, EventArgs e);

        /// <summary>
        ///     Use cached window settings to restore size/location.
        /// </summary>
        protected abstract void RestoreWindowSettings();

        #endregion

        private sealed class ShellViewImplementation : ShellWindowView
        {
            private readonly ShellCommands _shellCommands;

            internal ShellViewImplementation(ShellWindowViewModel viewModel)
            {
                DataContext = viewModel;
                _shellCommands = viewModel.ShellCommands;

                InitializeComponent();

                RestoreWindowSettings();
            }

            protected override void RestoreWindowSettings()
            {
                // Use cached window settings to restore size/location

                var settings = Settings.Default;

                if (!(Math.Abs(settings.MainWindowLeft + 1.0) > Double.Epsilon)) return;
                Left = settings.MainWindowLeft;
                Top = settings.MainWindowTop;
                Width = settings.MainWindowWidth;
                Height = settings.MainWindowHeight;
                if (settings.StartMaximized) WindowState = WindowState.Maximized;
            }

            protected override void CacheWindowLocation(object sender, EventArgs e)
            {
                var settings = Settings.Default;

                if (WindowState == WindowState.Normal && Top > -10)
                {
                    settings.MainWindowTop = Top;
                    settings.MainWindowLeft = Left;
                    settings.MainWindowHeight = Height;
                    settings.MainWindowWidth = Width;
                }

                settings.StartMaximized = WindowState == WindowState.Maximized;

                if (!settings.CloseToSystemTray) return;
                if (WindowState == WindowState.Minimized)
                    _shellCommands.MinimizeShellWindow();
            }
        }
    }
}