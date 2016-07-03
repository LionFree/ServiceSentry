// -----------------------------------------------------------------------
//  <copyright file="FileCommands.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels.Commands
{
    public abstract class FileCommands
    {
        internal static FileCommands GetInstance(Logger logger)
        {
            return GetInstance(FileSystem.GetInstance(logger), Dialogs.GetInstance());
        }

        internal static FileCommands GetInstance(FileSystem fileSystem, Dialogs dialogs)
        {
            return new FileCommandsImplementation(fileSystem, dialogs);
        }

        public abstract void OpenFolderPath(object originalSource);
        public abstract void OpenFilePath(object originalSource);
        public abstract void OpenFileLink(object originalSource);

        private sealed class FileCommandsImplementation : FileCommands
        {
            private readonly Dialogs _dialogs;
            private readonly FileSystem _fileSystem;

            internal FileCommandsImplementation(FileSystem fileSystem, Dialogs dialogs)
            {
                _fileSystem = fileSystem;
                _dialogs = dialogs;
            }

            public override void OpenFolderPath(object originalSource)
            {
                // Get the link information.
                var link = (Hyperlink) originalSource;
                var uriString = link.NavigateUri.AbsoluteUri;

                // Attempt to open the path.
                _fileSystem.OpenPath(uriString, true);
            }

            public override void OpenFilePath(object originalSource)
            {
                // Get the link information.
                var menuItem = (MenuItem) originalSource;
                var uriString = menuItem.Tag.ToString();

                // Attempt to open the path.
                _fileSystem.OpenPath(uriString, true);
            }

            public override void OpenFileLink(object originalSource)
            {
                // Get the link information.
                var link = (Hyperlink) originalSource;
                var uriString = link.NavigateUri.AbsoluteUri;

                // Attempt to open the path.
                var result = _fileSystem.OpenPath(uriString);
                if (!string.IsNullOrEmpty(result))
                    _dialogs.ShowError(result, Application.Current.MainWindow);
            }
        }
    }
}