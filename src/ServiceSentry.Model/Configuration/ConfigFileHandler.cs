// -----------------------------------------------------------------------
//  <copyright file="ConfigFileHandler.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.IO;
using System.Reflection;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Model
{
    public abstract class ConfigFileHandler : PropertyChangedBase
    {
        public static ConfigFileHandler GetInstance(Logger logger)
        {
            return GetInstance(ConfigFileHelper.GetHelperInstance(logger), logger);
        }
        
        internal static ConfigFileHandler GetInstance(ConfigFileHelper helper, Logger logger)
        {
            return new ConfigFileHandlerImplementation(helper, logger);
        }

        #region Abstract Members

        internal abstract string ConfigFilePath { get; }
        public abstract void WriteConfigFile(ConfigFile file);
        public abstract ConfigFile ReadConfigFile();
        public abstract ConfigFile ReadConfigFile(string path);
        public abstract ConfigFile NewConfigFile { get; }

        #endregion

        private sealed class ConfigFileHandlerImplementation : ConfigFileHandler
        {
            private readonly ConfigFileHelper _helper;
            private readonly string _configFilePath;
            private readonly Logger _logger;

            public ConfigFileHandlerImplementation(ConfigFileHelper helper, Logger logger)
            {
                _logger = logger;
                _helper = helper;
                _configFilePath = _helper.ConfigFilePath;
            }

            internal override string ConfigFilePath
            {
                get { return _configFilePath; }
            }

            public override void WriteConfigFile(ConfigFile file)
            {
                // Add more Logs
                _logger.Debug(Strings.Info_WritingConfigFile, ConfigFilePath);
                _helper.WriteFile(file, ConfigFilePath);
            }

            public override ConfigFile ReadConfigFile()
            {
                return ReadConfigFile(_helper.ConfigFilePath);
            }

            public override ConfigFile ReadConfigFile(string path)
            {
                // Check file exists
                var fileExists = _helper.ConfigFileExists(path);

                if (fileExists)
                {
                    return _helper.ReadFile(path);
                }
                
                var file = _helper.NewConfigFile;
                _helper.WriteFile(file, path);
                return file;
            }

            public override ConfigFile NewConfigFile
            {
                get { return ConfigFile.Default; }
            }
        }



        internal abstract class ConfigFileHelper
        {
            internal static ConfigFileHelper GetHelperInstance(Logger logger)
            {
                return GetInstance(ConfigFileSerializer.GetInstance(logger));
            }

            internal static ConfigFileHelper GetInstance(ConfigFileSerializer serializer)
            {
                return new ConfigFileHelperImplementation(serializer);
            }


            #region Abstract Members

            public abstract bool ConfigFileExists(string path);
            public abstract ConfigFile ReadFile(string filePath);
            public abstract bool WriteFile(ConfigFile file);
            public abstract bool WriteFile(ConfigFile file, string filePath);

            public abstract ConfigFile NewConfigFile { get; }
            internal abstract string ConfigFilePath { get; }

            #endregion

            private sealed class ConfigFileHelperImplementation : ConfigFileHelper
            {
                private readonly ConfigFileSerializer _serializer;
                private readonly string _configFilePath;

                public ConfigFileHelperImplementation(ConfigFileSerializer serializer)
                {
                    _serializer = serializer;

                    var assyPath = Assembly.GetExecutingAssembly().Location;
                    var dirPath = Path.GetDirectoryName(assyPath);
                    _configFilePath = (dirPath + "\\ServiceSentry.xml");
                }

                public override bool ConfigFileExists(string path)
                {
                    return FileInfoWrapper.GetInstance(path).Exists;
                }

                public override ConfigFile ReadFile(string filePath)
                {
                    return _serializer.ReadFile(filePath);
                }

                public override bool WriteFile(ConfigFile file)
                {
                    return WriteFile(file, _configFilePath);
                }

                public override bool WriteFile(ConfigFile file, string filePath)
                {
                    if (filePath == "") filePath = file.FilePath;
                    return _serializer.WriteFile(file, filePath);
                }

                public override ConfigFile NewConfigFile
                {
                    get { return ConfigFile.Default; }
                }

                internal override string ConfigFilePath
                {
                    get{return _configFilePath;}
                }
            }
        }
    }
}