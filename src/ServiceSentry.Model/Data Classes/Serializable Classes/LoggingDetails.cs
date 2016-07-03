// -----------------------------------------------------------------------
//  <copyright file="LoggingDetails.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.IO;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ImplementedLoggingDetails))]
    public abstract class LoggingDetails : EquatableBase
    {
        public static LoggingDetails Default
        {
            get
            {
                return new ImplementedLoggingDetails
                    {
                        ArchiveLogs = false,
                        ClearLogs = false,
                        ArchivePath = Environment.ExpandEnvironmentVariables("%SYSTEMDRIVE%\\Services_%TS%.zip")
                    };
            }
        }

        public static LoggingDetails GetInstance()
        {
            return Default;
        }

        #region Abstract Members

        [DataMember(IsRequired = true)]
        public abstract string ArchivePath { get; set; }

        [DataMember(IsRequired = true)]
        public abstract bool ArchiveLogs { get; set; }

        [DataMember(IsRequired = true)]
        public abstract bool ClearLogs { get; set; }

        public abstract bool IgnoreLogs { get; set; }

        public abstract string Directory { get; set; }

        public abstract string FileName { get; set; }

        public abstract string DisplayName { get; set; }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (LoggingDetails) obj;

            var samePath = ArchivePath == p.ArchivePath;
            var sameArchive = ArchiveLogs == p.ArchiveLogs;
            var sameClear = ClearLogs == p.ClearLogs;
            var same = (samePath && sameArchive && sameClear);

            return same;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 5;
                var hash = 19;

                // be sure to check for nullity, etc.
                hash *= seed + (ArchivePath != null ? ArchivePath.GetHashCode() : 0);
                hash *= seed + ArchiveLogs.GetHashCode();
                hash *= seed + ClearLogs.GetHashCode();
                hash *= seed + (DisplayName != null ? DisplayName.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        [DataContract]
        private sealed class ImplementedLoggingDetails : LoggingDetails
        {
            private bool _archive;
            private string _archivePath;
            private bool _clear;
            private string _directory = string.Empty;
            private string _displayName;
            private string _fileName;
            private bool _ignore;

            #region Properties

            public override string ArchivePath
            {
                get { return _archivePath; }
                set
                {
                    if (_archivePath == value) return;
                    _archivePath = value;
                    OnPropertyChanged();

                    if (string.IsNullOrEmpty(_archivePath))
                    {
                        DisplayName = string.Empty;
                        FileName = string.Empty;
                        Directory = string.Empty;
                        return;
                    }

                    // Update the folder.
                    Directory = Path.GetDirectoryName(_archivePath);

                    // Update the filename or regex.
                    if (string.IsNullOrEmpty(Directory))
                    {
                        Directory = string.Empty;
                    }

                    FileName = _archivePath.Substring(Directory.Length);
                    var separator = Directory.EndsWith("\\") ? "" : "\\";

                    // Update the display name.
                    DisplayName = Directory + separator + FileName;
                }
            }

            public override bool ArchiveLogs
            {
                get { return _archive; }
                set
                {
                    if (_archive == value) return;
                    _archive = value;

                    if (_archive)
                    {
                        if(_ignore)
                        {IgnoreLogs = false;}
                    }

                    if (!ClearLogs && !ArchiveLogs)
                    {
                        if(!_ignore)
                        {IgnoreLogs = true;}
                    }

                    OnPropertyChanged();
                }
            }

            public override bool ClearLogs
            {
                get { return _clear; }
                set
                {
                    if (_clear == value) return;
                    _clear = value;

                    if (_clear)
                    {
                        if(_ignore)
                        {IgnoreLogs = false;}
                    }

                    if (!ClearLogs && !ArchiveLogs)
                    {
                        if (!_ignore)
                        {IgnoreLogs = true;}
                    }

                    OnPropertyChanged();
                }
            }

            public override bool IgnoreLogs
            {
                get { return _ignore; }
                set
                {
                    if (_ignore == value) return;
                    _ignore = value;

                    if (_ignore)
                    {
                        ArchiveLogs = false;
                        ClearLogs = false;
                        OnPropertyChanged("ArchiveLogs");
                        OnPropertyChanged("ClearLogs");
                    }
                    else
                    {
                        // Don't let the user set "ignore" to false
                        // (in the UI) if no other options are set.
                        if (!_archive && !_clear)
                        {
                            if (!_ignore)
                            {
                                _ignore = true;
                                return;
                            }
                        }
                    }
                    OnPropertyChanged();
                }
            }

            public override string Directory
            {
                get { return _directory; }
                set
                {
                    if (_directory == value) return;
                    _directory = value;
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

            public override string DisplayName
            {
                get { return _displayName; }
                set
                {
                    if (_displayName == value) return;
                    _displayName = value;
                    OnPropertyChanged();
                }
            }

            #endregion

            [OnDeserialized]
            private void AfterDeserialization(StreamingContext context)
            {
                // Note to self: 
                // Deserialization does not normally respect default values.
                // In order to force nontrivial default values, this method 
                // (with the [OnDeserializing] attribute) is called at the 
                // beginning of the deserialization process.  
                // This is where we can initialize members with their nontrivial
                // default values.  
                _ignore = !ArchiveLogs && !ClearLogs;
            }
        }
    }
}