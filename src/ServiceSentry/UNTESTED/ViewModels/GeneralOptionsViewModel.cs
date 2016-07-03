// -----------------------------------------------------------------------
//  <copyright file="GeneralOptionsViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using ServiceSentry.Client.Properties;
using ServiceSentry.Client.UNTESTED.Infrastructure;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class GeneralOptionsViewModel : PropertyChangedBase
    {
        internal static GeneralOptionsViewModel GetInstance(ConfigFile file, Logger logger)
        {
            return new GOVMImplementation(file, ConfigFileHandler.GetInstance(logger), Dialogs.GetInstance(), logger);
        }

        #region Abstract Members

        public abstract string ViewTitle { get; }
        public abstract string LogArchiveFolder { get; set; }
        public abstract bool IsAutoStart { get; set; }
        public abstract bool CloseToTray { get; set; }
        public abstract string ApplicationName { get; }
        public abstract ICommand OpenFolderChooserCommand { get; }

        internal abstract void RefreshSettings(ConfigFile file);
        internal abstract void Commit(ConfigFile file);
        internal abstract void GetLogArchiveFolder();
        internal abstract string GetFolder();

        #endregion

        private sealed class GOVMImplementation : GeneralOptionsViewModel
        {
            private readonly ConfigFileHandler _configFileHandler;
            private readonly Dialogs _dialogs;
            private readonly Logger _logger;
            private bool _closeToTray;
            private bool _isAutoStart;
            private string _logArchiveFolder;
            private ICommand _openFolderChooserCommand;

            internal GOVMImplementation(ConfigFile file,
                                        ConfigFileHandler configFileHandler, Dialogs dialogs, Logger logger)
            {
                _logger = logger;
                _configFileHandler = configFileHandler;
                _dialogs = dialogs;
                RefreshSettings(file);
            }

            public override string ApplicationName
            {
                get { return Strings._ApplicationName; }
            }

            public override string ViewTitle
            {
                get { return Strings._ApplicationName; }
            }

            public override string LogArchiveFolder
            {
                get { return _logArchiveFolder; }
                set
                {
                    if (_logArchiveFolder == value) return;
                    var path = GetValidPath(value);
                    _logArchiveFolder = path;

                    OnPropertyChanged();
                }
            }


            public override bool IsAutoStart
            {
                get { return _isAutoStart; }
                set
                {
                    if (_isAutoStart == value) return;
                    _isAutoStart = value;
                    OnPropertyChanged("IsAutoStart");
                }
            }

            public override bool CloseToTray
            {
                get { return _closeToTray; }
                set
                {
                    if (_closeToTray == value) return;
                    _closeToTray = value;
                    OnPropertyChanged("CloseToTray");
                }
            }


            internal override void GetLogArchiveFolder()
            {
                var folder = GetFolder();
                if (!string.IsNullOrEmpty(folder)) LogArchiveFolder = folder;
            }

            internal override string GetFolder()
            {
                if (CommonFileDialog.IsPlatformSupported)
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

            internal override void Commit(ConfigFile file)
            {
                try
                {
                    AutoStart.GetInstance().SetAutoStart(IsAutoStart);
                }
                catch (Exception exception)
                {
                    _logger.ErrorException(exception);
                    return;
                }
                Settings.Default.CloseToSystemTray = CloseToTray;
                Settings.Default.Save();

                // Log Archival
                var fileName = Path.GetFileName(file.LogDetails.ArchivePath);
                if (!string.IsNullOrEmpty(LogArchiveFolder))
                {
                    if (!Directory.Exists(LogArchiveFolder))
                    {
                        // the path doesn't exist - ask to create it
                        var message = String.Format(Strings.Error_DirectoryDNE_1, LogArchiveFolder, Environment.NewLine, Strings.Error_DirectoryDNE_2);
                        var result = _dialogs.ShowYesNo(message, MessageBoxImage.Warning);
                        if (result != MessageBoxResult.Yes) return;

                        Directory.CreateDirectory(LogArchiveFolder);
                    }
                }
                var fullPath = LogArchiveFolder + fileName;
                file.LogDetails.ArchivePath = fullPath;

                _configFileHandler.WriteConfigFile(file);
            }

            internal override void RefreshSettings(ConfigFile file)
            {
                LogArchiveFolder = file.LogDetails.Directory;
                CloseToTray = Settings.Default.CloseToSystemTray;
                IsAutoStart = AutoStart.GetInstance().IsAutoStartEnabled;
            }

            #region Commands

            public override ICommand OpenFolderChooserCommand
            {
                get
                {
                    return _openFolderChooserCommand ??
                           (_openFolderChooserCommand =
                            new RelayCommand("OpenFolderChooser", param => GetLogArchiveFolder()));
                }
            }

            #endregion
        }
    }
}