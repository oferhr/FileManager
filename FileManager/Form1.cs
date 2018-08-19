using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using Application = System.Windows.Forms.Application;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Threading;

namespace FileManager
{


    public partial class Form1 : Form
    {
        private readonly string _path;
        private readonly string _config;
        private readonly string _excel;
        private readonly int _sleep;
        private bool _isMove = true;
        private int _numOfDuplicates;
        const string LtrMark = "\u200E";
        private static readonly object LockObject = new object ();
        private string CopiedFilesDirectory = "9876789";
        private readonly List<string> gfoldersList = new List<string>();
        private readonly string[] mailCheck = new [] {"איחוד-קצר", "בודד-זהה", "בודד-קצר" };

        public Form1()
        {
            InitializeComponent();
            btnMail.Enabled = false;
            this.MaximumSize = new Size(Size.Width, (Screen.PrimaryScreen.Bounds.Height - 50));

            try
            {
                _path = ConfigurationManager.AppSettings["basePath"];
                _config = ConfigurationManager.AppSettings ["ConfigPath"];
                _excel = ConfigurationManager.AppSettings ["ExcelPath"];
                var sleep = ConfigurationManager.AppSettings ["MailSleepSeconds"];
                
                if (!Int32.TryParse(sleep, out _sleep))
                {
                    _sleep = 0;
                }


                tabsMain.SelectedTab = tabReports;
                if (_config == string.Empty)
                {
                    MessageBox.Show("נתיב לקובץ קונפיגורציה משותף אינו קיים");
                    return;
                }
                if (_excel == string.Empty) {
                    MessageBox.Show ( "נתיב לקובץ האקסל אינו קיים" );
                    return;
                }
                string [] dirs;
                try
                {
                    dirs = Directory.GetDirectories(_path);
                }
                catch 
                {
                    MessageBox.Show("Path in config file does not exist, or you do not have permission to query it.");
                    return;
                }

                //adding files to checkbox list
                
                
                foreach (var path in dirs)
                {
                    var folder = Path.GetFileName(path);
                    if (folder != null)
                    {
                        DuplicateFolders.Items.Add(folder);
                        reportFolders.Items.Add(folder);
                        filenames.Items.Add(folder);
                        //cboDirs.Items.Add(folder);
                        dirList.Items.Add ( folder );
                        gfoldersList.Add(folder);
                        DelList.Items.Add(folder);
                    }
                }

                var countsConfigSettings = GetCountSettings ();
                var countDs = new List<CountSettings> ();
                foreach (var fol in gfoldersList)
                {
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


                var emailConfigList = GetEmailDirSettings ();
                var emailsDs = new List<EmailDirSettings>();
                foreach (var fol in gfoldersList)
                {
                    var curdir = emailConfigList.Find ( f => f.dir == fol );
                    if(curdir != null)
                    {
                        emailsDs.Add(new EmailDirSettings
                        {
                            dir = fol,
                            email = curdir.email,
                            check = curdir.icheck == 0  ? mailCheck[0] : curdir.icheck == 1 ? mailCheck [1] : mailCheck [2],
                            method = curdir.method
                        });
                    }
                    else
                    {
                        emailsDs.Add(new EmailDirSettings
                        {
                            dir = fol,
                            email = null,
                            check = mailCheck [0],
                            icheck = 0,
                            method = null
                        } );
                    }
                }

                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.DataSource = emailsDs;



                var folderSettings = GetFolderSettings ();

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
                string reportFoldersSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().reportsFolders;
                if (!String.IsNullOrEmpty ( reportFoldersSettings )) {
                    string [] folders = reportFoldersSettings.Split ( ',' );
                    for (int i = 0; i <= ( reportFolders.Items.Count - 1 ); i++) {
                        foreach (var fol in folders) {
                            if (reportFolders.Items [i].ToString () == fol) {
                                reportFolders.SetItemChecked ( i, true );
                            }
                        }

                    }
                }


                string duplicatesFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().duplicatesFolders;
                if (!String.IsNullOrEmpty(duplicatesFolderSettings))
                {
                    string[] folders = duplicatesFolderSettings.Split(',');
                    for (int i = 0; i <= (DuplicateFolders.Items.Count - 1); i++)
                    {
                        foreach (var fol in folders)
                        {
                            if (DuplicateFolders.Items[i].ToString() == fol)
                            {
                                DuplicateFolders.SetItemChecked(i, true);
                            }
                        }

                    }
                }
               // string fileNamesFolderSettings = Settings.Default.fileNamesFolders;
                string fileNamesFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().fileNamesFolders;
                if (!String.IsNullOrEmpty(fileNamesFolderSettings))
                {
                    string[] folders = fileNamesFolderSettings.Split(',');
                    for (int i = 0; i <= (filenames.Items.Count - 1); i++)
                    {
                        foreach (var fol in folders)
                        {
                            if (filenames.Items[i].ToString() == fol)
                            {
                                filenames.SetItemChecked(i, true);
                            }
                        }

                    }
                }

                string excelFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().ExcelFolders;
                if (!String.IsNullOrEmpty ( excelFolderSettings )) {
                    string [] folders = excelFolderSettings.Split ( ',' );
                    for (int i = 0; i <= ( dirList.Items.Count - 1 ); i++) {
                        foreach (var fol in folders) {
                            if (dirList.Items [i].ToString () == fol) {
                                dirList.SetItemChecked ( i, true );
                            }
                        }

                    }
                }

                string deletedFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First ().deleteFolders;
                if (!String.IsNullOrEmpty ( deletedFolderSettings )) {
                    string [] folders = deletedFolderSettings.Split ( ',' );
                    for (int i = 0; i <= ( DelList.Items.Count - 1 ); i++) {
                        foreach (var fol in folders) {
                            if (DelList.Items [i].ToString () == fol) {
                                DelList.SetItemChecked ( i, true );
                            }
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        

        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty ( txtReportDest.Text )) {
                MessageBox.Show ( "שם תיקיית היעד לשינוי שמות לדוחות לא קיים" );
                return;
            }
            boxSummary.Visible = false;
            Application.DoEvents();

            FixFileNames ( true );
            CountFiles ();
            SetExcelNames ();
            SetReportsNames();
            btnMail.Enabled = true;
        }

        //private void bMigdal_Click(object sender, EventArgs e)
        //{
        //    boxSummary.Visible = false;
        //    Application.DoEvents();
        //    FixFileNames(true);
        //    btnMail.Enabled = true;
        //}

        //private void btnExcelFiles_Click ( object sender, EventArgs e )
        //{
        //    SetExcelNames ();
        //}

        private void btnDel_Click ( object sender, EventArgs e )
        {
            DelteFiles();
        }

        private void Form1_Resize ( object sender, EventArgs e )
        {
            Invalidate ();
        }

        private void rMove_CheckedChanged ( object sender, EventArgs e )
        {
            _isMove = rMove.Checked;
        }

        private void Form1_Paint ( object sender, PaintEventArgs e )
        {

            try {
                using (var brush = new LinearGradientBrush ( ClientRectangle,
                                                                       Color.WhiteSmoke,
                                                                       Color.SteelBlue,
                                                                       90F )) {
                    e.Graphics.FillRectangle ( brush, ClientRectangle );
                }
            }
            catch {


            }
        }

        private void btnMail_Click ( object sender, EventArgs e )
        {
            try
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                lblProgressMessage.Text = "שולח מיילים";
                lblProgressMessage.Visible = true;
                Application.DoEvents();

                List<EmailDirSettings> dirSettings;
                Dictionary<string, string> dnames = new Dictionary<string, string>();
                List<string> lCopiedNames = new List<string>();
                List<string> lpaths = new List<string>();
                List<List<string>> arfiles = new List<List<string>>();
                List<string> ardirs = new List<string>();
                string configPath = Path.Combine(_config, "fileManager_emailDirConfig.json");

                // if shared config file exist - take values from there, and save email to the dir selected
                if (File.Exists(configPath))
                {

                    using (StreamReader r = new StreamReader(configPath))
                    {
                        string json = r.ReadToEnd();
                        dirSettings = JsonConvert.DeserializeObject<List<EmailDirSettings>>(json);
                    }
                }
                else
                {
                    dirSettings = new List<EmailDirSettings>();
                }
                var validDirs = dirSettings.Where(w => !string.IsNullOrEmpty(w.email));
                var counts = validDirs.Count();
                double pbPart = counts == 0 ? 100 : 100 / counts;
                //dirSettings = dirSettings.Where ( c => c.check ).ToList ();
                foreach (var dirSetting in dirSettings)
                {
                    // get all files in selected directory
                    arfiles.Clear();
                    lCopiedNames.Clear();
                    dnames.Clear();
                    lpaths.Clear();
                    ardirs.Clear();

                    if (string.IsNullOrEmpty(dirSetting.email))
                    {
                        continue;
                    }

                    string basePath = Path.Combine(_path, dirSetting.dir);
                    if (!Directory.Exists(basePath))
                    {
                        continue;
                    }
                    var lfiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
                        .Where(
                            s =>
                                s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") ||
                                s.ToLower().EndsWith(".pdf"));
                    var files = lfiles as IList<string> ?? lfiles.ToList();
                    if (files.Any())
                    {
                        //iterate through the files and get the names and insert the names and the paths to lists
                        foreach (string file in files)
                        {
                            // good directory is only with the name "1" of a date in format dd.mm.yy like 24.02.17
                            bool isGoodDirectory = false;
                            string currentDir = Path.GetFileName(Path.GetDirectoryName(file));
                            if (currentDir == "1")
                            {
                                if (!ardirs.Contains(currentDir))
                                {
                                    ardirs.Add(currentDir);
                                }
                                isGoodDirectory = true;
                            }
                            else
                            {
                                string regex = @"^(\d{1,2})([.])(\d{1,2})([.])(\d{1,2})$";
                                Match m = Regex.Match(currentDir, regex);
                                if (m.Success)
                                {
                                    if (!ardirs.Contains(currentDir))
                                    {
                                        ardirs.Add(currentDir);
                                    }

                                    isGoodDirectory = true;
                                }
                                else
                                {
                                    isGoodDirectory = false;
                                }
                            }
                            if (!isGoodDirectory)
                            {
                                continue;
                            }

                            string fileName = Path.GetFileName(file);
                            if (fileName == null || IsThumbsInPath(file))
                            {
                                continue;
                            }
                            var newFileName = fileName;
                            var newFile = file;
                            if (fileName.Trim().Contains(" "))
                            {
                                newFileName = fileName.Replace(" ", "_");
                                var copiedPath = Path.Combine(Path.GetDirectoryName(file), CopiedFilesDirectory);
                                if (!Directory.Exists(copiedPath))
                                {
                                    Directory.CreateDirectory(copiedPath);
                                }
                                newFile = Path.Combine(copiedPath, newFileName);
                                File.Copy(file, newFile, true);
                            }
                            lCopiedNames.Add(GetMailFileName(newFileName, dirSetting.icheck));
                            if (!dnames.ContainsKey(newFileName))
                            {
                                dnames.Add(newFileName, GetMailFileName(fileName, dirSetting.icheck).Split('.')[0]);
                            }
                            lpaths.Add(newFile);
                        }
                        // group file names 
                        var duplicateKeys = lCopiedNames.GroupBy(x => x)
                            .Select(group => group.Key);

                        var enumerable = duplicateKeys as string[] ?? duplicateKeys.ToArray();
                        if (enumerable.Any())
                        {
                            // create a list of lists - for every group name, get all the files that match it
                            // example - if group name is 111_222 then get all files like 111_222_1.tif, 111_222_2.tif
                            foreach (var duplicateKey in enumerable)
                            {
                                var ll = from ln in lpaths where ln.Contains(duplicateKey.Split('.')[0]) select ln;
                                arfiles.Add(new List<string>(ll.ToList()));
                            }
                        }


                        //for each group open a email from outlook and set the group name as subject, and all files as attachment
                        foreach (var arfile in arfiles)
                        {


                            double pbIncrement = arfiles.Count == 0 ? 100 : pbPart / arfiles.Count;
                            OutlookApp oApp = new OutlookApp();
                            MailItem oMsg = (MailItem) oApp.CreateItem(OlItemType.olMailItem);
                            oMsg.To = dirSetting.email;
                            var fileName = Path.GetFileName(arfile[0]);
                            // var mailFileName = GetMailFileName ( fileName, dirSetting.check ).Split ( '.' ) [0];
                            var subject = string.Empty;
                            if (fileName != null)
                            {
                                subject = dirSetting.icheck == 2 ? GetMailFileName( dnames [fileName], dirSetting.icheck, true) : dnames[fileName];
                            }
                            oMsg.Subject = subject;
                            foreach (var curFile in arfile)
                            {
                                //using (StreamWriter w = File.AppendText ( "log.txt" )) {
                                //    Log ( curFile, w );
                                //}
                                oMsg.Attachments.Add(curFile, OlAttachmentType.olByValue, Type.Missing, Type.Missing);
                            }
                            oMsg.GetInspector.Activate ();
                            var signature = oMsg.HTMLBody;
                            oMsg.HTMLBody = string.Empty + signature;

                            // oMsg.Display ( true );
                            //add this to add signiture to mail body
                            //oMsg.HTMLBody = string.Empty + oMsg.HTMLBody;
                            //oMsg.DeleteAfterSubmit = false;
                            //Outlook.Folder sentFolder = (Outlook.Folder)oApp.Session.GetDefaultFolder ( Outlook.OlDefaultFolders.olFolderSentMail );
                            //if (sentFolder != null)
                            //{
                            //    oMsg.SaveSentMessageFolder = sentFolder;
                            //}
                            
                            oMsg.Send();

                            oMsg = null;
                            oApp = null;

                            //SendMail ( arfile, subject, dirSetting.email );
                            double dVal = progressBar1.Value + pbIncrement;
                            int val = Convert.ToInt32(dVal);
                            if (val > 100)
                            {
                                val = 100;
                            }
                            progressBar1.Value = val;
                            Application.DoEvents();
                            if (_sleep > 0) {
                                Thread.Sleep ( _sleep * 1000 );
                            }
                        }



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

                        var dt = DateTime.Now;
                        var name = dt.Day.ToString().PadLeft(2, '0') + "." + dt.Month.ToString().PadLeft(2, '0') + "." +
                                   dt.Year % 100 + "." + dt.Hour.ToString().PadLeft(2, '0') + "." +
                                   dt.Minute.ToString().PadLeft(2, '0');
                        var newDir = Path.Combine(basePath, name);


                        Directory.CreateDirectory(newDir);
                        foreach (var ardir in ardirs)
                        {
                            var checkedPath = Path.Combine(basePath, ardir);
                            if (!Directory.Exists(checkedPath))
                            {
                                continue;
                            }
                            var dirFiles = Directory.GetFiles(checkedPath);
                            foreach (var dirFile in dirFiles)
                            {
                                File.Move(dirFile, Path.Combine(newDir, Path.GetFileName(dirFile)));
                            }

                        }
                        var curdirfiles = Directory.GetFiles(newDir);
                        if (curdirfiles.Length == 0)
                        {
                            try
                            {
                                Directory.Delete(newDir);
                            }
                            catch
                            {
                                using (StreamWriter w = File.AppendText("log.txt"))
                                {
                                    Log("could not delete directory:" + newDir, w);
                                }
                            }
                        }

                        foreach (var ardir in ardirs)
                        {
                            var checkedPath = Path.Combine(basePath, ardir);
                            if (!Directory.Exists(checkedPath))
                            {
                                continue;
                            }
                            Directory.Delete(checkedPath);

                        }



                    }
                }

                MessageBox.Show("המיילים נשלחו בהצלחה");
                btnMail.Enabled = false;
            }
            catch (System.Exception ex)
            {
                using (StreamWriter w = File.AppendText ( "log.txt" )) {
                    Log ("error in mail send : /n" + ex.Message + ",----/n" +  (ex.InnerException?.Message ?? "") + "----/n" + ex.StackTrace, w );
                }
            }
        }

        private void dataGridView1_CellEndEdit ( object sender, DataGridViewCellEventArgs e )
        {
            if (e.ColumnIndex > 0 && dataGridView1.Columns [e.ColumnIndex] != null && dataGridView1.Columns [e.ColumnIndex].Name == "check") {
                return;
            }
          //  string configPath = Path.Combine ( _config, "fileManager_emailDirConfig.json" );

            var mail = dataGridView1.Rows [e.RowIndex].Cells ["email"].Value == null ? string.Empty : dataGridView1.Rows [e.RowIndex].Cells ["email"].Value.ToString ();

            var fol = dataGridView1.Rows [e.RowIndex].Cells ["dir"].Value == null ? string.Empty : dataGridView1.Rows [e.RowIndex].Cells ["dir"].Value.ToString ();

            var method = dataGridView1.Rows [e.RowIndex].Cells ["method"].Value == null ? string.Empty : dataGridView1.Rows [e.RowIndex].Cells ["method"].Value.ToString ();

            var emailConfigList = GetEmailDirSettings ();


            var curdir = emailConfigList.Find ( f => f.dir == fol );
            if (curdir != null) {
                if (string.IsNullOrEmpty ( mail ) && string.IsNullOrEmpty ( method )) {
                    emailConfigList.Remove ( curdir );
                }
                else {
                    curdir.email = mail;
                    curdir.method = method;
                }

            }
            else {
                if (!string.IsNullOrEmpty ( mail ) || !string.IsNullOrEmpty ( method )) {
                    emailConfigList.Add ( new EmailDirSettings
                    {
                        dir = fol,
                        email = mail,
                        method = method,
                        check = mailCheck [0],
                        icheck = 0
                    } );
                }

            }

            setMailSettings(emailConfigList);

            var countsConfigList = GetCountSettings ();

            var curCountDir = countsConfigList.Find ( f => f.dir == fol );
            if (curCountDir != null) {
                curCountDir.method = string.IsNullOrEmpty ( method ) ? null : method;

            }
            else {
                if (!string.IsNullOrEmpty ( method )) {
                    countsConfigList.Add ( new CountSettings
                    {
                        dir = fol,
                        method = method,
                        check = false
                    } );
                }

            }

            setCountSettings ( countsConfigList );

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
            string check = dataGridView1 [e.ColumnIndex, e.RowIndex].Value.ToString();
            int icheck = 0;
            for (int i = 0; i < mailCheck.Length; i++)
            {
                if (mailCheck[i] == check)
                {
                    icheck = i;
                }
            }
            var dirSettings = GetEmailDirSettings ();
            var ddir = dataGridView1 [0, e.RowIndex].Value?.ToString ();
            var dirObj = dirSettings.Find ( f => f.dir == ddir );
            if (dirObj != null) {
                dirObj.icheck = icheck;
            }
            else {
                dirSettings.Add ( new EmailDirSettings
                {
                    dir = ddir,
                    icheck = icheck
                } );
            }


            string configPath = Path.Combine ( _config, "fileManager_emailDirConfig.json" );

            lock (LockObject) {
                var sjson = JsonConvert.SerializeObject ( dirSettings.ToArray () );
                File.WriteAllText ( configPath, sjson );
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

            if (grdCount.Columns [e.ColumnIndex].Name != "chb") {
                return;
            }
            bool check = (bool)grdCount.Rows [e.RowIndex].Cells ["chb"].Value;
            var dirSettings = GetCountSettings ();
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


            setCountSettings ( dirSettings );

        }

        private void grdCount_CellEndEdit ( object sender, DataGridViewCellEventArgs e )
        {
            if (e.ColumnIndex > 0 && grdCount.Columns [e.ColumnIndex] != null && grdCount.Columns [e.ColumnIndex].Name == "chb") {
                return;
            }
            // string configPath = Path.Combine ( _config, "fileManager_emailDirConfig.json" );
            var check = grdCount.Rows [e.RowIndex].Cells ["chb"].Value != null && (bool)grdCount.Rows [e.RowIndex].Cells ["chb"].Value;

            var method = grdCount.Rows [e.RowIndex].Cells ["methods"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["methods"].Value.ToString ();

            var fol = grdCount.Rows [e.RowIndex].Cells ["dirs"].Value == null ? string.Empty : grdCount.Rows [e.RowIndex].Cells ["dirs"].Value.ToString ();

            var countsConfigList = GetCountSettings ();
            

            var curCountDir = countsConfigList.Find ( f => f.dir == fol );
            if (curCountDir != null) {
                if (string.IsNullOrEmpty ( method ) && check == false) {
                    countsConfigList.Remove ( curCountDir );
                }
                else {
                    curCountDir.method = method;
                }

            }
            else {
                if (!string.IsNullOrEmpty ( method )) {
                    countsConfigList.Add ( new CountSettings
                    {
                        dir = fol,
                        method = method,
                        check = check
                    } );
                }

            }

            setCountSettings ( countsConfigList );

            var emailConfigList = GetEmailDirSettings ();

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
                        check = mailCheck [0],
                        icheck = 0,
                        email = null
                    } );
                }

            }
            setMailSettings(emailConfigList);
        }

        private void SetSelectedDeletedFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems) {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }




