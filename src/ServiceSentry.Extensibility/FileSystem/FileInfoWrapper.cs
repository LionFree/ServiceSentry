// -----------------------------------------------------------------------
//  <copyright file="FileInfoWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.IO;

#endregion

namespace ServiceSentry.Extensibility
{
    /// <summary>
    ///     Provides instance methods for the creation, copying, deletion,
    ///     moving, and opening of files, and aids in the creation of
    ///     <see cref="T:System.IO.FileStream" /> objects.
    /// </summary>
    public abstract class FileInfoWrapper
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:ServiceSentry.Extensibility.FileInfoWrapper" />
        ///     class, which acts as a wrapper for a file path.
        /// </summary>
        /// <param name="path">
        ///     The fully-qualified name of the new file, or the relative file name.
        /// </param>
        public static FileInfoWrapper GetInstance(string path)
        {
            return new FileInfoWrapperImplementation(path);
        }

        /// <summary>
        ///     Returns a string that represents the current <see cref="FileInfoWrapper" />.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #region Abstract Members

        /// <summary>
        ///     Creates an instance of the parent directory.
        /// </summary>
        public abstract DirectoryInfoWrapper Directory { get; }

        /// <summary>
        ///     Gets a value indicating whether the file exists.
        /// </summary>
        /// <returns>
        ///     true if the file exists; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract bool Exists { get; }

        /// <summary>
        ///     Gets the name of the file.
        /// </summary>
        /// <returns>
        ///     A string that is the name of a file, including the file name extension.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets or sets the time when the current file or directory was last written to.
        /// </summary>
        /// <returns>
        ///     The time the current file was last written.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">
        ///     <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.
        /// </exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Microsoft Windows NT or later.</exception>
        /// <filterpriority>1</filterpriority>
        public abstract DateTime LastWriteTime { get; set; }

        /// <summary>
        ///     Gets the full path of the directory or file.
        /// </summary>
        /// <returns>
        ///     A string containing the full path.
        /// </returns>
        /// <exception cref="T:System.IO.PathTooLongException">The fully qualified path and file name is 260 or more characters.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <filterpriority>1</filterpriority>
        public abstract string FullPath { get; }

        /// <summary>
        ///     Gets the length of a file, in bytes.
        /// </summary>
        public abstract long Length { get; }

        /// <summary>
        ///     Gets the version of the product this file is distributed with.
        /// </summary>
        /// <returns>
        ///     The version of the product this file is distributed with or null if the file did not contain version information.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract string ProductVersion { get; }

        /// <summary>
        ///     Deletes a file.  The file specified by the designated path is deleted.
        ///     If the file does not exist, Delete succeeds without throwing
        ///     an exception.
        ///     On NT, Delete will fail for a file that is open for normal I/O
        ///     or a file that is memory mapped.  On Win95, the file will be
        ///     deleted regardless of whether the file is being used.
        ///     Your application must have Delete permission to the target file.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        ///     Opens a file in the specified mode with read, write,
        ///     or read/write access and the specified sharing options.
        /// </summary>
        /// <param name="mode">
        ///     A <see cref="FileMode" /> constant specifying the
        ///     mode (for example, Open or Append) in which to open the file.
        /// </param>
        /// <param name="access">
        ///     A <see cref="FileAccess" /> constant specifying whether to
        ///     open the file with Read, Write, or ReadWrite access.
        /// </param>
        /// <param name="share">
        ///     A <see cref="FileShare" /> constant specifying the type of
        ///     access other FileStream objects have to this object.
        /// </param>
        public abstract FileStream Open(FileMode mode, FileAccess access, FileShare share);

        /// <summary>
        /// Refreshed the state of the object.
        /// </summary>
        public abstract void Refresh();

        #endregion

        private sealed class FileInfoWrapperImplementation : FileInfoWrapper
        {
            private readonly FileInfo _fileInfo;
            private readonly string _path;

            internal FileInfoWrapperImplementation(string path)
            {
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

                _path = path;
                _fileInfo = new FileInfo(_path);
            }

            public override string Name
            {
                get { return _fileInfo.Name; }
            }

            public override bool Exists
            {
                get { return _fileInfo.Exists; }
            }

            public override DirectoryInfoWrapper Directory
            {
                get { return DirectoryInfoWrapper.GetInstance(Path.GetDirectoryName(FullPath)); }
            }

            public override long Length
            {
                get { return _fileInfo.Length; }
            }

            public override DateTime LastWriteTime
            {
                get { return _fileInfo.LastWriteTimeUtc.ToLocalTime(); }
                set { _fileInfo.LastWriteTimeUtc = value.ToUniversalTime(); }
            }

            public override string FullPath
            {
                get { return _fileInfo.FullName; }
            }

            public override string ProductVersion
            {
                get
                {
                    var fvi = FileVersionInfo.GetVersionInfo(_path);
                    return fvi.ProductVersion;
                }
            }

            public override void Delete()
            {
                _fileInfo.Delete();
            }

            public override FileStream Open(FileMode mode, FileAccess access, FileShare share)
            {
                return _fileInfo.Open(mode, access, share);
            }

            public override void Refresh()
            {
                _fileInfo.Refresh();
            }
        }

    }
}