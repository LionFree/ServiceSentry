// -----------------------------------------------------------------------
//  <copyright file="ToggleServiceCommands.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Windows.Input;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels.Commands
{
    public abstract class ServiceTogglingCommands : PropertyChangedBase
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the
        ///     service buttons (start, stop, etc.) are enabled in the user interface (UI).
        /// </summary>
        public abstract bool EnableServiceButtons { get; set; }

        public abstract ICommand StartServiceCommand { get; }
        public abstract ICommand StartServiceGroupCommand { get; }
        public abstract ICommand StopAllServicesCommand { get; }
        public abstract ICommand StopServiceCommand { get; }
        public abstract ICommand StopServiceGroupCommand { get; }
        public abstract ICommand StartAllServicesCommand { get; }
        public abstract ICommand RestartAllServicesCommand { get; }
        public abstract ICommand RestartServiceCommand { get; }
        public abstract ICommand RestartServiceGroupCommand { get; }

        internal abstract void StartService(object service);
        internal abstract void StopService(object service);
        internal abstract void RestartService(object service);
        internal abstract void StopServiceGroup(object service);
        internal abstract void StartServiceGroup(object service);
        internal abstract void RestartServiceGroup(object service);
        internal abstract void StopAllServices(object service);
        internal abstract void StartAllServices(object service);
        internal abstract void RestartAllServices(object service);

        internal static ServiceTogglingCommands GetInstance(ApplicationState applicationState,
                                                            ServiceHandler serviceHandler)
        {
            return new ToggleServiceCommandsImplementation(applicationState, serviceHandler);
        }

        private sealed class ToggleServiceCommandsImplementation : ServiceTogglingCommands
        {
            private bool _enableServiceButtons = true;
            private ICommand _restartAllServicesCommand;
            private ICommand _startAllServicesCommand;
            private ICommand _startServiceCommand;
            private RelayCommand _stopAllServicesCommand;
            private ICommand _startServiceGroupCommand;
            private ICommand _stopServiceGroupCommand;
            private ICommand _stopServiceCommand;
            private ICommand _restartServiceCommand;
            private ICommand _restartServiceGroupCommand;
            private readonly ServiceHandler _serviceHandler;

            internal ToggleServiceCommandsImplementation(ApplicationState applicationState,
                                                         ServiceHandler serviceHandler)
            {
                _serviceHandler = serviceHandler;
                
                if (applicationState.Services == null) return;

                _serviceHandler.WorkerCompleted += ServiceList_WorkerCompleted;
            }


            public override bool EnableServiceButtons
            {
                get { return _enableServiceButtons; }
                set
                {
                    if (_enableServiceButtons == value) return;
                    _enableServiceButtons = value;
                    OnPropertyChanged();
                }
            }


            public override ICommand StartServiceCommand
            {
                get
                {
                    return _startServiceCommand ??
                           (_startServiceCommand = new RelayCommand("StartService", StartService, CanToggleServices));
                }
            }

            public override ICommand StartServiceGroupCommand
            {
                get
                {
                    return _startServiceGroupCommand ??
                         (_startServiceGroupCommand =
                          new RelayCommand("StartServiceGroup", StartServiceGroup, CanToggleServices));
                }
            }

            public override ICommand StopAllServicesCommand
            {
                get
                {
                    return _stopAllServicesCommand ??
                           (_stopAllServicesCommand =
                            new RelayCommand("StopAllServices", StopAllServices, CanToggleServices));
                }
            }

            public override ICommand StopServiceCommand
            {
                get
                {
                    return _stopServiceCommand ??
                        (_stopServiceCommand = new RelayCommand("StopService", StopService, CanToggleServices));
                }
            }

            public override ICommand StopServiceGroupCommand
            {
                get
                {
                    return _stopServiceGroupCommand ??
                       (_stopServiceGroupCommand =
                        new RelayCommand("StopServiceGroup", StopServiceGroup, CanToggleServices));
                }
            }

            public override ICommand StartAllServicesCommand
            {
                get
                {
                    return _startAllServicesCommand ??
                           (_startAllServicesCommand =
                            new RelayCommand("StartAllServices", StartAllServices, CanToggleServices));
                }
            }

            public override ICommand RestartAllServicesCommand
            {
                get
                {
                    return _restartAllServicesCommand ??
                           (_restartAllServicesCommand =
                            new RelayCommand("RestartAllServices", RestartAllServices, CanToggleServices));
                }
            }

            public override ICommand RestartServiceCommand
            {
                get
                {
                    return _restartServiceCommand ??
                      (_restartServiceCommand = new RelayCommand("RestartService", RestartService, CanToggleServices));
                }
            }

            public override ICommand RestartServiceGroupCommand
            {
                get
                {
                    return _restartServiceGroupCommand ??
                    (_restartServiceGroupCommand = new RelayCommand("RestartServiceGroup", RestartServiceGroup, CanToggleServices));
                }
            }

            private bool CanToggleServices(object obj)
            {
                return EnableServiceButtons;
            }

            internal override void StartService(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.StartService(service);
            }

            internal override void StopService(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.StopService(service);
            }

            internal override void RestartService(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.RestartService(service);
            }

            internal override void StopServiceGroup(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.StopServiceGroup(service);
            }

            internal override void StartServiceGroup(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.StartServiceGroup(service);
            }

            internal override void RestartServiceGroup(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.RestartServiceGroup(service);
            }

            internal override void StopAllServices(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.StopAllServices(service);
            }

            internal override void StartAllServices(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.StartAllServices(service);
            }

            internal override void RestartAllServices(object service)
            {
                EnableServiceButtons = false;
                _serviceHandler.RestartAllServices(service);
            }

            /// <summary>
            ///     Re-enables the service buttons and sets the cursor to a normal arrow.
            /// </summary>
            private void ServiceList_WorkerCompleted(object sender, EventArgs e)
            {
                EnableServiceButtons = true;

                //The WPF CommandBinding CommandManager only pays attention to certain conditions 
                // in determining when the CanExecute command target has changed, such as change 
                // in keyboard focus. In situations where the CommandManager does not sufficiently
                // determine a change in conditions that cause a command to not be able to execute, 
                // InvalidateRequerySuggested can be called to force the CommandManager to raise
                // the RequerySuggested event.
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}