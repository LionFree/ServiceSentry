// -----------------------------------------------------------------------
//  <copyright file="ViewController.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using ServiceSentry.Client.Infrastructure;
using ServiceSentry.Client.Properties;
using ServiceSentry.Client.UNTESTED.Framework.Notification;
using ServiceSentry.Client.UNTESTED.Infrastructure;
using ServiceSentry.Client.UNTESTED.Views;
using ServiceSentry.Client.UNTESTED.Views.Controls;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Imports;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Client;
using WPFNotifyIcon;

#endregion

namespace ServiceSentry.Client.UNTESTED.Model
{
    public abstract class ViewController : DisposableBase
    {
        internal static ViewController GetInstance(Logger logger, ExceptionHandler exceptionHandler,
                                                   ApplicationState applicationState, MonitorServiceWatchdog watchdog,
                                                   ConfigFile configFile)
        {
            return new ViewControllerImplementation(logger, exceptionHandler,
                                                    applicationState, watchdog, configFile);
        }

        #region Abstract Members

        internal abstract ViewModelLocator ViewModels { get; }
        internal abstract CommandLocator Commands { get; }
        internal abstract ViewLocator Views { get; }
        internal abstract NotificationArea NotificationArea { get; }

        /// <summary>
        ///     Provides access to the system tray area.
        /// </summary>
        internal abstract TaskbarIcon TaskbarIcon { get; set; }

        internal abstract ServiceHandler ServiceHandler { get; }
        internal abstract ServiceListBehavior Behavior { get; }
        internal abstract Logger Logger { get; }

        internal abstract void PublishViews(List<ImportedTabItem> extensionTabs,
                                            List<ImportedOptionsTabItem> optionExtensionTabs,
                                            List<ImportedContextMenu> contextMenuExtensions);


        /// <summary>
        ///     Displays the main application window and assigns
        ///     it as the application's <see cref="Application.MainWindow" />.
        /// </summary>
        internal abstract void ShowMainWindow();

        /// <summary>
        ///     Closes the main window and
        ///     terminates the application.
        /// </summary>
        protected abstract void OnMainWindowClosing(object sender, CancelEventArgs e);

        /// <summary>
        ///     Closes the main window and either exits
        ///     the application or displays the taskbar
        ///     icon and remains active.
        /// </summary>
        /// <param name="forceShutdown">
        ///     Whether the application
        ///     should perform a shutdown anyway.
        /// </param>
        internal abstract void CloseMainApplication(bool forceShutdown);

        /// <summary>
        ///     Minimizes the application to the system tray.
        /// </summary>
        internal abstract void MinimizeToNotificationArea();

        /// <summary>
        ///     Updates application settings to reflect a more
        ///     recent installation of the application, if necessary.
        /// </summary>
        internal abstract void UpgradeSettings();

        /// <summary>
        ///     Enginelies the culture selected in the control panel to databinding
        ///     (when the target property is a string), rather than the WPF default
        ///     of US English.
        /// </summary>
        internal abstract void ApplyInternationalizationFix();

        #endregion

        private sealed class ViewControllerImplementation : ViewController
        {
            private readonly CommandLocator _commands;
            private readonly ExceptionHandler _exceptionHandler;
            private readonly Logger _logger;
            private readonly NotificationArea _notificationArea;
            private readonly ServiceListBehavior _serviceBehavior;
            private readonly ServiceHandler _serviceHandler;
            private readonly ViewModelLocator _viewModels;
            private readonly ViewLocator _views;
            private TaskbarIcon _taskbarIcon;
            
            internal ViewControllerImplementation(
                Logger logger,
                ExceptionHandler exceptionHandler,
                ApplicationState applicationState,
                MonitorServiceWatchdog watchdog,
                ConfigFile configFile)
            {
                _logger = logger;
                _exceptionHandler = exceptionHandler;
                _serviceBehavior = ServiceListBehavior.GetInstance(_logger, watchdog);

                _serviceHandler = ServiceHandler.GetInstance(_logger,
                                                             applicationState,
                                                             ConfigFileHandler.GetInstance(_logger),
                                                             _serviceBehavior, configFile, watchdog);

                _commands = CommandLocator.GetInstance(_logger, applicationState, this);
                _viewModels = ViewModelLocator.GetInstance(
                    applicationState, this, Commands, watchdog, _serviceHandler, _logger);
                _views = ViewLocator.GetInstance(ViewModels);
                _notificationArea = NotificationArea.GetInstance(this);

                _serviceHandler.MonitorError +=
                    MonitorErrorHandler.GetInstance(_notificationArea, _logger).OnMonitorError;

                
                TaskbarIcon = TrayIconView.GetInstance(ViewModels.TrayIconViewModel);

                Application.Current.MainWindow = null; // _views.ShellView;
            }

            internal override ServiceListBehavior Behavior
            {
                get { return _serviceBehavior; }
            }

            internal override ServiceHandler ServiceHandler
            {
                get { return _serviceHandler; }
            }

            internal override ViewModelLocator ViewModels
            {
                get { return _viewModels; }
            }

            internal override CommandLocator Commands
            {
                get { return _commands; }
            }

