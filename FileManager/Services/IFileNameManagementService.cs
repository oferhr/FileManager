using System.Collections.Generic;
using System.Windows.Forms;

namespace FileManager.Services
{
    public interface IFileNameManagementService
    {
        void FixFileNames(List<string> checkedItems, bool isMigdal);
        void SetSelectedFileNameFolders(List<string> checkedItems);
        void LoadFileNameFolders(CheckedListBox filenamesListBox);
    }
}
