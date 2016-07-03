// -----------------------------------------------------------------------
//  <copyright file="LogEntry.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

#endregion

namespace ServiceSentry.Extensibility.Logging
{
    public abstract class LogEntry : PropertyChangedBase
    {
        /// <summary>
        ///     Gets the date of the first log event created.
        /// </summary>
        public static readonly DateTime ZeroDate = DateTime.UtcNow;

        private static int _globalSequenceId;
        
        /// <summary>
        ///     Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///     Instance of <see cref="LogEntry" />.
        /// </returns>
        public static LogEntry Create(LogLevel logLevel, [Localizable(false)] string message)
        {
            return Create(logLevel, message, null);
        }

        /// <summary>
        ///     Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     Instance of <see cref="LogEntry" />.
        /// </returns>
        public static LogEntry Create(LogLevel logLevel, [Localizable(false)] string message, Exception exception)
        {
            return new LogEntryImplementation(logLevel, message, exception);
        }

        #region Abstract Members

        /// <summary>
        ///     Gets a value indicating whether stack trace has been set for this event.
        /// </summary>
        public abstract bool HasStackTrace { get; }

        /// <summary>
        ///     Gets or sets the exception information.
        /// </summary>
        public abstract Exception Exception { get; set; }

        /// <summary>
        ///     Gets or sets the exception information.
        /// </summary>
        public abstract bool HasException { get; }

        /// <summary>
        ///     Gets the unique identifier of log event which is automatically generated
        ///     and monotonically increasing.
        /// </summary>
        public abstract int SequenceID { get; }

        /// <summary>
        ///     Gets or sets the level of the logging event.
        /// </summary>
        public abstract LogLevel Level { get; set; }

        /// <summary>
        ///     Gets the stack frame of the method that did the logging.
        /// </summary>
        public abstract StackFrame UserStackFrame { get; }

        /// <summary>
        ///     Gets or sets the timestamp of the logging event.
        /// </summary>
        public abstract DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets the number index of the stack frame that represents the user
        ///     code (not the NLog code).
        /// </summary>
        public abstract int UserStackFrameNumber { get; protected set; }

        /// <summary>
        ///     Gets or sets the log message including any parameter placeholders.
        /// </summary>
        public abstract string Message { get; set; }

        /// <summary>
        ///     Gets the entire stack trace.
        /// </summary>
        public abstract StackTrace StackTrace { get; protected set; }

        /// <summary>
        ///     Sets the stack trace for the event info.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userStackFrame">Index of the first user stack frame within the stack trace.</param>
        public abstract void SetStackTrace(StackTrace stackTrace, int userStackFrame);

        #endregion

        private sealed class LogEntryImplementation : LogEntry
        {
            private readonly int _sequenceID;

            public LogEntryImplementation(LogLevel level, [Localizable(false)] string message, Exception exception)
            {
                TimeStamp = DateTime.Now;
                Level = level;
                Message = message;
                Exception = exception;
                _sequenceID = Interlocked.Increment(ref _globalSequenceId);
            }

            public override bool HasException
            {
                get { return Exception != null; }
            }

            public override int SequenceID
            {
                get { return _sequenceID; }
            }

            public override LogLevel Level { get; set; }

            public override DateTime TimeStamp { get; set; }

            public override StackFrame UserStackFrame
            {
                get { return (StackTrace != null) ? StackTrace.GetFrame(UserStackFrameNumber) : null; }
            }

            public override int UserStackFrameNumber { get; protected set; }

            public override string Message { get; set; }

            public override StackTrace StackTrace { get; protected set; }

            public override Exception Exception { get; set; }

            public override bool HasStackTrace
            {
                get { return StackTrace != null; }
            }

            /// <summary>
            ///     Returns a string representation of this log event.
            /// </summary>
            /// <returns>String representation of the log event.</returns>
            public override string ToString()
            {
                return String.Format("Log Event: Level={0} Message='{1}' SequenceID={2}", Level, Message,
                                     SequenceID);
            }

            public override void SetStackTrace(StackTrace stackTrace, int userStackFrame)
            {
                StackTrace = stackTrace;
                UserStackFrameNumber = userStackFrame;
            }
        }
    }
}