using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FileManager.Properties;

namespace FileManager
{
    

    public partial class Form1 : Form
    {
        private readonly string _path;
        private bool _isMove = true;
        private int _numOfDuplicates;
        const string LtrMark = "\u200E"; 
        public Form1()
        {
            InitializeComponent();

            this.MaximumSize = new Size(Size.Width, (Screen.PrimaryScreen.Bounds.Height - 50));

            try
            {
                _path = ConfigurationManager.AppSettings["basePath"];
                string[] dirs;
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
                foreach (string dir in dirs)
                {
                    string folder = Path.GetFileName(dir);
                    if (folder != null)
                    {
                        FileList.Items.Add(folder);
                        DuplicateFolders.Items.Add(folder);
                        filenames.Items.Add(folder);
                    }
                }
                // checking if the user has some saved checked folder list
                string countFolderSettings = Settings.Default.selectedFolders;
                if (!String.IsNullOrEmpty(countFolderSettings))
                {
                    string[] folders = countFolderSettings.Split(',');
                    for (int i = 0; i <= (FileList.Items.Count - 1); i++)
                    {
                        foreach (var fol in folders)
                        {
                            if (FileList.Items[i].ToString() == fol)
                            {
                                FileList.SetItemChecked(i, true);
                            }
                        }

                    }
                }
                string duplicatesFolderSettings = Settings.Default.duplicatesFolders;
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
                string fileNamesFolderSettings = Settings.Default.fileNamesFolders;
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
            }
            catch (Exception ex)
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
            boxSummary.Visible = false;
            Application.DoEvents();
            CountFiles();
        }
        private void bMigdal_Click(object sender, EventArgs e)
        {
            boxSummary.Visible = false;
            Application.DoEvents();
            FixFileNames();
        }
        private void SetSelectedDuplicatesFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }
            Settings.Default.duplicatesFolders = items.TrimEnd(',');
            Settings.Default.Save();
            Settings.Default.Reload();
        }

        private void SetSelectedFileNameFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            string items = String.Empty;
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }
            Settings.Default.fileNamesFolders = items.TrimEnd(',');
            Settings.Default.Save();
            Settings.Default.Reload();
        }
        
        private List<FileCount> SetSelectedItems(CheckedListBox.CheckedItemCollection checkedItems, bool useProgressBar)
        {
            var list = new List<FileCount>();
            string items = String.Empty;
            int percent = 0;
            if (useProgressBar)
            {
                var itemCount = checkedItems.Count;
                percent = Convert.ToInt32(Math.Round(100.0 / itemCount, 0));
            }
            //loop over the checked items
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";

                var lfiles = Directory.GetFiles(Path.Combine(_path, checkedItem.ToString()), "*.*",
                    SearchOption.AllDirectories)
                     .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".pdf"));
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
                        FileName = checkedItem.ToString(),
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
            Settings.Default.selectedFolders = items.TrimEnd(',');
            Settings.Default.Save();
            Settings.Default.Reload();
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

        private void bSelectAll_Click(object sender, EventArgs e)
        {
            bool isCounts = tabsMain.SelectedIndex == 0;
            bool isDup = tabsMain.SelectedIndex == 1;
            //bool isFileNames = tabsMain.SelectedIndex == 2;
            for (int i = 0; i <= (FileList.Items.Count - 1); i++)
            {
                if (isCounts)
                    FileList.SetItemChecked(i, true);
                else if (isDup)
                    DuplicateFolders.SetItemChecked(i, true);
                else
                    filenames.SetItemChecked(i, true);
            }
        }

        private void bUnselect_Click(object sender, EventArgs e)
        {
            bool isCounts = tabsMain.SelectedIndex == 0;
            bool isDup = tabsMain.SelectedIndex == 1;
            for (int i = 0; i <= (FileList.Items.Count - 1); i++)
            {
                if (isCounts)
                    FileList.SetItemChecked(i, false);
                else if (isDup)
                    DuplicateFolders.SetItemChecked(i, false);
                else
                    filenames.SetItemChecked(i, false);
            }
        }

        private void CountFiles()
        {
            try
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                lblProgressMessage.Text = "סופר קבצים";
                lblProgressMessage.Visible = true;
                Application.DoEvents();
                var checkedItems = FileList.CheckedItems;
                if (checkedItems.Count == 0)
                {
                    FixDuplicates();
                    FixFileNames();
                    return;
                }
                
                var list = SetSelectedItems(checkedItems, true);
                FixDuplicates();
                FixFileNames();
                //open the gris form with the selected data.
                var grid = new ResultGrid(list);
                grid.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("CountFiles Error :\r" + ex.Message);
            }
        }

        private void FixDuplicates()
        {
            try
            {
                string[] dirs;
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
                int percent = Convert.ToInt32(Math.Round(95.0 / itemCount, 0));
                foreach (var checkedItem in checkedItems)
                {
                    int counter = 1;
                    // get the base path of each folder
                    string basePath = Path.Combine(_path, checkedItem.ToString());
                    string allFilesPath = String.Empty;
                    //get all sub folders of base folder
                    dirs = Directory.GetDirectories(basePath);
                    if (dirs.Length > 0)
                    {
                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (string dir in dirs)
                        {
                            string folder = Path.GetFileName(dir);
                            if (folder == counter.ToString())
                            {
                                // if it is the first folder (dir/1) rename all files 
                                // from fileName.xxx to fileName_1.xxx
                                if (counter == 1)
                                {
                                    var lfiles = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                                         .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".pdf"));
                                    var files = lfiles as IList<string> ?? lfiles.ToList();
                                    if (files.Any())
                                    {
                                        if (CheckFolderIfNotFirstDuplication(files, dir))
                                        {
                                            allFilesPath = dir;
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
                                           if(!File.Exists(Path.Combine(dir, newName)))
                                           {
                                               move(Path.Combine(dir, fileName),
                                                   Path.Combine(dir, newName));
                                           }
                                            
                                        }
                                    }
                                    allFilesPath = dir;
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
                        foreach (string dir in dirs)
                        {
                            // if the folder is the base folder (dir/1) continue - check all duplicated folders only
                            if (counter == 1)
                            {
                                counter++;
                                continue;
                            }
                            var lfiles = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".pdf"));
                            var files = lfiles as IList<string> ?? lfiles.ToList();
                            if (files.Any())
                            {
                                //run over the files in the duplicated folder (dir/2...)
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
                                                CopyFiles(file,
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
                    dirs = Directory.GetDirectories(basePath);
                    if (dirs.Length > 1)
                    {
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            string fileName = Path.GetFileName(dirs[i]);
                            if (fileName == "1")
                            {
                                continue;
                            }
                            // delete all remaining files so get all files not only tif and pdf
                            var lfiles = Directory.GetFiles(dirs[i], "*.*", SearchOption.TopDirectoryOnly);
                            var files = lfiles as IList<string> ?? lfiles.ToList();
                            if (!files.Any())
                            {
                                try
                                {
                                    Directory.Delete(dirs[i]);
                                }
                                catch (IOException)
                                {
                                    Directory.Delete(dirs[i], true);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Directory.Delete(dirs[i], true);
                                }
                            }
                            else if (files.Count == 1 && IsThumbsInPath(files[0]))
                            {
                                try
                                {
                                    File.SetAttributes(files[0], FileAttributes.Normal);
                                    File.Delete(files[0]);
                                    Directory.Delete(dirs[i]);
                                }
                                catch (IOException)
                                {
                                    Directory.Delete(dirs[i], true);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Directory.Delete(dirs[i], true);
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
            catch (Exception ex)
            {
                MessageBox.Show("FixDuplicates Error :\r" + ex.Message);
            }

        }

        private void FixFileNames()
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
                int percent = Convert.ToInt32(Math.Round(95.0/itemCount, 0));
                int progress = 0;
                //loop over the checked items
                foreach (var checkedItem in checkedItems)
                {
                    string basePath = Path.Combine(_path, checkedItem.ToString());
                    var dirs = Directory.GetDirectories(basePath);
                    if (dirs.Length > 0)
                    {
                        // run over the array to get the dir/1 folder path where all the files are
                        foreach (string dir in dirs)
                        {
                            var lfiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                               .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".pdf"));

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
                                        if (i == 0)
                                        {
                                            newName += part + "-";
                                        }
                                        else
                                        {
                                            newName += part + "_";
                                        }

                                    }
                                }
                                move(Path.Combine(dir, file), Path.Combine(dir, newName.TrimEnd('_') + "." + extSplits[1]));
                            }
                            
                        }
                    }
                    progress += percent;
                    progressBar1.Value = progress;
                }
                progressBar1.Value = 100;
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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

        
        

       

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            try
            {
                using (var brush = new LinearGradientBrush(ClientRectangle,
                                                                       Color.WhiteSmoke,
                                                                       Color.SteelBlue,
                                                                       90F))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
            catch 
            {
                
              
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void rMove_CheckedChanged(object sender, EventArgs e)
        {
            _isMove = rMove.Checked;
        }

        


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
