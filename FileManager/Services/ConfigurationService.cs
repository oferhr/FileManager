using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FileManager.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configPath;
        private static readonly object LockObject = new object();

        public ConfigurationService(string configPath)
        {
            _configPath = configPath;
        }

        public List<EmailDirSettings> GetEmailDirSettings()
        {
            var dirSettings = new List<EmailDirSettings>();
            var configPath = Path.Combine(_configPath, "fileManager_emailDirConfig.json");
            
            if (File.Exists(configPath))
            {
                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    dirSettings = JsonConvert.DeserializeObject<List<EmailDirSettings>>(json);
                }
            }
            return dirSettings;
        }

        public void SetEmailDirSettings(List<EmailDirSettings> settings)
        {
            var configPath = Path.Combine(_configPath, "fileManager_emailDirConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<FolderSettings> GetFolderSettings()
        {
            var configPath = Path.Combine(_configPath, "fileManager_foldersConfig.json");
            var folderSettings = new List<FolderSettings>();
            
            if (File.Exists(configPath))
            {
                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    folderSettings = JsonConvert.DeserializeObject<List<FolderSettings>>(json);
                }
            }
            return folderSettings;
        }

        public void SetFolderSettings(List<FolderSettings> settings)
        {
            var configPath = Path.Combine(_configPath, "fileManager_foldersConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<CountSettings> GetCountSettings()
        {
            var configPath = Path.Combine(_configPath, "fileManager_countsConfig.json");
            List<CountSettings> folderSettings;
            
            if (File.Exists(configPath))
            {
                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    folderSettings = JsonConvert.DeserializeObject<List<CountSettings>>(json);
                }
            }
            else
            {
                folderSettings = new List<CountSettings>();
            }
            return folderSettings;
        }

        public void SetCountSettings(List<CountSettings> settings)
        {
            var configPath = Path.Combine(_configPath, "fileManager_countsConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<SplitSettings> GetSplitSettings()
        {
            var configPath = Path.Combine(_configPath, "fileManager_splitConfig.json");
            List<SplitSettings> folderSettings;
            
            if (File.Exists(configPath))
            {
                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    folderSettings = JsonConvert.DeserializeObject<List<SplitSettings>>(json);
                }
            }
            else
            {
                folderSettings = new List<SplitSettings>();
            }
            return folderSettings;
        }

        public void SetSplitSettings(List<SplitSettings> settings)
        {
            var configPath = Path.Combine(_configPath, "fileManager_splitConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<CopySettings> GetCopySettings()
        {
            var configPath = Path.Combine(_configPath, "fileManager_copyConfig.json");
            List<CopySettings> folderSettings;
            
            if (File.Exists(configPath))
            {
                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    folderSettings = JsonConvert.DeserializeObject<List<CopySettings>>(json);
                }
            }
            else
            {
                folderSettings = new List<CopySettings>();
            }
            return folderSettings;
        }

        public void SetCopySettings(List<CopySettings> settings)
        {
            var configPath = Path.Combine(_configPath, "fileManager_CopyConfig.json");
            SaveSettings(configPath, settings);
        }

        public List<ArchiveSettings> GetArchiveSettings()
        {
            var configPath = Path.Combine(_configPath, "fileManager_archiveConfig.json");
            List<ArchiveSettings> folderSettings;
            
            if (File.Exists(configPath))
            {
                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    folderSettings = JsonConvert.DeserializeObject<List<ArchiveSettings>>(json);
                }
            }
            else
            {
                folderSettings = new List<ArchiveSettings>();
            }
            return folderSettings;
        }

        public void SetArchiveSettings(List<ArchiveSettings> settings)
        {
            var configPath = Path.Combine(_configPath, "fileManager_archiveConfig.json");
            SaveSettings(configPath, settings);
        }

        private void SaveSettings<T>(string configPath, List<T> settings)
        {
            lock (LockObject)
            {
                var sjson = JsonConvert.SerializeObject(settings.ToArray());
                File.WriteAllText(configPath, sjson);
            }
        }
    }
}
