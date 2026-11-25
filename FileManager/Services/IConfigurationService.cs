using System.Collections.Generic;
using FileManager;

namespace FileManager.Services
{
    public interface IConfigurationService
    {
        List<EmailDirSettings> GetEmailDirSettings();
        // Set* methods return true when the settings are persisted (or already on disk unchanged),
        // and false when the write was skipped because the config location is read-only/locked.
        // Background callers may ignore the result; interactive callers can use it to notify the user.
        bool SetEmailDirSettings(List<EmailDirSettings> settings);
        List<FolderSettings> GetFolderSettings();
        bool SetFolderSettings(List<FolderSettings> settings);
        List<CountSettings> GetCountSettings();
        bool SetCountSettings(List<CountSettings> settings);
        List<SplitSettings> GetSplitSettings();
        bool SetSplitSettings(List<SplitSettings> settings);
        List<CopySettings> GetCopySettings();
        bool SetCopySettings(List<CopySettings> settings);
        List<ArchiveSettings> GetArchiveSettings();
        bool SetArchiveSettings(List<ArchiveSettings> settings);
    }
}
