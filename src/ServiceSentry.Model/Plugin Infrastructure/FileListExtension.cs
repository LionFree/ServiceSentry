// -----------------------------------------------------------------------
//  <copyright file="FileListExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    public abstract class FileListExtension : PropertyChangedBase, IFileListExtension
    {
        public abstract string ExtensionName { get; }

        /// <summary>
        ///     Indicates whether the services should be loaded/imported or not.
        /// </summary>
        public abstract bool CanExecute { get; }

        public virtual void OnImportsSatisfied()
        {
        }

        public abstract List<ExternalFile> Files { get; set; }
    }
}