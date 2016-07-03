using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Extensions;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    internal abstract class ExtensionHandler : IPartImportsSatisfiedNotification
    {
        internal static string DefaultExtensionSubfolder = Extensibility.Strings._DefaultExtensionsSubfolder;

        /// <summary>
        ///     Creates a new instance of the <see cref="ExtensionHandler" /> class.
        /// </summary>
        internal static ExtensionHandler GetInstance(Dialogs dialogs, Logger logger)
        {
            return new ExtensionHandlerImplementation(dialogs, logger, FileSystem.GetInstance(logger));
        }

        #region Abstract Members

        internal abstract List<ImportedContextMenu> ContextMenuExtensions { get; set; }
        internal abstract List<ImportedFileList> FileListExtensions { get; set; }
        internal abstract List<ImportedOptionsTabItem> OptionTabExtensions { get; set; }
        internal abstract List<ImportedServicesList> ServiceExtensions { get; set; }
        internal abstract List<ImportedTabItem> TabExtensions { get; set; }
        internal abstract List<ImportedTimerItem> TimerExtensions { get; set; }
        public abstract void OnImportsSatisfied();


        /// <summary>
        ///     Loads imports from other assemblies in the default
        ///     extensions folder into memory.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if successful, otherwise <c>false</c>.
        /// </returns>
        internal abstract bool ComposeExtensions();

        /// <summary>
        ///     Loads imports from other assemblies in the specified folder
        ///     into memory.
        /// </summary>
        /// <param name="extensionsFolderPath"></param>
        /// <returns>
        ///     <c>true</c> if successful, otherwise <c>false</c>.
        /// </returns>
        internal abstract bool ComposeExtensions(string extensionsFolderPath);

        /// <summary>
        ///     Activate extensions, e.g., publish services
        ///     to ServiceHandler and activate timers.
        /// </summary>
        internal abstract void ActivateExtensions(ApplicationState state);

        /// <summary>
        ///     Parses a list of imports into a locally-accessible List of extensions.
        /// </summary>
        /// <typeparam name="T">The type of import to parse.</typeparam>
        /// <param name="extensionsList">The list of imports to parse.</param>
        /// <returns></returns>
        protected abstract List<T> PopulateList<T>(IEnumerable extensionsList) where T : new();

        #endregion

        private sealed class ExtensionHandlerImplementation : ExtensionHandler
        {
            internal ExtensionHandlerImplementation(Dialogs dialogs, Logger logger, FileSystem fileSystem)
            {
                _dialogs = dialogs;
                _logger = logger;
                _fileSystem = fileSystem;
            }

            #region Properties

            private static CompositionContainer _container;
            private readonly Dialogs _dialogs;
            private readonly FileSystem _fileSystem;
            private readonly Logger _logger;
            internal override List<ImportedContextMenu> ContextMenuExtensions { get; set; }
            internal override List<ImportedFileList> FileListExtensions { get; set; }
            internal override List<ImportedOptionsTabItem> OptionTabExtensions { get; set; }
            internal override List<ImportedServicesList> ServiceExtensions { get; set; }
            internal override List<ImportedTabItem> TabExtensions { get; set; }
            internal override List<ImportedTimerItem> TimerExtensions { get; set; }

            [ImportMany("Services", typeof (ServiceListExtension), AllowRecomposition = true)]
            private IEnumerable<ServiceListExtension> ServiceImports { get; set; }

            [ImportMany("Timers", typeof (TimerExtension), AllowRecomposition = true)]
            private IEnumerable<TimerExtension> TimerImports { get; set; }

            [ImportMany("ContextMenuItems", typeof (ContextMenuExtension), AllowRecomposition = true)]
            private IEnumerable<ContextMenuExtension> ContextMenuImports { get; set; }

            [ImportMany("Tabs", typeof (TabExtension), AllowRecomposition = true)]
            private IEnumerable<TabExtension> TabImports { get; set; }

            [ImportMany("OptionTabs", typeof (OptionsTabExtension), AllowRecomposition = true)]
            private IEnumerable<OptionsTabExtension> OptionTabImports { get; set; }

            #endregion

            public override void OnImportsSatisfied()
            {
                ServiceExtensions = PopulateList<ImportedServicesList>(ServiceImports);
                TimerExtensions = PopulateList<ImportedTimerItem>(TimerImports);
                TabExtensions = PopulateList<ImportedTabItem>(TabImports);
                ContextMenuExtensions = PopulateList<ImportedContextMenu>(ContextMenuImports);
                OptionTabExtensions = PopulateList<ImportedOptionsTabItem>(OptionTabImports);
            }

            internal override void ActivateExtensions(ApplicationState state)
            {
                _logger.Trace(Extensibility.Strings.Debug_ActivatingExtensions);

                // Publish Services
                foreach (var item in ServiceExtensions)
                {
                    // Don't even publish extensions that can't execute.
                    if (!item.CanExecute) continue;


                    if (item.OtherFiles != null)
                    {
                        foreach (var logFile in item.OtherFiles)
                        {
                            if (!state.LocalConfigs.Contains(logFile))
                            {
                                state.LocalConfigs.Add(logFile);
                            }
                        }
                    }

                    if (item.Services != null)
                    {
                        foreach (var service in item.Services)
                        {
                            if (!state.LocalConfigs.Contains(service))
                            {
                                state.LocalConfigs.Add(service);
                            }
                        }
                    }
                }

                // Activate timers
                foreach (var item in TimerExtensions)
                {
                    if (item.CanExecute) item.Start();
                }
            }

            internal override bool ComposeExtensions()
            {
                return ComposeExtensions(string.Empty);
            }

            internal override bool ComposeExtensions(string extensionsFolderPath)
            {
                try
                {
                    _logger.Trace(Strings.Debug_ImportingExtensions);

                    var catalog = new AggregateCatalog();
                    var dc = new DirectoryCatalog(".");

                    catalog.Catalogs.Add(dc);

                    // Set the extensions folder path, if it wasn't explicitly set.
                    if (String.IsNullOrEmpty(extensionsFolderPath))
                    {
                        // Get the extensions folder path.
                        var thisAssembly = Assembly.GetExecutingAssembly().Location;
                        var thisAssemblyPath = Path.GetDirectoryName(thisAssembly);
                        extensionsFolderPath = thisAssemblyPath + DefaultExtensionSubfolder;
                    }

                    // Find out if the pluginsFolder exists.
                    var folderExists = _fileSystem.DirectoryExists(extensionsFolderPath);
                    if (folderExists)
                        catalog.Catalogs.Add(new DirectoryCatalog(extensionsFolderPath));

                    catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                    _container = new CompositionContainer(catalog);
                    _container.ComposeParts(this);
                }
                catch (CompositionException compositionException)
                {
                    // TODO: logger
                    _dialogs.ShowError(compositionException.ToString(),
                                       Application.Current.MainWindow);

                    return false;
                }

                _container.Dispose();
                return true;
            }

            protected override List<T> PopulateList<T>(IEnumerable extensionsList)
            {
                if (extensionsList == null)
                {
                    throw new ArgumentNullException("extensionsList");
                }


                try
                {
                    var tempDictionary = new List<T>();
                    foreach (var item in extensionsList)
                    {
                        var tI = (T) Activator.CreateInstance(typeof (T), new[] {_logger, item});
                        tempDictionary.Add(tI);
                    }
                    return tempDictionary;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

                return new List<T>();
            }
        }
    }
}