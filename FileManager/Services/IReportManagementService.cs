using System.Collections.Generic;

namespace FileManager.Services
{
    public interface IReportManagementService
    {
        void SetReportsNames(List<string> checkedItems, string destFolName);
    }
}
