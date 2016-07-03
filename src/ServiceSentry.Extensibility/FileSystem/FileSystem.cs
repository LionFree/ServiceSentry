// -----------------------------------------------------------------------
//  <copyright file="FileSystem.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using ServiceSentry.Extensibility.Implementations;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility
{
    public abstract class FileSystem
    {
        public static FileSystem GetUnloggedInstance()
        {
            return GetUnloggedInstance(Environment.CurrentDirectory);
        }
        public static FileSystem GetUnloggedInstance(string path)
        {
            return GetUnloggedInstance(path, FileInfoWrapper.GetInstance(path),
                                  DirectoryInfoWrapper.GetInstance(path), FileSystemFactory.GetFactory());
        }
        internal static FileSystem GetUnloggedInstance(string path, FileInfoWrapper fileInfo, DirectoryInfoWrapper directoryInfo,
                                              FileSystemFactory factory)
        {
            return new FileSystemImplementation(path, fileInfo, directoryInfo, factory);
        }

        public static FileSystem GetInstance(Logger logger)
        {
            return GetInstance(logger, Environment.CurrentDirectory);
        }
        public static FileSystem GetInstance(Logger logger, string path)
        {
            return GetInstance(logger, GetUnloggedInstance(path,
                                                         FileInfoWrapper.GetInstance(path),
                                                         DirectoryInfoWrapper.GetInstance(path),
                                                         FileSystemFactory.GetFactory()));
        }
        internal static FileSystem GetInstance(Logger logger, FileSystem fileSystem)
        {
            return new LoggedFileSystemImplementation(logger, fileSystem);
        }

        #region Abstract Members
        
        public abstract DirectoryInfoWrapper DirectoryInfo { get; set; }
        public abstract FileInfoWrapper FileInfoWrapper { get; set; }
        public abstract string BasePath { get; set; }
        public abstract string ProductVersion { get; }
        public abstract void SetFocus(string newPath);

        public abstract void SetFocus(string newPath, FileInfoWrapper fileInfo,
                                      DirectoryInfoWrapper directoryInfo);

        public abstract string DirectoryName();
        public abstract bool FileExists();
        public abstract bool FileExists(string path);
        public abstract long FileLength();
        public abstract DateTime LastWriteTime();
        public abstract bool DirectoryExists();
        public abstract bool DirectoryExists(string path);

        public abstract FileInfoWrapper FileInfo(string path);

        public abstract FileInfoWrapper[] GetFiles(string searchPattern = "*");
        public abstract DirectoryInfoWrapper[] GetDirectories(string searchPattern = "*");
        public abstract void DeleteFolder();
        public abstract void DeleteFolder(string folder);
        public abstract string OpenPath(string uriString, bool isDirectory = false);

        public abstract List<string> GetExistingFilesFromList(List<string> possiblePaths);
        public abstract List<string> GetExistingFoldersFromList(List<string> possibleFolders);
        public abstract string GetExistingFileFromList(List<string> possiblePaths);
        public abstract string GetExistingFolderFromList(List<string> possibleFolders);
        
        #endregion
    }

    internal abstract class FileSystemFactory
    {
        internal static FileSystemFactory GetFactory()
        {
            return new FSFactoryImplementation();
        }

        internal abstract DirectoryInfoWrapper DirectoryInfo(string path);
        internal abstract FileInfoWrapper FileInfo(string path);
        internal abstract PathFinder GetPathFinder(FileSystem fileSystem);

        private sealed class FSFactoryImplementation : FileSystemFactory
        {
            internal override DirectoryInfoWrapper DirectoryInfo(string path)
            {
                return DirectoryInfoWrapper.GetInstance(path);
            }

            internal override FileInfoWrapper FileInfo(string path)
            {
                return FileInfoWrapper.GetInstance(path);
            }

            internal override PathFinder GetPathFinder(FileSystem fileSystem)
            {
                return PathFinder.GetInstance(fileSystem);
            }
        }
    }
}