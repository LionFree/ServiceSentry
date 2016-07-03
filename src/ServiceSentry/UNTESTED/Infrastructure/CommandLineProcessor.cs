// -----------------------------------------------------------------------
//  <copyright file="CommandLineProcessor.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Common.CommandLine;

#endregion

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    internal abstract class CommandLineProcessor
    {
        internal abstract bool DebugMode { get; }
        internal abstract bool StartMinimized { get; }
        internal abstract bool UnitTest { get; }

        internal static CommandLineProcessor GetInstance()
        {
                return GetInstance(CommandLineParser.GetInstance(),
                                   CommandLineUsageViewModel.GetInstance());
        }

        internal static CommandLineProcessor GetInstance(CommandLineParser parser, CommandLineUsageViewModel viewModel)
        {
            return new CommandLineProcImplementation(parser, viewModel);
        }

        public virtual event EventHandler<CommandLineArgs> CommandLineArgsPassed;

        internal abstract void ShowDefaultHelpOnConsole(string[] args);

        /// <summary>
        ///     Processes the command-line arguments.
        /// </summary>
        /// <param name="args">The arguments to process.</param>
        /// <returns>
        ///     <c>true</c> if successful, otherwise <c>false</c>.
        /// </returns>
        internal abstract bool Process(string[] args);

        protected abstract void OnCommandLineArgsPassed();


        private sealed class CommandLineProcImplementation : CommandLineProcessor
        {
            private readonly CommandLineParser _parser;
            private readonly CommandLineUsageViewModel _viewModel;
            private bool _debugMode;
            private bool _startStartMinimized;

            internal CommandLineProcImplementation(CommandLineParser parser, CommandLineUsageViewModel viewModel)
            {
                _parser = parser;
                _viewModel = viewModel;
            }

            internal override bool DebugMode
            {
                get { return _debugMode; }
            }

            internal override bool StartMinimized
            {
                get { return _startStartMinimized; }
            }

            internal override bool UnitTest
            {
                get { return false; }
            }

            internal override void ShowDefaultHelpOnConsole(string[] args)
            {
            }

            internal override bool Process(string[] args)
            {
                if (args == null || args.Length < 1) return true;

                var commandLine = _parser.Parse(args);

                var argsPassed = false;

                if (commandLine["d"] != null || commandLine["debug"] != null)
                {
                    _debugMode = true;
                    argsPassed = true;
                }

                if (commandLine[AutoStart.MinimizedArgumentShort] != null ||
                    commandLine[AutoStart.MinimizedArgumentLong] != null)
                {
                    _startStartMinimized = true;
                    argsPassed = true;
                }

                if (argsPassed)
                {
                    OnCommandLineArgsPassed();
                    return true;
                }

                _viewModel.ShowDialog(args);
                return false;
            }

            protected override void OnCommandLineArgsPassed()
            {
                var handler = CommandLineArgsPassed;
                if (handler == null) return;

                var args = new CommandLineArgs {DebugMode = _debugMode, StartMinimized = _startStartMinimized};
                handler(this, args);
            }
        }
    }

    internal sealed class CommandLineArgs : EventArgs
    {
        internal bool DebugMode { get; set; }
        internal bool StartMinimized { get; set; }
    }
}