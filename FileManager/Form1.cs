using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using Excel = Microsoft.Office.Interop.Excel;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using FileManager.Services;
using FileManager.Utilities;

namespace FileManager
{
    /// <summary>
    /// Main application form for the FileManager system.
    /// Provides comprehensive file management capabilities including:
    /// - File organization and duplicate detection
    /// - Excel file processing and conversion
    /// - PDF creation and merging
    /// - Automated email distribution via Outlook
    /// - Archive management
    /// - File counting and reporting
    /// </summary>
    /// <remarks>
    /// This form integrates with Microsoft Office (Excel and Outlook) for automation.
    /// Supports Hebrew language and Right-to-Left (RTL) text rendering.
    /// Uses NLog for comprehensive logging of operations and errors.
    /// Configuration is loaded from App.config at startup.
    /// </remarks>
    public partial class Form1 : Form
    {
        #region Private Fields

        /// <summary>
        /// Base path for file operations, loaded from configuration.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Path to shared configuration file.
        /// </summary>
        private readonly string _config;

        /// <summary>
        /// Path to Excel template/configuration file.
        /// </summary>
        private readonly string _excel;

        /// <summary>
        /// Sleep duration in seconds between email operations to avoid overwhelming the mail server.
        /// </summary>
        private readonly int _sleep;

        /// <summary>
        /// Flag indicating whether files should be moved (true) or copied (false).
        /// </summary>
        private bool _isMove = true;

        /// <summary>
        /// Counter tracking the number of duplicate files found.
        /// </summary>
        private int _numOfDuplicates;

        /// <summary>
        /// Left-to-Right mark Unicode character for mixed text direction handling.
        /// </summary>
        const string LtrMark = "\u200E";

        /// <summary>
        /// Lock object for thread-safe operations.
        /// </summary>
        private static readonly object LockObject = new object ();

        /// <summary>
        /// Temporary directory name for copied files during processing.
        /// </summary>
        private string CopiedFilesDirectory = "9876789";

        /// <summary>
        /// Timestamp for file deletion operations.
        /// </summary>
        private  DateTime dtDeleteFiles;

        /// <summary>
        /// Global list of folders being processed.
        /// </summary>
        private readonly List<string> gfoldersList = new List<string>();

        /// <summary>
        /// Array of mail processing method names in Hebrew.
        /// Defines the available email grouping and merging strategies.
        /// </summary>
        private readonly string[] mailCheck = new [] {"איחוד-קצר", "בודד-זהה", "בודד-קצר", "איחוד שמי", "איחוד לפי דוח" };

        /// <summary>
        /// NLog logger instance for this class.
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

        /// <summary>
        /// Flag indicating whether errors have occurred during processing.
        /// </summary>
        private bool hasErrors = false;
        
        // Services
        private readonly ILoggingService _loggingService;
        private readonly IFileCountService _fileCountService;
        private readonly IConfigurationService _configurationService;
        private readonly IFileService _fileService;
        private readonly IDuplicateManagementService _duplicateManagementService;
        private readonly IFileNameManagementService _fileNameManagementService;
        private readonly IEmailService _emailService;
        private readonly IExcelService _excelService;
        private readonly IActionConfigService _actionConfigService;

        /// <summary>
        /// Original unfiltered list of email directory settings for the email grid.
        /// Used to restore the full list when clearing the search filter.
        /// </summary>
        private List<EmailDirSettings> _originalEmailList;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the Form1 class.
        /// Loads configuration, initializes UI components, and populates folder lists.
        /// </summary>
        /// <remarks>
        /// The constructor performs the following initialization steps:
        /// 1. Loads configuration from App.config (paths and settings)
        /// 2. Validates required configuration paths exist
        /// 3. Queries the base directory for available folders
        /// 4. Populates UI controls with folder lists
        /// 5. Sets up default UI state (Reports tab selected, Mail button disabled)
        /// Any errors during initialization are logged and displayed to the user.
        /// </remarks>
        public Form1()
        {
            InitializeComponent();

            // Disable mail button until folders are selected
            btnMail.Enabled = false;

            // Set maximum form height to fit screen (leaving space for taskbar)
            this.MaximumSize = new Size(Size.Width, (Screen.PrimaryScreen.Bounds.Height - 50));

            try
            {
                // Load configuration settings from App.config
                _path = ConfigurationManager.AppSettings["basePath"];
                _config = ConfigurationManager.AppSettings ["ConfigPath"];
                _excel = ConfigurationManager.AppSettings ["ExcelPath"];
                var sleep = ConfigurationManager.AppSettings ["MailSleepSeconds"];

                // Parse sleep duration with fallback to 0 if invalid
                if (!Int32.TryParse(sleep, out _sleep))
                {
                    _sleep = 0;
                }

                // Initialize services
                _loggingService = new LoggingService();
                _actionConfigService = new ActionConfigService(_loggingService);
                _configurationService = new ConfigurationService(_config, _loggingService);
                _fileService = new FileService(true, _loggingService);
                _fileCountService = new FileCountService(
                    _path, 
                    _config, 
                    _fileService, 
                    _configurationService,
                    UpdateProgressBar,
                    LogMessage,
                    RefreshDataGridView1);
                _duplicateManagementService = new DuplicateManagementService(
                    _path,
                    _fileService,
                    _configurationService,
                    _loggingService,
                    UpdateProgressBar);
                _fileNameManagementService = new FileNameManagementService(
                    _path,
                    _fileService,
                    _configurationService,
                    _loggingService,
                    UpdateProgressBar,
                    LogMessage,
                    (visible) => progressBar1.Visible = visible,
                    (message) => lblProgressMessage.Text = message);
                _emailService = new EmailService(
                    _path,
                    _fileService,
                    _configurationService,
                    _fileCountService,
                    _loggingService,
                    UpdateProgressBar);
                _excelService = new ExcelService(
                    _path,
                    _excel,
                    _fileService,
                    _configurationService,
                    _loggingService,
                    UpdateProgressBar);

          
                tabsMain.SelectedTab = tabReports;

                // Validate required configuration paths
                if (_config == string.Empty)
                {
                    MessageBox.Show("נתיב לקובץ קונפיגורציה משותף אינו קיים");
                    return;
                }
                if (_excel == string.Empty) {
                    MessageBox.Show ( "נתיב לקובץ האקסל אינו קיים" );
                    return;
                }

                // Query available directories from base path
                string [] dirs;
                try
                {
                    dirs = Directory.GetDirectories(_path);
                }
                catch
                {
                    // Log and display error if base path is invalid or inaccessible
                    MessageBox.Show("Path in config file does not exist, or you do not have permission to query it.");

                    LogEventInfo eventInfo = new LogEventInfo
                    {
                        Level = LogLevel.Info,
                        Message = "Path in config file does not exist, or you do not have permission to query it."
                    };
                    logger.Log ( eventInfo );
                    return;
                }

                // Populate folder selection controls with available directories
                foreach (var path in dirs) {

                    var folder = Path.GetFileName ( path );

                    // Add folder to all relevant UI controls
                    if (folder != null) {

                        DuplicateFolders.Items.Add ( folder );  // Duplicate detection tab
                        reportFolders.Items.Add ( folder );      // Reports tab
                        filenames.Items.Add ( folder );          // File names tab
                        dirList.Items.Add ( folder );            // Directory list
                        gfoldersList.Add ( folder );             // Global folders list
                        DelList.Items.Add ( folder );            // Delete files tab
                    }
                }

                var countsConfigSettings = _fileCountService.GetCountSettings();
                var countDs = new List<CountSettings> ();
                foreach (var fol in gfoldersList)
                {
                    // Find existing settings for this folder or create default
                    var curdir = countsConfigSettings.Find ( f => f.dir == fol );
                    if (curdir != null) {
                        countDs.Add ( new CountSettings
                        {
                            dir = fol,
                            method = curdir.method,
                            check = curdir.check
                        } );
                    }
                    else {
                        // Default unchecked with no method
                        countDs.Add ( new CountSettings
                        {
                            dir = fol,
                            method = null,
                            check = false
                        } );
                    }
                }

                grdCount.AutoGenerateColumns = true;
                grdCount.DataSource = countDs;



                var splitConfigSettings = _configurationService.GetSplitSettings();
                var splitDs = new List<SplitSettings> ();

                foreach (var fol in gfoldersList) {
                    var curdir = splitConfigSettings.Find ( f => f.dir == fol );
                    if (curdir != null) {
                        splitDs.Add ( new SplitSettings
                        {
                            dir = fol,
                            dest = curdir.dest,
                            check = curdir.check
                        } );
                    }
                    else {
                        splitDs.Add ( new SplitSettings
                        {
                            dir = fol,
                            dest = null,
                            check = false
                        } );
                    }
                }

                // Bind split settings to grid
                grdFolderSplit.DataSource = splitDs;


                var CopyConfigSettings = _configurationService.GetCopySettings();
                var copyDs = new List<CopySettings>();

                foreach (var fol in gfoldersList)
                {
                    var curdir = CopyConfigSettings.Find(f => f.dir == fol);
                    if (curdir != null)
                    {
                        copyDs.Add(new CopySettings
                        {
                            dir = fol,
                            dest = curdir.dest,
                            check = curdir.check,
                            str = curdir.str
                        });
                    }
                    else
                    {
                        copyDs.Add(new CopySettings
                        {
                            dir = fol,
                            dest = null,
                            check = false,
                            str = null
                        });
                    }
                }

                // Bind copy settings to grid
                gridCopy.DataSource = copyDs;


               

                // Bind email settings to grid (manual column configuration)
                dataGridView1.AutoGenerateColumns = false;
                var emailsDs = _emailService.GetEmailDirSettingsForGrid(gfoldersList);
                _originalEmailList = emailsDs; // Store original list for filtering
                dataGridView1.DataSource = emailsDs;

                // Wire up real-time filtering on the search textbox
                txtSearchEmail.TextChanged += TxtSearchEmail_TextChanged;

                // Initialize grdArchive from JSON configuration file
                var archiveConfigSettings = _configurationService.GetArchiveSettings();
                var archiveDs = new List<ArchiveSettings>();
                
                // If archive source and destination are already set, populate the grid
                if (!string.IsNullOrEmpty(Properties.Settings.Default.ArchiveSourceName) && 
                    !string.IsNullOrEmpty(Properties.Settings.Default.ArchiveDestName))
                {
                    try
                    {
                        var ArchDirs = Directory.GetDirectories(Properties.Settings.Default.ArchiveSourceName);
                        var fols = new List<string>();
                        foreach (var path in ArchDirs)
                        {
                            var folder = Path.GetFileName(path);
                            if (folder != null)
                            {
                                fols.Add(folder);
                            }
                        }
                        
                        var parentFolder = Properties.Settings.Default.ArchiveSourceName;
                        foreach (var fol in fols)
                        {
                            var curdir = archiveConfigSettings.Find(f => f.dir == fol && f.sourceDir == parentFolder);
                            if (curdir != null)
                            {
                                archiveDs.Add(new ArchiveSettings
                                {
                                    dir = fol,
                                    dest = curdir.dest,
                                    check = curdir.check,
                                    sourceDir = parentFolder
                                });
                            }
                            else
                            {
                                archiveDs.Add(new ArchiveSettings
                                {
                                    dir = fol,
                                    dest = null,
                                    check = false,
                                    sourceDir = parentFolder
                                });
                            }
                        }
                    }
                    catch
                    {
                        // If directory access fails, just continue with empty grid
                    }
                }
                
                grdArchive.DataSource = archiveDs;

                if (!string.IsNullOrEmpty ( Properties.Settings.Default.ArchiveDestName )) {
                    txtFolderArchiveDest.Text = Properties.Settings.Default.ArchiveDestName;
                }
                if (!string.IsNullOrEmpty ( Properties.Settings.Default.ArchiveSourceName )) {
                    txtFolderArchiveParent.Text = Properties.Settings.Default.ArchiveSourceName;
                }

                var folderSettings = _configurationService.GetFolderSettings();

                // checking if the user has some saved checked folder list
                // string countFolderSettings = Settings.Default.selectedFolders;
                //string countFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().selectedFolders;
                //if (!String.IsNullOrEmpty(countFolderSettings))
                //{
                //    string[] folders = countFolderSettings.Split(',');
                //    for (int i = 0; i <= (FileList.Items.Count - 1); i++)
                //    {
                //        foreach (var fol in folders)
                //        {
                //            if (FileList.Items[i].ToString() == fol)
                //            {
                //                FileList.SetItemChecked(i, true);
                //            }
                //        }

                //    }
                //}
                //string duplicatesFolderSettings = Settings.Default.duplicatesFolders;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.reportsDestName))
                {
                    txtReportDest.Text = Properties.Settings.Default.reportsDestName;
                }
                var reportFoldersSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().reportsFolders;
                if (!String.IsNullOrEmpty ( reportFoldersSettings )) {
                    var folders = reportFoldersSettings.Split ( ',' );
                    for (var i = 0; i <= ( reportFolders.Items.Count - 1 ); i++) {
                        foreach (var fol in folders) {
                            if (reportFolders.Items [i].ToString () == fol) {
                                reportFolders.SetItemChecked ( i, true );
                            }
                        }

                    }
                }


                _duplicateManagementService.LoadCheckedDuplicateFolders(DuplicateFolders);
                _fileNameManagementService.LoadFileNameFolders(filenames);
                _excelService.LoadExcelFolders(dirList);

                var deletedFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().deleteFolders;
                if (!String.IsNullOrEmpty ( deletedFolderSettings )) {
                    var folders = deletedFolderSettings.Split ( ',' );
                    for (var i = 0; i <= ( DelList.Items.Count - 1 ); i++) {
                        foreach (var fol in folders) {
                            if (DelList.Items [i].ToString () == fol) {
                                DelList.SetItemChecked ( i, true );
                            }
                        }

                    }
                }
                if (nDaysToDelete.Value < 0)
                {
                    MessageBox.Show("מספר הימים למחיקה קטן מאפס");
                    return;
                }
                var dys = Properties.Settings.Default.DaysToDelete;
                nDaysToDelete.Value = dys;
                
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "Main method Error"
                };
                logger.Log ( eventInfo );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for the Close button.
        /// Closes the main application form.
        /// </summary>
        
        //private bool checkFolder(string folder)
        //{
        //    int n;
        //    if (int.TryParse(folder, out n))
        //    {
        //        if (n < 100)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        /// <summary>
        /// Checks <c>actions.json</c> for the named action. Logs a skip message when disabled.
        /// Returns true when the action is enabled (or the file/key is missing).
        /// </summary>
        private bool IsActionAllowed(string actionName)
        {
            if (_actionConfigService == null)
            {
                return true;
            }
            if (_actionConfigService.IsActionEnabled(actionName))
            {
                return true;
            }
            _loggingService?.LogInfo($"Skipping action '{actionName}' — disabled in actions.json", "Form1");
            return false;
        }

        /// <summary>
        /// Gate for explicit user-clicked buttons. Returns true if the action is allowed.
        /// When disabled, shows a Hebrew RTL message so the user knows the click was deliberately ignored.
        /// </summary>
        private bool IsButtonActionAllowed(string actionName)
        {
            if (IsActionAllowed(actionName))
            {
                return true;
            }

            MessageBox.Show(
                $"הפעולה '{actionName}' מנוטרלת בקובץ ההגדרות actions.json",
                "פעולה מנוטרלת",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return false;
        }

        // Helper methods for service callbacks
        private void UpdateProgressBar(int value)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new Action<int>(UpdateProgressBar), value);
                return;
            }
            progressBar1.Value = value;
            Application.DoEvents();
        }

