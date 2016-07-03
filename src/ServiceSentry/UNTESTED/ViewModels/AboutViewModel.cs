// -----------------------------------------------------------------------
//  <copyright file="AboutViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class AboutViewModel
    {
        public abstract string ViewTitle { get; }

        /// <summary>
        ///     Returns the list of installed Curtis Kaler applications.
        /// </summary>
        public abstract AssemblyWrapper AssemblyWrapper { get; }

        /// <summary>
        ///     A collection of attributes that descibe the application and its assembly.
        /// </summary>
        public abstract List<Tuple<string, string>> InstalledEngines { get; }

        internal static AboutViewModel GetInstance(Logger logger)
        {
            return GetInstance(RegistryBehavior.GetInstance(), AssemblyWrapper.GetInstance(logger));
        }

        internal static AboutViewModel GetInstance(RegistryBehavior registryBehavior,
            AssemblyWrapper assemblyWrapper)
        {
            return new AboutVMImplementation(registryBehavior, assemblyWrapper);
        }

        private sealed class AboutVMImplementation : AboutViewModel
        {
            private readonly AssemblyWrapper _assemblyWrapper;
            private readonly RegistryBehavior _registry;

            internal AboutVMImplementation(RegistryBehavior registryBehavior, AssemblyWrapper assemblyWrapper)
            {
                _assemblyWrapper = assemblyWrapper;
                _registry = registryBehavior;
            }

            public override List<Tuple<string, string>> InstalledEngines
            {
                get { return _registry.InstalledApps; }
            }

            public override AssemblyWrapper AssemblyWrapper
            {
                get { return _assemblyWrapper; }
            }

            public override string ViewTitle
            {
                get { return Extensibility.Strings._ApplicationName; }
            }
        }
    }
}