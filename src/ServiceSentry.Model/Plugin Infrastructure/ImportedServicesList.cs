// -----------------------------------------------------------------------
//  <copyright file="ImportedServicesList.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model
{
    /// <summary>
    ///     A class to hold imported timer items.
    /// </summary>
    public class ImportedServicesList : ImportedExtension
    {
        private readonly List<ExternalFile> _files = new List<ExternalFile>();
        private readonly List<Service> _services = new List<Service>();

        public ImportedServicesList()
        {
        }

        public ImportedServicesList(Logger logger, ServiceListExtension control)
        {
            //Contract.Requires(control != null);
            control.Configure(logger);
            ExtensionName = control.ExtensionName;
            CanExecute = control.CanExecute;
            _services = control.Services;
            _files = control.OtherFiles;
        }

        public List<Service> Services
        {
            get { return _services; }
        }

        public List<ExternalFile> OtherFiles
        {
            get { return _files; }
        }
    }
}