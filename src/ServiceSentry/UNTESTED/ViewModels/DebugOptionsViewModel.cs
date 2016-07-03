// -----------------------------------------------------------------------
//  <copyright file="DebugOptionsViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class DebugOptionsViewModel : PropertyChangedBase
    {
        public abstract string LogFileFolder { get; set; }
        public abstract ICommand GetLogPathCommand { get; }
        public abstract ObservableCollection<string> FormattingOptions { get; }
        public abstract ObservableCollection<string> SelectedFormattingOptions { get; set; }

        public abstract string LogFileNameFormat { get; set; }
        //public abstract ObservableCollection<string> LogLevels { get; }
        public abstract ObservableCollection<string> SelectedLogLevels { get; set; }
        public abstract ObservableCollection<LogLevel> SelectedLevels { get; set; }


        internal static DebugOptionsViewModel GetInstance(LogConfiguration config, Logger logger)
        {
            return new DOVMImplementation(config, logger);
        }

        internal abstract void RefreshSettings(LogConfiguration config);
        internal abstract LogConfiguration Commit(LogConfiguration config);
        internal abstract void GetLogFileFolder();
        internal abstract string GetFolder();

        private sealed class DOVMImplementation : DebugOptionsViewModel
        {
            //private readonly Dialogs _dialogs;
            private readonly Logger _logger;
            private ICommand _getLogPathCommand;
            private string _logFileFolder;
            private ObservableCollection<string> _selectedFormattingOptions;
            private ObservableCollection<string> _selectedLogLevels;
            private ObservableCollection<LogLevel> _selectedLevels;

            internal DOVMImplementation(LogConfiguration config, Logger logger)
            {
                _logger = logger;
                RefreshSettings(config);
            }


            public override ObservableCollection<string> FormattingOptions
            {
                get { return FileNameFormattingOption.AllOptionNames; }
            }

            public override ObservableCollection<string> SelectedFormattingOptions
            {
                get { return _selectedFormattingOptions; }
                set
                {
                    if (_selectedFormattingOptions == value) return;
                    _selectedFormattingOptions = value;
                    OnPropertyChanged();
                }
            }

            public override string LogFileNameFormat { get; set; }

            //public override ObservableCollection<string> LogLevels
            //{
            //    get { return LogLevel.AllLevels; }
            //}

            public override ObservableCollection<string> SelectedLogLevels
            {
                get { return _selectedLogLevels; }
                set
                {
                    if (_selectedLogLevels == value) return;
                    _selectedLogLevels = value;
                    OnPropertyChanged();
                }
            }

            public override ObservableCollection<LogLevel> SelectedLevels
            {
                get { return _selectedLevels; }
                set
                {
                    if (_selectedLevels == value) return;
                    _selectedLevels = value;
                    OnPropertyChanged();
                }
            }


            public override string LogFileFolder
            {
                get { return _logFileFolder; }
                set
                {
                    if (_logFileFolder == value) return;
                    var path = GetValidPath(value);
                    _logFileFolder = path;

                    OnPropertyChanged();
                }
            }

            public override ICommand GetLogPathCommand
            {
                get
                {
                    return _getLogPathCommand ??
                           (_getLogPathCommand =
                            new RelayCommand("GetLogPathCommand", param => GetLogFileFolder()));
                }
            }

            private string GetValidPath(string value)
            {
                try
                {
                    if (string.IsNullOrEmpty(value)) return string.Empty;

                    var path = Environment.ExpandEnvironmentVariables(value);
                    path = Path.GetFullPath(path);

                    if (string.IsNullOrEmpty(path)) return string.Empty;

                    // Disallow relative paths
                    if (!Path.IsPathRooted(path))
                    {
                        // TODO: Throw an error about relative paths.
                        return string.Empty;
                    }

                    // Make sure there's a trailing \
                    path = path.Trim(Path.DirectorySeparatorChar);
                    path += Path.DirectorySeparatorChar;
                    return path;
                }
                catch (Exception)
                {
                    //throw;
                    return string.Empty;
                }
            }

            internal override void GetLogFileFolder()
            {
                var folder = GetFolder();
                if (!string.IsNullOrEmpty(folder)) LogFileFolder = folder;
            }

            internal override string GetFolder()
            {
                var available = CommonFileDialog.IsPlatformSupported;

                if (available)
                {
                    var dialog = new CommonFolderPicker {Multiselect = false};
                    var result = dialog.ShowDialog();
                    return result != CommonFileDialogResult.Ok ? string.Empty : dialog.FileName;
                }
                else
                {
                    var dialog = new FolderBrowserDialog();
                    var result = dialog.ShowDialog();
                    return result != DialogResult.OK ? string.Empty : dialog.SelectedPath;
                }
            }

            internal override void RefreshSettings(LogConfiguration config)
            {
                LogFileNameFormat = config.FileNameFormat;
                LogFileFolder = config.LogFolder;
                SelectedFormattingOptions = new ObservableCollection<string>(config.FormattingOptionStrings);
                SelectedLevels = config.Levels.Collection;
            }

            internal override LogConfiguration Commit(LogConfiguration config)
            {
                // Log Configuration
                config.LogFolder = LogFileFolder;

                config.Levels.Collection = SelectedLevels;
                config.FormattingOptions = SelectedFormattingOptions.Select(FileNameFormattingOption.ByName).ToArray();
                config.FileNameFormat = LogFileNameFormat;
                return config;
            }
        }
    }
}