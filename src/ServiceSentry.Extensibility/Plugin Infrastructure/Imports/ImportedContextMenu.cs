// -----------------------------------------------------------------------
//  <copyright file="ImportedContextMenu.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

//using System.Diagnostics.Contracts;
using System.Windows.Controls;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Imports
{
    /// <summary>
    ///     A class to hold imported timer items.
    /// </summary>
    public class ImportedContextMenu : ImportedExtension
    {
        public ContextMenu Menu = new ContextMenu();

        public ImportedContextMenu()
        {
        }

        public ImportedContextMenu(Logger logger, ContextMenuExtension control)
        {
            //Contract.Requires(control != null);

            ExtensionName = control.ExtensionName;
            Menu = control.ContextMenu(logger);
            CanExecute = control.CanExecute;
        }
    }
}