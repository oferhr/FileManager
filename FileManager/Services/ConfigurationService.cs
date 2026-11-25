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

            // Allow empty/null config path to maintain backward compatibility with Form1's validation flow
            // Form1 checks for empty config path and shows user-friendly Hebrew message
            if (string.IsNullOrWhiteSpace(configPath))
            {
                _loggingService.LogWarning("ConfigurationService initialized with empty path - operations will fail until valid path is provided");
                _configPath = string.Empty;
                return;
            }

            // Validate configuration path for security issues (traversal, invalid chars, etc)
            // but allow the constructor to succeed so Form1 can show user-friendly error messages
            if (!PathValidator.IsValidPath(configPath, out string validationError))
            {
                _loggingService.LogSecurityEvent("PathValidationFailure", $"Configuration path has validation warnings: {validationError}",
                    new Dictionary<string, object> { { "ConfigPath", configPath } });
                // Don't throw - just log warning and store the path as-is
                // Operations using this path will fail gracefully later
                _configPath = configPath;
            }
            else
            {
                _configPath = Path.GetFullPath(configPath);
            }

            _loggingService.LogInfo($"ConfigurationService initialized with path: {_configPath}", "ConfigurationService");
        }

        public List<EmailDirSettings> GetEmailDirSettings()
        {
            return ReadSettings<EmailDirSettings>("fileManager_emailDirConfig.json");
        }

        public bool SetEmailDirSettings(List<EmailDirSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_emailDirConfig.json");
            return SaveSettings(configPath, settings);
        }

        public List<FolderSettings> GetFolderSettings()
        {
            return ReadSettings<FolderSettings>("fileManager_foldersConfig.json");
        }

        public bool SetFolderSettings(List<FolderSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_foldersConfig.json");
            return SaveSettings(configPath, settings);
        }

        public List<CountSettings> GetCountSettings()
        {
            return ReadSettings<CountSettings>("fileManager_countsConfig.json");
        }

        public bool SetCountSettings(List<CountSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_countsConfig.json");
            return SaveSettings(configPath, settings);
        }

        public List<SplitSettings> GetSplitSettings()
        {
            return ReadSettings<SplitSettings>("fileManager_splitConfig.json");
        }

        public bool SetSplitSettings(List<SplitSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_splitConfig.json");
            return SaveSettings(configPath, settings);
        }

        public List<CopySettings> GetCopySettings()
        {
            return ReadSettings<CopySettings>("fileManager_copyConfig.json");
        }

        public bool SetCopySettings(List<CopySettings> settings)
        {
            // Fix: Use lowercase "copyConfig" for consistency
            var configPath = GetValidatedConfigPath("fileManager_copyConfig.json");
            return SaveSettings(configPath, settings);
        }

        public List<ArchiveSettings> GetArchiveSettings()
        {
            return ReadSettings<ArchiveSettings>("fileManager_archiveConfig.json");
        }

        public bool SetArchiveSettings(List<ArchiveSettings> settings)
        {
            var configPath = GetValidatedConfigPath("fileManager_archiveConfig.json");
            return SaveSettings(configPath, settings);
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

        /// <summary>
        /// Persists settings to disk. Returns true when the content is written (or already on disk
        /// unchanged), and false when the write is skipped because the location is read-only/locked.
        /// Throws for any other failure.
        /// </summary>
        private bool SaveSettings<T>(string configPath, List<T> settings)
        {
            lock (LockObject)
            {
                var sjson = JsonConvert.SerializeObject(settings.ToArray());

                // Skip the write when the on-disk content is already identical. Most of these
                // saves re-persist a selection that was just loaded from the same file, so this
                // avoids pointless writes — and means a read-only config location no longer
                // fails when nothing actually changed.
                try
                {
                    if (File.Exists(configPath) &&
                        string.Equals(File.ReadAllText(configPath), sjson, StringComparison.Ordinal))
                    {
                        _loggingService.LogInfo($"ConfigWrite skipped — content unchanged: {configPath}", "ConfigurationService");
                        return true;
                    }
                }
                catch (IOException) { /* unreadable — fall through and attempt the write */ }
                catch (UnauthorizedAccessException) { /* unreadable — fall through and attempt the write */ }

                try
                {
                    File.WriteAllText(configPath, sjson);
                    _loggingService.LogFileOperation("ConfigWrite", configPath, true);
                    return true;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    // Persisting settings is a convenience, not a prerequisite for the running
                    // action. When the config location is read-only or locked (e.g. a shared drive
                    // where this user lacks write access), log a warning and continue rather than
                    // aborting the caller's workflow. This is a handled, non-fatal condition, so it
                    // is logged at Warn (not as a failed file operation, which would be Error-level).
                    // Other exception types still propagate below.
                    _loggingService.LogWarning(
                        $"Could not persist settings to {configPath} (location not writable): {ex.Message}. " +
                        "Continuing without saving.",
                        "ConfigurationService");
                    return false;
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
