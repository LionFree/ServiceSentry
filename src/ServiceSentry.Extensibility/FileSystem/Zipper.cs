// -----------------------------------------------------------------------
//  <copyright file="Zipper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility
{
    public abstract class Zipper
    {
        public static Zipper GetInstance(Logger logger)
        {
            return new ZipperImplementation(logger);
        }

        public abstract bool ZipFiles(string path, IEnumerable<FileInfoWrapper> filesToCompress);


        private sealed class ZipperImplementation : Zipper
        {
            private readonly Logger _logger;

            internal ZipperImplementation(Logger logger)
            {
                _logger = logger;
            }


            public override bool ZipFiles(string path, IEnumerable<FileInfoWrapper> filesToCompress)
            {
                if (path == null) throw new ArgumentNullException("path");
                if (filesToCompress == null) throw new ArgumentNullException("filesToCompress");
                ZipFile zip = null;
                try
                {
                    using (zip = new ZipFile(path))
                    {
                        zip.AddFiles(filesToCompress.Where(i => i.Exists).Select(i => i.FullPath), "");
                        zip.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                    return false;
                }
                finally
                {
                    if (zip != null) zip.Dispose();
                }
            }
        }
    }
}