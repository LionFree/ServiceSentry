// -----------------------------------------------------------------------
//  <copyright file="ExternalFileBehavior.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using System.Linq;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model.ExternalFiles
{
    public abstract class ExternalFileBehavior
    {
        public static ExternalFileBehavior GetInstance(Logger logger)
        {
            return new ExternalFileBehaviorImplementation(FileSystem.GetInstance(logger));
        }

        public static ExternalFileBehavior GetInstance(FileSystem behavior)
        {
            return new ExternalFileBehaviorImplementation(behavior);
        }

        #region Abstract Members

        public abstract string ParseFileName(string fileName);
        public abstract bool GoodFileName(string fileName);

        #endregion

        private sealed class ExternalFileBehaviorImplementation : ExternalFileBehavior
        {
            private readonly FileSystem _fileSystem;

            public ExternalFileBehaviorImplementation(FileSystem behavior)
            {
                _fileSystem = behavior;
            }

            public override string ParseFileName(string fileName)
            {
                if (string.IsNullOrEmpty(fileName)) return string.Empty;

                var result = Environment.ExpandEnvironmentVariables(fileName);

                if (!result.Contains("*")) return result;

                var searchPattern = Path.GetFileName(result);

                // Parse out the path.
                var path = Path.GetDirectoryName(result);
                if (string.IsNullOrEmpty(path)) path = Environment.CurrentDirectory;

                _fileSystem.SetFocus(path);
                // Identify the files in the path.
                var dirExists = _fileSystem.DirectoryExists();
                if (!dirExists)
                    return string.Empty;

                // Get the file Info for all files that match the pattern.
                var files = _fileSystem.GetFiles(searchPattern);

                if (files.Length == 0) return string.Empty;

                // Identify the most recently written file.
                var lastFile = (from file in files
                                orderby file.LastWriteTime descending
                                select file).First();

                return lastFile.FullPath;
            }

            public override bool GoodFileName(string fileName)
            {
                // Check for invalid wildcards.
                // If we use an invalid wildcard in the filename,
                // it will show up as a blank line in the logs 
                // list and context menu.
                return ParseFileName(fileName) != string.Empty;
            }
        }
    }
}