using System.Collections.Generic;
using FileManager;

namespace FileManager.Services
{
    public interface IFileCountService
    {
        List<FileCount> CountFilesInDirectories(List<CountSettings> checkedItems, bool useProgressBar = false);
        
        // Grid management methods
        void InitializeGrid(List<CountSettings> countSettings, List<string> foldersList);
        void HandleGridCellValueChanged(int rowIndex, int columnIndex, string columnName, object value);
        void HandleGridCellEndEdit(int rowIndex, string method, string folder, bool check);
        
        // Configuration methods
        List<CountSettings> GetCountSettings();
        void SetCountSettings(List<CountSettings> settings);
        
        // Utility methods
        void SaveSelectedFoldersToSettings(List<CountSettings> checkedItems);
    }
}
