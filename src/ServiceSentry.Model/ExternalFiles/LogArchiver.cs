// -----------------------------------------------------------------------
//  <copyright file="LogArchiver.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model.ExternalFiles
{
    public abstract class LogArchiver
    {
        public static LogArchiver GetInstance(Logger logger)
        {
            return GetInstance(Zipper.GetInstance(logger), logger);
        }

        internal static LogArchiver GetInstance(Zipper zipper, Logger logger)
        {
            return new LogArchiverImplementation(zipper, logger);
        }

        public abstract void ArchiveAndClearLogs(List<FileInfoWrapper> logFiles, LoggingDetails logDetails);


        private sealed class LogArchiverImplementation : LogArchiver
        {
            private readonly Logger _logger;
            private readonly Zipper _zipper;

            public LogArchiverImplementation(Zipper zipper, Logger logger)
            {
                _logger = logger;
                _zipper = zipper;
            }

            private string CurrentTimeStamp
            {
                get { return DateTime.Now.ToString(Strings.Archive_DateTime); }
            }

            private bool CreateArchive(IEnumerable<FileInfoWrapper> filesList, string archiveFilePath)
            {
                //Contract.Requires(filesList != null);

                // De-duplicate any possible files.
                var filesToCompress = filesList.Distinct().ToList();

                _logger.Info(Strings.Info_CreatingArchive, archiveFilePath);
                var success = _zipper.ZipFiles(archiveFilePath, filesToCompress);
                return success;
            }

            private void ClearLogs(List<FileInfoWrapper> filesToDelete)
            {
                //Contract.Requires(filesToDelete != null);

                // If there's nothing to clear, 
                // e.g., if we've already cleared the logs
                // and they haven't been reconstructed, 
                // then get out.
                if (CountExistingFiles(filesToDelete) == 0) return;

                // Clear the log files.
                _logger.Info(Strings.Info_ClearingLogs);
                try
                {
                    foreach (var file in filesToDelete)
                    {
                        file.Delete();
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
            }

            private int CountExistingFiles(IEnumerable<FileInfoWrapper> filesToCount)
            {
                //Contract.Requires(filesToCount != null);

                // Count the number of files in the list
                // that actually exist.
                var count = 0;
                foreach (var fileInfo in filesToCount)
                {
                    if (fileInfo.Exists)
                    {
                        count++;
                    }
                }
                return count;
            }

            public override void ArchiveAndClearLogs(List<FileInfoWrapper> logFiles, LoggingDetails logDetails)
            {
                var success = true;
                // Get the filename of the Archive.
                var path = Path.ChangeExtension(logDetails.DisplayName.Replace("%TS%", CurrentTimeStamp), ".zip");

                // Archive logs if necessary.
                if (logDetails.ArchiveLogs)
                {
                    success = CreateArchive(logFiles, path);
                }

                // Delete the files if necessary.
                if (logDetails.ClearLogs && success)
                {
                    ClearLogs(logFiles);
                }
            }
        }
    }
}