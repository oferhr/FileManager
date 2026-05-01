using System;
using System.Collections.Generic;
using System.Configuration;
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
        /// <summary>
        /// Internal data structure that pairs the computed subject key (group name without
        /// extension) with the list of copied file paths belonging to that group.  Used only
        /// inside EmailService and intentionally not exposed through the interface.
        /// </summary>
        private sealed class MailGroup
        {
            /// <summary>Group name without extension — used verbatim as the Outlook subject.</summary>
            public string SubjectKey { get; set; }

            /// <summary>Full paths of the temporary copies inside the <c>9876789</c> subdirectory.</summary>
            public List<string> Files { get; set; } = new List<string>();
        }

        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly IFileCountService _fileCountService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;
        private readonly string[] _mailCheck = { "איחוד-קצר", "בודד-זהה", "בודד-קצר", "איחוד שמי", "איחוד לפי דוח" };
        private const string CopiedFilesDirectory = "9876789";

        /// <summary>
        /// When <c>true</c>, per-file and per-group diagnostic info logs are emitted.
        /// Controlled by the <c>EmailDiagnosticsEnabled</c> key in App.config.
        /// Defaults to <c>true</c> when the key is absent or unparseable so that
        /// diagnostics are on by default in new or upgraded installations.
        /// </summary>
        private readonly bool _diagnosticsEnabled;

        public EmailService(
            string basePath,
            IFileService fileService,
            IConfigurationService configurationService,
            IFileCountService fileCountService,
            ILoggingService loggingService,
            Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _configurationService = configurationService;
            _fileCountService = fileCountService;
            _loggingService = loggingService;
            _progressCallback = progressCallback;

            // Read diagnostic flag. Default to true when the key is missing or unparseable
            // so that diagnostics are enabled in existing installations that predate the key.
            var diagFlag = ConfigurationManager.AppSettings["EmailDiagnosticsEnabled"];
            bool parsed;
            _diagnosticsEnabled = !bool.TryParse(diagFlag, out parsed) || parsed;
        }

        /// <summary>
        /// Emits a diagnostic <see cref="ILoggingService.LogInfo"/> message only when
        /// <see cref="_diagnosticsEnabled"/> is <c>true</c>.  Use for high-volume per-file
        /// and per-group traces.  Skip paths and warnings should use
        /// <see cref="ILoggingService.LogWarning"/> directly so they are always visible.
        /// </summary>
        private void LogDiag(string message)
        {
            if (_diagnosticsEnabled)
            {
                _loggingService.LogInfo(message);
            }
        }

        public EmailSendResult SendEmails(List<EmailDirSettings> dirSettings, int sleepSeconds)
        {
            var result = new EmailSendResult();

            // Pre-filter loop: validate each row and count skips by reason before building
            // the list of directories that are safe to process.
            var validDirs = new List<EmailDirSettings>();
            foreach (var w in dirSettings)
            {
                if (string.IsNullOrEmpty(w.email))
                {
                    result.SkippedNoEmail++;
                    _loggingService.LogWarning($"[SendEmails] Skipped row with empty email. dir='{w.dir}'");
                    continue;
                }

                string emailError;
                if (!EmailValidator.IsValidEmailList(w.email, out emailError))
                {
                    result.SkippedInvalidFormat++;
                    _loggingService.LogSecurityEvent($"Invalid email address(es) blocked: {w.email} for directory: {w.dir}. Error: {emailError}");
                    continue;
                }

                validDirs.Add(w);
            }

            var counts = validDirs.Count;
            double pbPart = counts == 0 ? 100 : 100.0 / counts;

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
                    result.SkippedMissingFolder++;
                    _loggingService.LogSecurityEvent("PathValidationFailure",
                        $"EmailService rejected directory outside allowed boundary: {pathError}",
                        new Dictionary<string, object> { { "dir", dirSetting.dir }, { "basePath", _basePath } });
                    continue;
                }

                if (!Directory.Exists(basePath))
                {
                    result.SkippedMissingFolder++;
                    _loggingService.LogWarning($"Email directory does not exist: {basePath}");
                    continue;
                }

                var lfiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                var files = lfiles as IList<string> ?? lfiles.ToList();

                if (!files.Any())
                {
                    result.SkippedNoFiles++;
                    _loggingService.LogWarning($"No .tif/.tiff/.pdf files found in directory: {basePath}");
                    continue;
                }

                var arfiles = ProcessFilesForEmail(files, dirSetting);
                if (arfiles.Count == 0)
                {
                    _loggingService.LogWarning($"[SendEmails] No mail groups produced for dir='{dirSetting.dir}'. Likely no files in '1' subdirectory.");
                }

                SendEmailAttachments(arfiles, dirSetting, pbPart, sleepSeconds, result);
                CleanupCopiedFiles(arfiles);
                ArchiveProcessedFiles(basePath);
            }

            return result;
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

        /// <summary>
        /// Copies all eligible files from the <c>1</c> subdirectory into a temporary
        /// <c>9876789</c> staging folder, computes the mail file name for each file, and
        /// groups the resulting paths into <see cref="MailGroup"/> instances.
        /// </summary>
        /// <remarks>
        /// Grouping is performed by index rather than substring matching so that files
        /// whose names are substrings of one another (e.g. <c>1.pdf</c> and <c>11.pdf</c>)
        /// are correctly placed into separate groups.
        /// </remarks>
        private List<MailGroup> ProcessFilesForEmail(IList<string> files, EmailDirSettings dirSetting)
        {
            // Parallel lists: lCopiedNames[i] is the computed mail-name for the file whose
            // staging path is lpaths[i].  Index correspondence is maintained throughout so
            // that GroupBy on lCopiedNames can resolve back to the correct lpaths entries.
            var lCopiedNames = new List<string>();
            var lpaths = new List<string>();
            var ardirs = new List<string>();

            foreach (var file in files)
            {
                var currentDir = Path.GetFileName(Path.GetDirectoryName(file));

                if (currentDir != "1")
                {
                    LogDiag($"[ProcessFilesForEmail] Skipped (not in '1' subdir). file='{file}'");
                    continue;
                }

                if (!ardirs.Contains(currentDir))
                {
                    ardirs.Add(currentDir);
                }

                var fileName = Path.GetFileName(file);
                if (fileName == null || _fileService.IsThumbsInPath(file))
                {
                    LogDiag($"[ProcessFilesForEmail] Skipped (Thumbs.db or null name). file='{file}'");
                    continue;
                }

                // Replace spaces in the destination filename to avoid issues with paths
                // that contain spaces when Outlook processes the attachment.
                var newFileName = fileName.Trim().Contains(" ") ? fileName.Replace(" ", "_") : fileName;

                var copiedPath = Path.Combine(Path.GetDirectoryName(file), CopiedFilesDirectory);
                if (!Directory.Exists(copiedPath))
                {
                    Directory.CreateDirectory(copiedPath);
                }

                var newFile = Path.Combine(copiedPath, newFileName);
                File.Copy(file, newFile, true);

                // Record the computed mail name (used as the group key) and the staged path.
                lCopiedNames.Add(_fileService.GetMailFileName(fileName, dirSetting.icheck));
                lpaths.Add(newFile);
                LogDiag($"[ProcessFilesForEmail] Added file. dir='{dirSetting.dir}', original='{fileName}', mailName='{lCopiedNames[lCopiedNames.Count - 1]}', staged='{newFile}'");
            }

            // Group by the computed mail name using index pairs so that exact equality is
            // used for matching.  This prevents a file named "1.pdf" from being placed into
            // the same group as "11.pdf" (the old Contains-based approach would mis-group them).
            var arfiles = new List<MailGroup>();

            var groups = Enumerable.Range(0, lCopiedNames.Count).GroupBy(i => lCopiedNames[i]);
            foreach (var group in groups)
            {
                arfiles.Add(new MailGroup
                {
                    // Strip the extension from the group key to reproduce the v1.2.44
                    // subject behaviour: subject is the group name without extension.
                    SubjectKey = Path.GetFileNameWithoutExtension(group.Key),
                    Files = group.Select(i => lpaths[i]).ToList()
                });
                LogDiag($"[ProcessFilesForEmail] Group built. dir='{dirSetting.dir}', subjectKey='{Path.GetFileNameWithoutExtension(group.Key)}', fileCount={group.Count()}, files=[{string.Join(", ", group.Select(i => Path.GetFileName(lpaths[i])))}]");
            }

            return arfiles;
        }

        private void SendEmailAttachments(List<MailGroup> arfiles, EmailDirSettings dirSetting, double pbPart, int sleepSeconds, EmailSendResult result)
        {
            var pbIncrement = arfiles.Count == 0 ? 100 : pbPart / arfiles.Count;

            foreach (var arfile in arfiles)
            {
                // Count this email as attempted before entering the try block so that
                // exceptions during setup (e.g. COM creation failure) are still tracked.
                result.Attempted++;

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

                    // Restore v1.2.44 subject behaviour:
                    //   - For icheck == 2: the pre-refactor pipeline ran GetMailFileName once
                    //     during grouping (producing SubjectKey) and then again with
                    //     keepExtension=true before using it as the subject.  We reproduce
                    //     that by calling GetMailFileName on SubjectKey and stripping the
                    //     extension from the result.
                    //   - For all other icheck values: the subject is SubjectKey directly
                    //     (group name without extension), which matches v1.2.44 exactly.
                    string subject;
                    if (dirSetting.icheck == 2)
                    {
                        subject = Path.GetFileNameWithoutExtension(
                            _fileService.GetMailFileName(arfile.SubjectKey, dirSetting.icheck, true));
                    }
                    else
                    {
                        subject = arfile.SubjectKey;
                    }

                    // Sanitize subject to prevent header injection
                    subject = InputValidator.SanitizeString(subject);
                    oMsg.Subject = subject;

                    foreach (var curFile in arfile.Files)
                    {
                        oMsg.Attachments.Add(curFile, OlAttachmentType.olByValue, Type.Missing, Type.Missing);
                    }

                    LogDiag($"[SendEmailAttachments] Group prepared. recipient='{sanitizedEmail}', subject='{subject}', attachments=[{string.Join(", ", arfile.Files.Select(Path.GetFileName))}]");

                    oMsg.GetInspector.Activate();
                    var signature = oMsg.HTMLBody;
                    oMsg.HTMLBody = string.Empty + signature;

                    _loggingService.LogInfo($"Sending email to {sanitizedEmail} with {arfile.Files.Count} attachments, subject: {subject}");
                    oMsg.Send();
                    result.Succeeded++;

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
                    result.FailedRecipients.Add($"{dirSetting.email}: {ex.Message}");
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

        private void CleanupCopiedFiles(List<MailGroup> arfiles)
        {
            foreach (var arfile in arfiles)
            {
                foreach (var curFile in arfile.Files)
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
