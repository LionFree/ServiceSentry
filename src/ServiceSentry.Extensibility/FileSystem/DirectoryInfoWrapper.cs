// -----------------------------------------------------------------------
//  <copyright file="DirectoryInfoWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using System.IO;

#endregion

namespace ServiceSentry.Extensibility
{
    /// <summary>
    ///     Exposes instance methods for creating, moving, and
    ///     enumerating through directories and subdirectories.
    /// </summary>
    public abstract class DirectoryInfoWrapper : EquatableBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:ServiceSentry.Extensibility.DirectoryInfoWrapper" />
        ///     class on the specified path.
        /// </summary>
        /// <param name="path">
        ///     A string specifying the path on which to create the
        ///     <see cref="T:ServiceSentry.Extensibility.DirectoryInfoWrapper" />.
        /// </param>
        public static DirectoryInfoWrapper GetInstance(string path)
        {
            return new DirectoryInfoImplementation(path);
        }

        #region Abstract Members

        /// <summary>
        ///     Gets the parent directory of a specified subdirectory.
        /// </summary>
        /// <exception cref="System.Security.SecurityException" />
        public abstract DirectoryInfoWrapper Parent { get; }

        /// <summary>
        ///     Gets the root portion of a path.
        /// </summary>
        /// <exception cref="System.Security.SecurityException" />
        public abstract DirectoryInfoWrapper Root { get; }

        /// <summary>
        ///     Gets a value indicating whether the directory exists.
        /// </summary>
        /// <returns>
        ///     true if the file or directory exists; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract bool Exists { get; }

        /// <summary>
        ///     Gets the name of the last directory in the hierarchy if a hierarchy exists. Otherwise, the Name property gets the name of the directory.
        /// </summary>
        /// <returns>
        ///     A string that is the name of the parent directory, or the name of the last directory in the hierarchy.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets the full path of the directory.
        /// </summary>
        /// <returns>
        ///     A string containing the full path.
        /// </returns>
        /// <exception cref="T:System.IO.PathTooLongException">The fully qualified path and file name is 260 or more characters.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <filterpriority>1</filterpriority>
        public abstract string FullPath { get; }

        /// <summary>
        ///     Returns an array of directories in the current
        ///     <see
        ///         cref="T:ServiceSentry.Extensibility.DirectoryInfoWrapper" />
        ///     matching the given search criteria and using a value to determine whether to search subdirectories.
        /// </summary>
        /// <returns>
        ///     An array of type DirectoryInfoWrapper matching <paramref name="searchPattern" />.
        /// </returns>
        /// <param name="searchPattern">
        ///     The search string, such as "System*", used to search for all directories beginning with the word "System".
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="searchPattern " />contains invalid characters. To determine the invalid characters, use the
        ///     <see
        ///         cref="M:System.IO.Path.GetInvalidPathChars" />
        ///     method.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="searchPattern" /> is null.
        /// </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the DirectoryInfoWrapper object is invalid, such as being on an unmapped drive. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
        public abstract DirectoryInfoWrapper[] GetDirectories(string searchPattern = "*");

        /// <summary>
        ///     Returns an array of Files in the current DirectoryInfoWrapper matching the
        ///     given search criteria (ie, "*.txt").
        /// </summary>
        /// <param name="searchPattern">
        ///     The search string to match against the names of files in the current directory.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="searchPattern " />contains invalid characters. To determine the invalid
        ///     characters, use the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="searchPattern" /> is null.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters. </exception>
        /// <exception cref="T:System.IO.IOException">A network error has occurred. </exception>
        /// <returns>
        ///     An array of type <see cref="FileInfoWrapper" /> of files in the specified directory.
        /// </returns>
        public abstract FileInfoWrapper[] GetFiles(string searchPattern = "*");

        public abstract void Delete(bool recursive = false);

        #endregion

        #region Special Methods

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (DirectoryInfoWrapper) obj;
            var same1 = (Name == p.Name);
            var same2 = (FullPath == p.FullPath);
            var same3 = (Exists == p.Exists);

            var same = same1 && same2 && same3;
            return same;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 7;
                var hash = 11;

                // be sure to check for nullity, etc.
                hash *= seed + (Name != null ? Name.GetHashCode() : 0);
                hash *= seed + (FullPath != null ? FullPath.GetHashCode() : 0);
                hash *= seed + Exists.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion

        private sealed class DirectoryInfoImplementation : DirectoryInfoWrapper
        {
            private readonly DirectoryInfo _directoryInfo;
            
