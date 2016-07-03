// -----------------------------------------------------------------------
//  <copyright file="ExternalFile.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Windows.Threading;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.ExternalFiles;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ExternalFileImplementation))]
    public abstract class ExternalFile : EquatableBase
    {
        public static int DefaultLogCheckingDelay = 1000; // milliseconds


        public static ExternalFile GetInstance(Logger logger, string fullPath, string commonName)
        {
            var fileSystem = FileSystem.GetInstance(logger);
            return GetInstance(logger, fileSystem, ExternalFileBehavior.GetInstance(fileSystem), fullPath, commonName,
                               DefaultLogCheckingDelay);
        }

        internal static ExternalFile GetInstance(Logger logger, FileSystem fileSystem, ExternalFileBehavior behavior,
                                                 string fullPath, string commonName, int logDelay)
        {
            return GetInstance(fileSystem, behavior, ExternalFileHelper.GetInstance(), fullPath, commonName, logDelay);
        }

        internal static ExternalFile GetInstance(FileSystem fileSystem,
                                                 ExternalFileBehavior behavior,
                                                 ExternalFileHelper helper,
                                                 string fullPath, string commonName, int logDelay)
        {
            return new ExternalFileImplementation(fileSystem, behavior, helper, logDelay, fullPath, commonName);
        }


        public abstract void CheckForDuplicateNames(IList<ExternalFile> files);

        #region Abstract Members

        [DataMember]
        public abstract string CommonName { get; set; }

        [DataMember]
        public abstract string FullPath { get; set; }

        public abstract string FileName { get; set; }

        public abstract string Directory { get; set; }

        public abstract string DisplayName { get; set; }

        public abstract string ParsedName { get; }

        public abstract string ShortParsedName { get; }

        public abstract string FileSize { get; }

        public abstract string LastModified { get; }

        public abstract bool Exists { get; }

        public abstract bool ShowParentDirectory { get; set; }
        
        public event EventHandler<FileChangedEventArgs> FileChanged;
        public abstract void OnFileChanged(FileChangedEventArgs e);

        public abstract void StartObservingFile(int updateDelay = 0);

        /// <summary>
        ///     Attach a logger to this <see cref="ExternalFile" /> after deserializing.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="Logger" /> to attach.
        /// </param>
        public abstract void AttachLogger(Logger logger);

        #endregion

        #region Special Methods

        /// <summary>
        ///     Converts the value of this <see cref="ExternalFile" /> to its equivalent string representation.
        /// </summary>
        public override string ToString()
        {
            return FullPath;
        }

        /// <summary>
        ///     Determines whether the specifies <see cref="object" /> is equal to the current <see cref="ExternalFile" />.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="object" /> to compare with the current <see cref="ExternalFile" />.
        /// </param>
        public override bool Equals(object obj)
        {
            var item = obj as ExternalFile;
            if (item == null)
            {
                return false;
            }

            var same = (FullPath == item.FullPath);
            return same;
        }

        /// <summary>
        ///     Returns the hashcode for this <see cref="ExternalFile" />.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 3;
                var hash = 29;

                // be sure to check for nullity, etc.
                hash *= seed + (!string.IsNullOrEmpty(FullPath) ? FullPath.GetHashCode() : 0);
                hash *= seed + (!string.IsNullOrEmpty(CommonName) ? CommonName.GetHashCode() : 0);
                hash *= seed + ShowParentDirectory.GetHashCode();

                return hash;
            }
        }

        #endregion

        [DataContract]
        private sealed class ExternalFileImplementation : ExternalFile
        {
            private ExternalFileHelper _helper;
            private ExternalFileBehavior _behavior;
            private string _commonName;
            private string _displayName;
            private string _fileName;
            private FileSystem _fileSystem;
            private string _folder;
            private string _fullPath;
            private string _lastDate;
            private TimeSpan _logUpdateDelay;
            private Logger _logger;
            private bool _showParentDirectory;


            internal ExternalFileImplementation(FileSystem fileSystem, ExternalFileBehavior behavior,
                                                ExternalFileHelper helper, int updateDelay,
                                                string fullPath, string commonName)
            {
                _fileSystem = fileSystem;
                _behavior = behavior;
                _helper = helper;

                _logUpdateDelay = TimeSpan.FromMilliseconds(updateDelay);
                StartObservingFile(updateDelay);

                FullPath = fullPath;
                CommonName = commonName;

                // Do this after file is deserialized
                _helper.SetDirectory(this);
            }

            #region Properties

            public override string CommonName
            {
                get { return _commonName; }
                set
                {
                    if (_commonName == value) return;
                    _commonName = value;
                    OnPropertyChanged();
                }
            }

            public override string FullPath
            {
                get { return _fullPath; }
                set
                {
                    if (_fullPath == value) return;
                    _fullPath = value;
                    OnPropertyChanged();
                }
            }

            public override string FileName
            {
                get { return _fileName; }
                set
                {
                    if (_fileName == value) return;
                    _fileName = value;
                    OnPropertyChanged();
                }
            }

            public override string Directory
            {
                get { return _folder; }
                set
                {
                    if (_folder == value) return;
                    _folder = value;
                    OnPropertyChanged();
                }
            }

            public override string DisplayName
            {
                get
                {
                    if (string.IsNullOrEmpty(_displayName))
                    {
                        {
                            if (_showParentDirectory)
                            {
                                if (Directory == null) return ShortParsedName;

                                // Get the name of the name of the file's parent folder.
                                var parent = Path.GetFileName(Directory);

                                return parent + "\\" + ShortParsedName;
                            }
                            _displayName = ShortParsedName;
                        }
                    }
                    return _displayName;
                }
                set
                {
                    if (_displayName == value) return;
                    _displayName = value;
                    OnPropertyChanged();
                }
            }

            public override string ParsedName
            {
                get
                {
                    if (_behavior == null)
                    {
                        if (_logger == null)
                        {
                            MessageBox.Show(String.Format("Logger is null on ExternalFile '{0}'", FileName));
                        }
                        else
                        {
                            _logger.Error("ExternalFileBehavior is null on ExternalFile '{0}'", FileName);
                        }

                        return FullPath;
                    }

                    return _behavior.ParseFileName(FullPath);
                }
            }

            public override string ShortParsedName
            {
                get { return Path.GetFileName(ParsedName); }
            }

            public override string FileSize
            {
                get
                {
                    _fileSystem.SetFocus(ParsedName);
                    if (_fileSystem.FileExists())
                    {
                        var length = _fileSystem.FileLength();
                        return _helper.GetSizeString(length);
                    }

                    // The file doesn't exist, so return "-".
                    return "-";
                }
            }

            public override string LastModified
            {
                get
                {
                    var oldDate = _lastDate;
                    _fileSystem.SetFocus(ParsedName);

                    if (_fileSystem.FileExists())
                    {
                        // Update the last modified date.
                        var date = _fileSystem.LastWriteTime().ToString(CultureInfo.CurrentCulture);
                        _lastDate = date;

                        if (oldDate != null && _lastDate != oldDate)
                            OnFileChanged(new FileChangedEventArgs
                                {
                                    LogFileName = ShortParsedName,
                                    LogFilePath = FullPath
                                });

                        return date;
                    }

                    // The file doesn't exist, so return "-".
                    return "-";
                }
            }

            public override bool Exists
            {
                get
                {
                    _fileSystem.SetFocus(ParsedName);
                    return ParsedName != string.Empty && _fileSystem.FileExists();
                }
            }

            public override bool ShowParentDirectory
            {
                get { return _showParentDirectory; }
                set
                {
                    if (_showParentDirectory == value) return;
                    _showParentDirectory = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                }
            }

            #endregion

            #region Events

            public override void OnFileChanged(FileChangedEventArgs e)
            {
                var handler = FileChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            [OnDeserializedAttribute]
            private void OnDeserialized(StreamingContext context)
            {
                _helper = ExternalFileHelper.GetInstance();

                // Do this after file is deserialized
                _helper.SetDirectory(this);
            }
            #endregion

            #region Methods

            public override void StartObservingFile(int updateDelay = 0)
            {
                // Set a timer to update the log file information
                // every (so many) seconds.  This will automatically
                // update the UI without having to refresh the whole
                // datagrid.
                if (updateDelay != 0) _logUpdateDelay = TimeSpan.FromMilliseconds(updateDelay);

                var dispatcherTimer = new DispatcherTimer();

                dispatcherTimer.Tick += (o, e) =>
                    {
                        OnPropertyChanged("FileSize");
                        OnPropertyChanged("LastModified");
                        OnPropertyChanged("ParsedName");
                        OnPropertyChanged("ShortParsedName");
                    };

                dispatcherTimer.Interval = _logUpdateDelay;
                dispatcherTimer.Start();
            }
            public override void AttachLogger(Logger logger)
            {
                if (logger == null) throw new ArgumentNullException("logger");

                _logger = logger;

                if (_fileSystem == null) _fileSystem = FileSystem.GetInstance(_logger);
                if (_behavior == null) _behavior = ExternalFileBehavior.GetInstance(_fileSystem);

                StartObservingFile(DefaultLogCheckingDelay);
            }
            public override void CheckForDuplicateNames(IList<ExternalFile> files)
            {
                if (files == null) throw new ArgumentNullException("files");

                var header = ShortParsedName;

                if (ShowParentDirectory) return;

                // Make sure the header is unique, e.g., when 
                // there are more than one web.config files.
                for (var i = 0; i < files.Count; i++)
                {
                    var testName = files[i].ShortParsedName;
                    var currentIndex = files.IndexOf(this);

                    if (header == testName && currentIndex != i)
                    {
                        ShowParentDirectory = true;
                    }
                }
            }

            #endregion
        }
    }
    
    internal abstract class ExternalFileHelper
    {
        internal static ExternalFileHelper GetInstance()
        {
            return new ExternalFileHelperImp();
        }

        internal abstract string GetSizeString(long fileSize);
        internal abstract void SetDirectory(ExternalFile file);

        private sealed class ExternalFileHelperImp : ExternalFileHelper
        {
            internal override void SetDirectory(ExternalFile file)
            {
                if (string.IsNullOrEmpty(file.FullPath)) return;
                var output = Path.GetDirectoryName(file.FullPath);
                file.Directory = output;

                if (!string.IsNullOrEmpty(output))
                {
                    file.FileName = file.FullPath.Substring(output.Length);
                }

                file.OnPropertyChanged("DisplayName");
            }


            internal override string GetSizeString(long fileSize)
            {
                string[] sizes = {"B", "KB", "MB", "GB"};

                double length = fileSize;
                var order = 0;

                while (length > 1024 && order + 1 < sizes.Length)
                {
                    order++;
                    length = length/1024;
                }

                return string.Format("{0:0.##} {1}", length, sizes[order]);
            }
        }
    }
}