// -----------------------------------------------------------------------
//  <copyright file="Log.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using ServiceSentry.Extensibility.Logging.Exceptions;

#endregion

namespace ServiceSentry.Extensibility.Logging
{
    public abstract class Log : PropertyChangedBase
    {
        public abstract ThreadSafeObservableCollection<LogEntry> LogItems { get; }
        public abstract ThreadSafeObservableCollection<string> LogEntries { get; }
        public abstract LogConfiguration Configuration { get; set; }

        public static Log Default
        {
            get { return Create(LogConfiguration.Default); }
        }

        public static Log Null
        {
            get { return Create(LogConfiguration.Null); }
        }

        public static Log Create(LogConfiguration config)
        {
            return new LogImplementation(config, ExceptionWriter.Default);
        }

        internal abstract void AddEntry(LogEntry entry);
        internal abstract void AddEntry(LogLevel level, Exception exception, string message, params object[] args);
        internal abstract bool IsEnabled(LogLevel logLevel2);

        private sealed class LogImplementation : Log
        {
            private readonly ThreadSafeObservableCollection<string> _logEntries;
            private readonly ThreadSafeObservableCollection<LogEntry> _logItems;
            private readonly ExceptionWriter _writer;
            private LogConfiguration _configuration;
            private bool _fileError;

            internal LogImplementation(LogConfiguration config, ExceptionWriter writer)
            {
                if (config==null) throw new ArgumentNullException("config");
                _logItems = new ThreadSafeObservableCollection<LogEntry>();
                _logEntries = new ThreadSafeObservableCollection<string>();
                _configuration = config;
                _writer = writer;
            }

            public override ThreadSafeObservableCollection<LogEntry> LogItems
            {
                get { return _logItems; }
            }

            public override ThreadSafeObservableCollection<string> LogEntries
            {
                get { return _logEntries; }
            }

            public override LogConfiguration Configuration
            {
                get { return _configuration; }
                set
                {
                    if (_configuration == value) return;
                    _configuration = value;
                    OnPropertyChanged();
                }
            }

            internal override void AddEntry(LogEntry entry)
            {
                LogItems.Add(entry);
                LogEntries.Add(entry.ToString());

                if (_fileError || Configuration.FullPath == string.Empty) return;

                try
                {
                    using (var writer = File.AppendText(Configuration.FullPath))
                    {
                        writer.WriteLine("{0:yyyy'-'MM'-'dd HH':'mm':'ss} [{1}]\t{2}",
                                         entry.TimeStamp,
                                         entry.Level.ToString().ToUpper(),
                                         entry.Message);

                        if (entry.HasException)
                        {
                            _writer.WriteException(entry.Exception, writer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _fileError = true;
                    AddEntry(LogLevel.Error, ex, ex.Message);
                }
            }

            internal override void AddEntry(LogLevel level, Exception exception, string message,
                                            params object[] args)
            {
                if (Configuration.Levels == null) Configuration.Levels = EnabledLevels.All;
                if (!Configuration.Levels.IsEnabled(level)) return;
                AddEntry(LogEntry.Create(level, String.Format(message, args), exception));
            }

            internal override bool IsEnabled(LogLevel level)
            {
                return Configuration.IsEnabled(level);
            }
        }
    }
}