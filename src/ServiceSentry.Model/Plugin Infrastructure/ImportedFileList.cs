// -----------------------------------------------------------------------
//  <copyright file="ImportedFileList.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using ServiceSentry.Extensibility.Extensions;

#endregion

namespace ServiceSentry.Model
{
    /// <summary>
    ///     A class to hold imported timer items.
    /// </summary>
    public class ImportedFileList : ImportedExtension
    {
        public ImportedFileList()
        {
        }

        public ImportedFileList(FileListExtension control)
        {
            //Contract.Requires(control != null);

            ExtensionName = control.ExtensionName;
            CanExecute = control.CanExecute;
            Items = control.Files;
        }

        public List<ExternalFile> Items { get; set; }
    }
}