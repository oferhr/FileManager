namespace FileManager
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bClose = new System.Windows.Forms.Button();
            this.bStart = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.rCopy = new System.Windows.Forms.RadioButton();
            this.rMove = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabsMain = new System.Windows.Forms.TabControl();
            this.tabCounts = new System.Windows.Forms.TabPage();
            this.grdCount = new System.Windows.Forms.DataGridView();
            this.chb = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dirs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.methods = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabDuplicates = new System.Windows.Forms.TabPage();
            this.DuplicateFolders = new System.Windows.Forms.CheckedListBox();
            this.tabNames = new System.Windows.Forms.TabPage();
            this.filenames = new System.Windows.Forms.CheckedListBox();
            this.tabEEmail = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dir = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.check = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.email = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dirList = new System.Windows.Forms.CheckedListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.DelList = new System.Windows.Forms.CheckedListBox();
            this.tabReports = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtReportDest = new System.Windows.Forms.TextBox();
            this.reportFolders = new System.Windows.Forms.CheckedListBox();
            this.lblProgressMessage = new System.Windows.Forms.Label();
            this.boxSummary = new System.Windows.Forms.GroupBox();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lblDuplicateNum = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.btnMail = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tabsMain.SuspendLayout();
            this.tabCounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdCount)).BeginInit();
            this.tabDuplicates.SuspendLayout();
            this.tabNames.SuspendLayout();
            this.tabEEmail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabReports.SuspendLayout();
            this.boxSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // bClose
            // 
            this.bClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bClose.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bClose.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.bClose.ForeColor = System.Drawing.Color.DarkBlue;
            this.bClose.Location = new System.Drawing.Point(1010, 710);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(75, 23);
            this.bClose.TabIndex = 0;
            this.bClose.Text = "סגור";
            this.bClose.UseVisualStyleBackColor = false;
            this.bClose.Click += new System.EventHandler(this.bClose_Click);
            // 
            // bStart
            // 
            this.bStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bStart.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bStart.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.bStart.ForeColor = System.Drawing.Color.DarkBlue;
            this.bStart.Location = new System.Drawing.Point(952, 53);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(129, 23);
            this.bStart.TabIndex = 2;
            this.bStart.Text = "התחל";
            this.bStart.UseVisualStyleBackColor = false;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(881, 223);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(200, 23);
            this.progressBar1.TabIndex = 6;
            this.progressBar1.Visible = false;
            // 
            // rCopy
            // 
            this.rCopy.AutoSize = true;
            this.rCopy.BackColor = System.Drawing.Color.WhiteSmoke;
            this.rCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rCopy.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.rCopy.Location = new System.Drawing.Point(38, 39);
            this.rCopy.Name = "rCopy";
            this.rCopy.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.rCopy.Size = new System.Drawing.Size(52, 19);
            this.rCopy.TabIndex = 8;
            this.rCopy.Text = "Copy";
            this.rCopy.UseVisualStyleBackColor = false;
            // 
            // rMove
            // 
            this.rMove.AutoSize = true;
            this.rMove.BackColor = System.Drawing.Color.WhiteSmoke;
            this.rMove.Checked = true;
            this.rMove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rMove.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.rMove.Location = new System.Drawing.Point(129, 39);
            this.rMove.Name = "rMove";
            this.rMove.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.rMove.Size = new System.Drawing.Size(52, 19);
            this.rMove.TabIndex = 9;
            this.rMove.TabStop = true;
            this.rMove.Text = "Move";
            this.rMove.UseVisualStyleBackColor = false;
            this.rMove.CheckedChanged += new System.EventHandler(this.rMove_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.groupBox1.Controls.Add(this.rMove);
            this.groupBox1.Controls.Add(this.rCopy);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Font = new System.Drawing.Font("Guttman Frank", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.groupBox1.ForeColor = System.Drawing.Color.Navy;
            this.groupBox1.Location = new System.Drawing.Point(881, 272);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox1.Size = new System.Drawing.Size(200, 81);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "אפשרויות העתקת קבצים";
            // 
            // tabsMain
            // 
            this.tabsMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tabsMain.Controls.Add(this.tabCounts);
            this.tabsMain.Controls.Add(this.tabDuplicates);
            this.tabsMain.Controls.Add(this.tabNames);
            this.tabsMain.Controls.Add(this.tabEEmail);
            this.tabsMain.Controls.Add(this.tabPage1);
            this.tabsMain.Controls.Add(this.tabPage2);
            this.tabsMain.Controls.Add(this.tabReports);
            this.tabsMain.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tabsMain.Location = new System.Drawing.Point(12, 12);
            this.tabsMain.Name = "tabsMain";
            this.tabsMain.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tabsMain.RightToLeftLayout = true;
            this.tabsMain.SelectedIndex = 0;
            this.tabsMain.Size = new System.Drawing.Size(834, 721);
            this.tabsMain.TabIndex = 11;
            // 
            // tabCounts
            // 
            this.tabCounts.Controls.Add(this.grdCount);
            this.tabCounts.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tabCounts.Location = new System.Drawing.Point(4, 24);
            this.tabCounts.Name = "tabCounts";
            this.tabCounts.Padding = new System.Windows.Forms.Padding(3);
            this.tabCounts.Size = new System.Drawing.Size(826, 693);
            this.tabCounts.TabIndex = 0;
            this.tabCounts.Text = "תיקיות לספירה";
            this.tabCounts.UseVisualStyleBackColor = true;
            // 
            // grdCount
            // 
            this.grdCount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdCount.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdCount.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chb,
            this.dirs,
            this.methods});
            this.grdCount.Location = new System.Drawing.Point(0, 3);
            this.grdCount.Name = "grdCount";
            this.grdCount.RowHeadersVisible = false;
            this.grdCount.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdCount.Size = new System.Drawing.Size(830, 694);
            this.grdCount.TabIndex = 0;
            this.grdCount.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdCount_CellContentClick);
            this.grdCount.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdCount_CellEndEdit);
            this.grdCount.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdCount_CellValueChanged);
            // 
            // chb
            // 
            this.chb.DataPropertyName = "check";
            this.chb.HeaderText = "";
            this.chb.Name = "chb";
            this.chb.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.chb.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.chb.Width = 30;
            // 
            // dirs
            // 
            this.dirs.DataPropertyName = "dir";
            this.dirs.HeaderText = "תיקיות";
            this.dirs.Name = "dirs";
            this.dirs.ReadOnly = true;
            this.dirs.Width = 110;
            // 
            // methods
            // 
            this.methods.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.methods.DataPropertyName = "method";
            this.methods.HeaderText = "אופן שליחה";
            this.methods.Name = "methods";
            // 
            // tabDuplicates
            // 
            this.tabDuplicates.Controls.Add(this.DuplicateFolders);
            this.tabDuplicates.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tabDuplicates.Location = new System.Drawing.Point(4, 24);
            this.tabDuplicates.Name = "tabDuplicates";
            this.tabDuplicates.Padding = new System.Windows.Forms.Padding(3);
            this.tabDuplicates.Size = new System.Drawing.Size(826, 693);
            this.tabDuplicates.TabIndex = 1;
            this.tabDuplicates.Text = "תיקיות לכפילויות";
            this.tabDuplicates.UseVisualStyleBackColor = true;
            // 
            // DuplicateFolders
            // 
            this.DuplicateFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DuplicateFolders.CheckOnClick = true;
            this.DuplicateFolders.FormattingEnabled = true;
            this.DuplicateFolders.Location = new System.Drawing.Point(3, 6);
            this.DuplicateFolders.Name = "DuplicateFolders";
            this.DuplicateFolders.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DuplicateFolders.Size = new System.Drawing.Size(820, 679);
            this.DuplicateFolders.TabIndex = 2;
            // 
            // tabNames
            // 
            this.tabNames.Controls.Add(this.filenames);
            this.tabNames.Font = new System.Drawing.Font("Arial", 8.25F);
            this.tabNames.Location = new System.Drawing.Point(4, 24);
            this.tabNames.Name = "tabNames";
            this.tabNames.Size = new System.Drawing.Size(826, 693);
            this.tabNames.TabIndex = 2;
            this.tabNames.Text = "תיקון שמות";
            this.tabNames.UseVisualStyleBackColor = true;
            // 
            // filenames
            // 
            this.filenames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.filenames.CheckOnClick = true;
            this.filenames.FormattingEnabled = true;
            this.filenames.Location = new System.Drawing.Point(3, 0);
            this.filenames.Name = "filenames";
            this.filenames.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.filenames.Size = new System.Drawing.Size(805, 679);
            this.filenames.TabIndex = 3;
            // 
            // tabEEmail
            // 
            this.tabEEmail.Controls.Add(this.dataGridView1);
            this.tabEEmail.Font = new System.Drawing.Font("Arial", 8.25F);
            this.tabEEmail.Location = new System.Drawing.Point(4, 24);
            this.tabEEmail.Name = "tabEEmail";
            this.tabEEmail.Padding = new System.Windows.Forms.Padding(3);
            this.tabEEmail.Size = new System.Drawing.Size(826, 693);
            this.tabEEmail.TabIndex = 3;
            this.tabEEmail.Text = "יצירת מיילים";
            this.tabEEmail.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dir,
            this.check,
            this.email,
            this.method});
            this.dataGridView1.Location = new System.Drawing.Point(0, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(810, 685);
            this.dataGridView1.TabIndex = 16;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            // 
            // dir
            // 
            this.dir.DataPropertyName = "dir";
            this.dir.HeaderText = "תיקייה";
            this.dir.Name = "dir";
            this.dir.ReadOnly = true;
            this.dir.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dir.Width = 90;
            // 
            // check
            // 
            this.check.DataPropertyName = "check";
            this.check.HeaderText = "איחוד קבצים";
            this.check.Items.AddRange(new object[] {
            "איחוד-קצר",
            "בודד-זהה",
            "בודד-קצר"});
            this.check.Name = "check";
            this.check.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.check.Width = 85;
            // 
            // email
            // 
            this.email.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.email.DataPropertyName = "email";
            this.email.HeaderText = "מייל";
            this.email.Name = "email";
            // 
            // method
            // 
            this.method.DataPropertyName = "method";
            this.method.HeaderText = "אופן שליחה";
            this.method.Name = "method";
            this.method.Width = 120;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dirList);
            this.tabPage1.Font = new System.Drawing.Font("Arial", 8.25F);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(826, 693);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "שמות אקסל";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dirList
            // 
            this.dirList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dirList.CheckOnClick = true;
            this.dirList.FormattingEnabled = true;
            this.dirList.Location = new System.Drawing.Point(3, 2);
            this.dirList.Name = "dirList";
            this.dirList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dirList.Size = new System.Drawing.Size(806, 679);
            this.dirList.TabIndex = 4;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.DelList);
            this.tabPage2.Font = new System.Drawing.Font("Arial", 8.25F);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(826, 693);
            this.tabPage2.TabIndex = 5;
            this.tabPage2.Text = "מחיקת קבצים";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // DelList
            // 
            this.DelList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DelList.CheckOnClick = true;
            this.DelList.FormattingEnabled = true;
            this.DelList.Location = new System.Drawing.Point(5, 6);
            this.DelList.Name = "DelList";
            this.DelList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DelList.Size = new System.Drawing.Size(803, 679);
            this.DelList.TabIndex = 5;
            // 
            // tabReports
            // 
            this.tabReports.Controls.Add(this.label2);
            this.tabReports.Controls.Add(this.label1);
            this.tabReports.Controls.Add(this.txtLog);
            this.tabReports.Controls.Add(this.txtReportDest);
            this.tabReports.Controls.Add(this.reportFolders);
            this.tabReports.Font = new System.Drawing.Font("Arial", 8.25F);
            this.tabReports.Location = new System.Drawing.Point(4, 24);
            this.tabReports.Name = "tabReports";
            this.tabReports.Padding = new System.Windows.Forms.Padding(3);
            this.tabReports.Size = new System.Drawing.Size(826, 693);
            this.tabReports.TabIndex = 6;
            this.tabReports.Text = "שמות לדוחות";
            this.tabReports.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Guttman Frank", 9F);
            this.label2.ForeColor = System.Drawing.Color.DarkBlue;
            this.label2.Location = new System.Drawing.Point(733, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 16;
            this.label2.Text = "פירוט התהליך";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Guttman Frank", 9F);
            this.label1.ForeColor = System.Drawing.Color.DarkBlue;
            this.label1.Location = new System.Drawing.Point(733, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 15);
            this.label1.TabIndex = 15;
            this.label1.Text = "שם תקיית יעד";
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtLog.Location = new System.Drawing.Point(268, 91);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(552, 597);
            this.txtLog.TabIndex = 14;
            // 
            // txtReportDest
            // 
            this.txtReportDest.Location = new System.Drawing.Point(520, 37);
            this.txtReportDest.Name = "txtReportDest";
            this.txtReportDest.Size = new System.Drawing.Size(169, 20);
            this.txtReportDest.TabIndex = 4;
            // 
            // reportFolders
            // 
            this.reportFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.reportFolders.CheckOnClick = true;
            this.reportFolders.FormattingEnabled = true;
            this.reportFolders.Location = new System.Drawing.Point(3, 9);
            this.reportFolders.Name = "reportFolders";
            this.reportFolders.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.reportFolders.Size = new System.Drawing.Size(259, 679);
            this.reportFolders.TabIndex = 3;
            // 
            // lblProgressMessage
            // 
            this.lblProgressMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressMessage.AutoSize = true;
            this.lblProgressMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblProgressMessage.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblProgressMessage.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblProgressMessage.Location = new System.Drawing.Point(1003, 205);
            this.lblProgressMessage.Name = "lblProgressMessage";
            this.lblProgressMessage.Size = new System.Drawing.Size(78, 15);
            this.lblProgressMessage.TabIndex = 12;
            this.lblProgressMessage.Text = "מתקן כפילויות";
            this.lblProgressMessage.Visible = false;
            // 
            // boxSummary
            // 
            this.boxSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boxSummary.BackColor = System.Drawing.Color.WhiteSmoke;
            this.boxSummary.Controls.Add(this.lbl2);
            this.boxSummary.Controls.Add(this.lblDuplicateNum);
            this.boxSummary.Controls.Add(this.lbl1);
            this.boxSummary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.boxSummary.Font = new System.Drawing.Font("Guttman Frank", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.boxSummary.ForeColor = System.Drawing.Color.Navy;
            this.boxSummary.Location = new System.Drawing.Point(881, 381);
            this.boxSummary.Name = "boxSummary";
            this.boxSummary.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.boxSummary.Size = new System.Drawing.Size(200, 81);
            this.boxSummary.TabIndex = 13;
            this.boxSummary.TabStop = false;
            this.boxSummary.Text = "סיכום קבצים כפולים";
            this.boxSummary.Visible = false;
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.BackColor = System.Drawing.Color.Transparent;
            this.lbl2.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl2.ForeColor = System.Drawing.Color.DarkBlue;
            this.lbl2.Location = new System.Drawing.Point(19, 37);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(73, 15);
            this.lbl2.TabIndex = 15;
            this.lbl2.Text = "קבצים כפולים";
            this.lbl2.Visible = false;
            // 
            // lblDuplicateNum
            // 
            this.lblDuplicateNum.AutoSize = true;
            this.lblDuplicateNum.BackColor = System.Drawing.Color.Transparent;
            this.lblDuplicateNum.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblDuplicateNum.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblDuplicateNum.Location = new System.Drawing.Point(96, 37);
            this.lblDuplicateNum.Name = "lblDuplicateNum";
            this.lblDuplicateNum.Size = new System.Drawing.Size(27, 15);
            this.lblDuplicateNum.TabIndex = 14;
            this.lblDuplicateNum.Text = "1000";
            this.lblDuplicateNum.Visible = false;
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.BackColor = System.Drawing.Color.Transparent;
            this.lbl1.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl1.ForeColor = System.Drawing.Color.DarkBlue;
            this.lbl1.Location = new System.Drawing.Point(129, 37);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(62, 15);
            this.lbl1.TabIndex = 13;
            this.lbl1.Text = "סה\"כ תוקנו";
            this.lbl1.Visible = false;
            // 
            // btnMail
            // 
            this.btnMail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMail.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnMail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMail.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnMail.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnMail.Location = new System.Drawing.Point(952, 82);
            this.btnMail.Name = "btnMail";
            this.btnMail.Size = new System.Drawing.Size(129, 23);
            this.btnMail.TabIndex = 15;
            this.btnMail.Text = "יצירת מיילים";
            this.btnMail.UseVisualStyleBackColor = false;
            this.btnMail.Click += new System.EventHandler(this.btnMail_Click);
            // 
            // btnDel
            // 
            this.btnDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnDel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDel.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnDel.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnDel.Location = new System.Drawing.Point(952, 111);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(129, 23);
            this.btnDel.TabIndex = 15;
            this.btnDel.Text = "מחיקת קבצים";
            this.btnDel.UseVisualStyleBackColor = false;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1107, 745);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnMail);
            this.Controls.Add(this.boxSummary);
            this.Controls.Add(this.lblProgressMessage);
            this.Controls.Add(this.tabsMain);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.bStart);
            this.Controls.Add(this.bClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximumSize = new System.Drawing.Size(1200, 1200);
            this.MinimumSize = new System.Drawing.Size(613, 528);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "ערן מור ניהול קבצים";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabsMain.ResumeLayout(false);
            this.tabCounts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdCount)).EndInit();
            this.tabDuplicates.ResumeLayout(false);
            this.tabNames.ResumeLayout(false);
            this.tabEEmail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabReports.ResumeLayout(false);
            this.tabReports.PerformLayout();
            this.boxSummary.ResumeLayout(false);
            this.boxSummary.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bClose;
        private System.Windows.Forms.Button bStart;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RadioButton rCopy;
        private System.Windows.Forms.RadioButton rMove;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tabsMain;
        private System.Windows.Forms.TabPage tabCounts;
        private System.Windows.Forms.TabPage tabDuplicates;
        private System.Windows.Forms.CheckedListBox DuplicateFolders;
        private System.Windows.Forms.Label lblProgressMessage;
        private System.Windows.Forms.GroupBox boxSummary;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Label lblDuplicateNum;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.TabPage tabNames;
        private System.Windows.Forms.CheckedListBox filenames;
        private System.Windows.Forms.TabPage tabEEmail;
        private System.Windows.Forms.Button btnMail;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridView grdCount;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chb;
        private System.Windows.Forms.DataGridViewTextBoxColumn dirs;
        private System.Windows.Forms.DataGridViewTextBoxColumn methods;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckedListBox dirList;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckedListBox DelList;
        private System.Windows.Forms.TabPage tabReports;
        private System.Windows.Forms.DataGridViewTextBoxColumn dir;
        private System.Windows.Forms.DataGridViewComboBoxColumn check;
        private System.Windows.Forms.DataGridViewTextBoxColumn email;
        private System.Windows.Forms.DataGridViewTextBoxColumn method;
        private System.Windows.Forms.CheckedListBox reportFolders;
        private System.Windows.Forms.TextBox txtReportDest;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

