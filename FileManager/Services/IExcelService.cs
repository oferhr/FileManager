using System.Collections.Generic;
using System.Windows.Forms;

namespace FileManager.Services
{
    public interface IExcelService
    {
        void SetExcelNames(List<string> checkedItems);
        object[,] GetExcelValues();
        string CheckExcelForItems(object[,] arr, string part);
        string SetExcelFileName(List<string> parts, string part, string result);
        string SetExcelFileName2(List<string> parts, string part, string result);
        void SetSelectedExcelFolders(List<string> checkedItems);
        void LoadExcelFolders(CheckedListBox dirList);
    }
}
