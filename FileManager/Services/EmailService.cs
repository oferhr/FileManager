using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using FileManager;
using FileManager.Utilities;
using Newtonsoft.Json;

namespace FileManager.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly IFileCountService _fileCountService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;
        private readonly List<string> _allowedMailDomains;
        private readonly string[] _mailCheck = { "איחוד-קצר", "בודד-זהה", "בודד-קצר", "איחוד שמי", "איחוד לפי דוח" };
        private const string CopiedFilesDirectory = "9876789";

        public EmailService(
            string basePath,
            IFileService fileService,
            IConfigurationService configurationService,
            IFileCountService fileCountService,
            ILoggingService loggingService,
            List<string> allowedMailDomains,
            Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _configurationService = configurationService;
            _fileCountService = fileCountService;
            _loggingService = loggingService;
            _allowedMailDomains = allowedMailDomains ?? new List<string>();
            _progressCallback = progressCallback;
        }

        public bool IsEmailDomainAllowed(string email, out string errorMessage)
        {
            if (_allowedMailDomains.Count == 0)
            {
                errorMessage = "No allowed mail domains configured (AllowedMailDomains in App.config is empty).";
                return false;
            }
            return EmailValidator.IsEmailFromAllowedDomain(email, _allowedMailDomains, out errorMessage);
        }

        public void SendEmails(List<EmailDirSettings> dirSettings, int sleepSeconds)
        {
            // Validate all email addresses before processing. Each row may contain a single
            // address or multiple addresses separated by comma/semicolon.
            // Materialize with .ToList() to avoid duplicate enumeration and duplicate security event logging
            var validDirs = dirSettings.Where(w =>
            {
                if (string.IsNullOrEmpty(w.email))
                    return false;

                string emailError;
                if (!EmailValidator.IsValidEmailList(w.email, out emailError))
                {
                    _loggingService.LogSecurityEvent($"Invalid email address(es) blocked: {w.email} for directory: {w.dir}. Error: {emailError}");
                    return false;
                }

                string domainError;
                if (!IsEmailDomainAllowed(w.email, out domainError))
                {
                    _loggingService.LogSecurityEvent("EmailDomainNotAllowed",
                        $"Blocked send to disallowed domain: {w.email}. {domainError}",
                        new Dictionary<string, object> { { "Email", w.email }, { "Dir", w.dir } });
                    return false;
                }

                return true;
            }).ToList();

            var counts = validDirs.Count;
            double pbPart = counts == 0 ? 100 : 100 / counts;

            foreach (var dirSetting in validDirs)
            {
                var sanitizedEmail = EmailValidator.SanitizeEmailList(dirSetting.email);
                if (sanitizedEmail != dirSetting.email)
                {
                    _loggingService.LogInfo($"Email address sanitized from '{dirSetting.email}' to '{sanitizedEmail}'");
                    dirSetting.email = sanitizedEmail;
                }

                var combinedPath = Path.Combine(_basePath, dirSetting.dir);
                string basePath, pathError;
                if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out pathError))
                {
                    _loggingService.LogSecurityEvent("PathValidationFailure",
                        $"EmailService rejected directory outside allowed boundary: {pathError}",
                        new Dictionary<string, object> { { "dir", dirSetting.dir }, { "basePath", _basePath } });
                    continue;
                }

                if (!Directory.Exists(basePath))
                {
                    continue;
                }

                var lfiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                var files = lfiles as IList<string> ?? lfiles.ToList();

                if (files.Any())
                {
                    var arfiles = ProcessFilesForEmail(files, dirSetting);
                    SendEmailAttachments(arfiles, dirSetting, pbPart, sleepSeconds);
                    CleanupCopiedFiles(arfiles);
                    ArchiveProcessedFiles(basePath);
                }
            }
        }

        public void HandleGridCellEndEdit(int rowIndex, string email, string folder, string method)
        {
            // Validate email address(es) if provided. A row may contain a single address or
            // multiple addresses separated by comma/semicolon.
            // Note: Don't throw exception here as this method is called for all column edits,
            // not just email column. Email validation is handled in Form1's dataGridView1_CellEndEdit
            // for the email column specifically. This is just a safety check.
            if (!string.IsNullOrEmpty(email))
            {
                string emailError;
                if (!EmailValidator.IsValidEmailList(email, out emailError))
                {
                    _loggingService.LogWarning($"Skipping grid update due to invalid email(s): {email}. Error: {emailError}");
                    return; // Skip updating configuration with invalid email
                }

                email = EmailValidator.SanitizeEmailList(email);
            }

            var emailConfigList = _configurationService.GetEmailDirSettings();

            var curdir = emailConfigList.Find(f => f.dir == folder);
            if (curdir != null)
            {
                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(method))
                {
                    emailConfigList.Remove(curdir);
                }
                else
                {
                    curdir.email = email;
                    curdir.method = method;
                    _loggingService.LogInfo($"Updated email settings for folder {folder}: {email}");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(method))
                {
                    emailConfigList.Add(new EmailDirSettings
                    {
                        dir = folder,
                        email = email,
                        method = method,
                        check = _mailCheck[0],
                        icheck = 0
                    });
                    _loggingService.LogInfo($"Added email settings for folder {folder}: {email}");
                }
            }

            _configurationService.SetEmailDirSettings(emailConfigList);

            var countsConfigList = _fileCountService.GetCountSettings();

            var curCountDir = countsConfigList.Find(f => f.dir == folder);
            if (curCountDir != null)
            {
                curCountDir.method = string.IsNullOrEmpty(method) ? null : method;
            }
            else
            {
                if (!string.IsNullOrEmpty(method))
                {
                    countsConfigList.Add(new CountSettings
                    {
                        dir = folder,
                        method = method,
                        check = false
                    });
                }
            }

            _fileCountService.SetCountSettings(countsConfigList);
        }

        public void HandleGridCellValueChanged(int rowIndex, string checkValue, string folder)
        {
            var icheck = 0;
            for (var i = 0; i < _mailCheck.Length; i++)
            {
                if (_mailCheck[i] == checkValue)
                {
                    icheck = i;
                }
            }

            var dirSettings = _configurationService.GetEmailDirSettings();
            var dirObj = dirSettings.Find(f => f.dir == folder);
            if (dirObj != null)
            {
                dirObj.icheck = icheck;
            }
            else
            {
                dirSettings.Add(new EmailDirSettings
                {
                    dir = folder,
                    icheck = icheck
                });
            }

            _configurationService.SetEmailDirSettings(dirSettings);
        }

        public void RefreshEmailGrid(DataGridView dataGridView, List<string> foldersList)
        {
            var emailsDs = GetEmailDirSettingsForGrid(foldersList);
            dataGridView.DataSource = null;
            dataGridView.DataSource = emailsDs;
        }

        public List<EmailDirSettings> GetEmailDirSettingsForGrid(List<string> foldersList)
        {
            var emailConfigList = _configurationService.GetEmailDirSettings();
            var emailsDs = new List<EmailDirSettings>();
            
            foreach (var fol in foldersList)
            {
                var curdir = emailConfigList.Find(f => f.dir == fol);
                if (curdir != null)
                {
                    emailsDs.Add(new EmailDirSettings
                    {
                        dir = fol,
                        email = curdir.email,
                        check = curdir.icheck == 0 ? _mailCheck[0] : curdir.icheck == 1 ? _mailCheck[1] : curdir.icheck == 2 ? _mailCheck[2] : _mailCheck[3],
                        method = curdir.method
                    });
                }
                else
                {
                    emailsDs.Add(new EmailDirSettings
                    {
                        dir = fol,
                        email = null,
                        check = _mailCheck[1],
                        icheck = 0,
                        method = null
                    });
                }
            }

            return emailsDs;
        }

        private List<List<string>> ProcessFilesForEmail(IList<string> files, EmailDirSettings dirSetting)
        {
            var lCopiedNames = new List<string>();
            var lpaths = new List<string>();
            var arfiles = new List<List<string>>();
            var ardirs = new List<string>();

            foreach (var file in files)
            {
                var isGoodDirectory = false;
                var currentDir = Path.GetFileName(Path.GetDirectoryName(file));
                
                if (currentDir == "1")
                {
                    if (!ardirs.Contains(currentDir))
                    {
                        ardirs.Add(currentDir);
                    }
                    isGoodDirectory = true;
                }

                if (!isGoodDirectory)
                {
                    continue;
                }

                var fileName = Path.GetFileName(file);
                if (fileName == null || _fileService.IsThumbsInPath(file))
                {
                    continue;
                }

                var newFileName = fileName;
                var newFile = file;
                if (fileName.Trim().Contains(" "))
                {
                    newFileName = fileName.Replace(" ", "_");
                }

                var copiedPath = Path.Combine(Path.GetDirectoryName(file), CopiedFilesDirectory);
                if (!Directory.Exists(copiedPath))
                {
                    Directory.CreateDirectory(copiedPath);
                }
                
                newFile = Path.Combine(copiedPath, newFileName);
                File.Copy(file, newFile, true);

                lCopiedNames.Add(_fileService.GetMailFileName(fileName, dirSetting.icheck));
                lpaths.Add(newFile);
            }

            // Group file names
            var duplicateKeys = lCopiedNames.GroupBy(x => x).Select(group => group.Key);
            var enumerable = duplicateKeys as string[] ?? duplicateKeys.ToArray();
            
            if (enumerable.Any())
            {
                foreach (var duplicateKey in enumerable)
                {
                    var xduplicateKey = Path.GetFileNameWithoutExtension(duplicateKey);
                    xduplicateKey = duplicateKey.Replace(" ", "_");
                    var ll = from ln in lpaths where ln.Contains(xduplicateKey) select ln;
                    arfiles.Add(new List<string>(ll.ToList()));
                }
            }

            return arfiles;
        }

        private void SendEmailAttachments(List<List<string>> arfiles, EmailDirSettings dirSetting, double pbPart, int sleepSeconds)
        {
            var pbIncrement = arfiles.Count == 0 ? 100 : pbPart / arfiles.Count;

            foreach (var arfile in arfiles)
            {
                Microsoft.Office.Interop.Outlook.Application oApp = null;
                MailItem oMsg = null;

                try
                {
                    oApp = new Microsoft.Office.Interop.Outlook.Application();
                    oMsg = (MailItem)oApp.CreateItem(OlItemType.olMailItem);

                    // Outlook resolves a semicolon-delimited To string natively, so hand it
                    // the canonical sanitized list (one or many addresses).
                    var sanitizedEmail = EmailValidator.SanitizeEmailList(dirSetting.email);
                    oMsg.To = sanitizedEmail;

                    string fileName = "";
                    try
                    {
                        fileName = Path.GetFileName(arfile[0]);
                    }
                    catch (System.Exception ex)
                    {
                        // Don't access arfile[0] again if it caused the exception (e.g., IndexOutOfRangeException)
                        var safeFileInfo = arfile != null && arfile.Count > 0 ? arfile[0] : "[empty attachment list]";
                        _loggingService.LogError($"Failed to get filename for email attachment: {safeFileInfo}", ex);
                        throw; // Rethrow to outer catch which will cleanup COM objects
                    }

                    var subject = string.Empty;
                    if (fileName != null)
                    {
                        subject = dirSetting.icheck == 2 ? _fileService.GetMailFileName(fileName, dirSetting.icheck, true) : fileName;
                    }

                    // Sanitize subject to prevent header injection
                    subject = InputValidator.SanitizeString(subject);
                    oMsg.Subject = subject;

                    foreach (var curFile in arfile)
                    {
                        oMsg.Attachments.Add(curFile, OlAttachmentType.olByValue, Type.Missing, Type.Missing);
                    }

                    oMsg.GetInspector.Activate();
                    var signature = oMsg.HTMLBody;
                    oMsg.HTMLBody = string.Empty + signature;

                    _loggingService.LogInfo($"Sending email to {sanitizedEmail} with {arfile.Count} attachments, subject: {subject}");
                    oMsg.Send();

                    var dVal = pbIncrement;
                    var val = Convert.ToInt32(dVal);
                    if (val > 100)
                    {
                        val = 100;
                    }
                    _progressCallback?.Invoke(val);

                    if (sleepSeconds > 0)
                    {
                        Thread.Sleep(sleepSeconds * 1000);
                    }
                }
                catch (System.Exception ex)
                {
                    _loggingService.LogError($"Failed to send email to {dirSetting.email}", ex);
                    // Continue processing other emails even if one fails
                }
                finally
                {
                    // Always release COM objects to prevent resource leaks
                    if (oMsg != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oMsg);
                        oMsg = null;
                    }
                    if (oApp != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oApp);
                        oApp = null;
                    }
                }
            }
        }

        private void CleanupCopiedFiles(List<List<string>> arfiles)
        {
            foreach (var arfile in arfiles)
            {
                foreach (var curFile in arfile)
                {
                    if (curFile.Contains(CopiedFilesDirectory))
                    {
                        var path = Path.GetDirectoryName(curFile);
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                    }
                }
            }
        }

        private void ArchiveProcessedFiles(string basePath)
        {
            var dt = DateTime.Now;
            var name = dt.Day.ToString().PadLeft(2, '0') + "." + dt.Month.ToString().PadLeft(2, '0') + "." +
                       dt.Year % 100 + "." + dt.Hour.ToString().PadLeft(2, '0') + "." +
                       dt.Minute.ToString().PadLeft(2, '0');
            var newDir = Path.Combine(basePath, name);

            Directory.CreateDirectory(newDir);
            var checkedPath = Path.Combine(basePath, "1");
            if (Directory.Exists(checkedPath))
            {
                var dirFiles = Directory.GetFiles(checkedPath);
                foreach (var dirFile in dirFiles)
                {
                    try
                    {
                        _fileService.MoveFiles(dirFile, Path.Combine(newDir, Path.GetFileName(dirFile)));
                    }
                    catch (System.Exception ex)
                    {
                        _loggingService.LogError($"Failed to archive file during email processing: {dirFile}", ex);
                        // Continue processing remaining files even if one fails
                    }
                }
            }

            var curdirfiles = Directory.GetFiles(newDir);
            if (curdirfiles.Length == 0)
            {
                try
                {
                    Directory.Delete(newDir, true);
                }
                catch
                {
                    // Log error if needed
                }
            }

            if (Directory.Exists(checkedPath))
            {
                try
                {
                    Directory.Delete(checkedPath);
                }
                catch (IOException)
                {
                    Directory.Delete(checkedPath, true);
                }
                catch (UnauthorizedAccessException)
                {
                    Directory.Delete(checkedPath, true);
                }
            }
        }
    }
}