        private void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(LogMessage), message);
                return;
            }
            txtLog.AppendText(message + Environment.NewLine);
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Event handler for the Start button on the Reports tab.
        /// Executes the complete file processing workflow in sequence.
        /// </summary>
        /// <remarks>
        /// Workflow sequence:
        /// 1. Validates destination folder is set
        /// 2. Copies files based on configuration
        /// 3. Splits folders according to split settings
        /// 4. Fixes file names (Migdal format)
        /// 5. Counts files in specified directories
        /// 6. Sets Excel file names based on content
        /// 7. Sets report names according to rules
        /// 8. Enables mail button for sending reports
        ///
        /// Any errors encountered during processing are logged and a notification is shown.
        /// </remarks>
        private void bStart_Click(object sender, EventArgs e)
        {
            // Validate destination folder is configured
            if (string.IsNullOrEmpty ( txtReportDest.Text )) {
                MessageBox.Show ( "שם תיקיית היעד לשינוי שמות לדוחות לא קיים" );
                return;
            }

            // Hide summary box and prepare UI for processing
            boxSummary.Visible = false;
            Application.DoEvents();

            // Execute file processing workflow (each step gated by actions.json).
            // Each action is now independent — FixDuplicates and the non-Migdal FixFileNames
            // were previously nested inside CountFiles and are now hoisted to their own gates.
            var anyRan = false;
            if (IsActionAllowed(ActionNames.CopyFiles))       { CopyFiles();        anyRan = true; }
            if (IsActionAllowed(ActionNames.SplitFolders))    { SplitFolders();     anyRan = true; }
            if (IsActionAllowed(ActionNames.FixFileNames))    { FixFileNames(true); anyRan = true; }
            if (IsActionAllowed(ActionNames.CountFiles))      { CountFiles();       anyRan = true; }
            if (IsActionAllowed(ActionNames.FixDuplicates))   { FixDuplicates();    anyRan = true; }
            if (IsActionAllowed(ActionNames.FixFileNames))    { FixFileNames(false); anyRan = true; }
            if (IsActionAllowed(ActionNames.SetExcelNames))   { SetExcelNames();    anyRan = true; }
            if (IsActionAllowed(ActionNames.SetReportsNames)) { SetReportsNames();  anyRan = true; }

            // Only enable mail when something actually ran (and SendEmails is allowed)
            btnMail.Enabled = anyRan && _actionConfigService.IsActionEnabled(ActionNames.SendEmails);

            // Notify user of completion status
            if (!anyRan)
            {
                MessageBox.Show(
                    "לא בוצעו פעולות — כל הפעולות מנוטרלות בקובץ actions.json",
                    "אין פעולות לביצוע",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
            else if (hasErrors)
            {
                MessageBox.Show("הפעולה הסתיימה עם בעיותץ בדוק קובץ לוג");
            }
            else
            {
                MessageBox.Show("הפעולה הסתיימה בהצלחה");
            }
        }

        //private void bMigdal_Click(object sender, EventArgs e)
        //{
        //    boxSummary.Visible = false;
        //    Application.DoEvents();
        //    FixFileNames(true);
        //    btnMail.Enabled = true;
        //}

        /// <summary>
        /// Event handler for the Delete button.
        /// Initiates the file deletion process based on age criteria.
        /// </summary>
        private void btnDel_Click ( object sender, EventArgs e )
        {
            if (!IsButtonActionAllowed(ActionNames.DeleteFiles)) return;
            DelteFiles();
        }

        /// <summary>
        /// Event handler for form resize.
        /// Invalidates the form to trigger a repaint with gradient background.
        /// </summary>
        private void Form1_Resize ( object sender, EventArgs e )
        {
            Invalidate ();
        }

        /// <summary>
        /// Event handler for Move/Copy radio button change.
        /// Updates the operation mode flag.
        /// </summary>
        /// <param name="sender">The radio button control.</param>
        /// <param name="e">Event arguments.</param>
        private void rMove_CheckedChanged ( object sender, EventArgs e )
        {
            _isMove = rMove.Checked;
        }

        /// <summary>
        /// Paints the form background with a vertical gradient effect.
        /// </summary>
        /// <param name="sender">The form being painted.</param>
        /// <param name="e">Paint event arguments with graphics context.</param>
        private void Form1_Paint ( object sender, PaintEventArgs e )
        {
            try {
                // Create vertical gradient from WhiteSmoke to SteelBlue
                using (var brush = new LinearGradientBrush ( ClientRectangle,
                                                                       Color.WhiteSmoke,
                                                                       Color.SteelBlue,
                                                                       90F )) {
                    e.Graphics.FillRectangle ( brush, ClientRectangle );
                }
            }
            catch {
                // Silently ignore painting errors
            }
        }

        /// <summary>
        /// Event handler for the Send Mail button.
        /// Processes files from configured directories and sends them via Outlook email.
        /// </summary>
        /// <remarks>
        /// Email Processing Workflow:
        /// 1. Loads email directory configuration from JSON
        /// 2. Iterates through each configured directory
        /// 3. Finds all TIFF and PDF files in subdirectories
        /// 4. Groups files based on naming conventions and mail method settings:
        ///    - "בודד-זהה" (Single-Identical): Each file sent separately
        ///    - "בודד-קצר" (Single-Short): Files with shortened names
        ///    - "איחוד-קצר" (Merge-Short): Multiple files merged
        ///    - "איחוד שמי" (Merge-Named): Files merged by name pattern
        ///    - "איחוד לפי דוח" (Merge-By-Report): Files merged by report criteria
        /// 5. Creates/merges PDFs as needed
        /// 6. Sends emails via Outlook with attachments
        /// 7. Archives sent files
        /// 8. Updates progress bar during processing
        ///
        /// Configuration is loaded from shared JSON file or grid data source.
        /// Files are only processed from "1" subdirectories or valid date-formatted folders.
        /// Sleep delay between emails prevents server overload.
        /// </remarks>
        private void btnMail_Click ( object sender, EventArgs e )
        {
            if (!IsButtonActionAllowed(ActionNames.SendEmails)) return;
            try
            {
                // Initialize progress indicators
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                lblProgressMessage.Text = "שולח מיילים";  // "Sending emails"
                lblProgressMessage.Visible = true;
                Application.DoEvents();

                var dirSettings = _configurationService.GetEmailDirSettings();
                var validDirs = dirSettings.Where(w => !string.IsNullOrEmpty(w.email));
                
                if (!validDirs.Any())
                {
                    MessageBox.Show("אין מיילים מוגדרים לשליחה");
                    return;
                }

                var result = _emailService.SendEmails(dirSettings, _sleep);

                _loggingService.LogInfo($"Mail batch result: attempted={result.Attempted}, succeeded={result.Succeeded}, failed={result.FailedRecipients.Count}, skipped={result.SkippedTotal}");

                string message;
                MessageBoxIcon icon;
                if (result.AllSucceeded)
                {
                    message = $"המיילים נשלחו בהצלחה ({result.Succeeded})";
                    icon = MessageBoxIcon.Information;
                }
                else if (result.Succeeded == 0 && result.FailedRecipients.Count == 0 && result.SkippedTotal > 0)
                {
                    message = $"לא נשלחו מיילים. דולגו: {result.SkippedTotal} — בדוק קובץ הלוג";
                    icon = MessageBoxIcon.Warning;
                }
                else
                {
                    message = string.Format(
                        "המיילים נשלחו: {0} הצליחו, {1} נכשלו, {2} דולגו — בדוק קובץ הלוג",
                        result.Succeeded,
                        result.FailedRecipients.Count,
                        result.SkippedTotal);
                    icon = MessageBoxIcon.Warning;
                }

                MessageBox.Show(message, "שליחת מיילים", MessageBoxButtons.OK, icon,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

                btnMail.Enabled = false;
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError("btnMail_Click", ex, "Mail send aborted with unexpected exception in btnMail_Click");
                MessageBox.Show(
                    $"המיילים לא נשלחו בהצלחה: {ex.Message}",
                    "שליחת מיילים",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
        }

        /// <summary>
        /// Event handler for email search clear button click.
        /// Clears the search filter and restores the full email list.
        /// </summary>
        private void btnEmailSearchClear_Click(object sender, EventArgs e)
        {
            txtSearchEmail.Text = string.Empty;
            RestoreFullEmailList();
        }

        /// <summary>
        /// Event handler for real-time filtering as the user types in the search box.
        /// </summary>
        private void TxtSearchEmail_TextChanged(object sender, EventArgs e)
        {
            FilterEmailGrid(txtSearchEmail.Text);
        }

        /// <summary>
        /// Filters the email grid to show only rows where the email column contains the search text.
        /// </summary>
        /// <param name="searchText">The text to search for in the email column.</param>
        private void FilterEmailGrid(string searchText)
        {
            if (_originalEmailList == null)
                return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                RestoreFullEmailList();
                return;
            }

            // Filter the list by email column (case-insensitive contains)
            var filteredList = _originalEmailList
                .Where(item => !string.IsNullOrEmpty(item.email) &&
                              item.email.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = filteredList;
        }

        /// <summary>
        /// Restores the full unfiltered email list to the grid.
        /// </summary>
        private void RestoreFullEmailList()
        {
            if (_originalEmailList == null)
                return;

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = _originalEmailList;
        }
        /// <summary>
        /// Event handler for when a cell edit is completed in the email grid.
        /// Validates email addresses when the email column is edited.
        /// </summary>
        private void dataGridView1_CellEndEdit ( object sender, DataGridViewCellEventArgs e )
        {
            if (e.ColumnIndex > 0 && dataGridView1.Columns [e.ColumnIndex] != null && dataGridView1.Columns [e.ColumnIndex].Name == "check") {
                return;
            }

            var mail = dataGridView1.Rows [e.RowIndex].Cells ["email"].Value == null ? string.Empty : dataGridView1.Rows [e.RowIndex].Cells ["email"].Value.ToString ();
            var fol = dataGridView1.Rows [e.RowIndex].Cells ["dir"].Value == null ? string.Empty : dataGridView1.Rows [e.RowIndex].Cells ["dir"].Value.ToString ();
            var method = dataGridView1.Rows [e.RowIndex].Cells ["method"].Value == null ? string.Empty : dataGridView1.Rows [e.RowIndex].Cells ["method"].Value.ToString ();

            // Validate email address(es) if the email column was edited and contains data.
            // Accepts one or more addresses separated by comma or semicolon.
            if (dataGridView1.Columns[e.ColumnIndex].Name == "email" && !string.IsNullOrWhiteSpace(mail))
            {
                if (!EmailValidator.IsValidEmailList(mail, out string emailError))
                {
                    // Log validation failure
                    _loggingService.LogValidationFailure("EmailValidation", mail, emailError);

                    // Show error message to user
                    MessageBox.Show(
                        $"כתובת דואר אלקטרוני לא תקינה: {emailError}\n\nהכתובת: {mail}",
                        "שגיאת אימות",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

                    // Keep focus on the cell to allow correction
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells["email"];
                    dataGridView1.BeginEdit(true);
                    return;
                }

                _loggingService.LogInfo($"Email validation successful: {mail}", "EmailValidation");
            }

            _emailService.HandleGridCellEndEdit(e.RowIndex, mail, fol, method);
        }

        private void dataGridView1_CellContentClick ( object sender, DataGridViewCellEventArgs e )
        {
            dataGridView1.CommitEdit ( DataGridViewDataErrorContexts.Commit );
        }

        private void dataGridView1_CellValueChanged ( object sender, DataGridViewCellEventArgs e )
        {
            if (e.RowIndex == -1)
                return;

            if (dataGridView1.Columns [e.ColumnIndex].Name != "check") {
                return;
            }
            var check = dataGridView1 [e.ColumnIndex, e.RowIndex].Value.ToString();
            var ddir = dataGridView1 [0, e.RowIndex].Value?.ToString ();
            
            _emailService.HandleGridCellValueChanged(e.RowIndex, check, ddir);
        }
        private void grdFolderSplit_CellContentClick ( object sender, DataGridViewCellEventArgs e )
        {
            if (grdFolderSplit.Columns [e.ColumnIndex].Name == "chbdirs")
            {
                grdFolderSplit.Rows[e.RowIndex].Cells[2].Value = Convert.ToBoolean(grdFolderSplit.Rows[e.RowIndex].Cells[2].EditedFormattedValue) != true;
                var check = (bool)grdFolderSplit.Rows [e.RowIndex].Cells ["chbdirs"].Value;
                var ddir = grdFolderSplit.Rows [e.RowIndex].Cells ["cdir"].Value == null ? string.Empty : grdFolderSplit.Rows [e.RowIndex].Cells ["cdir"].Value.ToString ();
                var dest = grdFolderSplit.Rows [e.RowIndex].Cells ["dest"].Value == null ? string.Empty : grdFolderSplit.Rows [e.RowIndex].Cells ["dest"].Value.ToString ();
                var dirSettings = GetSplitSettings();
                var dirObj = dirSettings.Find ( f => f.dir == ddir );
                if (dirObj != null) {
                    dirObj.check = check;
                }
                else {
                    dirSettings.Add ( new SplitSettings
                    {

                        dir = ddir,
                        dest = dest,
                        check = check
                    } );
                }
                setSplitSettings( dirSettings );
            }
        }
        
        private void grdFolderSplit_CellClick ( object sender, DataGridViewCellEventArgs e )
        {
            if (grdFolderSplit.Columns[e.ColumnIndex].Name != "chbdirs")
            {
                var dialog = new FolderBrowserDialog ();
                if (dialog.ShowDialog () == DialogResult.OK) {
                    grdFolderSplit [e.ColumnIndex, e.RowIndex].Value = dialog.SelectedPath;
                    var check = (bool)grdFolderSplit.Rows [e.RowIndex].Cells ["chbdirs"].Value;
                    var ddir = grdFolderSplit.Rows [e.RowIndex].Cells ["cdir"].Value == null ? string.Empty : grdFolderSplit.Rows [e.RowIndex].Cells ["cdir"].Value.ToString ();
                    var dest = grdFolderSplit.Rows [e.RowIndex].Cells ["dest"].Value == null ? string.Empty : grdFolderSplit.Rows [e.RowIndex].Cells ["dest"].Value.ToString ();
                    var dirSettings = GetSplitSettings ();
                    var dirObj = dirSettings.Find ( f => f.dir == ddir );
                    if (dirObj != null) {
                        dirObj.dest = dest;
                    }
                    else {
                        dirSettings.Add ( new SplitSettings
                        {

                            dir = ddir,
                            dest = dest,
                            check = check
                        } );
                    }
                    setSplitSettings ( dirSettings );
                }
                
            }
        }
        private void gridCopy_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gridCopy.Columns[e.ColumnIndex].Name == "copyDest")
            {
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    gridCopy[e.ColumnIndex, e.RowIndex].Value = dialog.SelectedPath;
                    var check = (bool)gridCopy.Rows[e.RowIndex].Cells["chbCopy"].Value;
                    var ddir = gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value.ToString();
                    var dest = gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value.ToString();
                    var str = gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value.ToString();
                    var dirSettings = GetCopySettings();
                    var dirObj = dirSettings.Find(f => f.dir == ddir);
                    if (dirObj != null)
                    {
                        dirObj.dest = dest;
                        dirObj.str = str;
                    }
                    else
                    {
                        dirSettings.Add(new CopySettings
                        {

                            dir = ddir,
                            dest = dest,
                            check = check,
                            str = str
                        });
                    }
                    setCopySettings(dirSettings);
                }

            }
            else if (gridCopy.Columns[e.ColumnIndex].Name == "copyStr")
            {
                
                gridCopy.CurrentCell = gridCopy.Rows[e.RowIndex].Cells[e.ColumnIndex];
                gridCopy.CurrentCell.ReadOnly = false;
                gridCopy.BeginEdit(true);
                //var dialog = new OpenFileDialog();
                //if (dialog.ShowDialog() == DialogResult.OK)
                //{
                //    gridCopy[e.ColumnIndex, e.RowIndex].Value = dialog.FileName;
                //    var check = (bool)gridCopy.Rows[e.RowIndex].Cells["chbCopy"].Value;
                //    var ddir = gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value.ToString();
                //    var dest = gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value.ToString();
                //    var str = gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value.ToString();
                //    var dirSettings = GetCopySettings();
                //    var dirObj = dirSettings.Find(f => f.dir == ddir);
                //    if (dirObj != null)
                //    {
                //        dirObj.dest = dest;
                //        dirObj.str = str;
                //    }
                //    else
                //    {
                //        dirSettings.Add(new CopySettings
                //        {

                //            dir = ddir,
                //            dest = dest,
                //            check = check,
                //            str = str
                //        });
                //    }
                //    setCopySettings(dirSettings);
                //}
            }
        }
        private void gridCopy_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (gridCopy.Columns[e.ColumnIndex].Name == "copyStr")
            {
                var check = (bool)gridCopy.Rows[e.RowIndex].Cells["chbCopy"].Value;
                var ddir = gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value.ToString();
                var dest = gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value.ToString();
                var str = gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value.ToString();
                var dirSettings = GetCopySettings();
                var dirObj = dirSettings.Find(f => f.dir == ddir);
                if (dirObj != null)
                {
                    dirObj.dest = dest;
                    dirObj.str = str;
                }
                else
                {
                    dirSettings.Add(new CopySettings
                    {

                        dir = ddir,
                        dest = dest,
                        check = check,
                        str = str
                    });
                }
                setCopySettings(dirSettings);
            }
        }
        private void gridCopy_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gridCopy.Columns[e.ColumnIndex].Name == "chbCopy")
            {
                gridCopy.Rows[e.RowIndex].Cells[2].Value = Convert.ToBoolean(gridCopy.Rows[e.RowIndex].Cells[2].EditedFormattedValue) != true;
                var check = (bool)gridCopy.Rows[e.RowIndex].Cells["chbCopy"].Value;
                var ddir = gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDirs"].Value.ToString();
                var dest = gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyDest"].Value.ToString();
                var str = gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value == null ? string.Empty : gridCopy.Rows[e.RowIndex].Cells["copyStr"].Value.ToString();
                var dirSettings = GetCopySettings();
                var dirObj = dirSettings.Find(f => f.dir == ddir);
                if (dirObj != null)
                {
                    dirObj.check = check;
                }
                else
                {
                    dirSettings.Add(new CopySettings
                    {

                        dir = ddir,
                        dest = dest,
                        check = check,
                        str = str
                    });
                }
                setCopySettings(dirSettings);
            }
        }
        private void grdArchive_CellContentClick ( object sender, DataGridViewCellEventArgs e )
        {
            if (grdArchive.Columns [e.ColumnIndex].Name == "achbdirs") {
                grdArchive.Rows [e.RowIndex].Cells [3].Value = Convert.ToBoolean ( grdArchive.Rows [e.RowIndex].Cells [3].EditedFormattedValue ) != true;
                var vcheck = (bool)grdArchive.Rows [e.RowIndex].Cells ["achbdirs"].Value;
                var ddir = grdArchive.Rows [e.RowIndex].Cells ["adir"].Value == null ? string.Empty : grdArchive.Rows [e.RowIndex].Cells ["adir"].Value.ToString ();
                var vdest = grdArchive.Rows [e.RowIndex].Cells ["adest"].Value == null ? string.Empty : grdArchive.Rows [e.RowIndex].Cells ["adest"].Value.ToString ();
                var dirSettings = GetArchiveSettings();
                var dirObj = dirSettings.Find ( f => f.dir == ddir );
                if (dirObj != null) {
                    dirObj.check = vcheck;
                }
                else {
                    dirSettings.Add ( new ArchiveSettings
                    {

                        dir = ddir,
                        dest = vdest,
                        check = vcheck,
                        sourceDir = txtFolderArchiveParent.Text
                    } );
                }
                setArchiveSettings( dirSettings );
                
                // Refresh the grid to ensure UI consistency
                RefreshGrdArchive();
            }
        }
        private void grdArchive_CellClick ( object sender, DataGridViewCellEventArgs e )
        {
            if (grdArchive.Columns[e.ColumnIndex].Name != "achbdirs")
            {
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedFolder = Path.GetFileName(dialog.SelectedPath);
                    grdArchive[e.ColumnIndex, e.RowIndex].Value = selectedFolder;
                    var check = (bool) grdArchive.Rows[e.RowIndex].Cells["achbdirs"].Value;
                    var ddir = grdArchive.Rows[e.RowIndex].Cells["adir"].Value == null
                        ? string.Empty
                        : grdArchive.Rows[e.RowIndex].Cells["adir"].Value.ToString();
                    var dest = grdArchive.Rows[e.RowIndex].Cells["adest"].Value == null
                        ? string.Empty
                        : grdArchive.Rows[e.RowIndex].Cells["adest"].Value.ToString();
                    var dirSettings = GetArchiveSettings();
                    var dirObj = dirSettings.Find(f => f.dir == ddir && f.sourceDir == txtFolderArchiveParent.Text);
                    if (dirObj != null)
                    {
                        dirObj.dest = dest;
                    }
                    else
                    {
                        dirSettings.Add(new ArchiveSettings
                        {

                            dir = ddir,
                            dest = dest,
                            sourceDir = txtFolderArchiveParent.Text,
                            check = check
                        });
                    }
                    setArchiveSettings(dirSettings);
                    
                    // Refresh the grid to ensure UI consistency
                    RefreshGrdArchive();
                }
            }
        }

       

        private void grdCount_CellContentClick ( object sender, DataGridViewCellEventArgs e )
        {
            grdCount.CommitEdit ( DataGridViewDataErrorContexts.Commit );
        }

        private void grdCount_CellValueChanged ( object sender, DataGridViewCellEventArgs e )
        {
            if (e.RowIndex == -1)
                return;

            var columnName = grdCount.Columns[e.ColumnIndex].Name;
            var value = grdCount.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            
            // Handle checkbox column changes
            if (columnName == "chb") {
                var check = (bool)value;
                var dirSettings = _fileCountService.GetCountSettings();
                var ddir = grdCount.Rows [e.RowIndex].Cells ["dirs"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["dirs"].Value.ToString ();
                var method = grdCount.Rows [e.RowIndex].Cells ["methods"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["methods"].Value.ToString ();
                var dirObj = dirSettings.Find ( f => f.dir == ddir );
                if (dirObj != null) {
                    dirObj.check = check;
                }
                else {
                    dirSettings.Add ( new CountSettings
                    {
                        dir = ddir,
                        method = method,
                        check = check
                    } );
                }
                _fileCountService.SetCountSettings(dirSettings);
            }
            // Handle methods column changes
            else if (columnName == "methods") {
                var method = value == null ? string.Empty : value.ToString();
                var fol = grdCount.Rows [e.RowIndex].Cells ["dirs"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["dirs"].Value.ToString ();
                
                // Update email settings
                var emailConfigList = _configurationService.GetEmailDirSettings();
                var curMailDir = emailConfigList.Find ( f => f.dir == fol );
                if (curMailDir != null) {
                    curMailDir.method = string.IsNullOrEmpty ( method ) ? null : method;
                }
                else {
                    if (!string.IsNullOrEmpty ( method )) {
                        emailConfigList.Add ( new EmailDirSettings
                        {
                            dir = fol,
                            method = method,
                            check = "איחוד-קצר",
                            icheck = 0,
                            email = null
                        } );
                    }
                }
                _configurationService.SetEmailDirSettings(emailConfigList);
                
                // Refresh dataGridView1 to reflect the method changes
                RefreshDataGridView1();
            }
        }

        /// <summary>
        /// Refreshes the email grid with fresh data from the service.
        /// Preserves the current search filter if one is active.
        /// </summary>
        private void RefreshDataGridView1()
        {
            // Get fresh data and update the original list
            _originalEmailList = _emailService.GetEmailDirSettingsForGrid(gfoldersList);

            // Re-apply the current filter if one exists
            if (!string.IsNullOrWhiteSpace(txtSearchEmail.Text))
            {
                FilterEmailGrid(txtSearchEmail.Text);
            }
            else
            {
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = _originalEmailList;
            }
        }

        private void RefreshGrdArchive()
        {
            if (string.IsNullOrEmpty(txtFolderArchiveParent.Text) || string.IsNullOrEmpty(txtFolderArchiveDest.Text))
            {
                return;
            }

            try
            {
                var dirs = Directory.GetDirectories(txtFolderArchiveParent.Text);
                var fols = new List<string>();
                foreach (var path in dirs)
                {
                    var folder = Path.GetFileName(path);
                    if (folder != null)
                    {
                        fols.Add(folder);
                    }
                }

                var archiveConfigSettings = GetArchiveSettings();
                var archiveDs = new List<ArchiveSettings>();
                var parentFolder = txtFolderArchiveParent.Text;
                
                foreach (var fol in fols)
                {
                    var curdir = archiveConfigSettings.Find(f => f.dir == fol && f.sourceDir == parentFolder);
                    if (curdir != null)
                    {
                        archiveDs.Add(new ArchiveSettings
                        {
                            dir = fol,
                            dest = curdir.dest,
                            check = curdir.check,
                            sourceDir = parentFolder
                        });
                    }
                    else
                    {
                        archiveDs.Add(new ArchiveSettings
                        {
                            dir = fol,
                            dest = null,
                            check = false,
                            sourceDir = parentFolder
                        });
                    }
                }

                grdArchive.DataSource = null;
                grdArchive.DataSource = archiveDs;
            }
            catch
            {
                // If directory access fails, just continue with empty grid
            }
        }

        private void grdCount_CellEndEdit ( object sender, DataGridViewCellEventArgs e )
        {
            if (e.ColumnIndex > 0 && grdCount.Columns [e.ColumnIndex] != null && grdCount.Columns [e.ColumnIndex].Name == "chb") {
                return;
            }
            
            var check = grdCount.Rows [e.RowIndex].Cells ["chb"].Value != null && (bool)grdCount.Rows [e.RowIndex].Cells ["chb"].Value;
            var method = grdCount.Rows [e.RowIndex].Cells ["methods"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["methods"].Value.ToString ();
            var fol = grdCount.Rows [e.RowIndex].Cells ["dirs"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["dirs"].Value.ToString ();

            _fileCountService.HandleGridCellEndEdit(e.RowIndex, method, fol, check);
        }

        private void btnBrowsDest_Click ( object sender, EventArgs e )
        {
            var dialog = new FolderBrowserDialog ();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderArchiveDest.Text = dialog.SelectedPath;
                Properties.Settings.Default.ArchiveDestName = txtFolderArchiveDest.Text;
                Properties.Settings.Default.Save ();
            }
        }

        private void btnBrowse_Click ( object sender, EventArgs e )
        {
            var dialog = new FolderBrowserDialog ();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderArchiveParent.Text = dialog.SelectedPath;
                Properties.Settings.Default.ArchiveSourceName = txtFolderArchiveParent.Text;
                Properties.Settings.Default.Save ();
                
                // Refresh the archive grid when source folder changes
                RefreshGrdArchive();
            }

        }
        private void btnArchiveStart_Click ( object sender, EventArgs e )
        {
            SetArchive ();
        }
        private void SetArchive()
        {
            if (string.IsNullOrEmpty(txtFolderArchiveParent.Text) || string.IsNullOrEmpty(txtFolderArchiveDest.Text))
            {
                MessageBox.Show("יש להכניס  תקיית המקור ותקיית היעד");
                return;
            }
            
            try
            {
                Properties.Settings.Default.ArchiveSourceName = txtFolderArchiveParent.Text;
                Properties.Settings.Default.ArchiveDestName = txtFolderArchiveDest.Text;
                Properties.Settings.Default.Save();

                // Use the helper method to refresh the grid
                RefreshGrdArchive();
            }
            catch
            {
                MessageBox.Show("Path does not exist, or you do not have permission to query it.");

                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Info,
                    Message = "Path does not exist, or you do not have permission to query it."
                };
                logger.Log(eventInfo);
                return;
            }
        }

        private void btnArchive_Click ( object sender, EventArgs e )
        {
            if (!IsButtonActionAllowed(ActionNames.Archive)) return;

            try {
                var parentPath = txtFolderArchiveParent.Text;
                var destPath = txtFolderArchiveDest.Text;
                if (string.IsNullOrEmpty ( parentPath ) || string.IsNullOrEmpty ( destPath )) {
                    MessageBox.Show ( "יש להכניס תקיית מקור ותקיית יעד" );
                    return;
                }
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                lblProgressMessage.Text = "מעביר לארכיון";
                lblProgressMessage.Visible = true;
                Application.DoEvents ();
                var archiveSettings = GetArchiveSettings ();
                var currentSettings = archiveSettings.Where ( f => f.sourceDir == parentPath && f.check );
                var index = 0;
                foreach (var asettings in currentSettings) {
                    index++;
                    var pbIncrement = !currentSettings.Any () ? 100 : index / currentSettings.Count ();
                    var dVal = progressBar1.Value + pbIncrement;
                    var val = Convert.ToInt32 ( dVal );
                    if (val > 100) {
                        val = 100;
                    }
                    progressBar1.Value = val;
                    Application.DoEvents ();
                    if (!string.IsNullOrEmpty ( asettings.dest )) {
                        var dirs = Directory.GetDirectories ( Path.Combine ( parentPath, asettings.dir ) );
                        foreach (var dirPath in dirs) {
                            var dir = Path.GetFileName ( dirPath );
                            if (dir.Length > 2 && Regex.IsMatch ( dir, @"\d" )) {
                                var midPath = Path.Combine ( destPath, asettings.dest );
                                if (!Directory.Exists ( midPath )) {
                                    Directory.CreateDirectory ( midPath );
                                }
                                var dirDest = Path.Combine ( midPath, dir );
                                if (!Directory.Exists(dirDest))
                                {
                                    Directory.CreateDirectory(dirDest);
                                }
                                //Directory.Move ( dirPath, dirDest );
                                foreach (string ddir in Directory.GetDirectories ( dirPath, "*", SearchOption.AllDirectories )) {
                                    Directory.CreateDirectory ( Path.Combine ( dirDest, ddir.Substring ( dirPath.Length + 1 ) ) );
                                }

                                foreach (string file_name in Directory.GetFiles ( dirPath, "*", SearchOption.AllDirectories )) {
                                    File.Move ( file_name, Path.Combine ( dirDest, file_name.Substring ( dirPath.Length + 1 ) ) );
                                }
                                Directory.Delete ( dirPath, true );
                            }
                        }
                    }
                }
                progressBar1.Value = 100;
                Application.DoEvents ();
                MessageBox.Show ( "העברת תקיות לארכיון התבצעה בהצלחה" );
            }
            catch (System.Exception ex) {

                MessageBox.Show ( "העברת תקיות לארכיון נכשלה - " + " " + ex.Message );
            }

        }
        private void SetSelectedDeletedFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            var items = String.Empty;
            foreach (var checkedItem in checkedItems) {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }




            var folderSettings = _configurationService.GetFolderSettings();
            if (folderSettings.Count > 0) {
                folderSettings.First ().deleteFolders = items.TrimEnd ( ',' );
            }
            else {
                folderSettings.Add ( new FolderSettings
                {
                    deleteFolders = items.TrimEnd ( ',' ),
                    reportsFolders = string.Empty,
                    fileNamesFolders = string.Empty,
                    selectedFolders = string.Empty,
                    duplicatesFolders = string.Empty
                } );
            }
            _configurationService.SetFolderSettings(folderSettings);
        }

        private void SetSelectedReportFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            var items = String.Empty;
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }

            
            

            var folderSettings = _configurationService.GetFolderSettings();
            if (folderSettings.Count > 0) {
                folderSettings.First ().reportsFolders = items.TrimEnd ( ',' );
            }
            else {
                folderSettings.Add ( new FolderSettings
                {
                    duplicatesFolders = string.Empty,
                    reportsFolders = items.TrimEnd ( ',' ),
                    fileNamesFolders = string.Empty,
                    selectedFolders = string.Empty,
                    deleteFolders = string.Empty
                } );
            }
            _configurationService.SetFolderSettings(folderSettings);
            //Settings.Default.duplicatesFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
        }

        private void SetSelectedExcelFolders ( CheckedListBox.CheckedItemCollection checkedItems )
        {
            var itemsList = checkedItems.Cast<string>().ToList();
            _excelService.SetSelectedExcelFolders(itemsList);
        }

        private void SetSelectedFileNameFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            var itemsList = checkedItems.Cast<string>().ToList();
            _fileNameManagementService.SetSelectedFileNameFolders(itemsList);
        }
        
        private List<FileCount> SetSelectedItems(List<CountSettings> checkedItems, bool useProgressBar)
        {
            // Save selected folders to settings
            _fileCountService.SaveSelectedFoldersToSettings(checkedItems);
            
            // Use the service to count files
            return _fileCountService.CountFilesInDirectories(checkedItems, useProgressBar);
        }


        private void CopyFiles()
        {
            progressBar1.Value = 0;
            lblProgressMessage.Text = "משכפל קבצים";
            lblProgressMessage.Visible = true;
            Application.DoEvents();

            string destDir = null, destFile = null, curdir = null, sFile = null;

            var copySettings = GetCopySettings();

            var checkedItems = copySettings.FindAll(f => f.check);


            if (checkedItems.Count == 0)
            {
                return;
            }

            try
            {
                foreach (var checkedItem in checkedItems)
                {
                    // get the base path of each folder
                    var basePath = Path.Combine(_path, checkedItem.dir);
                    var destPath = checkedItem.dest;
                    var str = checkedItem.str;
                    var availDirs = Directory.GetDirectories(basePath);

                    if (availDirs.Length > 0)
                    {

                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (var cd in availDirs)
                        {
                            curdir = cd;
                            txtLog.AppendText("בודק תיקיה - " + curdir + Environment.NewLine);
                            var checkdir = Path.GetFileNameWithoutExtension(curdir);
                            if (checkdir == null || checkdir.Length > 2)
                            {
                                continue;
                            }
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }
                            destDir = Path.Combine(destPath, checkdir);
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }
                            var files = Directory.GetFiles(curdir, "*.*", SearchOption.TopDirectoryOnly);
                            if (!files.Any())
                            {
                                continue;
                            }
                            var counter = 1;
                            foreach (var file in files)
                            {
                                counter++;
                                var fname = Path.GetFileName(file);
                                sFile = null;
                                if (fname == null)
                                {
                                    continue;
                                }
                                if(fname.Contains(str))
                                {
                                    try
                                    {
                                        destFile = Path.Combine(destDir, fname);
                                        sFile = Path.Combine(curdir, fname);

                                        if (File.Exists(destFile))
                                        {
                                            MessageBox.Show("שיכפול קבצים - שם קובץ כפול - " + destFile);
                                            LogEventInfo eventInfo = new LogEventInfo
                                            {
                                                Level = LogLevel.Info,

                                                Message = "שיכפול קבצים - שם קובץ כפול - " + destFile
                                            };
                                            logger.Log(eventInfo);
                                            hasErrors = true;
                                            continue;
                                        }
                                        File.Copy(sFile, destFile);
                                        txtLog.AppendText("מעביר קובץ - " + sFile + Environment.NewLine);
                                        Application.DoEvents();
                                        Thread.Sleep(100);
                                    }
                                    catch (System.Exception e)
                                    {
                                        txtLog.AppendText("נכשל בהעברת קובץ " + sFile + "----" + e.Message +
                                                          Environment.NewLine);

                                        var props =
                                            new
                                            {
                                                method = "CopyFiles",
                                                destDir = destDir,
                                                destFile = destFile,
                                                curdir = curdir,
                                                curFile = sFile,
                                                str = str
                                            };
                                        LogEventInfo eventInfo = new LogEventInfo
                                        {
                                            Level = LogLevel.Error,
                                            Exception = e,
                                            Message = "נכשל בהעברת קובץ",
                                            Properties = { { "Files", props } }
                                        };
                                        logger.Log(eventInfo);
                                        hasErrors = true;

                                    }

                                    var val = progressBar1.Value + (counter / files.Length) * 100;
                                    if (val > 100)
                                    {
                                        val = 100;
                                    }
                                    progressBar1.Value = val;
                                    Application.DoEvents();
                                }

                            }

                        }
                        progressBar1.Value = 100;
                        Application.DoEvents();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("תקלה בשיכפול קבצים - " + ex.Message);

                var props =
                    new
                    {
                        destDir = destDir,
                        destFile = destFile,
                        curdir = curdir,
                        curFile = sFile
                    };
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "תקלה בשיכפול קבצים",
                    Properties = { { "Files", props } }
                };
                logger.Log(eventInfo);
                hasErrors = true;
            }
        }
        private void SplitFolders()
        {
            progressBar1.Value = 0;
            lblProgressMessage.Text = "מפצל תיקיות";
            lblProgressMessage.Visible = true;
            Application.DoEvents ();

            string destDir = null, destFile = null, curdir = null, sFile = null;

            var splitSettings = GetSplitSettings();

            var checkedItems = splitSettings.FindAll ( f => f.check );


            if (checkedItems.Count == 0) {
                return;
            }
            try
            {
                foreach (var checkedItem in checkedItems)
                {
                    // get the base path of each folder
                    var basePath = Path.Combine(_path, checkedItem.dir);
                    var destPath = checkedItem.dest;
                    var availDirs = Directory.GetDirectories(basePath);

                    if (availDirs.Length > 0)
                    {

                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (var cd in availDirs)
                        {
                            curdir = cd;
                            txtLog.AppendText("בודק תיקיה - " + curdir + Environment.NewLine);
                            var checkdir = Path.GetFileNameWithoutExtension(curdir);
                            if (checkdir == null || checkdir.Length > 2)
                            {
                                continue;
                            }
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }
                            destDir = Path.Combine(destPath, checkdir);
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }
                            var files = Directory.GetFiles(curdir, "*.*", SearchOption.TopDirectoryOnly);
                            if (!files.Any())
                            {
                                continue;
                            }
                            var counter = 1;
                            foreach (var file in files)
                            {
                                counter++;
                                var fname = Path.GetFileName(file);
                                sFile = null;
                                if (fname == null)
                                {
                                    continue;
                                }

                                if (IsContain888(fname))
                                {
                                    try
                                    {
                                        destFile = Path.Combine(destDir, fname);
                                        sFile = Path.Combine(curdir, fname);
                                        if (File.Exists(destFile))
                                        {
                                            MessageBox.Show("פיצול תיקיות - שם קובץ כפול - " + destFile);
                                            LogEventInfo eventInfo = new LogEventInfo
                                            {
                                                Level = LogLevel.Info,
                                                
                                                Message = "פיצול תיקיות - שם קובץ כפול - " + destFile
                                            };
                                            logger.Log ( eventInfo );
                                            hasErrors = true;
                                            continue;
                                        }
                                        File.Move(sFile, destFile);
                                        txtLog.AppendText("מעביר קובץ - " + sFile + Environment.NewLine);
                                        Application.DoEvents();
                                        Thread.Sleep(100);
                                    }
                                    catch (System.Exception e)
                                    {
                                        txtLog.AppendText("נכשל בהעברת קובץ " + sFile + "----" + e.Message +
                                                          Environment.NewLine);

                                        var props =
                                            new
                                            {
                                                method = "SplitFolders",
                                                destDir = destDir,
                                                destFile = destFile,
                                                curdir = curdir,
                                                curFile = sFile
                                            };
                                        LogEventInfo eventInfo = new LogEventInfo
                                        {
                                            Level = LogLevel.Error,
                                            Exception = e,
                                            Message = "נכשל בהעברת קובץ",
                                            Properties = { { "Files", props } }
                                        };
                                        logger.Log ( eventInfo );
                                        hasErrors = true;

                                    }

                                }
                                if (fname.Contains("777"))
                                {
                                    if (File.Exists(file))
                                    {
                                        File.Delete(file);
                                        continue;
                                    }
                                }
                                var val = progressBar1.Value + (counter / files.Length) * 100;
                                if (val > 100)
                                {
                                    val = 100;
                                }
                                progressBar1.Value = val;
                                Application.DoEvents();

                            }
                            var destFiles = Directory.GetFiles(destDir, "*.*", SearchOption.TopDirectoryOnly);
                            foreach (var df in destFiles)
                            {
                                sFile = df;
                                curdir = Path.GetDirectoryName(df);
                                var dname = Path.GetFileNameWithoutExtension(df);
                                var dext = Path.GetExtension(df);
                                string dnewName = Path.GetFileName(df);
                                if (IsContain888(dnewName))
                                {
                                    if (dname.Contains("888-"))
                                    {
                                        dnewName = dname.Replace("888-", "");
                                    }
                                    else if (dname.Contains("888_"))
                                    {
                                        dnewName = dname.Replace("888_", "");
                                    }
                                    destFile = Path.Combine(destDir, dnewName  + dext);
                                    //var dnewPath = Path.Combine(df, destFile);
                                    File.Move(df, destFile);
                                }
                               
                            }

                        }
                        progressBar1.Value = 100;
                        Application.DoEvents();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("תקלה בפיצול תיקיות - " + ex.Message);

                var props =
                    new
                    {
                        destDir = destDir,
                        destFile = destFile,
                        curdir = curdir,
                        curFile = sFile
                    };
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "תקלה בפיצול תיקיות",
                    Properties = { { "Files", props } }
                };
                logger.Log ( eventInfo );
                hasErrors = true;
            }


        }
        private bool IsContain888(string fname)
        {
            if (fname.Contains("-888-") || fname.Contains("_888_") || fname.Contains("_888-") ||
                fname.Contains("-888_") || fname.Contains("888_") || fname.Contains("_888") ||
                fname.Contains("-888") || fname.Contains("888-"))
            {
                return true;
            }
            return false;
        }
        private void CountFiles()
        {
            try
            {
                progressBar1.Value = 0;
                lblProgressMessage.Text = "סופר קבצים";
                lblProgressMessage.Visible = true;
                Application.DoEvents();

                var countsSettings = _fileCountService.GetCountSettings();
                var checkedItems = countsSettings.FindAll(f => f.check);

                if (checkedItems.Count == 0)
                {
                    return;
                }

                var list = SetSelectedItems(checkedItems, true);

                //open the grid form with the selected data.
                var grid = new ResultGrid(list.OrderBy(o=>o.FileName).ToList());
                grid.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(" Error :\r" + ex.Message);
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "CountFiles Error"
                };
                logger.Log(eventInfo);
                hasErrors = true;
            }
        }

        private void SetReportsNames()
        {
            try
            {
                string [] availDirs, allAvailDirs;
                string curdir = null;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                var isError = false;
                _numOfDuplicates = 0;
                lblProgressMessage.Text = "מתקן שמות דוחות";
                lblProgressMessage.Visible = true;
                Application.DoEvents ();
                var checkedItems = reportFolders.CheckedItems;
                if (checkedItems.Count == 0) {
                    return;
                }
                var destFolName = txtReportDest.Text;
                Properties.Settings.Default.reportsDestName = destFolName;
                Properties.Settings.Default.Save();
                // save selected items to Settings
                SetSelectedReportFolders ( checkedItems );
                var itemCount = checkedItems.Count;
                var percent = itemCount == 0 ? 100 : Convert.ToInt32 ( Math.Round ( 95.0 / itemCount, 0 ) );

                foreach (var checkedItem in checkedItems)
                {
                    //var counter = 1;
                    // get the base path of each folder
                    var basePath = Path.Combine(_path, checkedItem.ToString());
                    var dest = Path.Combine ( basePath, destFolName );
                    if (!Directory.Exists ( dest )) {
                        Directory.CreateDirectory ( dest );
                    }
                    //string allFilesPath = String.Empty;
                    //get all sub folders of base folder
                    allAvailDirs = Directory.GetDirectories(basePath);
                    availDirs = allAvailDirs.Where(w =>
                    {
                        var dn = Path.GetFileName(w);
                        return dn != null && dn.Length < 3 && dn != destFolName;
                    }).ToArray();


                    if (availDirs.Length > 0)
                    {
                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (var cd in availDirs)
                        {
                            curdir = cd;
                            var checkdir = Path.GetFileNameWithoutExtension(curdir);
                            if (checkdir == null || checkdir.Length > 2)
                            {
                                continue;
                            }
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }
                            var source = curdir;
                            var sourceName = Path.GetFileName( basePath ) + " -- " + Path.GetFileName ( source);
                            var reportFiles = Directory.GetFiles ( source, "*.*", SearchOption.TopDirectoryOnly );
                            if (!reportFiles.Any ()) {
                                continue;
                            }
                            var grouper = new List<Grouper> ();
                            txtLog.AppendText ( "מתחיל בבדיקת קבצי המקור לתקייה - " + sourceName  + Environment.NewLine );
                            txtLog.AppendText ( "נמצאו  " + reportFiles.Length + " קבצים " + Environment.NewLine );
                            Application.DoEvents ();
                            Thread.Sleep ( 100 );
                            try {
                                foreach (var file in reportFiles) {
                                    var parts = getParts ( file );
                                    var id = parts[parts.Length - 2];
                                   
                                    if (grouper.Exists ( f => f.id == id)) {
                                        grouper.Find ( f => f.id == id).files.Add ( file );
                                    }
                                    else {
                                        grouper.Add ( new Grouper
                                        {
                                            id = id,
                                            files = new List<string> { file }
                                        } );
                                    }
                                }
                            }
                            catch (System.Exception ex) {
                                txtLog.AppendText ( "בעיה נמצאה בקריאת קבצי המקור, " + ex.Message);

                                var props =
                                    new
                                    {
                                        fileName = sourceName,
                                        curdir = curdir
                                    };
                                LogEventInfo eventInfo = new LogEventInfo
                                {
                                    Level = LogLevel.Error,
                                    Exception = ex,
                                    Message = "בעיה נמצאה בקריאת קבצי המקור",
                                    Properties = { { "Files", props } }
                                };
                                logger.Log ( eventInfo );

                                
                                Application.DoEvents ();
                                isError = true;
                                hasErrors = true;
                                continue;
                            }

                            txtLog.AppendText ( "סיום בדיקת קבצי המקור לתיקיית - " + sourceName +  Environment.NewLine );
                            txtLog.AppendText ( Environment.NewLine );
                            Application.DoEvents ();

                            var max = 1;
                            var destFiles = Directory.GetFiles ( dest, "*.*", SearchOption.TopDirectoryOnly );
                            txtLog.AppendText ( "מתחיל בבדיקת קבצי היעד לתקיית - " + sourceName + Environment.NewLine );
                            Application.DoEvents ();
                            Thread.Sleep ( 100 );
                            string destFile = "";
                            try {
                                foreach (var df in destFiles)
                                {
                                    destFile = df;
                                    int num;
                                    var destParts = getParts ( destFile );
                                    if (int.TryParse ( destParts [destParts.Length - 1], out num )) {
                                        if (num > max) {
                                            max = num;
                                        }
                                    }
                                }
                                if (max > 1) {
                                    max++;
                                }
                            }
                            catch (System.Exception ex) {
                               
                                txtLog.AppendText ( "בעיה נמצאה בבדיקת קבצי היעד, " + ex.Message );

                                var props =
                                    new {
                                        fileName = destFile,
                                        curdir = curdir
                                    };
                                LogEventInfo eventInfo = new LogEventInfo
                                {
                                    Level = LogLevel.Error,
                                    Exception = ex,
                                    Message = "בעיה נמצאה בקריאת קבצי היעד",
                                    Properties = { { "Files", props } }
                                };
                                logger.Log ( eventInfo );

                                
                                Application.DoEvents ();
                                isError = true;
                                hasErrors = true;
                                continue;
                            }

                            txtLog.AppendText ( "סיום בדיקת קבצי היעד לתיקיית - " + sourceName + Environment.NewLine );
                            Application.DoEvents ();
                            Thread.Sleep ( 100 );
                            txtLog.AppendText ( Environment.NewLine );
                            txtLog.AppendText ( "מתחיל בהעברת הקבצים בתקיית - " + sourceName + Environment.NewLine );
                            Application.DoEvents ();
                            string grpFile=null, newFile=null;
                            try {
                                foreach (var grp in grouper) {
                                    var grpId = grp.id;
                                    foreach (var gf in grp.files)
                                    {
                                        grpFile = gf;
                                        
                                        //var ext = grpFile.Split ( '.' ) [1];
                                        var ext = Path.GetExtension(grpFile);
                                        var grpParts = getParts ( grpFile );
                                        grpParts [grpParts.Length - 1] = max.ToString ( "D3" );
                                        if(grpId == "00000000")
                                        {
                                            max++;
                                        }
                                        newFile = string.Empty;
                                        foreach (var grpPart in grpParts) {
                                            newFile += grpPart + "-";
                                        }
                                        newFile = newFile.TrimEnd ( '-' )  + ext;
                                        Thread.Sleep ( 50 );
                                        txtLog.AppendText ( "מעביר קובץ " + Path.GetFileName ( grpFile ) + Environment.NewLine );
                                        Application.DoEvents ();
                                        File.Move ( grpFile, Path.Combine ( dest, Path.GetFileName ( newFile ) ) );
                                    }
                                    if (grpId != "00000000")
                                    {
                                        max++;
                                    }
                                }
                            }
                            catch (System.Exception ex) {
                                txtLog.AppendText ( "בעיה בהעברת הקובץ, " + ex.Message );
                                var props =
                                    new {
                                        fileName = grpFile,
                                        destFile = newFile
                                    };
                                LogEventInfo eventInfo = new LogEventInfo
                                {
                                    Level = LogLevel.Error,
                                    Exception = ex,
                                    Message = "בעיה בהעברת הקובץ",
                                    Properties = { { "Files", props } }
                                };
                                logger.Log ( eventInfo );
                                
                                Application.DoEvents ();
                                hasErrors = true;
                                isError = true;
                            }
                            var msg = isError ? "הפעולה הסתיימה עם בעיות" : "הפעולה הסתיימה בהצלחה";
                            txtLog.AppendText ( Environment.NewLine );
                            txtLog.AppendText ( msg );
                            txtLog.AppendText ( Environment.NewLine );
                            txtLog.AppendText ( Environment.NewLine );
                            txtLog.AppendText ( Environment.NewLine );
                            Application.DoEvents ();

                        }
                    }
                    var val = progressBar1.Value + percent;
                    if (val > 100) {
                        val = 100;
                    }
                    progressBar1.Value = val;
                    Application.DoEvents ();
                }
                progressBar1.Value = 100;
                Application.DoEvents ();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show ( "Report names Error :\r" + ex.Message );
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "Report names Error"
                };
                logger.Log ( eventInfo );
                hasErrors = true;
            }
        }
        private void FixDuplicates()
        {
            try
            {
                progressBar1.Value = 0;
                _numOfDuplicates = 0;
                lblProgressMessage.Text = "מתקן כפילויות";
                Application.DoEvents();
                var checkedItems = DuplicateFolders.CheckedItems;
                if (checkedItems.Count == 0)
                {
                    return;
                }

                // save selected items to Settings
                _duplicateManagementService.SaveSelectedDuplicateFolders(checkedItems);
                // Convert CheckedItemCollection to List<string>
                var checkedItemsList = new List<string>();
                foreach (var item in checkedItems)
                {
                    checkedItemsList.Add(item.ToString());
                }

                // Use the service to fix duplicates
                _numOfDuplicates = _duplicateManagementService.FixDuplicates(checkedItemsList);

                // Update UI
                boxSummary.Visible = true;
                lbl1.Visible = true;
                lbl2.Visible = true;
                lblDuplicateNum.Visible = true;
                lblDuplicateNum.Text = _numOfDuplicates.ToString();
                Application.DoEvents();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("FixDuplicates Error :\r" + ex.Message);
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "FixDuplicates Error"
                };
                logger.Log(eventInfo);
                hasErrors = true;
            }
        }


        private void SetSelectedDuplicatesFolders ( CheckedListBox.CheckedItemCollection checkedItems )
        {
            var items = String.Empty;
            foreach (var checkedItem in checkedItems) {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }




            var folderSettings = _configurationService.GetFolderSettings ();
            if (folderSettings.Count > 0) {
                folderSettings.First ().duplicatesFolders = items.TrimEnd ( ',' );
            }
            else {
                folderSettings.Add ( new FolderSettings
                {
                    duplicatesFolders = items.TrimEnd ( ',' ),
                    fileNamesFolders = string.Empty,
                    selectedFolders = string.Empty,
                    deleteFolders = string.Empty,
                    reportsFolders = string.Empty
                } );
            }
            _configurationService.SetFolderSettings(folderSettings);
            //Settings.Default.duplicatesFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
        }
        private void FixFileNames(bool isMigdal)
        {
            try
            {
                var checkedItems = filenames.CheckedItems.Cast<string>().ToList();
                if (checkedItems.Count == 0)
                {
                    return;
                }

                _fileNameManagementService.FixFileNames(checkedItems, isMigdal);
            }
            catch (System.Exception ex)
            {
                hasErrors = true;
            }
        }

        private bool CheckFolderIfNotFirstDuplication(IEnumerable<string> files, string dir)
        {
            //checks if all the files ends with underscore and 1 or 2 digits
            //if yes than it is not the first duplication
            // and make sure all files have underscore
            var isDuplicated = false;
            var newFiles = new List<string>();
            var duplicatedFiles = new List<string>();
            var r = new Regex(@".+?[_][0-9]{1,2}$");
            //iterate the files and place all files in the right List<>
            foreach (var file in files)
            {
                var ff = Path.GetFileNameWithoutExtension(file);
                if (ff != null && !_fileService.IsThumbsInPath(ff) && ((r.Match(ff).Success || ff.Contains(LtrMark))))
                {
                    duplicatedFiles.Add(ff);
                    isDuplicated = true;
                }
                else
                {
                    if (ff != null && (!_fileService.IsThumbsInPath(ff) && !ff.Contains(LtrMark)))
                    {
                        newFiles.Add(file);
                    }
                    
                }
            }
            //if there are dulplicated files and some new files
            if (isDuplicated && newFiles.Count > 0)
            {
                foreach (var file in newFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var noExt = Path.GetFileNameWithoutExtension(file);
                    if (fileName == null || noExt == null)
                    {
                        continue;
                    }
                    //regex for file names with numbers
                    var matcher = new Regex(@"^" + noExt + "[_][0-9]{1,2}$");
                    //regex for file names with letters
                    var matcher2 = new Regex(@"^" + noExt + LtrMark + @"[ \d]{1,2}$");
                    var sameFiles = new List<string>();
                    //add matched files to list
                    sameFiles.AddRange(duplicatedFiles.FindAll(matcher.IsMatch));
                    sameFiles.AddRange(duplicatedFiles.FindAll(matcher2.IsMatch));
                    //if there are matched files add filename and a counter
                    // if not add filename and 1
                    if (sameFiles.Count > 0)
                    {
                        var miniCounter = 2;
                        var isCopy = false;
                        while (!isCopy)
                        {
                            var copyName = GetNewFileName(fileName, miniCounter);
                            if (File.Exists(Path.Combine(dir, copyName)))
                            {
                                miniCounter++;
                            }
                            else
                            {
                                isCopy = true;
                                move(file, Path.Combine(dir, copyName));
                            }
                        }
                    }
                    else
                    {
                        var newName = GetNewFileName(fileName, 1);
                        move(file, Path.Combine(dir, newName));
                    }
                }
            }
            return isDuplicated;
            

        }

        private void CopyFiles(string src, string dest)
        {
            if (_isMove)
            {
                move(src, dest);
            }
            else
            {
                copy(src, dest);
            }
        }

        private void move(string src, string dest)
        {
            try
            {
                File.SetAttributes(src, FileAttributes.Normal);
                File.Move(src, dest);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error while moving file:\r" + src + "\rTo:\r" + dest + "\rError Message:\r" + ex.Message);
            }
        }

        private void copy(string src, string dest)
        {
            try
            {
                File.Copy(src, dest);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error while copying file:\r" + src + "\rTo:\r" + dest + "\rError Message:\r" + ex.Message);
            }
        }

        private string GetNewFileName(string name, int num)
        {
            var fnwe = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            if (!string.IsNullOrEmpty(ext))
            {
                var re = new Regex(@"\d+");
                var m = re.Match(name);

                if (m.Success)
                {
                    return fnwe + "_" + num + ext;
                }


                var newname = fnwe + LtrMark + " " + num + ext;
                return newname;
            }
            
            //var splits = name.Split('.');
            //if (splits.Length == 2)
            //{
            //    var re = new Regex(@"\d+");
            //    var m = re.Match(name);

            //    if (m.Success)
            //    {
            //        return splits[0] + "_" + num + "." + splits[1];
            //    }


            //    var newname= splits[0] + LtrMark + " " + num + "." + splits[1];
            //    return newname;

            //}


            return null;
        }
        
        public static void Log ( string logMessage, TextWriter w )
        {
            w.Write ( "\r\nLog Entry : " );
            w.WriteLine ( "{0} {1}", DateTime.Now.ToLongTimeString (),
                DateTime.Now.ToLongDateString () );
            w.WriteLine ( "  :" );
            w.WriteLine ( "  :{0}", logMessage );
            w.WriteLine ( "-------------------------------" );
        }








        private void setSplitSettings ( List<SplitSettings> folSettings )
        {
            var configPath = Path.Combine ( _config, "fileManager_splitConfig.json" );

            lock (LockObject) {
                var sjson = JsonConvert.SerializeObject ( folSettings.ToArray () );
                File.WriteAllText ( configPath, sjson );
            }
        }
        private void setCopySettings(List<CopySettings> folSettings)
        {
            var configPath = Path.Combine(_config, "fileManager_CopyConfig.json");

            lock (LockObject)
            {
                var sjson = JsonConvert.SerializeObject(folSettings.ToArray());
                File.WriteAllText(configPath, sjson);
            }
        }
        private void setArchiveSettings ( List<ArchiveSettings> folSettings )
        {
            var configPath = Path.Combine ( _config, "fileManager_archiveConfig.json" );

            lock (LockObject) {
                var sjson = JsonConvert.SerializeObject ( folSettings.ToArray () );
                File.WriteAllText ( configPath, sjson );
            }
        }
        




        private List<SplitSettings> GetSplitSettings ()
        {
            var configPath = Path.Combine ( _config, "fileManager_splitConfig.json" );
            List<SplitSettings> folderSettings;
            if (File.Exists ( configPath )) {

                using (var r = new StreamReader ( configPath )) {
                    var json = r.ReadToEnd ();
                    folderSettings = JsonConvert.DeserializeObject<List<SplitSettings>> ( json );
                }
            }
            else {
                folderSettings = new List<SplitSettings> ();
            }

            return folderSettings;
        }
        private List<CopySettings> GetCopySettings()
        {
            var configPath = Path.Combine(_config, "fileManager_copyConfig.json");
            List<CopySettings> folderSettings;
            if (File.Exists(configPath))
            {

                using (var r = new StreamReader(configPath))
                {
                    var json = r.ReadToEnd();
                    folderSettings = JsonConvert.DeserializeObject<List<CopySettings>>(json);
                }
            }
            else
            {
                folderSettings = new List<CopySettings>();
            }

            return folderSettings;
        }
        private List<ArchiveSettings> GetArchiveSettings ()
        {
            var configPath = Path.Combine ( _config, "fileManager_archiveConfig.json" );
            List<ArchiveSettings> folderSettings;
            if (File.Exists ( configPath )) {

                using (var r = new StreamReader ( configPath )) {
                    var json = r.ReadToEnd ();
                    folderSettings = JsonConvert.DeserializeObject<List<ArchiveSettings>> ( json );
                }
            }
            else {
                folderSettings = new List<ArchiveSettings> ();
            }

            return folderSettings;
        }
        private List<CountSettings> GetExcelSettings ()
        {
            var configPath = Path.Combine ( _config, "fileManager_countsConfig.json" );
            List<CountSettings> folderSettings;
            if (File.Exists ( configPath )) {

                using (var r = new StreamReader ( configPath )) {
                    var json = r.ReadToEnd ();
                    folderSettings = JsonConvert.DeserializeObject<List<CountSettings>> ( json );
                }
            }
            else {
                folderSettings = new List<CountSettings> ();
            }

            return folderSettings;
        }

        private void SetExcelNames ()
        {
            try {
                var checkedItems = dirList.CheckedItems.Cast<string>().ToList();
                if (checkedItems.Count == 0) {
                    return;
                }
                
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                lblProgressMessage.Text = "מתקן שמות מאקסל";
                lblProgressMessage.Visible = true;
                Application.DoEvents ();

                SetSelectedExcelFolders ( dirList.CheckedItems );
                _excelService.SetExcelNames(checkedItems);
            }
            catch (System.Exception ex)
            {
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "SetExcelNames Error"
                };
                logger.Log(eventInfo);
                hasErrors = true;
            }
        }



        private void DelteFiles ()
        {
            string delFile = null;
            try
            {
                progressBar1.Visible = true;
                var filesToDelete = new List<string> ();
                progressBar1.Value = 0;
                lblProgressMessage.Text = "מוחק קבצים";
                lblProgressMessage.Visible = true;
                Application.DoEvents();
                var checkedItems = DelList.CheckedItems;
                if (checkedItems.Count == 0)
                {
                    return;
                }
                var daysToDelete = nDaysToDelete.Value;

                Properties.Settings.Default.DaysToDelete = (int)daysToDelete;
                Properties.Settings.Default.Save();

                dtDeleteFiles = DateTime.Now.AddDays ( (double)daysToDelete * -1 );
                var tts = new TimeSpan ( 0, 0, 0 );
                dtDeleteFiles = dtDeleteFiles.Date + tts;
                // save selected items to Settings
                SetSelectedDeletedFolders (checkedItems);

                var itemCount = checkedItems.Count;

                var percent = itemCount == 0 ? 100 : Convert.ToInt32 ( Math.Round ( 95.0 / itemCount, 0 ) );
                foreach (var checkedItem in checkedItems)
                {
                    // get the base path of each folder
                    var basePath = Path.Combine(_path, checkedItem.ToString());
                    var fileParts = new List<string>();
                    
                    //get all sub folders of base folder
                    var dirs1 = Directory.GetDirectories(basePath);
                    if (dirs1.Length > 0)
                    {
                        foreach (var curdir in dirs1)
                        {
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }
                            var lfiles = Directory.GetFiles(curdir, "*.*", SearchOption.AllDirectories)
                                .Where(s =>
                                        s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") ||
                                        s.ToLower().EndsWith(".pdf"));
                            var files = lfiles as IList<string> ?? lfiles.ToList();
                            if (files.Any())
                            {
                                foreach (var file in files)
                                {
                                    delFile = file;
                                       var creatineDate = File.GetLastWriteTime ( file);
                                    if (creatineDate >= dtDeleteFiles)
                                    {
                                        continue;
                                    } 
                                    var curFile = Path.GetFileNameWithoutExtension(file);

                                    if (curFile != null)
                                    {
                                        var splitHyphen = curFile.Split('-');
                                        foreach (var parts in splitHyphen)
                                        {
                                            var splitUnderscore = parts.Split('_');
                                            foreach (var uParts in splitUnderscore)
                                            {
                                                fileParts.Add(uParts);
                                            }
                                        }
                                    }


                                    foreach (var filePart in fileParts)
                                    {
                                        if (filePart == "888")
                                        {
                                            filesToDelete.Add(file);
                                        }
                                    }
                                    fileParts.Clear();
                                }


                            }
                        }
                        foreach (var fileToDelete in filesToDelete) {
                            File.Delete ( fileToDelete );
                        }
                        var dirsToDelete = new List<string> ();
                        foreach (var curdir in dirs1) {
                            if (!Directory.Exists ( curdir )) {
                                continue;
                            }
                            var lfiles = Directory.GetFiles ( curdir, "*.*", SearchOption.AllDirectories )
                                .Where ( s =>
                                      s.ToLower ().EndsWith ( ".tif" ) || s.ToLower ().EndsWith ( ".tiff" ) ||
                                      s.ToLower ().EndsWith ( ".pdf" ) );
                            var files = lfiles as IList<string> ?? lfiles.ToList ();
                            if (!files.Any ()) {
                                dirsToDelete.Add ( curdir );
                            }
                        }
                        foreach (var dtd in dirsToDelete) {
                            Directory.Delete ( dtd );
                        }
                        filesToDelete.Clear();
                        dirsToDelete.Clear();
                    }
                    var val = progressBar1.Value + percent;
                    if (val > 100) {
                        val = 100;
                    }
                    progressBar1.Value = val;
                    Application.DoEvents ();


                    
                }
                

                progressBar1.Value = 100;
                Application.DoEvents ();
            }
            catch (System.Exception ex) {
                MessageBox.Show ( "DeleteFiles Error :\r" + ex.Message );
                var props = new { file = delFile };
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "DeleteFiles Error",
                    Properties = { { "Files", props } }
                };
                logger.Log ( eventInfo );
                hasErrors = true;
            }
        }
        private string [] getParts ( string file )
        {
            //var names = file.Split ( '.' );
            //var name = names [0];
            return Path.GetFileNameWithoutExtension(file).Split ( '-' );
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Create Excel application
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = null;
                Excel._Worksheet xlWorksheet = null;

                try
                {
                    // Create new workbook
                    xlWorkbook = xlApp.Workbooks.Add();
                    xlApp.Visible = false;
                    string filePath = null;

                    // Use SaveFileDialog to let user choose file location
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                        saveFileDialog.FilterIndex = 1;
                        saveFileDialog.FileName = $"FileManager-Export-{DateTime.Now:yyyy-MM-dd}.xlsx";
                        saveFileDialog.Title = "שמור קובץ אקסל";

                        if (saveFileDialog.ShowDialog() != DialogResult.OK)
                        {
                            // User cancelled the dialog
                            xlWorkbook.Close(false);
                            xlApp.Quit();
                            return;
                        }

                        filePath = saveFileDialog.FileName;
                    }

                    // Export each grid/control to a separate worksheet
                    
                    ExportDataGridViewToExcel(xlWorkbook, grdArchive, "ארכיון");
                    ExportDataGridViewToExcel(xlWorkbook, gridCopy, "שכפול קבצים");
                    ExportDataGridViewToExcel(xlWorkbook, grdFolderSplit, "תקיות לפיצול");
                    ExportCheckedListBoxToExcel(xlWorkbook, reportFolders, "שמות לדוחות");
                    ExportCheckedListBoxToExcel(xlWorkbook, DelList, "מחיקת קבצים");
                    ExportCheckedListBoxToExcel(xlWorkbook, dirList, "שמות לאקסל");
                    ExportDataGridViewToExcel(xlWorkbook, dataGridView1, "יצירת מיילים");
                    ExportCheckedListBoxToExcel(xlWorkbook, filenames, "תיקון שמות");
                    ExportCheckedListBoxToExcel(xlWorkbook, DuplicateFolders, "תיקון כפילויות");
                    ExportDataGridViewToExcel(xlWorkbook, grdCount, "תקיות לספירה");


                    // Save the workbook
                    xlWorkbook.SaveAs(filePath);
                    xlWorkbook.Close(true);
                    xlApp.Quit();

                    MessageBox.Show($"הקובץ נשמר בהצלחה: {filePath}", "ייצוא לאקסל", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"שגיאה בייצוא לאקסל: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    LogEventInfo eventInfo = new LogEventInfo
                    {
                        Level = LogLevel.Error,
                        Message = $"Excel export error: {ex.Message}"
                    };
                    logger.Log(eventInfo);
                }
                finally
                {
                    // Clean up COM objects
                    if (xlWorksheet != null)
                    {
                        Marshal.ReleaseComObject(xlWorksheet);
                        xlWorksheet = null;
                    }
                    if (xlWorkbook != null)
                    {
                        Marshal.ReleaseComObject(xlWorkbook);
                        xlWorkbook = null;
                    }
                    if (xlApp != null)
                    {
                        Marshal.ReleaseComObject(xlApp);
                        xlApp = null;
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"שגיאה כללית בייצוא לאקסל: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Message = $"General Excel export error: {ex.Message}"
                };
                logger.Log(eventInfo);
            }
        }

        private void ExportDataGridViewToExcel(Excel.Workbook workbook, DataGridView grid, string sheetName)
        {
            try
            {
                // Create new worksheet
                Excel._Worksheet worksheet = (Excel._Worksheet)workbook.Sheets.Add();
                worksheet.Name = sheetName;

                // Export headers
                for (int col = 0; col < grid.Columns.Count; col++)
                {
                    if (grid.Columns[col].Visible)
                    {
                        worksheet.Cells[1, col + 1] = grid.Columns[col].HeaderText;
                        Excel.Range headerCell = worksheet.Cells[1, col + 1] as Excel.Range;
                        if (headerCell != null)
                        {
                            headerCell.Font.Bold = true;
                        }
                       
                    }
                }

                // Export data
                int excelRow = 2;
                for (int row = 0; row < grid.Rows.Count; row++)
                {
                    if (!grid.Rows[row].IsNewRow)
                    {
                        int excelCol = 1;
                        for (int col = 0; col < grid.Columns.Count; col++)
                        {
                            if (grid.Columns[col].Visible)
                            {
                                var cellValue = grid.Rows[row].Cells[col].Value;
                                if (cellValue != null)
                                {
                                    // Handle checkbox columns
                                    if (grid.Columns[col] is DataGridViewCheckBoxColumn)
                                    {
                                        worksheet.Cells[excelRow, excelCol] = (bool)cellValue ? "✓" : "✗";
                                    }
                                    else
                                    {
                                        worksheet.Cells[excelRow, excelCol] = cellValue.ToString();
                                    }
                                }
                                excelCol++;
                            }
                        }
                        excelRow++;
                    }
                }

                // Auto-fit columns
                Excel.Range usedRange = worksheet.UsedRange;
                usedRange.Columns.AutoFit();

                // Release worksheet
                Marshal.ReleaseComObject(worksheet);
            }
            catch (System.Exception ex)
            {
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Message = $"Error exporting DataGridView {sheetName}: {ex.Message}"
                };
                logger.Log(eventInfo);
            }
        }

        private void ExportCheckedListBoxToExcel(Excel.Workbook workbook, CheckedListBox listBox, string sheetName)
        {
            try
            {
                // Create new worksheet
                Excel._Worksheet worksheet = (Excel._Worksheet)workbook.Sheets.Add();
                worksheet.Name = sheetName;

                // Export headers
                worksheet.Cells[1, 1] = "פריט";
                worksheet.Cells[1, 2] = "נבחר";
                Excel.Range headerCell1 = worksheet.Cells[1, 1] as Excel.Range;
                Excel.Range headerCell2 = worksheet.Cells[1, 2] as Excel.Range;
                if (headerCell1 != null) headerCell1.Font.Bold = true;
                if (headerCell2 != null) headerCell2.Font.Bold = true;

                // Export data
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    worksheet.Cells[i + 2, 1] = listBox.Items[i].ToString();
                    worksheet.Cells[i + 2, 2] = listBox.GetItemChecked(i) ? "✓" : "✗";
                }

                // Auto-fit columns
                Excel.Range usedRange = worksheet.UsedRange;
                usedRange.Columns.AutoFit();

                // Release worksheet
                Marshal.ReleaseComObject(worksheet);
            }
            catch (System.Exception ex)
            {
                LogEventInfo eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Message = $"Error exporting CheckedListBox {sheetName}: {ex.Message}"
                };
                logger.Log(eventInfo);
            }
        }

        
    }
}
#endregion