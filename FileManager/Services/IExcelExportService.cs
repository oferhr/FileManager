using System.Windows.Forms;

namespace FileManager.Services
{
    public interface IExcelExportService
    {
        void ExportToExcel(DataGridView[] grids, CheckedListBox[] listBoxes, string[] sheetNames);
    }
}