            internal override ViewLocator Views
            {
                get { return _views; }
            }

            internal override Logger Logger
            {
                get { return _logger; }
            }

            internal override NotificationArea NotificationArea
            {
                get { return _notificationArea; }
            }

            internal override TaskbarIcon TaskbarIcon
            {
                get { return _taskbarIcon; }
                set
                {
                    if (_taskbarIcon != null)
                    {
                        //dispose current tray handler
                        _taskbarIcon.Dispose();
                    }

                    _taskbarIcon = value;
                    OnPropertyChanged();
                }
            }

            internal override void PublishViews(List<ImportedTabItem> extensionTabs,
                                                List<ImportedOptionsTabItem> optionExtensionTabs,
                                                List<ImportedContextMenu> contextMenuExtensions)
            {
                var tabs = new[] {ServicesTabHarness.GetInstance(ViewModels.ServicesViewModel).GetTabItem()};

                ViewModels.ShellWindowViewModel.PublishTabs(tabs, extensionTabs);
                ViewModels.OptionsViewModel.PublishTabs(optionExtensionTabs);

                _serviceHandler.RegenerateServiceGroups();
                ViewModels.TrayIconViewModel.PublishAll(contextMenuExtensions);
            }

            internal override void ShowMainWindow()
            {
                var app = Application.Current;

                if (app.MainWindow == null || !(app.MainWindow is ShellWindowView))
                {
                    app.MainWindow = ShellWindowView.GetInstance(ViewModels.ShellWindowViewModel);

                    app.MainWindow.Show();
                    app.MainWindow.Activate();
                    app.MainWindow.Closing += OnMainWindowClosing;
                }
                else
                {
                    //just show the window on top of others
                    if (app.MainWindow.WindowState == WindowState.Minimized)
                        app.MainWindow.WindowState = WindowState.Normal;

                    app.MainWindow.Activate();
                    app.MainWindow.BringIntoView();
                }

                // show tray icon
                //if (TaskbarIcon == null)
                //{
                //    //TaskbarIcon = Utilities.InitializeNotificationAreaIcon(ViewModels.TrayIconViewModel);
                //    TaskbarIcon = TrayIconView.GetInstance(ViewModels.TrayIconViewModel);
                //}
                TaskbarIcon.Visibility = Visibility.Visible;
            }

            protected override void OnMainWindowClosing(object sender, CancelEventArgs e)
            {
                ////Contract.Requires(sender != null);

                //deregister event listener, if required
                ((Window) sender).Closing -= OnMainWindowClosing;

                //reset main window in order to prevent further code
                //to close it again while it is being closed
                Application.Current.MainWindow = null;

                //close application if necessary
                CloseMainApplication(false);
            }

            internal override void MinimizeToNotificationArea()
            {
                //close main window
                //var mainWindow = Application.Current.MainWindow;
                if (Application.Current.MainWindow != null)
                {
                    // deregister closing event listener - if this 
                    // method is not invoked due to the window 
                    // being closed already, the closing window
                    // will not trigger any further action
                    Application.Current.MainWindow.Closing -= OnMainWindowClosing;
                    Application.Current.MainWindow.Close();
                }

                //if (TaskbarIcon == null)
                //{
                //    TaskbarIcon = TrayIconView.GetInstance(ViewModels.TrayIconViewModel);
                //}

                //show tray icon
                TaskbarIcon.Visibility = Visibility.Visible;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // dispose of the system tray handler
                    if (TaskbarIcon != null)
                    {
                        TaskbarIcon.Dispose();
                        TaskbarIcon = null;
                    }
                }

                base.Dispose(disposing);
            }

            internal override void CloseMainApplication(bool forceShutdown)
            {
                if (!forceShutdown && Settings.Default.CloseToSystemTray)
                {
                    //if the app was configured to close to tray
                    //don't shutdown but display system tray icon
                    MinimizeToNotificationArea();
                }
                else
                {
                    try
                    {
                        Settings.Default.Save();
                    }
                    catch (Exception e)
                    {
                        //e.ReportAndLogException(Resources.Error_Displayed_When_Saving_Settings_Fails);
                        _exceptionHandler.ReportException(e, Application.Current.MainWindow);
                    }

                    //dispose manager
                    Dispose();

                    //shutdown the application
                    Application.Current.Shutdown();
                }
            }

            [DebuggerStepThrough]
            internal override void UpgradeSettings()
            {
                if (Utilities.IsDesignMode) return;

                if (Settings.Default.IsUpdateRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.IsUpdateRequired = false;
                }
            }

            [DebuggerStepThrough]
            internal override void ApplyInternationalizationFix()
            {
                // By default, when you use data binding and the target property 
                // is a string, WPF will format your value using the US English
                // culture, to use the correct setting (the one selected in the
                // control panel) add the following code before loading any GUI 
                // (the Application.Startup event is a good place.)
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof (FrameworkElement),
                    new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(
                            CultureInfo.CurrentCulture.IetfLanguageTag)));

                FrameworkContentElement.LanguageProperty.OverrideMetadata(
                    typeof (Run),
                    new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(
                            CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
        }
    }

    
}