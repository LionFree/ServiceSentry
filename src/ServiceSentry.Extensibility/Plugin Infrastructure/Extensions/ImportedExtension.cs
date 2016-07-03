// -----------------------------------------------------------------------
//  <copyright file="ImportedGenericItem.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace ServiceSentry.Extensibility.Extensions
{
    public abstract class ImportedExtension
    {
        public string ExtensionName { get; set; }
        public bool CanExecute { get; set; }
    }
}