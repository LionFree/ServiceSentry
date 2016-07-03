// -----------------------------------------------------------------------
//  <copyright file="Logger.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Extensibility.Logging
{
    public abstract class Logger : PropertyChangedBase
    {
        /// <summary>
        ///     Gets a new instance of the <see cref="T:ServiceSentry.Extensibility.Logging.Logger" />
        ///     class, using the default Log.
        /// </summary>
        public static Logger GetInstance()
        {
            var logger = CreateLog(Log.Default);
            return logger;
        }

        /// <summary>
        ///     Gets a new instance of the null <see cref="T:ServiceSentry.Extensibility.Logging.Logger" />,
        ///     for use in places where a logger is required by convention, but not by code.
        /// </summary>
        public static Logger Null
        {
            get { return new LoggerImplementation(Log.Null); }
        }

        /// <summary>
        ///     Gets a new instance of the <see cref="T:ServiceSentry.Extensibility.Logging.Logger" />
        ///     class, using the given <see cref="Log" /> and <see cref="LogLevel" />s.
        /// </summary>
        public static Logger CreateLog(Log config)
        {
            return new LoggerImplementation(config);
        }

        /// <summary>
        ///     Sets the logger configuration.
        /// </summary>
        /// <param name="logConfiguration">
        ///     The <see cref="T:ServiceSentry.Extensibility.Logging.LogConfiguration" /> to use.
        /// </param>
        public abstract void Configure(LogConfiguration logConfiguration);

        #region LogLevels

        #endregion

        #region Abstract Members

        /// <summary>
        ///     Gets the associated <see cref="T:ServiceSentry.Extensibility.Logging.Log" />.
        /// </summary>
        public abstract Log Log { get; }

        #region Enabled Levels

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
        /// </summary>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsTraceEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsDebugEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsInfoEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsWarnEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsErrorEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsFatalEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>
        ///     A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns
        ///     <see
        ///         langword="false" />
        ///     .
        /// </returns>
        public abstract bool IsEnabled(LogLevel level);

        #endregion

        #region Write Methods

        /// <summary>
        ///     Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="args">Arguments to format.</param>
        public abstract void Trace(string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Trace</c> level
        ///     using the exception message as the diagnostic message.
        /// </summary>
        /// <param name="exception">An exception to be logged.</param>
        public abstract void TraceException(Exception exception);

        /// <summary>
        ///     Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public abstract void TraceException(Exception exception, string message, params object[] args);


        /// <summary>
        ///     Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="args">Arguments to format.</param>
        public abstract void Debug(string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Debug</c> level
        ///     using the exception message as the diagnostic message.
        /// </summary>
        /// <param name="exception">An exception to be logged.</param>
        public abstract void DebugException(Exception exception);

        /// <summary>
        ///     Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public abstract void DebugException(Exception exception, string message, params object[] args);

        /// <summary>
        ///     Writes the exception at the <c>Error</c> level
        ///     using the exception message as the diagnostic message.
        /// </summary>
        /// <param name="exception">An exception to be logged.</param>
        public abstract void ErrorException(Exception exception);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="args">Arguments to format.</param>
        public abstract void Error(string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public abstract void ErrorException(Exception exception, string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="args">Arguments to format.</param>
        public abstract void Fatal(string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Fatal</c> level
        ///     using the exception message as the diagnostic message.
        /// </summary>
        /// <param name="exception">An exception to be logged.</param>
        public abstract void FatalException(Exception exception);

        /// <summary>
        ///     Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public abstract void FatalException(Exception exception, string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="args">Arguments to format.</param>
        public abstract void Info(string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Info</c> level
        ///     using the exception message as the diagnostic message.
        /// </summary>
        /// <param name="exception">An exception to be logged.</param>
        public abstract void InfoException(Exception exception);

        /// <summary>
        ///     Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public abstract void InfoException(Exception exception, string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="args">Arguments to format.</param>
        public abstract void Warn(string message, params object[] args);

        /// <summary>
        ///     Writes the diagnostic message at the <c>Warn</c> level
        ///     using the exception message as the diagnostic message.
        /// </summary>
        /// <param name="exception">An exception to be logged.</param>
        public abstract void WarnException(Exception exception);

        /// <summary>
        ///     Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">
        ///     A <see langword="string" /> containing format items.
        /// </param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public abstract void WarnException(Exception exception, string message, params object[] args);

        #endregion

        #endregion

        private sealed class LoggerImplementation : Logger
        {
            private readonly Log _log;

            internal LoggerImplementation(Log log)
            {
                _log = log;
            }

            public override Log Log
            {
                get { return _log; }
            }

            public override bool IsTraceEnabled
            {
                get { return _log.IsEnabled(LogLevel.Trace); }
            }

            public override bool IsDebugEnabled
            {
                get { return _log.IsEnabled(LogLevel.Debug); }
            }

            public override bool IsInfoEnabled
            {
                get { return _log.IsEnabled(LogLevel.Info); }
            }

            public override bool IsWarnEnabled
            {
                get { return _log.IsEnabled(LogLevel.Warn); }
            }

            public override bool IsErrorEnabled
            {
                get { return _log.IsEnabled(LogLevel.Error); }
            }

            public override bool IsFatalEnabled
            {
                get { return _log.IsEnabled(LogLevel.Fatal); }
            }

            public override void Configure(LogConfiguration logConfiguration)
            {
                Log.Configuration = logConfiguration;
                Info(Strings.Debug_LoggerConfigured);
            }

            public override void Trace(string message, params object[] args)
            {
                TraceException(null, message, args);
            }

            public override void TraceException(Exception exception, string message, params object[] args)
            {
                System.Diagnostics.Trace.WriteLine(String.Format(message, args));
                _log.AddEntry(LogLevel.Trace, exception, message, args);
            }

            public override void TraceException(Exception exception)
            {
                TraceException(exception, exception.Message);
            }


            public override void Debug(string message, params object[] args)
            {
                DebugException(null, message, args);
            }

            public override void DebugException(Exception exception, string message, params object[] args)
            {
                _log.AddEntry(LogLevel.Debug, exception, message, args);
            }

            public override void DebugException(Exception exception)
            {
                DebugException(exception, exception.Message);
            }

            public override void Info(string message, params object[] args)
            {
                InfoException(null, message, args);
            }

            public override void InfoException(Exception exception, string message, params object[] args)
            {
                _log.AddEntry(LogLevel.Info, exception, message, args);
            }

            public override void InfoException(Exception exception)
            {
                InfoException(exception, exception.Message);
            }

            public override void Error(string message, params object[] args)
            {
                ErrorException(null, message, args);
            }

            public override void ErrorException(Exception exception, string message, params object[] args)
            {
                _log.AddEntry(LogLevel.Error, exception, message, args);
            }

            public override void ErrorException(Exception exception)
            {
                ErrorException(exception, exception.Message);
            }

            public override void Warn(string message, params object[] args)
            {
                WarnException(null, message, args);
            }

            public override void WarnException(Exception exception, string message, params object[] args)
            {
                _log.AddEntry(LogLevel.Warn, exception, message, args);
            }

            public override void WarnException(Exception exception)
            {
                WarnException(exception, exception.Message);
            }

            public override void Fatal(string message, params object[] args)
            {
                _log.AddEntry(LogLevel.Fatal, null, message, args);
            }

            public override void FatalException(Exception exception, string message, params object[] args)
            {
                _log.AddEntry(LogLevel.Fatal, exception, message, args);
            }

            public override void FatalException(Exception exception)
            {
                FatalException(exception, exception.Message);
            }

            public override bool IsEnabled(LogLevel level)
            {
                return _log.IsEnabled(level);
            }
        }

        
    }
}