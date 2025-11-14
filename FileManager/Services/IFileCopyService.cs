using System.Collections.Generic;

namespace FileManager.Services
{
    public interface IFileCopyService
    {
        void CopyFiles(List<CopySettings> checkedItems);
    }
}
