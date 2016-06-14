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
            this.FileList = new System.Windows.Forms.CheckedListBox();
            this.bStart = new System.Windows.Forms.Button();
            this.bSelectAll = new System.Windows.Forms.Button();
            this.bUnselect = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.rCopy = new System.Windows.Forms.RadioButton();
            this.rMove = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabsMain = new System.Windows.Forms.TabControl();
            this.tabCounts = new System.Windows.Forms.TabPage();
            this.tabDuplicates = new System.Windows.Forms.TabPage();
            this.DuplicateFolders = new System.Windows.Forms.CheckedListBox();
            this.tabNames = new System.Windows.Forms.TabPage();
            this.filenames = new System.Windows.Forms.CheckedListBox();
            this.lblProgressMessage = new System.Windows.Forms.Label();
            this.boxSummary = new System.Windows.Forms.GroupBox();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lblDuplicateNum = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.bMigdal = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tabsMain.SuspendLayout();
            this.tabCounts.SuspendLayout();
            this.tabDuplicates.SuspendLayout();
            this.tabNames.SuspendLayout();
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
            this.bClose.Location = new System.Drawing.Point(496, 487);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(75, 23);
            this.bClose.TabIndex = 0;
            this.bClose.Text = "סגור";
            this.bClose.UseVisualStyleBackColor = false;
            this.bClose.Click += new System.EventHandler(this.bClose_Click);
            // 
            // FileList
            // 
            this.FileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileList.CheckOnClick = true;
            this.FileList.FormattingEnabled = true;
            this.FileList.Location = new System.Drawing.Point(3, 6);
            this.FileList.Name = "FileList";
            this.FileList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FileList.Size = new System.Drawing.Size(323, 394);
            this.FileList.TabIndex = 1;
            // 
            // bStart
            // 
            this.bStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bStart.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bStart.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.bStart.ForeColor = System.Drawing.Color.DarkBlue;
            this.bStart.Location = new System.Drawing.Point(442, 62);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(129, 23);
            this.bStart.TabIndex = 2;
            this.bStart.Text = "התחל";
            this.bStart.UseVisualStyleBackColor = false;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // bSelectAll
            // 
            this.bSelectAll.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bSelectAll.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.bSelectAll.ForeColor = System.Drawing.Color.DarkBlue;
            this.bSelectAll.Location = new System.Drawing.Point(12, 12);
            this.bSelectAll.Name = "bSelectAll";
            this.bSelectAll.Size = new System.Drawing.Size(75, 23);
            this.bSelectAll.TabIndex = 3;
            this.bSelectAll.Text = "בחר הכל";
            this.bSelectAll.UseVisualStyleBackColor = false;
            this.bSelectAll.Click += new System.EventHandler(this.bSelectAll_Click);
            // 
            // bUnselect
            // 
            this.bUnselect.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bUnselect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bUnselect.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.bUnselect.ForeColor = System.Drawing.Color.DarkBlue;
            this.bUnselect.Location = new System.Drawing.Point(162, 12);
            this.bUnselect.Name = "bUnselect";
            this.bUnselect.Size = new System.Drawing.Size(75, 23);
            this.bUnselect.TabIndex = 4;
            this.bUnselect.Text = "נקה";
            this.bUnselect.UseVisualStyleBackColor = false;
            this.bUnselect.Click += new System.EventHandler(this.bUnselect_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(371, 170);
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
            this.groupBox1.Location = new System.Drawing.Point(371, 219);
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
            this.tabsMain.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tabsMain.Location = new System.Drawing.Point(12, 53);
            this.tabsMain.Name = "tabsMain";
            this.tabsMain.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tabsMain.RightToLeftLayout = true;
            this.tabsMain.SelectedIndex = 0;
            this.tabsMain.Size = new System.Drawing.Size(340, 455);
            this.tabsMain.TabIndex = 11;
            // 
            // tabCounts
            // 
            this.tabCounts.Controls.Add(this.FileList);
            this.tabCounts.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tabCounts.Location = new System.Drawing.Point(4, 24);
            this.tabCounts.Name = "tabCounts";
            this.tabCounts.Padding = new System.Windows.Forms.Padding(3);
            this.tabCounts.Size = new System.Drawing.Size(332, 427);
            this.tabCounts.TabIndex = 0;
            this.tabCounts.Text = "תיקיות לספירה";
            this.tabCounts.UseVisualStyleBackColor = true;
            // 
            // tabDuplicates
            // 
            this.tabDuplicates.Controls.Add(this.DuplicateFolders);
            this.tabDuplicates.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tabDuplicates.Location = new System.Drawing.Point(4, 24);
            this.tabDuplicates.Name = "tabDuplicates";
            this.tabDuplicates.Padding = new System.Windows.Forms.Padding(3);
            this.tabDuplicates.Size = new System.Drawing.Size(332, 427);
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
            this.DuplicateFolders.Size = new System.Drawing.Size(323, 409);
            this.DuplicateFolders.TabIndex = 2;
            // 
            // tabNames
            // 
            this.tabNames.Controls.Add(this.filenames);
            this.tabNames.Location = new System.Drawing.Point(4, 24);
            this.tabNames.Name = "tabNames";
            this.tabNames.Size = new System.Drawing.Size(332, 427);
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
            this.filenames.Location = new System.Drawing.Point(5, 13);
            this.filenames.Name = "filenames";
            this.filenames.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.filenames.Size = new System.Drawing.Size(323, 400);
            this.filenames.TabIndex = 3;
            // 
            // lblProgressMessage
            // 
            this.lblProgressMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressMessage.AutoSize = true;
            this.lblProgressMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblProgressMessage.Font = new System.Drawing.Font("Guttman Frank", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblProgressMessage.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblProgressMessage.Location = new System.Drawing.Point(493, 152);
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
            this.boxSummary.Location = new System.Drawing.Point(371, 328);
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
            // bMigdal
            // 
            this.bMigdal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bMigdal.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bMigdal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bMigdal.Font = new System.Drawing.Font("Guttman Frank", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.bMigdal.ForeColor = System.Drawing.Color.DarkBlue;
            this.bMigdal.Location = new System.Drawing.Point(442, 104);
            this.bMigdal.Name = "bMigdal";
            this.bMigdal.Size = new System.Drawing.Size(129, 23);
            this.bMigdal.TabIndex = 14;
            this.bMigdal.Text = "מגדל";
            this.bMigdal.UseVisualStyleBackColor = false;
            this.bMigdal.Click += new System.EventHandler(this.bMigdal_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 520);
            this.Controls.Add(this.bMigdal);
            this.Controls.Add(this.boxSummary);
            this.Controls.Add(this.lblProgressMessage);
            this.Controls.Add(this.tabsMain);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.bUnselect);
            this.Controls.Add(this.bSelectAll);
            this.Controls.Add(this.bStart);
            this.Controls.Add(this.bClose);
            this.MaximumSize = new System.Drawing.Size(613, 800);
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
            this.tabDuplicates.ResumeLayout(false);
            this.tabNames.ResumeLayout(false);
            this.boxSummary.ResumeLayout(false);
            this.boxSummary.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bClose;
        private System.Windows.Forms.CheckedListBox FileList;
        private System.Windows.Forms.Button bStart;
        private System.Windows.Forms.Button bSelectAll;
        private System.Windows.Forms.Button bUnselect;
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
        private System.Windows.Forms.Button bMigdal;
    }
}

