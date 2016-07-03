// -----------------------------------------------------------------------
//  <copyright file="ConfigFileSerializer.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model
{
    public abstract class ConfigFileSerializer
    {
        public static ConfigFileSerializer GetInstance(Logger logger)
        {
            return new ServiceFileDataContractSerializer(logger);
        }

        public abstract ConfigFile ReadFile(string filePath);
        public abstract bool WriteFile(ConfigFile file, string filePath);

        private sealed class ServiceFileDataContractSerializer : ConfigFileSerializer
        {
            private readonly Logger _logger;
            private readonly DataContractSerializer _serializer;

            internal ServiceFileDataContractSerializer(Logger logger)
            {
                //Contract.Requires(logger != null);
                _logger = logger;
                var type = ConfigFile.GetInstance().GetType();
                var knownTypes = new List<Type> {typeof (ConfigFile.ImplementedConfigFile)};
                _serializer = new DataContractSerializer(type, knownTypes);
            }

            public override ConfigFile ReadFile(string filePath)
            {
                try
                {
                    WaitForLock(filePath);

                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        using (var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                        {
                            var obj = ReadObject(reader, true);
                            var serviceFile = (ConfigFile) obj;

                            serviceFile.FilePath = filePath;
                            serviceFile.Services.LogDetails = serviceFile.LogDetails;

                            return serviceFile;
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    _logger.ErrorException(ex, Strings.Error_FileNotFound, filePath);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
                return ConfigFile.GetInstance();
            }

            public override bool WriteFile(ConfigFile file, string filePath)
            {
                var success = false;
                try
                {
                    WaitForLock(filePath);

                    var settings = new XmlWriterSettings {Indent = true};
                    using (var sw = XmlWriter.Create(filePath, settings))
                    {
                        WriteObject(sw, file);
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex);
                }
                return success;
            }

            private void WriteObject(XmlWriter writer, object graph)
            {
                _serializer.WriteObject(writer, graph);
            }

            private object ReadObject(XmlDictionaryReader reader, bool verifyObjectName = false)
            {
                return _serializer.ReadObject(reader, verifyObjectName);
            }

            private void WaitForLock(string filePath)
            {
                while (IsFileLocked(filePath))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                // File is no longer locked.
            }

            private bool IsFileLocked(string filePath)
            {
                //Contract.Requires(!string.IsNullOrEmpty(filePath));

                var file = FileInfoWrapper.GetInstance(filePath);
                FileStream stream = null;

                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (FileNotFoundException)
                {
                    if (stream != null)
                        stream.Close();
                    return false;
                }
                catch (Exception)
                {
                    return true;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
                return false;
            }
        }
    }
}