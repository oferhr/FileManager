using System.Collections.Generic;
using FileManager;

namespace FileManager.Services
{
    public interface IConfigurationService
    {
        List<EmailDirSettings> GetEmailDirSettings();
        void SetEmailDirSettings(List<EmailDirSettings> settings);
        List<FolderSettings> GetFolderSettings();
        void SetFolderSettings(List<FolderSettings> settings);
        List<CountSettings> GetCountSettings();
        void SetCountSettings(List<CountSettings> settings);
        List<SplitSettings> GetSplitSettings();
        void SetSplitSettings(List<SplitSettings> settings);
        List<CopySettings> GetCopySettings();
        void SetCopySettings(List<CopySettings> settings);
        List<ArchiveSettings> GetArchiveSettings();
        void SetArchiveSettings(List<ArchiveSettings> settings);
    }
}
