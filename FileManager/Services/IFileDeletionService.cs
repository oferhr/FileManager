using System.Collections.Generic;
namespace FileManager.Services
{
    public interface IFileDeletionService
    {
        void DeleteFiles(List<string> checkedItems, int daysToDelete);
    }
}
