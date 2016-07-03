// -----------------------------------------------------------------------
//  <copyright file="TabExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Controls;
using ServiceSentry.Extensibility.Interfaces;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Extensions
{
    public abstract class TabExtension : PropertyChangedBase, ITabExtension
    {
        /// <summary>
        ///     A boolean value that determines whether the extension will be loaded.
        /// </summary>
        public virtual bool CanExecute
        {
            get { return true; }
        }

        /// <summary>
        ///     Gets the name of this imported extension.
        /// </summary>
        public abstract string ExtensionName { get; }

        /// <summary>
        ///     Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
        }

        /// <summary>
        ///     Gets the tab within this extension.
        /// </summary>
        public abstract TabItem TabExtensionItem(Logger logger);
    }
}