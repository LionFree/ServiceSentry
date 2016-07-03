using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;

namespace ServiceSentry.Client.Infrastructure
{
    internal abstract class ExceptionHandler
    {
        [STAThread]
        internal static ExceptionHandler GetInstance()
        {
            return GetInstance(BindingErrorListener.GetInstance(), Dialogs.GetInstance());
        }

        [STAThread]
        internal static ExceptionHandler GetInstance(BindingErrorListener listener, Dialogs dialogs)
        {
            return new ExceptionHandlerImplementation(listener, dialogs);
        }

        #region Abstract Members

        /// <summary>
        ///     Attach the custom exception handler to the current
        ///     <see cref="Application" /> and <see cref="AppDomain" />.
        /// </summary>
        internal abstract void AttachExceptionHandler(Logger logger);

        /// <summary>
        ///     Attach the custom exception handler to the specified
        ///     <see cref="Application" /> and <see cref="AppDomain" />.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="application">
        ///     The <see cref="Application" /> to attach the custom
        ///     exception handler to.
        /// </param>
        /// <param name="appDomain">
        ///     The <see cref="Application" /> to attach the custom
        ///     exception handler to.
        /// </param>
        internal abstract void AttachExceptionHandler(Logger logger, Application application, AppDomain appDomain);

        /// <summary>
        ///     Tries to write a given exception to the log.
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        internal abstract void LogException(object exception);

        /// <summary>
        ///     Displays an error dialog containing an error message which is
        ///     formatted with the submitted exception's <see cref="Exception.Message" />.
        /// </summary>
        /// <param name="exception">Exception to be displayed.</param>
        /// <param name="window"></param>
        internal abstract void ReportException(Exception exception, Window window);

        /// <summary>
        ///     Forces data binding errors to be shown as exceptions in a dialog box.
        /// </summary>
        internal abstract void PromoteDataBindingErrors();

        /// <summary>
        ///     Gets any unhandled exceptions of the application.
        /// </summary>
        protected abstract void OnUnhandledException(object o,
                                                     DispatcherUnhandledExceptionEventArgs unhandledExceptionEventArgs);

        /// <summary>
        ///     Displays an error dialog containing an error message which is
        ///     formatted with the submitted exception's <see cref="Exception.Message" />,
        ///     and logs the exception to the application log.
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="window">The window that will own the error dialog.</param>
        internal abstract void LogAndReportException(Exception exception, Window window);

        #endregion

        private sealed class ExceptionHandlerImplementation : ExceptionHandler
        {
            private readonly Dialogs _dialogs;
            private readonly BindingErrorListener _listener;
            private Logger _logger;

            internal ExceptionHandlerImplementation(BindingErrorListener listener,
                                                    Dialogs dialogs)
            {
                _listener = listener;
                _dialogs = dialogs;
            }

            [DebuggerStepThrough]
            internal override void AttachExceptionHandler(Logger logger)
            {
                AttachExceptionHandler(logger, Application.Current, AppDomain.CurrentDomain);
            }

            [DebuggerStepThrough]
            internal override void AttachExceptionHandler(Logger logger, Application application, AppDomain appDomain)
            {
                // Make sure we're properly handling exceptions
                _logger = logger;

                application.DispatcherUnhandledException += OnUnhandledException;
                appDomain.UnhandledException += (s, args) => LogException(args.ExceptionObject);
            }

            [DebuggerStepThrough]
            protected override void OnUnhandledException(object sender,
                                                         DispatcherUnhandledExceptionEventArgs e)
            {
                if (Debugger.IsAttached) return;

                if (!e.Dispatcher.CheckAccess())
                {
                    //we are infact running on a dispatcher thread, but better safe than sorry
                    e.Dispatcher.Invoke(new Action(() =>
                                                   OnUnhandledException(sender, e)),
                                        DispatcherPriority.Normal,
                                        null);
                    return;
                }

                LogAndReportException(e.Exception, Application.Current.MainWindow);

                //mark exception as handled in order to keep the app alive
                e.Handled = true;
            }

            [DebuggerStepThrough]
            internal override void LogException(object exception)
            {
                var ex = exception as Exception;
                if (ex == null) throw new ArgumentException(Strings.Error_ErrorToLogMustBeAnException);

                _logger.ErrorException(ex, "{0}: {1}{2}", ex.GetType(), ex.Message,
                                       (ex.InnerException == null
                                            ? string.Empty
                                            : ex.InnerException.Message));
            }

            [DebuggerStepThrough]
            internal override void ReportException(Exception exception,
                                                   Window window)
            {
                //show error
                _dialogs.ShowError(
                    String.Format("EXCEPTION TargetSite {0}: {1}{2}{3}", exception.TargetSite,
                                  exception.Message,
                                  Environment.NewLine,
                                  exception.StackTrace),
                    window);
            }

            [DebuggerStepThrough]
            internal override void LogAndReportException(Exception exception,
                                                         Window window)
            {
                LogException(exception);

                //show error
                ReportException(exception, window);
            }

            [DebuggerStepThrough]
            internal override void PromoteDataBindingErrors()
            {
                _listener.Listen(_dialogs.ShowError);
            }
        }
    }

    internal abstract class BindingErrorListener : TraceListener
    {
        internal abstract Action<string> LogAction { get; }

        [DebuggerStepThrough]
        internal static BindingErrorListener GetInstance()
        {
            return GetInstance(null);
        }

        [DebuggerStepThrough]
        internal static BindingErrorListener GetInstance(Action<string> logAction)
        {
            return new Listener(logAction);
        }

        /// <summary>
        ///     Adds a trace listener to the trace source listener collection.
        /// </summary>
        /// <param name="logAction">The action to take when a trace event appears.</param>
        public abstract void Listen(Action<string> logAction);

        private sealed class Listener : BindingErrorListener
        {
            private readonly Action<string> _logAction;

            internal Listener(Action<string> logAction)
            {
                _logAction = logAction;
            }

            internal override Action<string> LogAction
            {
                get { return _logAction; }
            }

            [DebuggerStepThrough]
            public override void Listen(Action<string> logAction)
            {
                PresentationTraceSources.DataBindingSource.Listeners
                                        .Add(GetInstance(logAction));
            }

            [DebuggerStepThrough]
            public override void Write(string message)
            {
            }

            [DebuggerStepThrough]
            public override void WriteLine(string message)
            {
                LogAction(message);
            }
        }
    }
}