using System.Collections.Generic;
using System.Windows.Forms;

namespace FileManager.Services
{
    public interface IEmailService
    {
        void SendEmails(List<EmailDirSettings> dirSettings, int sleepSeconds);
        bool IsEmailDomainAllowed(string email, out string errorMessage);
        void HandleGridCellEndEdit(int rowIndex, string email, string folder, string method);
        void HandleGridCellValueChanged(int rowIndex, string checkValue, string folder);
        void RefreshEmailGrid(DataGridView dataGridView, List<string> foldersList);
        List<EmailDirSettings> GetEmailDirSettingsForGrid(List<string> foldersList);
    }
}
