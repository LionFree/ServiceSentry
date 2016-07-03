// -----------------------------------------------------------------------
//  <copyright file="TimerExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Diagnostics;
using System.Windows.Threading;
using ServiceSentry.Extensibility.Interfaces;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Extensions
{
    public abstract class TimerExtension : PropertyChangedBase, ITimerExtension
    {
        /// <summary>
        ///     Gets the name of this imported extension.
        /// </summary>
        public abstract string ExtensionName { get; }

        /// <summary>
        ///     A boolean value that determines whether the extension will be loaded.
        /// </summary>
        public virtual bool CanExecute
        {
            get { return false; }
        }

        /// <summary>
        ///     Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
            Debug.WriteLine("  " + ExtensionName + ": successfully exported.");
        }

        /// <summary>
        ///     Gets the DispatcherTimer in the extension.
        /// </summary>
        public abstract DispatcherTimer Timer(Logger logger);
    }
}