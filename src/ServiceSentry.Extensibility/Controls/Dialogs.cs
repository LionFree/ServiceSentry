// -----------------------------------------------------------------------
//  <copyright file="Dialogs.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    /// <summary>
    ///     Provides convenience methods to display message boxes.
    /// </summary>
    public abstract class Dialogs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="Dialogs" /> class,
        ///     using the default values.
        /// </summary>
        internal static Dialogs Default
        {
            get { return GetInstance(); }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Dialogs" /> class.
        /// </summary>
        /// <returns></returns>
        public static Dialogs GetInstance()
        {
            return new DialogsImplementation();
        }

        #region Abstract Members

        /// <summary>
        ///     Displays a messagebox dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="button">The button to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <returns>User selection.</returns>
        internal abstract MessageBoxResult Show(string message, MessageBoxButton button, MessageBoxImage icon,
                                                 Window owner = null);

        /// <summary>
        ///     Displays an Yes / No / Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public abstract MessageBoxResult ShowYesNoCancel(string message, MessageBoxImage icon, Window owner = null);

        /// <summary>
        ///     Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        public abstract void ShowError(string message);

        /// <summary>
        ///     Displays an error dialog with a given message.
        /// </summary>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        public abstract void ShowError(string message, Window owner);

        /// <summary>
        ///     Displays an error dialog with a given message.
        /// </summary>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        public abstract void ShowInformation(string message, Window owner = null);

        /// <summary>
        ///     Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="owner">The visual parent of the dialog box.</param>
        public abstract void ShowWarning(string message, Window owner = null);

        /// <summary>
        ///     Displays an error dialog with a given message and icon.
        /// </summary>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed with the message.</param>
        public abstract void ShowMessage(Window owner, string message, MessageBoxImage
                                                                             icon);

        /// <summary>
        ///     Displays an OK / Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public abstract MessageBoxResult ShowOkCancel(string message, MessageBoxImage icon, Window owner = null);

        /// <summary>
        ///     Displays a Yes/No dialog and returns the user input.
        /// </summary>
        /// <param name="owner">The visual parent of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public abstract MessageBoxResult ShowYesNo(string message, MessageBoxImage icon, Window owner = null);

        #endregion

        private sealed class DialogsImplementation : Dialogs
        {
            public override void ShowError(string message)
            {
                ShowMessage(null, message, MessageBoxImage.Error);
            }

            public override void ShowError(string message, Window owner)
            {
                ShowMessage(owner, message, MessageBoxImage.Error);
            }

            public override void ShowInformation(string message, Window owner = null)
            {
                ShowMessage(owner, message, MessageBoxImage.Information);
            }

            public override void ShowWarning(string message, Window owner = null)
            {
                ShowMessage(owner, message, MessageBoxImage.Warning);
            }

            public override void ShowMessage(Window owner, string message, MessageBoxImage icon)
            {
                var appName = Strings._ApplicationName;

                if (owner == null)
                {
                    MessageBox.Show(message, appName, MessageBoxButton.OK, icon, MessageBoxResult.None,
                                    MessageBoxOptions.ServiceNotification);
                    return;
                }

                MessageBox.Show(owner, message, appName, MessageBoxButton.OK, icon, MessageBoxResult.None);
            }

            public override MessageBoxResult ShowOkCancel(string message, MessageBoxImage icon, Window owner = null)
            {
                return Show(message, MessageBoxButton.OKCancel, icon, owner);
            }

            public override MessageBoxResult ShowYesNo(string message, MessageBoxImage icon, Window owner = null)
            {
                return Show(message, MessageBoxButton.YesNo, icon, owner);
            }

            public override MessageBoxResult ShowYesNoCancel(string message, MessageBoxImage icon, Window owner = null)
            {
                return Show(message, MessageBoxButton.YesNoCancel, icon, owner);
            }

            internal override MessageBoxResult Show(string message, MessageBoxButton button, MessageBoxImage icon,
                                                     Window owner = null)
            {
                var appName = Strings._ApplicationName;
                return owner == null
                           ? MessageBox.Show(message, appName, button, icon)
                           : MessageBox.Show(owner, message, appName, button, icon);
            }
        }
    }
}