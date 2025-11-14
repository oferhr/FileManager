using System.Collections.Generic;

namespace FileManager.Services
{
    public interface IArchiveService
    {
        void ArchiveFiles(List<ArchiveSettings> checkedItems, string parentPath, string destPath);
    }
}
