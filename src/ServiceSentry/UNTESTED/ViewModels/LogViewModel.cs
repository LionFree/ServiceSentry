// -----------------------------------------------------------------------
//  <copyright file="LogViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ServiceSentry.Client.UNTESTED.Views.Dialogs;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class LogViewModel : PropertyChangedBase
    {
        internal static LogViewModel GetInstance(Logger logger, LogLevel initialMinimumLevel)
        {
            return GetInstance(LogView.GetInstance(), logger,
                               LogFilter.GetInstance(initialMinimumLevel),
                               initialMinimumLevel);
        }

        internal static LogViewModel GetInstance(LogView view, Logger logger, LogFilter filter,
                                                 LogLevel initialMinimumLevel)
        {
            return new LogVMImplementation(view, logger, filter, initialMinimumLevel);
        }

        #region Abstract Members

        public abstract ObservableCollection<LogEntry> Log { get; }
        public abstract ICommand CloseCommand { get; }
        public abstract ICommand ClearLogCommand { get; }
        public abstract ICommand CopyToClipboardCommand { get; }
        public abstract int SelectedIconVisibility { get; set; }
        public abstract ICommand UpdateFilterCommand { get; }
        public abstract LogLevel InitialMinimumVisibility { get; set; }
        internal abstract void Show();
        internal abstract void Hide();
        internal abstract void CopyLogToClipboard();

        #endregion

        private sealed class LogVMImplementation : LogViewModel
        {
            private readonly LogFilter _filter;
            private readonly Logger _logger;
            private readonly LogView _view;
            private ICommand _clearLogCommand;
            private ICommand _closeCommand;
            private ICommand _copyToClipboardCommand;
            private LogLevel _initialMinimumLevel;
            private int _selectedIconVisibility;
            private ICommand _updateFilterCommand;

            internal LogVMImplementation(LogView view, Logger logger,
                                         LogFilter filter, LogLevel initialMinimumLevel)
            {
                _selectedIconVisibility = 1;
                _logger = logger;

                _view = view;
                _view.DataContext = this;

                _filter = filter;
                _initialMinimumLevel = initialMinimumLevel;
                _filter.Update(_view, this);
            }

            public override ObservableCollection<LogEntry> Log
            {
                get { return _logger.Log.LogItems; }
            }

            public override int SelectedIconVisibility
            {
                get { return _selectedIconVisibility; }
                set
                {
                    if (_selectedIconVisibility == value) return;
                    _selectedIconVisibility = value;
                    OnPropertyChanged();
                }
            }

            public override LogLevel InitialMinimumVisibility
            {
                get { return _initialMinimumLevel; }
                set
                {
                    if (_initialMinimumLevel == value) return;
                    _initialMinimumLevel = value;
                    OnPropertyChanged();
                }
            }

            #region Commands

            public override ICommand CloseCommand
            {
                get
                {
                    return _closeCommand ??
                           (_closeCommand =
                            new RelayCommand("CloseLogView", m => Hide()));
                }
            }

            public override ICommand CopyToClipboardCommand
            {
                get
                {
                    return _copyToClipboardCommand ??
                           (_copyToClipboardCommand =
                            new RelayCommand("CopyToClipboard", m => CopyLogToClipboard()));
                }
            }

            public override ICommand ClearLogCommand
            {
                get
                {
                    return _clearLogCommand ??
                           (_clearLogCommand =
                            new RelayCommand("ClearLog", m => Log.Clear()));
                }
            }

            public override ICommand UpdateFilterCommand
            {
                get
                {
                    return _updateFilterCommand ??
                           (_updateFilterCommand =
                            new RelayCommand("UpdateFilter", param =>
                                                             _filter.Update(_view, this, param)));
                }
            }

            #endregion

            internal override void CopyLogToClipboard()
            {
                var logText = String.Join(Environment.NewLine, _logger.Log.LogEntries.ToArray());
                Clipboard.SetData(DataFormats.UnicodeText, logText);
            }


            internal override void Show()
            {
                _view.Show();
            }

            internal override void Hide()
            {
                _view.Hide();
            }
        }
    }

    internal abstract class LogFilter
    {
        internal static LogFilter GetInstance(LogLevel minimumLevel)
        {
            return new FilterImplementation(minimumLevel);
        }

        internal abstract void Update(LogView view, LogViewModel vm, object logLevel);

        internal abstract void ToggleLevelVisibility(string level);

        internal abstract void Update(LogView view, LogViewModel vm);

        private sealed class FilterImplementation : LogFilter
        {
            private readonly ObservableCollection<string> _visibleLevels;

            internal FilterImplementation(LogLevel minimumLevel)
            {
                _visibleLevels = new ObservableCollection<string>();

                if (minimumLevel <= LogLevel.Trace)
                    _visibleLevels.Add(Extensibility.Strings.LogLevel_Trace);

                if (minimumLevel <= LogLevel.Debug)
                    _visibleLevels.Add(Extensibility.Strings.LogLevel_Debug);

                if (minimumLevel <= LogLevel.Info)
                    _visibleLevels.Add(Extensibility.Strings.LogLevel_Info);

                if (minimumLevel <= LogLevel.Warn)
                    _visibleLevels.Add(Extensibility.Strings.LogLevel_Warn);

                if (minimumLevel <= LogLevel.Error)
                    _visibleLevels.Add(Extensibility.Strings.LogLevel_Error);
            }


            internal override void Update(LogView view, LogViewModel vm, object logLevel)
            {
                var level = logLevel as string;
                if (string.IsNullOrEmpty(level)) throw new ArgumentException("ToggleButton.Tag was null.");
                ToggleLevelVisibility(level);

                Update(view, vm);
            }

            internal override void Update(LogView view, LogViewModel vm)
            {
                var dataView = CollectionViewSource.GetDefaultView(vm.Log) as CollectionView;
                if (dataView == null) return;
                dataView.Filter = (obj =>
                    {
                        var entry = obj as LogEntry;
                        return _visibleLevels.Contains(entry.Level.ToString());
                    });
            }

            internal override void ToggleLevelVisibility(string level)
            {
                if (_visibleLevels.Contains(level))
                {
                    _visibleLevels.Remove(level);
                }
                else
                {
                    _visibleLevels.Add(level);
                }
            }
        }
    }
}