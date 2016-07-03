// -----------------------------------------------------------------------
//  <copyright file="ITabExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Controls;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Interfaces
{
    /// <summary>
    ///     Provides a generalized mechanism for exporting a
    ///     TabItem and its events as an extension.
    /// </summary>
    public interface ITabExtension : IExtensionClass
    {
        /// <summary>
        ///     Gets the UserControl.
        /// </summary>
        TabItem TabExtensionItem(Logger logger);
    }
}