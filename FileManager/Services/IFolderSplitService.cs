using System.Collections.Generic;

namespace FileManager.Services
{
    public interface IFolderSplitService
    {
        void SplitFolders(List<SplitSettings> checkedItems);
        bool IsContain888(string fname);
    }
}
