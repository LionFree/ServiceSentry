// -----------------------------------------------------------------------
//  <copyright file="LoggedFileSystemImplementation.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Implementations
{
    internal sealed class LoggedFileSystemImplementation : FileSystem
    {
        private readonly FileSystem _fileSystem;
        private readonly Logger _logger;

        internal LoggedFileSystemImplementation(Logger logger, FileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public override FileInfoWrapper FileInfoWrapper
        {
            get { return _fileSystem.FileInfoWrapper; }
            set { _fileSystem.FileInfoWrapper = value; }
        }

        public override DirectoryInfoWrapper DirectoryInfo
        {
            get { return _fileSystem.DirectoryInfo; }
            set { _fileSystem.DirectoryInfo = value; }
        }

        public override string BasePath
        {
            get { return _fileSystem.BasePath; }
            set { _fileSystem.BasePath = value; }
        }

        public override string ProductVersion
        {
            get { return _fileSystem.ProductVersion; }
        }

        public override void SetFocus(string newPath)
        {
            _fileSystem.SetFocus(newPath);
        }

        public override void SetFocus(string newPath, FileInfoWrapper fileInfo, DirectoryInfoWrapper directoryInfo)
        {
            _fileSystem.SetFocus(newPath, fileInfo, directoryInfo);
        }

        public override string DirectoryName()
        {
            return _fileSystem.DirectoryName();
        }

        public override bool FileExists()
        {
            return _fileSystem.FileExists();
        }

        public override bool FileExists(string path)
        {
            return _fileSystem.FileExists(path);
        }

        public override long FileLength()
        {
            return _fileSystem.FileLength();
        }

        public override DateTime LastWriteTime()
        {
            return _fileSystem.LastWriteTime();
        }

        public override bool DirectoryExists()
        {
            return _fileSystem.DirectoryExists();
        }

        public override bool DirectoryExists(string path)
        {
            return _fileSystem.DirectoryExists(path);
        }

        public override FileInfoWrapper FileInfo(string path)
        {
            return _fileSystem.FileInfo(path);
        }

        public override FileInfoWrapper[] GetFiles(string searchPattern = "*")
        {
            return _fileSystem.GetFiles(searchPattern);
        }

        public override DirectoryInfoWrapper[] GetDirectories(string searchPattern = "*")
        {
            return _fileSystem.GetDirectories(searchPattern);
        }

        public override void DeleteFolder()
        {
            _fileSystem.DeleteFolder();
        }

        public override void DeleteFolder(string folder)
        {
            _fileSystem.DeleteFolder(folder);
        }

        public override string OpenPath(string uriString, bool isDirectory = false)
        {
            try
            {
                return _fileSystem.OpenPath(uriString, isDirectory);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex);
            }
            return string.Empty;
        }

        public override List<string> GetExistingFilesFromList(List<string> possiblePaths)
        {
            return _fileSystem.GetExistingFilesFromList(possiblePaths);
        }

        public override List<string> GetExistingFoldersFromList(List<string> possibleFolders)
        {
            return _fileSystem.GetExistingFoldersFromList(possibleFolders);
        }

        public override string GetExistingFileFromList(List<string> possiblePaths)
        {
            return _fileSystem.GetExistingFileFromList(possiblePaths);
        }

        public override string GetExistingFolderFromList(List<string> possibleFolders)
        {
            return _fileSystem.GetExistingFolderFromList(possibleFolders);
        }
    }
}