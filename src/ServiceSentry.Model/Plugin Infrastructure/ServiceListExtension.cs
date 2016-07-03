// -----------------------------------------------------------------------
//  <copyright file="ServiceListExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model
{
    public abstract class ServiceListExtension : PropertyChangedBase, IServiceListExtension
    {
        public abstract string ExtensionName { get; }

        /// <summary>
        ///     Indicates whether the services should be loaded/imported or not.
        /// </summary>
        public abstract bool CanExecute { get; }

        public virtual void OnImportsSatisfied()
        {
        }

        public abstract List<Service> Services { get; }
        public abstract List<ExternalFile> OtherFiles { get; }
        public abstract void Configure(Logger logger);
    }
}