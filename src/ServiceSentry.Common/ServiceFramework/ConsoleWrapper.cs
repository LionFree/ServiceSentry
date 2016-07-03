// -----------------------------------------------------------------------
//  <copyright file="ConsoleWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    internal abstract class ConsoleWrapper
    {
        internal static ConsoleWrapper Default
        {
            get { return new ConsoleWrapperImplementation(); }
        }

        #region Abstract Members

        /// <summary>
        ///     Gets or sets the foreground color of the console.
        /// </summary>
        internal abstract ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        ///     Clears the console buffer and corresponding console window of display information.
        /// </summary>
        internal abstract void Clear();

        /// <summary>
        ///     Reads the next line of characters from the standard input stream.
        /// </summary>
        internal abstract string ReadLine();

        /// <summary>
        ///     Obtains the next character or function key pressed by the user.
        ///     The pressed key is optionally displayed in the console.
        /// </summary>
        /// <param name="intercept">
        ///     Determines whether to display the pressed key in the console window.
        ///     true to not display the pressed key, otherwise false.
        /// </param>
        internal abstract ConsoleKeyInfo ReadKey(bool intercept);

        /// <summary>
        ///     Writes the text representation of the specified array of objects to
        ///     the standard output stream using the speficied format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">
        ///     An array of objects to write using <paramref name="format" />.
        /// </param>
        internal abstract void Write(string format, params object[] arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects,
        ///     followed by the current line terminator,
        ///     to the standard output stream using the speficied format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">
        ///     An array of objects to write using <paramref name="format" />.
        /// </param>
        internal abstract void WriteLine(string format, params object[] arg);

        /// <summary>
        ///     Clears all buffers for the standard output stream and causes any
        ///     buffered data to be written to the standard output stream.
        /// </summary>
        internal abstract void Flush();

        #endregion

        private sealed class ConsoleWrapperImplementation : ConsoleWrapper
        {
            internal override ConsoleColor ForegroundColor
            {
                get { return Console.ForegroundColor; }
                set { Console.ForegroundColor = value; }
            }

            internal override void Clear()
            {
                Console.Clear();
            }

            internal override ConsoleKeyInfo ReadKey(bool intercept)
            {
                return Console.ReadKey(intercept);
            }

            internal override void Write(string format, params object[] arg)
            {
                Console.Write(format, arg);
            }

            internal override void WriteLine(string format, params object[] arg)
            {
                Console.WriteLine(format, arg);
            }

            internal override void Flush()
            {
                Console.Out.Flush();
            }

            internal override string ReadLine()
            {
                return Console.ReadLine();
            }
        }
    }
}