// -----------------------------------------------------------------------
//  <copyright file="ConsoleHarness.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ServiceSentry.Common.Resources;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    /// <summary>
    ///     A mockable <see cref="Console" />.
    /// </summary>
    public abstract class ConsoleHarness
    {
        #region Factory Methods

        /// <summary>
        ///     Returns a new instance of the <see cref="ConsoleHarness" /> class.
        /// </summary>
        public static ConsoleHarness Default
        {
            get
            {
                return GetInstance(AssemblyInspector.Default.HasEntryAssembly
                                       ? AssemblyWrapper.Default
                                       : AssemblyWrapper.Null, ConsoleWrapper.Default);
            }
        }

        [DebuggerStepThrough]
        internal static ConsoleHarness GetInstance(AssemblyWrapper assemblyWrapper, ConsoleWrapper handler)
        {
            return new ConsoleWrapperImplementation(assemblyWrapper, handler);
        }

        #endregion

        #region Abstract Members

        /// <summary>
        ///     Clears the console buffer and corresponding
        ///     console window of display information.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        ///     Runs a service from the console given a service implementation.
        /// </summary>
        /// <param name="args">The command line arguments to pass to the service.</param>
        /// <param name="service">
        ///     The <see cref="WindowsService" /> implementation to start.
        /// </param>
        public abstract void Run(string[] args, WindowsService service);

        /// <summary>
        ///     Helper method to write a message to the console at the given foreground color.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatArguments">An object array that contains zero or more objects to format.</param>
        [DebuggerStepThrough]
        public abstract void WriteToConsole(string format, params object[] formatArguments);

        /// <summary>
        ///     Helper method to write a message to the console at the given foreground color.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The <see cref="ConsoleColor" /> in which to write the text.
        /// </param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatArguments">An object array that contains zero or more objects to format.</param>
        [DebuggerStepThrough]
        public abstract void WriteToConsole(ConsoleColor foregroundColor, string format,
                                            params object[] formatArguments);

        /// <summary>
        ///     Writes the text representation of the specified array of objects,
        ///     followed by the current line terminator, to the standard output stream
        ///     using the specified format information.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The <see cref="ConsoleColor" /> in which to write the text.
        /// </param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An array of objects to write using <paramref name="format" />.
        /// </param>
        [DebuggerStepThrough]
        public abstract void WriteLine(ConsoleColor foregroundColor, string format, params object[] args);

        /// <summary>
        ///     Writes the text representation of the specified array of objects,
        ///     followed by the current line terminator, to the standard output stream
        ///     using the specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An array of objects to write using <paramref name="format" />.
        /// </param>
        [DebuggerStepThrough]
        public abstract void WriteLine(string format, params object[] args);

        /// <summary>
        ///     Writes the text representation of the specified 32-bit signed integer value,
        ///     followed by the current line terminator, to the standard output stream
        ///     using the specified format information.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [DebuggerStepThrough]
        public abstract void WriteLine(int value);

        /// <summary>
        ///     Writes a blank line to the console harness.
        /// </summary>
        [DebuggerStepThrough]
        public abstract void WriteLine();

        /// <summary>
        ///     Writes the text representation of the specified 32-bit signed integer value
        ///     to the standard output stream using the specified format information.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [DebuggerStepThrough]
        public abstract void Write(int value);

        /// <summary>
        ///     Writes the text representation of the specified array of objects
        ///     to the standard output stream using the specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An array of objects to write using <paramref name="format" />.
        /// </param>
        [DebuggerStepThrough]
        public abstract void Write(string format, params object[] args);

        /// <summary>
        ///     Writes the text representation of the specified array of objects
        ///     to the standard output stream using the specified format information.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The <see cref="ConsoleColor" /> in which to write the text.
        /// </param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An array of objects to write using <paramref name="format" />.
        /// </param>
        [DebuggerStepThrough]
        public abstract void Write(ConsoleColor foregroundColor, string format, params object[] args);

        /// <summary>
        ///     Reads the next line of characters from the standard input stream.
        /// </summary>
        [DebuggerStepThrough]
        public abstract string ReadLine();

        /// <summary>
        ///     Writes the text representation of the specified
        ///     array of objects to the standard output stream
        ///     using the specified format information, and then
        ///     reads the next line of characters from the
        ///     standard input stream.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The <see cref="ConsoleColor" /> in which to
        ///     write the text.
        /// </param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An array of objects to write using
        ///     <paramref name="format" />.
        /// </param>
        [DebuggerStepThrough]
        public abstract string ReadLine(ConsoleColor foregroundColor, string format, params object[] args);

        /// <summary>
        ///     Writes the text representation of the specified
        ///     array of objects to the standard output stream
        ///     using the specified format information, and then
        ///     reads the next line of characters from the
        ///     standard input stream.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An array of objects to write using
        ///     <paramref name="format" />.
        /// </param>
        [DebuggerStepThrough]
        public abstract string ReadLine(string format, params object[] args);

        /// <summary>
        ///     Obtains the next character or function key pressed by the user.
        ///     The pressed key is displayed in the console window.
        /// </summary>
        [DebuggerStepThrough]
        public abstract ConsoleKeyInfo ReadKey();

        /// <summary>
        ///     Obtains the next character or function key pressed by the user.
        ///     The pressed key is optionally displayed in the console window.
        /// </summary>
        /// <param name="intercept">
        ///     Determines whether to display the pressed key
        ///     in the console window. true to display the pressed key, otherwise false.
        /// </param>
        [DebuggerStepThrough]
        public abstract ConsoleKeyInfo ReadKey(bool intercept);

        #endregion

        private sealed class ConsoleWrapperImplementation : ConsoleHarness
        {
            private readonly AssemblyWrapper _attributes;
            private readonly ConsoleWrapper _console;

            internal ConsoleWrapperImplementation(AssemblyWrapper attributes, ConsoleWrapper handler)
            {
                _attributes = attributes;
                _console = handler;
            }


            public override void Clear()
            {
                try
                {
                    _console.Clear();
                }
                catch (IOException ex)
                {
                    Trace.WriteLine(String.Format(CommonStrings.EXCEPTION_WithMessage, ex.Message));
                }
            }

            public override void Run(string[] args, WindowsService service)
            {
                var serviceName = service.ServiceName;
                var header = Environment.NewLine +
                             String.Format(CultureInfo.InvariantCulture, "{0} : built {1}", serviceName,
                                           _attributes.BuildDate);

                var endpoint = Environment.NewLine + String.Format("Service started.  Endpoint: {0}", service.Endpoint) +
                               Environment.NewLine;

                var isRunning = true;


                // Can't clear the console in a unit test,
                // so this line will throw an exception.
                Clear();
                WriteToConsole(ConsoleColor.White, header);
                WriteToConsole(ConsoleColor.White, endpoint);

                // simulate starting the windows service
                service.OnStart(args);

                // let it run as long as Q is not pressed
                while (isRunning)
                {
                    WriteLine();
                    WriteToConsole(ConsoleColor.Yellow, "Enter [P]ause, [R]esume, or [Q]uit : ");

                    isRunning = HandleConsoleInput(service, ReadKey());
                }

                // stop and shutdown
                service.OnStop();
                service.OnShutdown();
            }

            private bool HandleConsoleInput(WindowsService service, ConsoleKeyInfo key)
            {
                var canContinue = true;

                // Check input

                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        canContinue = false;
                        break;

                    case ConsoleKey.P:
                        service.OnPause();
                        break;

                    case ConsoleKey.R:
                        service.OnContinue();
                        break;

                    default:
                        WriteToConsole(ConsoleColor.Red, "Did not understand that input, try again.");
                        break;
                }

                return canContinue;
            }

            [DebuggerStepThrough]
            public override void WriteToConsole(string format, params object[] formatArguments)
            {
                WriteToConsole(ConsoleColor.Gray, format, formatArguments);
            }

            [DebuggerStepThrough]
            public override void WriteToConsole(ConsoleColor foregroundColor, string format,
                                                params object[] formatArguments)
            {
                var originalColor = _console.ForegroundColor;
                _console.ForegroundColor = foregroundColor;

                _console.WriteLine(format, formatArguments);
                Trace.WriteLine(string.Format(format, formatArguments));
                _console.Flush();

                _console.ForegroundColor = originalColor;
            }


            public override void WriteLine(ConsoleColor foregroundColor, string format, params object[] args)
            {
                WriteToConsole(foregroundColor, format, args);
            }

            public override void WriteLine(string format, params object[] formatArguments)
            {
                WriteToConsole(format, formatArguments);
            }

            public override void WriteLine(int value)
            {
                WriteLine("{0}", value.ToString(CultureInfo.InvariantCulture));
            }

            public override void WriteLine()
            {
                WriteLine(" ");
            }

            public override void Write(int value)
            {
                Write("{0}", value.ToString(CultureInfo.InvariantCulture));
            }

            public override void Write(string format, params object[] args)
            {
                Write(ConsoleColor.Gray, format, args);
            }

            
            public override void Write(ConsoleColor foregroundColor, string format, params object[] formatArguments)
            {
                var originalColor = _console.ForegroundColor;
                _console.ForegroundColor = foregroundColor;

                _console.Write(format, formatArguments);
                Trace.Write(string.Format(format, formatArguments));
                _console.Flush();

                _console.ForegroundColor = originalColor;
            }

            public override string ReadLine()
            {
                return ReadLine(ConsoleColor.Gray, string.Empty);
            }

            public override string ReadLine(ConsoleColor foregroundColor, string format, params object[] args)
            {
                Write(foregroundColor, format, args);
                return _console.ReadLine();
            }

            public override string ReadLine(string format, params object[] args)
            {
                return ReadLine(ConsoleColor.Gray, format, args);
            }

            public override ConsoleKeyInfo ReadKey()
            {
                return ReadKey(true);
            }

            public override ConsoleKeyInfo ReadKey(bool intercept)
            {
                return _console.ReadKey(intercept);
            }
        }
    }
}