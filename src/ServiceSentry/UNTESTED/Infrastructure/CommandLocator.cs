// -----------------------------------------------------------------------
//  <copyright file="CommandLocator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Client.UNTESTED.ViewModels.Commands;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    public abstract class CommandLocator
    {
        public abstract ServiceTogglingCommands ServiceTogglingCommands { get; }
        public abstract ShellCommands ShellCommands { get; }
        public abstract FileCommands FileCommands { get; }

        internal static CommandLocator GetInstance(Logger logger, ApplicationState applicationState,
                                                   ViewController viewController)
        {
            return new CommandLocatorImplementation(logger, applicationState, viewController);
        }

        private sealed class CommandLocatorImplementation : CommandLocator
        {
            private readonly FileCommands _fileCommands;
            private readonly ServiceTogglingCommands _serviceTogglingCommands;
            private readonly ShellCommands _shellCommands;

            internal CommandLocatorImplementation(Logger logger, ApplicationState applicationState,
                                                  ViewController viewController)
            {
                _serviceTogglingCommands = ServiceTogglingCommands.GetInstance(applicationState,
                                                                               viewController.ServiceHandler);
                _shellCommands = ShellCommands.GetInstance(viewController);
                _fileCommands = FileCommands.GetInstance(logger);
            }


            public override ServiceTogglingCommands ServiceTogglingCommands
            {
                get { return _serviceTogglingCommands; }
            }

            public override ShellCommands ShellCommands
            {
                get { return _shellCommands; }
            }

            public override FileCommands FileCommands
            {
                get { return _fileCommands; }
            }
        }
    }
}