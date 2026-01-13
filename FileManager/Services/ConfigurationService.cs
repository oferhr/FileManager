using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configPath;
        private readonly ILoggingService _loggingService;
        private static readonly object LockObject = new object();

        public ConfigurationService(string configPath, ILoggingService loggingService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

            // Validate configuration path
            if (!PathValidator.IsValidPath(configPath, out string validationError))
            {
                _loggingService.LogSecurityEvent("PathValidationFailure", $"Invalid configuration path: {validationError}",
                    new Dictionary<string, object> { { "ConfigPath", configPath } });
                throw new ArgumentException($"Invalid configuration path: {validationError}", nameof(configPath));
            }

            _configPath = Path.GetFullPath(configPath);
            _loggingService.LogInfo($"ConfigurationService initialized with path: {_configPath}", "ConfigurationService");
        }

        public List<EmailDirSettings> GetEmailDirSettings()
        {
            return ReadSettings<EmailDirSettings>("fileManager_emailDirConfig.json");
        }

        public void SetEmailDirSettings(List<EmailDirSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_emailDirConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<FolderSettings> GetFolderSettings()
        {
            return ReadSettings<FolderSettings>("fileManager_foldersConfig.json");
        }

        public void SetFolderSettings(List<FolderSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_foldersConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<CountSettings> GetCountSettings()
        {
            return ReadSettings<CountSettings>("fileManager_countsConfig.json");
        }

        public void SetCountSettings(List<CountSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_countsConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<SplitSettings> GetSplitSettings()
        {
            return ReadSettings<SplitSettings>("fileManager_splitConfig.json");
        }

        public void SetSplitSettings(List<SplitSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_splitConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<CopySettings> GetCopySettings()
        {
            return ReadSettings<CopySettings>("fileManager_copyConfig.json");
        }

        public void SetCopySettings(List<CopySettings> settings)
        {
            // Fix: Use lowercase "copyConfig" for consistency
            var configPath = GetValidatedConfigPath("fileManager_copyConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<ArchiveSettings> GetArchiveSettings()
        {
            return ReadSettings<ArchiveSettings>("fileManager_archiveConfig.json");
        }

        public void SetArchiveSettings(List<ArchiveSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_archiveConfig.json");
            SaveSettings(configPath, settings);
        }

        /// <summary>
        /// Validates and returns a config file path, ensuring it stays within the configuration boundary.
        /// </summary>
        private string GetValidatedConfigPath(string fileName)
        {
            var configPath = Path.Combine(_configPath, fileName);

            if (!PathValidator.IsPathWithinBoundary(configPath, _configPath, out string validationError))
            {
                _loggingService.LogSecurityEvent("PathBoundaryViolation", $"Config file path outside boundary: {validationError}",
                    new Dictionary<string, object> { { "ConfigPath", configPath }, { "BasePath", _configPath }, { "FileName", fileName } });
                throw new InvalidOperationException($"Configuration file path is invalid: {validationError}");
            }

            return configPath;
        }

        /// <summary>
        /// Reads settings from a configuration file with proper validation and error handling.
        /// </summary>
        private List<T> ReadSettings<T>(string fileName) where T : class
        {
            var configPath = GetValidatedConfigPath(fileName);
            var settings = new List<T>();

            try
            {
                if (File.Exists(configPath))
                {
                    using (var r = new StreamReader(configPath))
                    {
                        var json = r.ReadToEnd();
                        settings = JsonConvert.DeserializeObject<List<T>>(json);
                    }
                    _loggingService.LogFileOperation("ConfigRead", configPath, true);
                }
                else
                {
                    _loggingService.LogInfo($"Configuration file not found: {configPath}", "ConfigurationService");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"ConfigurationService.ReadSettings<{typeof(T).Name}>", ex, $"Failed to read settings from {configPath}");
                throw;
            }

            return settings;
        }

        private void SaveSettings<T>(string configPath, List<T> settings)
        {
            lock (LockObject)
            {
                try
                {
                    var sjson = JsonConvert.SerializeObject(settings.ToArray());
                    File.WriteAllText(configPath, sjson);
                    _loggingService.LogFileOperation("ConfigWrite", configPath, true);
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"ConfigurationService.SaveSettings<{typeof(T).Name}>", ex, $"Failed to save settings to {configPath}");
                    _loggingService.LogFileOperation("ConfigWrite", configPath, false, ex.Message);
                    throw;
                }
            }
        }
    }
}
