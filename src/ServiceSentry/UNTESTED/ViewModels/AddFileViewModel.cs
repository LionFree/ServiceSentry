// -----------------------------------------------------------------------
//  <copyright file="AddFileViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using ServiceSentry.Client.UNTESTED.Views.Dialogs;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class AddFileViewModel : PropertyChangedBase
    {
        public abstract string Filename { get; set; }
        public abstract ICommand GetFilenameCommand { get; }

        internal static AddFileViewModel GetInstance(Logger logger)
        {
            return GetInstance(FileVMHelper.GetInstance(logger), AddFileView.GetInstance());
        }
        
        internal static AddFileViewModel GetInstance(FileVMHelper helper, AddFileView view)
        {
            return new AddFileViewModelImplementation(helper, view);
        }

        
        private sealed class AddFileViewModelImplementation : AddFileViewModel
        {
            private string _filename;
            private ICommand _getFilenameCommand;
            private readonly FileVMHelper _helper;
            private readonly AddFileView _view;
            private ExternalFile _externalFile;

            internal AddFileViewModelImplementation(FileVMHelper helper, AddFileView view)
            {
                _helper = helper;
                _view = view;
                _view.DataContext = this;
                
            }

            public override string Filename
            {
                get { return _filename; }
                set
                {
                    if (_filename == value) return;
                    _filename = value;
                    OnPropertyChanged();
                }
            }

            public override ICommand GetFilenameCommand
            {
                get
                {
                    return
                        _getFilenameCommand ?? (_getFilenameCommand =
                                                new RelayCommand("GetFilename",
                                                                 param => _externalFile = _helper.AddFile()));
                }
            }

            public override ExternalFile GetFilename()
            {
                _view.ShowDialog();
                return _externalFile;
            }

        }

        public abstract ExternalFile GetFilename();
    }

    internal abstract class FileVMHelper
    {
        internal static FileVMHelper GetInstance(Logger logger)
        {
            return new FileVMHelperImplementation(logger);
        }

        private sealed class FileVMHelperImplementation : FileVMHelper
        {
            private readonly Logger _logger;

            internal FileVMHelperImplementation(Logger logger)
            {
                _logger = logger;
            }

            public override ExternalFile AddFile()
            {
                _logger.Trace("Adding log file.");

                // Open the file dialog
                var dialog = new OpenFileDialog
                {
                    Filter = Strings.Noun_LogFileFilter,
                    FilterIndex = 0,
                    Multiselect = false,
                    ShowReadOnly = false,
                    Title = Strings.Verb_AddLogFiles,
                };

                if (dialog.ShowDialog() != true) return null;

                var file = ExternalFile.GetInstance(
                    _logger,
                    dialog.FileName,
                    Path.GetFileNameWithoutExtension(dialog.FileName));

                return file;
            }
        }

        public abstract ExternalFile AddFile();
    }
}