            internal DirectoryInfoImplementation(string path)
            {
                //Contract.Requires(!(string.IsNullOrEmpty(path)));
                if (path == null)
                    throw new ArgumentNullException("path");

                _directoryInfo = new DirectoryInfo(path);
            }

            /// <summary>
            ///     Gets a value indicating whether the directory exists.
            /// </summary>
            /// <returns>
            ///     true if the file or directory exists; otherwise, false.
            /// </returns>
            /// <filterpriority>1</filterpriority>
            public override bool Exists
            {
                get { return _directoryInfo.Exists; }
            }

            /// <summary>
            ///     Gets the full path of the directory.
            /// </summary>
            /// <returns>
            ///     A string containing the full path.
            /// </returns>
            /// <exception cref="T:System.IO.PathTooLongException">The fully qualified path and file name is 260 or more characters.</exception>
            /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
            /// <filterpriority>1</filterpriority>
            public override string FullPath
            {
                get { return _directoryInfo.FullName; }
            }

            /// <summary>
            ///     Gets the name of the last directory in the hierarchy if a hierarchy exists. Otherwise, the Name property gets the name of the directory.
            /// </summary>
            /// <returns>
            ///     A string that is the name of the parent directory, or the name of the last directory in the hierarchy.
            /// </returns>
            /// <filterpriority>1</filterpriority>
            public override string Name
            {
                get { return _directoryInfo.Name; }
            }

            /// <summary>
            ///     Gets the parent directory of a specified subdirectory.
            /// </summary>
            /// <exception cref="System.Security.SecurityException" />
            public override DirectoryInfoWrapper Parent
            {
                get { return _directoryInfo.Parent == null ? null : GetInstance(_directoryInfo.Parent.FullName); }
            }

            /// <summary>
            ///     Gets the root portion of a path.
            /// </summary>
            /// <exception cref="System.Security.SecurityException" />
            public override DirectoryInfoWrapper Root
            {
                get { return GetInstance(_directoryInfo.Root.FullName); }
            }


            /// <summary>
            ///     Returns an array of directories in the current
            ///     <see
            ///         cref="T:ServiceSentry.Extensibility.DirectoryInfoWrapper" />
            ///     matching the given search criteria and using a value to determine whether to search subdirectories.
            /// </summary>
            /// <returns>
            ///     An array of type DirectoryInfoWrapper matching <paramref name="searchPattern" />.
            /// </returns>
            /// <param name="searchPattern">
            ///     The search string, such as "System*", used to search for all directories beginning with the word "System".
            /// </param>
            /// <exception cref="T:System.ArgumentException">
            ///     <paramref name="searchPattern " />contains invalid characters. To determine the invalid characters, use the
            ///     <see
            ///         cref="M:System.IO.Path.GetInvalidPathChars" />
            ///     method.
            /// </exception>
            /// <exception cref="T:System.ArgumentNullException">
            ///     <paramref name="searchPattern" /> is null.
            /// </exception>
            /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the DirectoryInfoWrapper object is invalid, such as being on an unmapped drive. </exception>
            /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
            public override DirectoryInfoWrapper[] GetDirectories(string searchPattern = "*")
            {
                var array = _directoryInfo.GetDirectories(searchPattern);
                var output = new DirectoryInfoWrapper[array.Length];

                for (var i = 0; i < array.Length; i++)
                {
                    output[i] = GetInstance(array[i].FullName);
                }

                return output;
            }

            /// <summary>
            ///     Returns an array of Files in the current DirectoryInfoWrapper matching the
            ///     given search criteria (ie, "*.txt").
            /// </summary>
            /// <param name="searchPattern">
            ///     The search string to match against the names of files in the current directory.
            /// </param>
            /// <exception cref="T:System.ArgumentException">
            ///     <paramref name="searchPattern " />contains invalid characters. To determine the invalid
            ///     characters, use the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.
            /// </exception>
            /// <exception cref="T:System.ArgumentNullException">
            ///     <paramref name="searchPattern" /> is null.
            /// </exception>
            /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
            /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
            /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters. </exception>
            /// <exception cref="T:System.IO.IOException">A network error has occurred. </exception>
            /// <returns>
            ///     An array of type <see cref="FileInfoWrapper" /> of files in the specified directory.
            /// </returns>
            public override FileInfoWrapper[] GetFiles(string searchPattern = "*")
            {
                var array = _directoryInfo.GetFiles(searchPattern);
                var output = new FileInfoWrapper[array.Length];

                for (var i = 0; i < array.Length; i++)
                {
                    output[i] = FileInfoWrapper.GetInstance(array[i].FullName);
                }

                return output;
            }


            public override void Delete(bool recursive = false)
            {
                _directoryInfo.Delete(recursive);
            }
        }
    }
}