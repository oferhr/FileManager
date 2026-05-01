using System.Collections.Generic;
using System.Windows.Forms;

namespace FileManager.Services
{
    public interface IEmailService
    {
        EmailSendResult SendEmails(List<EmailDirSettings> dirSettings, int sleepSeconds);
        void HandleGridCellEndEdit(int rowIndex, string email, string folder, string method);
        void HandleGridCellValueChanged(int rowIndex, string checkValue, string folder);
        void RefreshEmailGrid(DataGridView dataGridView, List<string> foldersList);
        List<EmailDirSettings> GetEmailDirSettingsForGrid(List<string> foldersList);
    }
}
