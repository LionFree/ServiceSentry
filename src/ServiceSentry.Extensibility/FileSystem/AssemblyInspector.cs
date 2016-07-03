// -----------------------------------------------------------------------
//  <copyright file="AssemblyInspector.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Reflection;

#endregion

namespace ServiceSentry.Extensibility
{
    public abstract class AssemblyInspector
    {
        public static AssemblyInspector Default
        {
            get { return new AWHelperImplementation(); }
        }

        public abstract bool HasEntryAssembly { get; }

        private sealed class AWHelperImplementation : AssemblyInspector
        {
            /// <summary>
            ///     Determines whether the EntryAssembly is available.
            /// </summary>
            public override bool HasEntryAssembly
            {
                get
                {
                    try
                    {
                        var assembly = Assembly.GetEntryAssembly();
                        return (assembly != null);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
    }
}