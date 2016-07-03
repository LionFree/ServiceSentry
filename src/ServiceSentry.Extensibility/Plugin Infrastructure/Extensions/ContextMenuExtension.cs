// -----------------------------------------------------------------------
//  <copyright file="ContextMenuExtension.cs" company="Curtis Kaler">
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
    public abstract class ContextMenuExtension : PropertyChangedBase, IContextMenuExtension
    {
        public abstract string ExtensionName { get; }
        public abstract ContextMenu ContextMenu(Logger logger);

        /// <summary>
        ///     Indicates whether the context menu should be loaded/imported or not.
        /// </summary>
        public virtual bool CanExecute
        {
            get { return false; }
        }


        public virtual void OnImportsSatisfied()
        {
        }
    }
}