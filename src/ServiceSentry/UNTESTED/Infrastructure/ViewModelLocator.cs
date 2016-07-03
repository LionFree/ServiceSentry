// -----------------------------------------------------------------------
//  <copyright file="ViewModelLocator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;

#endregion

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    public abstract class ViewModelLocator
    {
        internal abstract ServicesViewModel ServicesViewModel { get; }
        internal abstract NotificationOptionsViewModel NotificationOptionsViewModel { get; }
        internal abstract GeneralOptionsViewModel GeneralOptionsViewModel { get; }
        internal abstract OptionsViewModel OptionsViewModel { get; }
        internal abstract AboutViewModel AboutViewModel { get; }
        internal abstract AddServicesViewModel AddServicesViewModel { get; }
        internal abstract ShellWindowViewModel ShellWindowViewModel { get; }
        internal abstract TrayIconViewModel TrayIconViewModel { get; }
        internal abstract LogViewModel LogViewModel { get; }
        internal abstract DebugOptionsViewModel DebugOptionsViewModel { get; }
        internal abstract WardenOptionsViewModel WardenOptionsViewModel { get; }

        internal static ViewModelLocator GetInstance(
            ApplicationState applicationState,
            ViewController viewController,
            CommandLocator commandLocator,
            MonitorServiceWatchdog watchdog,
            ServiceHandler serviceHandler,
            Logger logger)
        {
            return new VMLocatorImplementation(applicationState, viewController,
                                               commandLocator, watchdog, serviceHandler, logger);
        }

        private sealed class VMLocatorImplementation : ViewModelLocator
        {
            private readonly AboutViewModel _aboutViewModel;
            private readonly AddServicesViewModel _addServicesViewModel;
            private readonly DebugOptionsViewModel _debugOptionsViewModel;
            private readonly GeneralOptionsViewModel _generalOptionsViewModel;
            private readonly LogViewModel _logViewModel;
            private readonly Logger _logger;
            private readonly NotificationOptionsViewModel _notificationOptionsViewModel;
            private readonly OptionsViewModel _optionsViewModel;
            private readonly ServiceParametersViewModel _serviceParametersViewModel;
            private readonly ServicesViewModel _servicesViewModel;
            private readonly ShellWindowViewModel _shellWindowViewModel;
            private readonly TrayIconViewModel _trayIconViewModel;
            private readonly WardenOptionsViewModel _wardenOptionsViewModel;

            public VMLocatorImplementation(ApplicationState applicationState,
                                           ViewController viewController,
                                           CommandLocator commandLocator,
                                           MonitorServiceWatchdog watchdog,
                                           ServiceHandler serviceHandler,
                                           Logger logger)
            {
                _logger = logger;
                _logViewModel = LogViewModel.GetInstance(_logger, LogLevel.Info);

                _addServicesViewModel = AddServicesViewModel.GetInstance(applicationState, viewController);
                _serviceParametersViewModel = ServiceParametersViewModel.GetInstance(_logger, applicationState,
                                                                                     viewController);
                _servicesViewModel = ServicesViewModel.GetInstance(_logger, applicationState, _addServicesViewModel,
                                                                   _serviceParametersViewModel,
                                                                   commandLocator.ServiceTogglingCommands,
                                                                   commandLocator.FileCommands, watchdog);

                _aboutViewModel = AboutViewModel.GetInstance(_logger);

                _generalOptionsViewModel = GeneralOptionsViewModel.GetInstance(applicationState.LocalConfigs, _logger);
                _wardenOptionsViewModel = WardenOptionsViewModel.GetInstance(
                    applicationState.LocalConfigs.WardenDetails, _logger);

                _notificationOptionsViewModel =
                    NotificationOptionsViewModel.GetInstance(applicationState.LocalConfigs.NotificationDetails, _logger);
                _debugOptionsViewModel =
                    DebugOptionsViewModel.GetInstance(applicationState.LocalConfigs.DebugLogConfiguration, _logger);


                _optionsViewModel = OptionsViewModel.GetInstance(applicationState.LocalConfigs,
                    _logger,
                    serviceHandler,
                    _generalOptionsViewModel,
                    _wardenOptionsViewModel,
                                                                 _notificationOptionsViewModel,
                                                                 _debugOptionsViewModel);

                _shellWindowViewModel = ShellWindowViewModel.GetInstance(_logger, applicationState.Services,
                                                                         commandLocator.ShellCommands);

                _trayIconViewModel = TrayIconViewModel.GetInstance(_logger,
                                                                   applicationState,
                                                                   commandLocator.ShellCommands,
                                                                   commandLocator.ServiceTogglingCommands,
                                                                   watchdog);
            }

            internal override LogViewModel LogViewModel
            {
                get { return _logViewModel; }
            }

            internal override DebugOptionsViewModel DebugOptionsViewModel
            {
                get { return _debugOptionsViewModel; }
            }

            internal override ShellWindowViewModel ShellWindowViewModel
            {
                get { return _shellWindowViewModel; }
            }

            internal override TrayIconViewModel TrayIconViewModel
            {
                get { return _trayIconViewModel; }
            }

            internal override ServicesViewModel ServicesViewModel
            {
                get { return _servicesViewModel; }
            }

            internal override AddServicesViewModel AddServicesViewModel
            {
                get { return _addServicesViewModel; }
            }

            internal override AboutViewModel AboutViewModel
            {
                get { return _aboutViewModel; }
            }

            internal override OptionsViewModel OptionsViewModel
            {
                get { return _optionsViewModel; }
            }

            internal override GeneralOptionsViewModel GeneralOptionsViewModel
            {
                get { return _generalOptionsViewModel; }
            }

            internal override NotificationOptionsViewModel NotificationOptionsViewModel
            {
                get { return _notificationOptionsViewModel; }
            }

            internal override WardenOptionsViewModel WardenOptionsViewModel
            {
                get { return _wardenOptionsViewModel; }
            }
        }
    }
}