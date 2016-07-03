// -----------------------------------------------------------------------
//  <copyright file="FileSystemImplementation.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace ServiceSentry.Extensibility.Implementations
{
    internal sealed class FileSystemImplementation : FileSystem
    {
        private readonly FileSystemFactory _factory;
        private readonly PathFinder _pathFinder;
        private DirectoryInfoWrapper _directoryInfo;
        private FileInfoWrapper _fileInfo;

        public FileSystemImplementation(string path, FileInfoWrapper fileInfo,
                                        DirectoryInfoWrapper directoryInfo, FileSystemFactory factory)
        {
            _factory = factory;
            _pathFinder = _factory.GetPathFinder(this);
            SetFocus(path, fileInfo, directoryInfo);
        }

        public override DirectoryInfoWrapper DirectoryInfo { get { return _directoryInfo; } set { _directoryInfo = value; } }
        public override FileInfoWrapper FileInfoWrapper { get { return _fileInfo; } set { _fileInfo = value; } }

        public override string BasePath { get; set; }

        public override string ProductVersion
        {
            get { return _fileInfo.ProductVersion; }
        }

        public override void SetFocus(string newPath)
        {
            if (newPath == null) return;
            SetFocus(newPath, FileInfoWrapper.GetInstance(newPath), DirectoryInfoWrapper.GetInstance(newPath));
        }

        public override void SetFocus(string newPath, FileInfoWrapper fileInfo, DirectoryInfoWrapper directoryInfo)
        {
            if (string.IsNullOrEmpty(newPath)) return;

            BasePath = newPath;
            _fileInfo = fileInfo;
            _directoryInfo = directoryInfo;
        }

        public override string DirectoryName()
        {
            return _directoryInfo.Name;
        }

        public override bool FileExists()
        {
            return _fileInfo.Exists;
        }

        public override bool FileExists(string path)
        {
            return _factory.FileInfo(path).Exists;
        }

        public override long FileLength()
        {
            return _fileInfo.Length;
        }

        public override DateTime LastWriteTime()
        {
            return _fileInfo.LastWriteTime;
        }

        public override bool DirectoryExists()
        {
            return _directoryInfo.Exists;
        }

        public override bool DirectoryExists(string path)
        {
            return _factory.DirectoryInfo(path).Exists;
        }

        public override FileInfoWrapper FileInfo(string path)
        {
            return _factory.FileInfo(path);
        }


        public override DirectoryInfoWrapper[] GetDirectories(string searchPattern = "*")
        {
            return _directoryInfo.GetDirectories(searchPattern);
        }


        public override FileInfoWrapper[] GetFiles(string searchPattern = "*")
        {
            return _directoryInfo.GetFiles(searchPattern);
        }

        public override void DeleteFolder()
        {
            _directoryInfo.Delete();
        }

        public override void DeleteFolder(string folder)
        {
            _factory.DirectoryInfo(folder).Delete();
        }

        public override string OpenPath(string uriString, bool isDirectory = false)
        {
            if (String.IsNullOrEmpty(uriString)) throw new ArgumentNullException(uriString);

            var path = _pathFinder.GetPathFromURI(uriString);
            if (path == null) return Strings.Error_InvalidURI;

            // Not sure how to test:
            //if (path == string.Empty) return "Path is empty.";

            var exists = isDirectory ? _factory.DirectoryInfo(path).Exists : _factory.FileInfo(path).Exists;
            if (!exists) return isDirectory ? Strings.Error_FolderDNE : Strings.Error_FileDNE;

            Process.Start(path);

            return string.Empty;
        }

        public override string GetExistingFolderFromList(List<string> possibleFolders)
        {
            var output = GetExistingFoldersFromList(possibleFolders);
            return (output.Count == 0) ? string.Empty : output[0];
        }

        public override List<string> GetExistingFoldersFromList(List<string> possibleFolders)
        {
            return _pathFinder.GetExistingPathsFromList(possibleFolders, true);
        }

        public override string GetExistingFileFromList(List<string> possiblePaths)
        {
            var output = GetExistingFilesFromList(possiblePaths);
            return (output.Count == 0) ? string.Empty : output[0];
        }

        public override List<string> GetExistingFilesFromList(List<String> possiblePaths)
        {
            return _pathFinder.GetExistingPathsFromList(possiblePaths, false);
        }
    }

    internal abstract class PathFinder
    {
        internal static PathFinder GetInstance(FileSystem fileSystem)
        {
            return new PathFinderImplementation(fileSystem, PathFinderHelper.GetInstance(fileSystem));
        }

        internal abstract List<string> GetExistingPathsFromList(List<string> possiblePaths, bool isFolder);

        private sealed class PathFinderImplementation : PathFinder
        {
            private readonly FileSystem _fileSystem;
            private readonly PathFinderHelper _helper;

            internal PathFinderImplementation(FileSystem fileSystem, PathFinderHelper helper)
            {
                _fileSystem = fileSystem;
                _helper = helper;
            }

            internal override List<String> GetExistingPathsFromList(List<string> possiblePaths, bool isFolder)
            {
                if (possiblePaths == null) throw new ArgumentNullException("possiblePaths");
                if (possiblePaths.Count == 0)
                    throw new ArgumentException(Strings.Error_PartialPathsPassedWithNoItems);

                var output = new List<string>();
                if (!_fileSystem.DirectoryInfo.Exists) return output;

                var getter = _helper.GetExistingPaths(isFolder);

                foreach (var pathToFind in possiblePaths)
                {
                    output.AddRange(getter(pathToFind));
                }

                if (output.Count == 0) output.Add(string.Empty);
                return output;
            }

            internal override string GetPathFromURI(string uriString)
            {
                Uri result;
                var success = Uri.TryCreate(uriString, UriKind.Absolute, out result);
                return success ? result.LocalPath : null;
            }

        }

        internal abstract string GetPathFromURI(string uriString);
    }

    internal abstract class PathFinderHelper
    {
        internal static PathFinderHelper GetInstance(FileSystem fileSystem)
        {
            return new PFHImplementation(fileSystem);
        }

        internal abstract Func<string, IEnumerable<string>> GetExistingPaths(bool isFolder);

        private sealed class PFHImplementation : PathFinderHelper
        {
            private readonly FileSystem _fileSystem;

            internal PFHImplementation(FileSystem fileSystem)
            {
                _fileSystem = fileSystem;
            }

            internal override Func<string, IEnumerable<string>> GetExistingPaths(bool isFolder)
            {
                if (isFolder)
                    return
                        searchPattern =>
                        _fileSystem.GetDirectories(searchPattern)
                                   .Select(i => i.FullPath);

                return
                    searchPattern =>
                    _fileSystem.GetFiles(searchPattern).Select(i => i.FullPath);
            }
        }
    }
}