            var folderSettings = GetFolderSettings ();
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
            setFolderSettings ( folderSettings );
        }

        private void SetSelectedReportFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }

            
            

            var folderSettings = GetFolderSettings ();
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
            setFolderSettings ( folderSettings );
            //Settings.Default.duplicatesFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
        }
        private void SetSelectedDuplicatesFolders ( CheckedListBox.CheckedItemCollection checkedItems )
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems) {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }




            var folderSettings = GetFolderSettings ();
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
            setFolderSettings ( folderSettings );
            //Settings.Default.duplicatesFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
        }
        private void SetSelectedExcelFolders ( CheckedListBox.CheckedItemCollection checkedItems )
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems) {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }




            var folderSettings = GetFolderSettings ();
            if (folderSettings.Count > 0) {
                folderSettings.First ().ExcelFolders = items.TrimEnd ( ',' );
            }
            else {
                folderSettings.Add ( new FolderSettings
                {
                    ExcelFolders = items.TrimEnd ( ',' ),
                    reportsFolders = string.Empty,
                    fileNamesFolders = string.Empty,
                    selectedFolders = string.Empty,
                    duplicatesFolders = string.Empty
                } );
            }
            setFolderSettings ( folderSettings );
            //Settings.Default.duplicatesFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
        }

        private void SetSelectedFileNameFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }

            var folderSettings = GetFolderSettings ();
            if (folderSettings.Count > 0) {
                folderSettings.First ().fileNamesFolders = items.TrimEnd ( ',' );
            }
            else {
                folderSettings.Add ( new FolderSettings
                {
                    duplicatesFolders = string.Empty,
                    fileNamesFolders = items.TrimEnd ( ',' ),
                    reportsFolders = string.Empty,
                    selectedFolders = string.Empty,
                    deleteFolders = string.Empty
                } );
            }
            setFolderSettings ( folderSettings );
            

            //Settings.Default.fileNamesFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
        }
        
        private List<FileCount> SetSelectedItems(List<CountSettings> checkedItems, bool useProgressBar)
        {
            var list = new List<FileCount>();
            string items = String.Empty;
            int percent = 0;
            if (useProgressBar)
            {
                var itemCount = checkedItems.Count;
                percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(100.0 / itemCount, 0));
            }
            //loop over the checked items
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                
                items += checkedItem.dir + ",";
                var checkedPath = Path.Combine ( _path, checkedItem.dir );
                if (!Directory.Exists( checkedPath )) {
                    continue;
                }
                var lfiles = Directory.GetFiles(checkedPath, "*.*",
                    SearchOption.AllDirectories)
                     .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower ().EndsWith ( ".tiff" ) || s.ToLower().EndsWith(".pdf"));
                //check if file Thumbs.db exist in folder. If yes than reduce one file from file count.
                var files = lfiles as IList<string> ?? lfiles.ToList();
                bool isThumbs = files.Any(IsThumbsInPath);

                int fileCount;
                if (isThumbs)
                {
                    fileCount = files.Count() - 1;
                }
                else
                {
                    fileCount = files.Count();
                }
                //create a list to be sent to the grid form
                if (fileCount > 0)
                {
                    list.Add(new FileCount
                    {
                        FileName = checkedItem.dir,
                        method = checkedItem.method,
                        Count = fileCount
                    });
                }
                
                if (useProgressBar)
                {
                    int val = progressBar1.Value + percent;
                    if (val > 100)
                    {
                        val = 100;
                    }
                    progressBar1.Value = val;
                }
                
            }

            //save selected items to Settings
            var folderSettings = GetFolderSettings ();
            if(folderSettings.Count > 0)
            {
                folderSettings.First ().selectedFolders = items.TrimEnd ( ',' );
            }
            else
            {
                folderSettings.Add(new FolderSettings
                {
                    duplicatesFolders = string.Empty,
                    fileNamesFolders = string.Empty,
                    reportsFolders = string.Empty,
                    selectedFolders = items.TrimEnd ( ',' ),
                    deleteFolders = string.Empty
                } );
            }
            setFolderSettings ( folderSettings );

            
            //Settings.Default.selectedFolders = items.TrimEnd(',');
            //Settings.Default.Save();
            //Settings.Default.Reload();
            if (useProgressBar)
            {
                progressBar1.Value = 100;
            }
            
            return list;
        }

        private bool IsThumbsInPath(string filePath)
        {
            if (filePath.Contains("Thumbs"))
                return true;
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

                var countsSettings = GetCountSettings ();

                var checkedItems = countsSettings.FindAll ( f => f.check);

                
                if (checkedItems.Count == 0)
                {
                    FixDuplicates();
                    FixFileNames(false);
                    return;
                }
                
                var list = SetSelectedItems(checkedItems, true);
                FixDuplicates();
                FixFileNames( false );
                
                //open the gris form with the selected data.
                var grid = new ResultGrid(list.OrderBy(o=>o.FileName).ToList());
                grid.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("CountFiles Error :\r" + ex.Message);
            }
        }

        private void SetReportsNames()
        {
            try
            {
                string [] availDirs, allAvailDirs;
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
                int percent = itemCount == 0 ? 100 : Convert.ToInt32 ( Math.Round ( 95.0 / itemCount, 0 ) );

                foreach (var checkedItem in checkedItems)
                {
                    int counter = 1;
                    // get the base path of each folder
                    string basePath = Path.Combine(_path, checkedItem.ToString());
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
                        foreach (string curdir in availDirs)
                        {
                            
                            var checkdir = Path.GetFileName(curdir);
                            if (checkdir == null || checkdir.Length > 2)
                            {
                                continue;
                            }
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }
                            string source = curdir;
                            string sourceName = Path.GetFileName( basePath ) + " -- " + Path.GetFileName ( source);
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
                                    var id = parts [1];
                                    var report = parts [parts.Length - 1];
                                    var concatId = string.Concat ( id, report );
                                    if (grouper.Exists ( f => f.id == concatId )) {
                                        grouper.Find ( f => f.id == concatId ).files.Add ( file );
                                    }
                                    else {
                                        grouper.Add ( new Grouper
                                        {
                                            id = concatId,
                                            files = new List<string> { file }
                                        } );
                                    }
                                }
                            }
                            catch (System.Exception ex) {
                                txtLog.AppendText ( "בעיה נמצאה בקריאת קבצי המקור, " + ex.Message);
                                Application.DoEvents ();
                                isError = true;
                                continue;
                            }

                            txtLog.AppendText ( "סיום בדיקת קבצי המקור לתיקיית - " + sourceName +  Environment.NewLine );
                            txtLog.AppendText ( Environment.NewLine );
                            Application.DoEvents ();

                            int max = 1;
                            var destFiles = Directory.GetFiles ( dest, "*.*", SearchOption.TopDirectoryOnly );
                            txtLog.AppendText ( "מתחיל בבדיקת קבצי היעד לתקיית - " + sourceName + Environment.NewLine );
                            Application.DoEvents ();
                            Thread.Sleep ( 100 );
                            try {
                                foreach (var destFile in destFiles) {
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
                                Application.DoEvents ();
                                isError = true;
                                continue;
                            }

                            txtLog.AppendText ( "סיום בדיקת קבצי היעד לתיקיית - " + sourceName + Environment.NewLine );
                            Application.DoEvents ();
                            Thread.Sleep ( 100 );
                            txtLog.AppendText ( Environment.NewLine );
                            txtLog.AppendText ( "מתחיל בהעברת הקבצים בתקיית - " + sourceName + Environment.NewLine );
                            Application.DoEvents ();

                            try {
                                foreach (var grp in grouper) {
                                    foreach (var grpFile in grp.files) {
                                        var ext = grpFile.Split ( '.' ) [1];
                                        var grpParts = getParts ( grpFile );
                                        grpParts [grpParts.Length - 1] = max.ToString ( "D3" );
                                        var newFile = string.Empty;
                                        foreach (var grpPart in grpParts) {
                                            newFile += grpPart + "-";
                                        }
                                        newFile = newFile.TrimEnd ( '-' ) + "." + ext;
                                        Thread.Sleep ( 50 );
                                        txtLog.AppendText ( "מעביר קובץ " + Path.GetFileName ( grpFile ) + Environment.NewLine );
                                        Application.DoEvents ();
                                        File.Move ( grpFile, Path.Combine ( dest, Path.GetFileName ( newFile ) ) );
                                    }
                                    max++;
                                }
                            }
                            catch (System.Exception ex) {
                                txtLog.AppendText ( "בעיה בהעברת הקובץ, " + ex.Message );
                                Application.DoEvents ();
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
                    int val = progressBar1.Value + percent;
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
            }
        }
        private void FixDuplicates()
        {
            try
            {
                string[] availDirs, allAvailDirs;
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
                SetSelectedDuplicatesFolders(checkedItems);
                var itemCount = checkedItems.Count;
                int percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(95.0 / itemCount, 0));
                foreach (var checkedItem in checkedItems)
                {
                    int counter = 1;
                    // get the base path of each folder
                    string basePath = Path.Combine(_path, checkedItem.ToString());
                    string allFilesPath = String.Empty;
                    //get all sub folders of base folder
                    allAvailDirs = Directory.GetDirectories(basePath);
                    availDirs = allAvailDirs.Where(w =>
                    {
                        var dn = Path.GetFileName(w);
                        return dn != null && dn.Length < 3;
                    }).ToArray();
                    if (availDirs.Length > 0)
                    {
                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (string curdir in availDirs)
                        {
                            var checkdir = Path.GetFileName ( curdir );
                            if (checkdir == null || checkdir.Length > 2) {
                                continue;
                            }
                            if (!Directory.Exists ( curdir )) {
                                continue;
                            }
                            string folder = Path.GetFileName(curdir);
                            if (folder == counter.ToString())
                            {
                                // if it is the first folder (dir/1) rename all files 
                                // from fileName.xxx to fileName_1.xxx
                                if (counter == 1)
                                {
                                    var lfiles = Directory.GetFiles(curdir, "*.*", SearchOption.TopDirectoryOnly)
                                         .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower ().EndsWith ( ".tiff" ) || s.ToLower().EndsWith(".pdf"));
                                    var files = lfiles as IList<string> ?? lfiles.ToList();
                                    if (files.Any())
                                    {
                                        if (CheckFolderIfNotFirstDuplication(files, curdir))
                                        {
                                            allFilesPath = curdir;
                                            break;
                                        }
                                        foreach (string file in files)
                                        {
                                           
                                            string fileName = Path.GetFileName(file);
                                            
                                            if (fileName == null || IsThumbsInPath(file))
                                            {
                                                continue;
                                            }
                                            string newName = GetNewFileName(fileName, 1);
                                            if (String.IsNullOrEmpty(newName))
                                            {
                                                continue;
                                            }
                                            // if it is the first duplication of the file first rename the file in the
                                            // base folder (dir/1) from fileName.xxx to fileName_1.xxx
                                           if(!File.Exists(Path.Combine(curdir, newName)))
                                           {
                                               move(Path.Combine(curdir, fileName),
                                                   Path.Combine(curdir, newName));
                                           }
                                            
                                        }
                                    }
                                    allFilesPath = curdir;
                                    break;
                                }

                            }
                        }
                        // if the folder does not contain dir/1 folder continue.
                        if (String.IsNullOrEmpty(allFilesPath))
                        {
                            continue;
                        }
                        counter = 1;
                        // run again to get the duplicated folders
                        //foreach (string curdir in availDirs)
                        for (int i = 0; i < availDirs.Length; i++)
                        {
                            string curAvailDir = availDirs[i];
                            if (!Directory.Exists ( curAvailDir )) {
                                continue;
                            }
                            var checkdir = Path.GetFileName ( curAvailDir );
                            // if the folder is the base folder (dir/1) continue - check all duplicated folders only
                            if (checkdir == "1")
                            {
                                counter++;
                                continue;
                            }
                            
                            if (checkdir == null || checkdir.Length > 2) {
                                continue;
                            }
                            var llfiles = Directory.GetFiles( curAvailDir, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower ().EndsWith ( ".tiff" ) || s.ToLower().EndsWith(".pdf"));
                            var ffiles = llfiles as IList<string> ?? llfiles.ToList();
                            if (ffiles.Any())
                            {
                                //run over the files in the duplicated folder (dir/2...)
                                foreach (string xfile in ffiles)
                                {
                                    string fileName = Path.GetFileName(xfile);
                                    if (fileName == null || IsThumbsInPath(xfile))
                                    {
                                        continue;
                                    }

                                    string newName = GetNewFileName(fileName, 1);
                                    if (string.IsNullOrEmpty(newName))
                                    {
                                        continue;
                                    }
                                    if (File.Exists(Path.Combine(allFilesPath, newName)))
                                    {
                                        int miniCounter = 2;
                                        bool isCopy = false;
                                        while (!isCopy)
                                        {
                                            var copyName = GetNewFileName(fileName, miniCounter);
                                            if (File.Exists(Path.Combine(allFilesPath, copyName)))
                                            {
                                                miniCounter++;
                                            }
                                            else
                                            {
                                                isCopy = true;
                                                _numOfDuplicates++;
                                                CopyFiles(xfile,
                                                Path.Combine(allFilesPath, copyName));
                                            }
                                        }

                                    }
                                    else
                                    {
                                        MessageBox.Show("file name : \r" + fileName + "\r" +
                                                        "exist in duplicated folder but not in base folder");
                                    }
                                }
                            }
                            counter++;
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            
                            //string folder = Path.GetFileName(dir);
                            //if (folder == counter.ToString())
                            //{
                            //    if (counter == 1)
                            //    {
                            //        counter++;
                            //        continue;
                            //    }
                            //    string[] files = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
                            //    if (files.Length > 0)
                            //    {
                            //        //run over the files in the duplicated folder (dir/2...)
                            //        foreach (string file in files)
                            //        {
                            //            string fileName = Path.GetFileName(file);
                            //            if (fileName == null || IsThumbsInPath(file))
                            //            {
                            //                continue;
                            //            }
                                       
                            //            string newName = GetNewFileName(fileName, 1);
                            //            if (String.IsNullOrEmpty(newName))
                            //            {
                            //                continue;
                            //            }
                            //            if (File.Exists(Path.Combine(allFilesPath, newName)))
                            //            {
                            //                var copyName = GetNewFileName(fileName, counter);
                            //                if (!File.Exists(Path.Combine(allFilesPath, copyName)))
                            //                {
                            //                    NumOfDuplicates++;
                            //                    CopyFiles(file,
                            //                        Path.Combine(allFilesPath, copyName));
                            //                }
                            //                else
                            //                {
                            //                    int miniCounter = counter + 1;
                            //                    bool isCopy = false;
                            //                    while (!isCopy)
                            //                    {
                            //                        copyName = GetNewFileName(fileName, miniCounter);
                            //                        if (File.Exists(Path.Combine(allFilesPath, copyName)))
                            //                        {
                            //                            miniCounter++;
                            //                        }
                            //                        else
                            //                        {
                            //                            isCopy = true;
                            //                            CopyFiles(file,
                            //                            Path.Combine(allFilesPath, copyName));
                            //                        }
                            //                    }
                                                
                            //                }



                                            
                            //            }
                            //            else
                            //            {
                            //                MessageBox.Show("file name : \r" + fileName + "\r" +
                            //                                "exist in duplicated folder but not in base folder");
                            //            }
                            //            //}
                            //        }
                            //    }
                            //    counter++;
                            //}
                        }
                    }
                    int val = progressBar1.Value + percent;
                    if (val > 100)
                    {
                        val = 100;
                    }
                    progressBar1.Value = val;
                }
                // run second time over the folders to delete empty folders
                foreach (var checkedItem in checkedItems)
                {
                    string basePath = Path.Combine(_path, checkedItem.ToString());
                    allAvailDirs = Directory.GetDirectories ( basePath );
                    availDirs = allAvailDirs.Where ( w => {
                        var dn = Path.GetFileName ( w );
                        return dn != null && dn.Length < 3;
                    } ).ToArray ();
                    //availDirs = Directory.GetDirectories(basePath);
                    if (availDirs.Length > 1)
                    {
                        for (int i = 0; i < availDirs.Length; i++)
                        {
                            if (!Directory.Exists ( availDirs[i] )) {
                                continue;
                            }
                            string fileName = Path.GetFileName(availDirs[i]);
                            if (fileName == "1" || fileName.Length > 2)
                            {
                                continue;
                            }
                            // delete all remaining files so get all files not only tif and pdf
                            var lfiles = Directory.GetFiles(availDirs[i], "*.*", SearchOption.TopDirectoryOnly);
                            var files = lfiles as IList<string> ?? lfiles.ToList();
                            if (!files.Any())
                            {
                                try
                                {
                                    Directory.Delete(availDirs[i]);
                                }
                                catch (IOException)
                                {
                                    Directory.Delete(availDirs[i], true);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Directory.Delete(availDirs[i], true);
                                }
                            }
                            else if (files.Count == 1 && IsThumbsInPath(files[0]))
                            {
                                try
                                {
                                    File.SetAttributes(files[0], FileAttributes.Normal);
                                    File.Delete(files[0]);
                                    Directory.Delete(availDirs[i]);
                                }
                                catch (IOException)
                                {
                                    Directory.Delete(availDirs[i], true);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Directory.Delete(availDirs[i], true);
                                }
                            }
                        }
                    }
                    progressBar1.Value = 100;
                }

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
            }

        }

        private void FixFileNames(bool isMigdal)
        {
            try
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                _numOfDuplicates = 0;
                lblProgressMessage.Text = "מתקן שמות";
                lblProgressMessage.Visible = true;
                Application.DoEvents();
                var checkedItems = filenames.CheckedItems;
                if (checkedItems.Count == 0)
                {
                    return;
                }
                // save selected items to Settings
                SetSelectedFileNameFolders(checkedItems);
                //int counter;
                var itemCount = checkedItems.Count;
                int percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(95.0/itemCount, 0));
                int progress = 0;
                //loop over the checked items
                foreach (var checkedItem in checkedItems)
                {
                    string basePath = Path.Combine(_path, checkedItem.ToString());
                    var dirs = Directory.GetDirectories(basePath);
                    if (dirs.Length > 0)
                    {
                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (string curdir in dirs)
                        {
                            if (!Directory.Exists ( curdir )) {
                                continue;
                            }
                            var checkdir = Path.GetFileName ( curdir );
                            if (checkdir == null || checkdir.Length > 2) {
                                continue;
                            }
                            var lfiles = Directory.GetFiles(curdir, "*.*", SearchOption.AllDirectories)
                               .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower ().EndsWith ( ".tiff" ) || s.ToLower().EndsWith(".pdf"));

                            var files = lfiles as IList<string> ?? lfiles.ToList();


                            foreach (string file in files)
                            {
                                string fileName = Path.GetFileName(file);
                                

                                if (fileName == null || fileName.Contains("-"))
                                {
                                    continue;
                                }
                                var extSplits = fileName.Split('.');
                                var splits = extSplits[0].Split('_');
                                if (splits.Length != 3)
                                {
                                    continue;
                                }
                                string newName = String.Empty;
                                if (splits[1] == "9999999999")
                                {
                                    for (int i = 0; i < splits.Length; i++)
                                    {
                                        if (i == 1) continue;
                                        var part = splits[i];
                                        newName += part + "_";
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < splits.Length; i++)
                                    {
                                        var part = splits[i];
                                        if (i == 0 && isMigdal)
                                        {
                                            newName += part + "-";
                                        }
                                        else
                                        {
                                            newName += part + "_";
                                        }

                                    }
                                }
                                int miniCounter = 1;
                                bool isCopy = false;
                                while (!isCopy) {
                                    var based = Directory.GetParent ( curdir ).FullName;
                                    var newdir = Path.Combine ( based, miniCounter.ToString() );
                                    
                                    string copyName = Path.Combine ( newdir, newName.TrimEnd ( '_' ) + "." + extSplits [1] ) ;
                                    if (File.Exists ( copyName )) {
                                        miniCounter++;
                                    }
                                    else {
                                        isCopy = true;
                                        if (!Directory.Exists ( newdir )) {
                                            Directory.CreateDirectory ( newdir );
                                        }
                                        move ( file, copyName );
                                        //move ( file, Path.Combine ( curdir, copyName ) );
                                    }
                                }
                                
                            }
                            
                        }
                    }
                    progress += percent;
                    progressBar1.Value = progress;
                }
                progressBar1.Value = 100;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("FixFileNames Error :\r" + ex.Message);
            }
        }

        private bool CheckFolderIfNotFirstDuplication(IEnumerable<string> files, string dir)
        {
            //checks if all the files ends with underscore and 1 or 2 digits
            //if yes than it is not the first duplication
            // and make sure all files have underscore
            bool isDuplicated = false;
            var newFiles = new List<string>();
            var duplicatedFiles = new List<string>();
            Regex r = new Regex(@".+?[_][0-9]{1,2}$");
            //iterate the files and place all files in the right List<>
            foreach (var file in files)
            {
                string ff = Path.GetFileNameWithoutExtension(file);
                if (ff != null && !IsThumbsInPath(ff) && ((r.Match(ff).Success || ff.Contains(LtrMark))))
                {
                    duplicatedFiles.Add(ff);
                    isDuplicated = true;
                }
                else
                {
                    if (ff != null && (!IsThumbsInPath(ff) && !ff.Contains(LtrMark)))
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
                    string fileName = Path.GetFileName(file);
                    string noExt = Path.GetFileNameWithoutExtension(file);
                    if (fileName == null || noExt == null)
                    {
                        continue;
                    }
                    //regex for file names with numbers
                    Regex matcher = new Regex(@"^" + noExt + "[_][0-9]{1,2}$");
                    //regex for file names with letters
                    Regex matcher2 = new Regex(@"^" + noExt + LtrMark + @"[ \d]{1,2}$");
                    var sameFiles = new List<string>();
                    //add matched files to list
                    sameFiles.AddRange(duplicatedFiles.FindAll(matcher.IsMatch));
                    sameFiles.AddRange(duplicatedFiles.FindAll(matcher2.IsMatch));
                    //if there are matched files add filename and a counter
                    // if not add filename and 1
                    if (sameFiles.Count > 0)
                    {
                        int miniCounter = 2;
                        bool isCopy = false;
                        while (!isCopy)
                        {
                            string copyName = GetNewFileName(fileName, miniCounter);
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
                        string newName = GetNewFileName(fileName, 1);
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
            string[] splits = name.Split('.');
            if (splits.Length == 2)
            {
                Regex re = new Regex(@"\d+");
                Match m = re.Match(name);

                if (m.Success)
                {
                    return splits[0] + "_" + num + "." + splits[1];
                }
                
                
                string newname= splits[0] + LtrMark + " " + num + "." + splits[1];
                return newname;
                
            }
            
            
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

        private  string GetMailFileName(string fileName, int isCheck, bool shortenName = false )
        {
            // try to plit with _, -, space
            //var isCheck = ischeck;
            var type = 1;
            var ext = fileName.Split ( '.' );
            var splits = ext[0].Split('_');
            if (splits.Length == 1)
            {
                type = 2;
                splits = fileName.Split('-');
                if (splits.Length == 1)
                {
                    type = 3;
                    splits = fileName.Split(' ');
                }
            }
            // if checkbox is checked remove the last part of the splits
            if (isCheck == 0 || shortenName)
            {
                var first = splits[0];
                var last = splits[splits.Length - 1];
                string[] newName;
                string sep = type == 1 ? "_" : type == 2 ? "-" : " ";
                //check if the first split is diget only
                if (!first.Any(ch => ch < '0' || ch > '9'))
                {
                    newName = splits.Skip(1).Take ( splits.Count () ).ToArray ();
                }
                else if (!last.Any(ch => ch < '0' || ch > '9'))
                {
                    newName = splits.Take(splits.Count() - 1).ToArray();
                }
                else
                {
                    newName = splits;
                }
                
                var name = string.Join(sep, newName);
                return ext.Length == 1 ? name : name + "." + ext[1];
            }
            else
            {
                return fileName;
            }


        }

        private List<EmailDirSettings> GetEmailDirSettings ()
        {
            List<EmailDirSettings> dirSettings = new List<EmailDirSettings> ();
            string configPath = Path.Combine ( _config, "fileManager_emailDirConfig.json" );
            if (File.Exists ( configPath )) {

                using (StreamReader r = new StreamReader ( configPath )) {
                    string json = r.ReadToEnd ();
                    dirSettings = JsonConvert.DeserializeObject<List<EmailDirSettings>> ( json );
                }
            }
            return dirSettings;
        }

        private void setFolderSettings (List<FolderSettings> folSettings )
        {
            string configPath = Path.Combine ( _config, "fileManager_foldersConfig.json" );

            lock (LockObject) {
                var sjson = JsonConvert.SerializeObject ( folSettings.ToArray () );
                File.WriteAllText ( configPath, sjson );
            }
        }

        private List<FolderSettings> GetFolderSettings ()
        {
            string configPath = Path.Combine ( _config, "fileManager_foldersConfig.json" );
            var folderSettings = new List<FolderSettings> ();
            if (File.Exists ( configPath )) {

                using (StreamReader r = new StreamReader ( configPath )) {
                    string json = r.ReadToEnd ();
                    folderSettings = JsonConvert.DeserializeObject<List<FolderSettings>> ( json );
                }
            }
            else {
                folderSettings = new List<FolderSettings> ();
            }

            return folderSettings;
        }

        private void setCountSettings ( List<CountSettings> folSettings )
        {
            string configPath = Path.Combine ( _config, "fileManager_countsConfig.json" );

            lock (LockObject) {
                var sjson = JsonConvert.SerializeObject ( folSettings.ToArray () );
                File.WriteAllText ( configPath, sjson );
            }
        }

        private void setMailSettings ( List<EmailDirSettings> folSettings )
        {
            string configPath = Path.Combine ( _config, "fileManager_emailDirConfig.json" );

            lock (LockObject) {
                var sjson = JsonConvert.SerializeObject ( folSettings.ToArray () );
                File.WriteAllText ( configPath, sjson );
            }
        }

        private List<CountSettings> GetCountSettings ()
        {
            string configPath = Path.Combine ( _config, "fileManager_countsConfig.json" );
            List<CountSettings> folderSettings;
            if (File.Exists ( configPath )) {

                using (StreamReader r = new StreamReader ( configPath )) {
                    string json = r.ReadToEnd ();
                    folderSettings = JsonConvert.DeserializeObject<List<CountSettings>> ( json );
                }
            }
            else {
                folderSettings = new List<CountSettings> ();
            }

            return folderSettings;
        }

        private List<CountSettings> GetExcelSettings ()
        {
            string configPath = Path.Combine ( _config, "fileManager_countsConfig.json" );
            List<CountSettings> folderSettings;
            if (File.Exists ( configPath )) {

                using (StreamReader r = new StreamReader ( configPath )) {
                    string json = r.ReadToEnd ();
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
            var checkedItems = dirList.CheckedItems;
            if (checkedItems.Count == 0) {
                return;
            }
            var arr = GetExcelValues ();
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            lblProgressMessage.Text = "מתקן שמות מאקסל";
            lblProgressMessage.Visible = true;
            Application.DoEvents ();

            SetSelectedExcelFolders(checkedItems);

            var itemCount = checkedItems.Count;
            var itemParts = itemCount == 0 ? 100 : Convert.ToInt32 ( Math.Round ( 100.0 / itemCount, 0 ) );
            int percent = 0;
            int progress = 0;
            int index = 0;
            foreach (var checkedItem in checkedItems) {
                string basePath = Path.Combine ( _path, checkedItem.ToString () );
                if (!Directory.Exists ( basePath )) {
                    continue;
                }
                var lfiles = Directory.GetFiles ( basePath, "*.*", SearchOption.AllDirectories )
                           .Where ( s => s.ToLower ().EndsWith ( ".tif" ) || s.ToLower ().EndsWith ( ".tiff" ) || s.ToLower ().EndsWith ( ".pdf" ) );
                // var dirs = Directory.GetDirectories ( basePath );
                // if (dirs.Length > 0) {
                // run over the array to get the dir/1 folder path where all the files are
                // foreach (string dir in dirs) {


                var files = lfiles as IList<string> ?? lfiles.ToList ();
                percent =  files.Count == 0 ? 100 : itemParts / files.Count;

                foreach (string file in files) {
                    
                    string fileName = Path.GetFileName ( file );

                    if (fileName == null) {
                        continue;
                    }
                   
                    var extSplits = fileName.Split ( '.' );
                    if(extSplits.Length == 0)
                    {
                        continue;
                    }
                    var hyphenSplits = extSplits [0].Split ( '-' );
                    //var splitIndex = 0;
                    //if (hyphenSplits.Length > 1)
                    //{
                    //    splitIndex = 1;
                    //}
                    //var splits = hyphenSplits [splitIndex].Split ( '_' );
                    //if (splits.Length == 1) {
                    //    continue;
                    //}
                    
                    List<string> parts = new List<string>();
                    foreach (var split in hyphenSplits)
                    {
                        var splits = split.Split ( '_' );
                        foreach (var s in splits)
                        {
                            parts.Add (s);
                        }
                        
                    }

                    foreach (var part in parts) {
                        
                        string result = CheckExcelForItems (arr, part );
                        
                        if (!string.IsNullOrEmpty ( result )) {
                            
                            var newFilePath = file.Replace ( part, string.Join ( "", result.Split ( Path.GetInvalidFileNameChars () ) ) );
                            
                            File.Move ( file, newFilePath );
                            break;
                        }
                    }
                    progress += ( index * itemParts ) + percent;
                    if (progress > 100) progress = 100;
                    progressBar1.Value = progress;
                    //  }
                    //  }
                }
                index++;
            }
            progressBar1.Value = 100;
            MessageBox.Show ( "הפעולה הסתיימה בהצלחה" );
        }

        private object[,] GetExcelValues (  )
        {
            var lst = new List<string> ();
            Excel.Application xlApp = new Excel.Application ();
            Excel.Workbook xlWorkbook = null;
            Excel._Worksheet xlWorksheet = null;
            Excel.Range xlRange = null;
            try {

                xlWorkbook = xlApp.Workbooks.Open ( _excel );
                xlApp.Visible = false;
                xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets [1];
                xlRange = xlWorksheet.UsedRange;

                return (object[,])xlRange.Cells.Value2;
            }
            catch(System.Exception ex) {

                return new object [0,0];
            }
            finally {
                GC.Collect ();
                GC.WaitForPendingFinalizers ();

                //rule of thumb for releasing com objects:
                //  never use two dots, all COM objects must be referenced and released individually
                //  ex: [somthing].[something].[something] is bad

                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject ( xlRange );
                Marshal.ReleaseComObject ( xlWorksheet );

                //close and release
                xlWorkbook.Close ();
                Marshal.ReleaseComObject ( xlWorkbook );

                //quit and release
                xlApp.Quit ();
                Marshal.ReleaseComObject ( xlApp );
            }
            
        }

        private string CheckExcelForItems ( object [,] arr, string part )
        {
            var lst = new List<string> ();
            for (int x = 1; x <= arr.GetLength ( 0 ); x++) {
                if (part == arr [x, 1].ToString ()) {
                    for (int y = 2; y <= arr.GetLength ( 1 ); y++) {
                        var val = arr [x, y]?.ToString ();
                        if (!string.IsNullOrEmpty ( val )) {
                            lst.Add ( val );
                        }
                    }
                }
            }
            if (lst.Count > 0) {
                return String.Join ( " ", lst.ToArray () ).Replace ( " ", "_" );
            }
            return null;
        }

        private void DelteFiles ()
        {
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
                // save selected items to Settings
                SetSelectedDeletedFolders(checkedItems);

                var itemCount = checkedItems.Count;

                int percent = itemCount == 0 ? 100 : Convert.ToInt32 ( Math.Round ( 95.0 / itemCount, 0 ) );
                foreach (var checkedItem in checkedItems)
                {
                    // get the base path of each folder
                    var basePath = Path.Combine(_path, checkedItem.ToString());
                    var fileParts = new List<string>();
                    
                    //get all sub folders of base folder
                    var dirs1 = Directory.GetDirectories(basePath);
                    if (dirs1.Length > 0)
                    {
                        foreach (string curdir in dirs1)
                        {
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }
                            var lfiles = Directory.GetFiles(curdir, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(s =>
                                        s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") ||
                                        s.ToLower().EndsWith(".pdf"));
                            var files = lfiles as IList<string> ?? lfiles.ToList();
                            if (files.Any())
                            {
                                foreach (string file in files)
                                {
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
                        foreach (string curdir in dirs1) {
                            if (!Directory.Exists ( curdir )) {
                                continue;
                            }
                            var lfiles = Directory.GetFiles ( curdir, "*.*", SearchOption.TopDirectoryOnly )
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
                    int val = progressBar1.Value + percent;
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
            }
        }
        private string [] getParts ( string file )
        {
            var names = file.Split ( '.' );
            var name = names [0];
            return name.Split ( '-' );
        }
        //private static void SendMail(List<string> files, string subject, string email)
        //{
        //    var server = ConfigurationManager.AppSettings["smptServer"];
        //    var port = ConfigurationManager.AppSettings ["smptPort"];
        //    var from = ConfigurationManager.AppSettings ["mailFrom"];
        //    var user = ConfigurationManager.AppSettings ["userName"];
        //    var pwd = ConfigurationManager.AppSettings ["password"];

        //    using (MailMessage mailMsg = new MailMessage())
        //    {
        //        mailMsg.To.Add ( email );
        //        // From
        //        MailAddress mailAddress = new MailAddress ( from );
        //        mailMsg.From = mailAddress;

        //        // Subject and Body
        //        mailMsg.Subject = subject;
        //        mailMsg.Body = string.Empty + mailMsg.Body;

        //        foreach (var file in files) {
        //            var attach = new Attachment ( file );
        //            mailMsg.Attachments.Add ( attach );
        //        }


        //        int iport = string.IsNullOrEmpty ( port ) ? 80 : int.Parse ( port );
        //        // Init SmtpClient and send on port 587 in my case. (Usual=port25)
        //        SmtpClient smtpClient = new SmtpClient ( server, iport );
        //        smtpClient.EnableSsl = true;
        //        System.Net.NetworkCredential credentials =
        //           new System.Net.NetworkCredential ( user, pwd );
        //        smtpClient.Credentials = credentials;

        //        smtpClient.Send ( mailMsg );
        //    }


        //}

        //private string CheckExcelForItems ( string part )
        //{
        //    var lst = new List<string> ();
        //    Excel.Application xlApp = new Excel.Application ();
        //    Excel.Workbook xlWorkbook = null;
        //    Excel._Worksheet xlWorksheet = null;
        //    Excel.Range xlRange = null;
        //    try {

        //        xlWorkbook = xlApp.Workbooks.Open ( _excel );
        //        xlApp.Visible = false;
        //        xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets [1];
        //        xlRange = xlWorksheet.UsedRange;

        //        int rowCount = xlRange.Rows.Count;
        //        int colCount = xlRange.Columns.Count;

        //        for (int t = 2; t <= rowCount; t++) {
        //            string val = ( xlRange.Cells [t, 1] as Excel.Range ).Value2.ToString ();
        //            if (val == part) {
        //                for (int j = 2; j <= colCount; j++) {
        //                    string temp = ( xlRange.Cells [t, j] as Excel.Range ).Value2?.ToString ();
        //                    if (!string.IsNullOrEmpty ( temp )) {
        //                        lst.Add ( temp );
        //                    }
        //                }
        //                break;
        //            }
        //        }


        //        if (lst.Count > 0) {
        //            return String.Join ( " ", lst.ToArray () ).Replace ( " ", "_" );
        //        }
        //        return null;

        //    }
        //    catch (System.Exception ex) {

        //        return null;
        //    }
        //    finally {
        //        GC.Collect ();
        //        GC.WaitForPendingFinalizers ();

        //        //rule of thumb for releasing com objects:
        //        //  never use two dots, all COM objects must be referenced and released individually
        //        //  ex: [somthing].[something].[something] is bad

        //        //release com objects to fully kill excel process from running in the background
        //        Marshal.ReleaseComObject ( xlRange );
        //        Marshal.ReleaseComObject ( xlWorksheet );

        //        //close and release
        //        xlWorkbook.Close ();
        //        Marshal.ReleaseComObject ( xlWorkbook );

        //        //quit and release
        //        xlApp.Quit ();
        //        Marshal.ReleaseComObject ( xlApp );




        //    }

        //}
        //private string GetName(int ctr, string path, string ext)
        //{
        //    Dictionary<string, int> bases = new Dictionary<string, int>
        //    {
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\AIG\1", 201300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Amitim\1", 301300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Ayalon\1", 401300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Clal\1", 501300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Hachshara\1", 601300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Harel\1", 701300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Inbal\1", 801300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Macabi\1", 901300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Migdal\1", 101300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Pool\1", 111300010},
        //        {@"C:\Users\ofer\Documents\Visual Studio 2013\Projects\FileManager\Files\Shlomo\1", 121300010}
        //    };
        //    int num = bases[path];
        //    int newName = num + ctr;
        //    return newName + "." + ext;
        //}
    }
}
