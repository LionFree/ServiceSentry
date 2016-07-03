// -----------------------------------------------------------------------
//  <copyright file="ShellCommands.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Input;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.Views.Dialogs;
using ServiceSentry.Client.UNTESTED.Views.Helpers;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels.Commands
{
    public abstract class ShellCommands : PropertyChangedBase
    {
        //public abstract ICommand CheckForUpdate { get; }
        //public abstract ICommand CloseCommand { get; }

        public abstract ICommand ExitApplicationCommand { get; }
        public abstract ICommand OpenOptionsViewCommand { get; }
        public abstract ICommand OpenAboutViewCommand { get; }
        public abstract ICommand OpenShellWindowCommand { get; }
        public abstract ICommand MinimizeShellWindowCommand { get; }
        public abstract ICommand OpenLogViewCommand { get; }

        internal static ShellCommands GetInstance(ViewController controller)
        {
            return new ShellCommandsImplementation(controller);
        }

        internal abstract void MinimizeShellWindow();

        private sealed class ShellCommandsImplementation : ShellCommands
        {
            private readonly ViewController _controller;
            private ICommand _exitApplicationCommand;
            private ICommand _minimizeShellWindowCommand;
            private ICommand _openAboutViewCommand;
            private ICommand _openShellWindowCommand;
            private ICommand _optionsCommand;
            private ICommand _logCommand;

            internal ShellCommandsImplementation(ViewController controller)
            {
                _controller = controller;
            }

            public override ICommand OpenShellWindowCommand
            {
                get
                {
                    return _openShellWindowCommand ??
                           (_openShellWindowCommand = new RelayCommand("Open", param => _controller.ShowMainWindow()));
                }
            }

            public override ICommand ExitApplicationCommand
            {
                get
                {
                    return _exitApplicationCommand ??
                           (_exitApplicationCommand =
                            new RelayCommand("ExitApplication", param => _controller.CloseMainApplication(true)));
                }
            }

            public override ICommand OpenLogViewCommand
            {
                get
                {
                    return _logCommand ??
                           (_logCommand =
                            new RelayCommand("Log",
                                             param =>
                                             _controller.ViewModels.LogViewModel.Show()));
                } 
            }

            public override ICommand OpenOptionsViewCommand
            {
                get
                {
                    return _optionsCommand ??
                           (_optionsCommand =
                            new RelayCommand("Options",
                                             param =>
                                             OptionsView.GetInstance(_controller.ViewModels.OptionsViewModel)
                                                        .ShowCentered()));
                }
            }

            public override ICommand OpenAboutViewCommand
            {
                get
                {
                    return _openAboutViewCommand ??
                           (_openAboutViewCommand =
                            new RelayCommand("About",
                                             param =>
                                             AboutView.GetInstance(_controller.ViewModels.AboutViewModel).ShowCentered()));
                }
            }

            public override ICommand MinimizeShellWindowCommand
            {
                get
                {
                    return _minimizeShellWindowCommand ??
                           (_minimizeShellWindowCommand =
                            new RelayCommand("Minimize", param => MinimizeShellWindow()));
                }
            }

            internal override void MinimizeShellWindow()
            {
                _controller.MinimizeToNotificationArea();
            }

            //public override ICommand CheckForUpdate
            //{
            //    get { throw new NotImplementedException(); }
            //}

            //public override ICommand CloseCommand
            //{
            //    get
            //    {
            //        return _closeCommand ??
            //               (_closeCommand =
            //                new RelayCommand("Close", param => _controller.CloseMainApplication(false)));
            //    }
            //}
        }
    }
}