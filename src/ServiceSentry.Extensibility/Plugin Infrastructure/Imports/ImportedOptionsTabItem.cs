// -----------------------------------------------------------------------
//  <copyright file="ImportedOptionsTabItem.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using System.Windows.Controls;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Imports
{
    public class ImportedOptionsTabItem : ImportedExtension
    {
        public TabItem OptionTabItem = new TabItem();

        public ImportedOptionsTabItem()
        {
        }

        public ImportedOptionsTabItem(Logger logger, OptionsTabExtension control)
        {
            //Contract.Requires(control != null);
            ExtensionName = control.ExtensionName;
            CanExecute = control.CanExecute;
            OptionTabItem = control.OptionTabItem(logger);
            CommitOptions = control.CommitOptions;
            RefreshOptionSettings = control.RefreshOptionSettings;
        }

        public Action CommitOptions { get; set; }
        public Action RefreshOptionSettings { get; set; }
    }
}