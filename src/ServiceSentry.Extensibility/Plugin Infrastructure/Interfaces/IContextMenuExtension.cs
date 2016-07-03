// -----------------------------------------------------------------------
//  <copyright file="IContextMenuExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Controls;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Interfaces
{
    public interface IContextMenuExtension : IExtensionClass
    {
        /// <summary>
        ///     Gets the UserControl.
        /// </summary>
        ContextMenu ContextMenu(Logger logger);
    }
}