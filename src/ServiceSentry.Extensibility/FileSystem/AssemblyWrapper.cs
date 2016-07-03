// -----------------------------------------------------------------------
//  <copyright file="AssemblyWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility
{
    /// <summary>
    ///     Represents an assembly, which is a reusable, versionable, and
    ///     self-describing block of a common runtime language application.
    /// </summary>
    public abstract class AssemblyWrapper
    {
        /// <summary>
        ///     Generates a new instance of this class, using the null <see cref="Logger" />.
        /// </summary>
        public static AssemblyWrapper Default
        {
            get
            {
                return AssemblyInspector.Default.HasEntryAssembly
                           ? GetInstance(Assembly.GetEntryAssembly(),
                                         FileSystem.GetUnloggedInstance(Assembly.GetEntryAssembly().Location))
                           : Null;
            }
        }

        /// <summary>
        ///     Generates a new instance of this class, using no EntryAssembly and the null <see cref="Logger" />.
        /// </summary>
        public static AssemblyWrapper Null
        {
            get { return GetInstance(null, FileSystem.GetUnloggedInstance()); }
        }

        /// <summary>
        ///     Generates a new instance of this class.
        /// </summary>
        public static AssemblyWrapper GetInstance(Logger logger)
        {
            return AssemblyInspector.Default.HasEntryAssembly
                       ? GetInstance(Assembly.GetEntryAssembly(),
                                     FileSystem.GetInstance(logger, Assembly.GetEntryAssembly().Location))
                       : Null;
        }

        /// <summary>
        ///     Generates a new instance of this class.
        /// </summary>
        public static AssemblyWrapper GetUnloggedInstance()
        {
            return GetInstance(Assembly.GetEntryAssembly(),
                               FileSystem.GetUnloggedInstance(Assembly.GetEntryAssembly().Location));
        }

        /// <summary>
        ///     Generates a new instance of this class.
        /// </summary>
        internal static AssemblyWrapper GetInstance(FileSystem fileSystem)
        {
            return GetInstance(Assembly.GetEntryAssembly(), fileSystem);
        }

        /// <summary>
        ///     Generates a new instance of this class.
        /// </summary>
        public static AssemblyWrapper GetInstance(Assembly entryAssembly)
        {
            //Contract.Requires(entryAssembly != null);
            return GetInstance(entryAssembly, FileSystem.GetUnloggedInstance(entryAssembly.Location));
        }

        /// <summary>
        ///     Generates a new instance of this class.
        /// </summary>
        public static AssemblyWrapper GetInstance(Assembly entryAssembly, Logger logger)
        {
            //Contract.Requires(entryAssembly != null);
            //Contract.Requires(logger != null);
            return GetInstance(entryAssembly, FileSystem.GetInstance(logger, entryAssembly.Location));
        }
        
        /// <summary>
        ///     Generates a new instance of this class.
        /// </summary>
        public static AssemblyWrapper GetInstance(Assembly entryAssembly, FileSystem fileSystem)
        {
            return GetInstance(entryAssembly, fileSystem, VersionConverter.Default);
        }

        internal static AssemblyWrapper GetInstance(Assembly entryAssembly, FileSystem fileSystem,
                                                    VersionConverter versionConverter)
        {
            return new AssemblyWrapperImplementation(entryAssembly, fileSystem, versionConverter);
        }

        #region Abstract Members

        /// <summary>
        ///     Gets assembly title information.
        /// </summary>
        public abstract string AssemblyTitle { get; }

        /// <summary>
        ///     Gets assembly product version information.
        /// </summary>
        public abstract string ProductVersion { get; }

        /// <summary>
        ///     Gets the major, minor, build, and revision numbers of the assembly.
        /// </summary>
        public abstract string AssemblyVersion { get; }


        /// <summary>
        ///     Gets assembly description information.
        /// </summary>
        public abstract string AssemblyDescription { get; }

        /// <summary>
        ///     Gets product name information.
        /// </summary>
        public abstract string AssemblyProduct { get; }

        /// <summary>
        ///     Gets copyright information.
        /// </summary>
        public abstract string AssemblyCopyright { get; }

        /// <summary>
        ///     Gets company information.
        /// </summary>
        public abstract string AssemblyCompany { get; }

        /// <summary>
        ///     Gets the date and time of the last time the assembly was compiled.
        /// </summary>
        public abstract string BuildDate { get; }


        /// <summary>
        ///     Gets the process executable in the default application domain.
        ///     In other application domains, this is the first executable
        ///     that was executed by AppDomain.ExecuteAssembly.
        /// </summary>
        public abstract Assembly GetEntryAssembly();

        /// <summary>
        ///     Gets the major, minor, build, and revision numbers of the given
        ///     assembly.
        /// </summary>
        public abstract Version Version(Assembly assembly);

        /// <summary>
        ///     Gets the major, minor, build, and revision numbers of the current
        ///     assembly.
        /// </summary>
        public abstract Version Version();

        /// <summary>
        ///     Gets the major, minor, build, and revision numbers of the assembly
        ///     at the given path.
        /// </summary>
        /// <param name="imagePath">The path to the assembly.</param>
        public abstract Version Version(string imagePath);

        #endregion

        private sealed class AssemblyWrapperImplementation : AssemblyWrapper
        {
            private readonly Assembly _assembly;
            private readonly VersionConverter _converter;
            private readonly FileSystem _fileSystem;

            internal AssemblyWrapperImplementation(Assembly entryAssembly, FileSystem fileSystem,
                                                   VersionConverter versionConverter)
            {
                _assembly = entryAssembly;
                _fileSystem = fileSystem;
                _converter = versionConverter;
            }

            public override string BuildDate
            {
                get
                {
                    var version = _assembly.GetName().Version;
                    return _converter.ConvertVersionToDate(version).ToString(CultureInfo.InvariantCulture);
                }
            }

            public override string AssemblyTitle
            {
                get
                {
                    var attributes =
                        _assembly.GetCustomAttributes(typeof (AssemblyTitleAttribute), false);
                    if (attributes.Length > 0)
                    {
                        var titleAttribute = (AssemblyTitleAttribute) attributes[0];
                        if (titleAttribute.Title != "")
                        {
                            return titleAttribute.Title;
                        }
                    }
                    return Path.GetFileNameWithoutExtension(_assembly.CodeBase);
                }
            }

            public override string ProductVersion
            {
                get { return "Version " + _fileSystem.ProductVersion; }
            }

            public override string AssemblyVersion
            {
                get { return "Version " + _assembly.GetName().Version; }
            }

            public override string AssemblyDescription
            {
                get
                {
                    var attributes =
                        _assembly.GetCustomAttributes(typeof (AssemblyDescriptionAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return "";
                    }
                    return ((AssemblyDescriptionAttribute) attributes[0]).Description;
                }
            }

            public override string AssemblyProduct
            {
                get
                {
                    var attributes =
                        _assembly.GetCustomAttributes(typeof (AssemblyProductAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return "";
                    }
                    return ((AssemblyProductAttribute) attributes[0]).Product;
                }
            }

            public override string AssemblyCopyright
            {
                get
                {
                    var attributes =
                        _assembly.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return "";
                    }
                    return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
                }
            }

            public override string AssemblyCompany
            {
                get
                {
                    var attributes =
                        _assembly.GetCustomAttributes(typeof (AssemblyCompanyAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return "";
                    }
                    return ((AssemblyCompanyAttribute) attributes[0]).Company;
                }
            }

            public override Assembly GetEntryAssembly()
            {
                return Assembly.GetEntryAssembly();
            }

            public override Version Version()
            {
                return Version(_assembly);
            }

            public override Version Version(Assembly assembly)
            {
                return assembly.GetName().Version;
            }

            public override Version Version(string imagePath)
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(imagePath);
                return Version(assembly);
            }
        }
    }
    
    internal abstract class VersionConverter
    {
        internal static VersionConverter Default
        {
            get { return new VersionConverterImplementation(); }
        }

        /// <summary>
        ///     Converts the major, minor, build, and revision numbers
        ///     into the date it represents.
        /// </summary>
        /// <param name="version">The version to convert.</param>
        internal abstract DateTime ConvertVersionToDate(Version version);

        private sealed class VersionConverterImplementation : VersionConverter
        {
            internal override DateTime ConvertVersionToDate(Version version)
            {
                var results = new DateTime(2000, 1, 1);
                results = results.AddDays(version.Build);
                results = results.AddSeconds(version.Revision*2);
                if (TimeZone.IsDaylightSavingTime(results,
                                                  TimeZone.CurrentTimeZone.GetDaylightChanges(results.Year)))
                {
                    results = results.AddHours(1);
                }

                return results;
            }
        }
    }
}