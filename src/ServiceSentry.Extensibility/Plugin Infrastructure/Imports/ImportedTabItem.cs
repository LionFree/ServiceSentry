// -----------------------------------------------------------------------
//  <copyright file="ImportedTabItem.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

//using System.Diagnostics.Contracts;
using System.Windows.Controls;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Interfaces;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Imports
{
    /// <summary>
    ///     ImportedTabItem
    /// </summary>
    public class ImportedTabItem : ImportedExtension
    {
        /// <summary>
        ///     ExtensionTabItems
        /// </summary>
        public TabItem ExtensionTabItem = new TabItem();

        /// <summary>
        ///     Creates a new Instance of the <T:ImportedTabItem /> class.
        /// </summary>
        public ImportedTabItem()
        {
        }

        /// <summary>
        ///     Creates a new Instance of the <T:ImportedTabItem /> class,
        ///     based on the <paramref name="control" /> import.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="Logger" /> to pass to the extension.
        /// </param>
        /// <param name="control">The extension to import.</param>
        public ImportedTabItem(Logger logger, ITabExtension control)
        {
            //Contract.Requires(control != null);

            ExtensionName = control.ExtensionName;
            ExtensionTabItem = control.TabExtensionItem(logger);
            CanExecute = control.CanExecute;
        }
    }
}