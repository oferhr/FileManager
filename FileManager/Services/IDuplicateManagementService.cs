using System.Collections.Generic;
using System.Windows.Forms;

namespace FileManager.Services
{
    public interface IDuplicateManagementService
    {
        int FixDuplicates(List<string> checkedItems);
        bool CheckFolderIfNotFirstDuplication(IEnumerable<string> files, string dir);
        
        // CheckedListBox management methods
        void AddFolderToDuplicateFolders(string folder);
        void LoadCheckedDuplicateFolders(CheckedListBox duplicateFolders);
        void SaveSelectedDuplicateFolders(CheckedListBox.CheckedItemCollection checkedItems);
    }
}
