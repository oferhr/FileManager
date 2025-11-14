using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;

namespace FileManager.Services
{
    public class FolderSplitService : IFolderSplitService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly Action<int> _progressCallback;

        public FolderSplitService(string basePath, IFileService fileService, Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _progressCallback = progressCallback;
        }

        public void SplitFolders(List<SplitSettings> checkedItems)
        {
            if (checkedItems.Count == 0)
            {
                return;
            }

            try
            {
                foreach (var checkedItem in checkedItems)
                {
                    var basePath = Path.Combine(_basePath, checkedItem.dir);
                    var destPath = checkedItem.dest;
                    var availDirs = Directory.GetDirectories(basePath);

                    if (availDirs.Length > 0)
                    {
                        foreach (var cd in availDirs)
                        {
                            var checkdir = Path.GetFileNameWithoutExtension(cd);
                            if (checkdir == null || checkdir.Length > 2 || !Directory.Exists(cd))
                            {
                                continue;
                            }

                            var destDir = Path.Combine(destPath, checkdir);
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }

                            var files = Directory.GetFiles(cd, "*.*", SearchOption.TopDirectoryOnly);
                            if (!files.Any())
                            {
                                continue;
                            }

                            var counter = 1;
                            foreach (var file in files)
                            {
                                counter++;
                                var fname = Path.GetFileName(file);
                                if (fname == null)
                                {
                                    continue;
                                }

                                if (IsContain888(fname))
                                {
                                    try
                                    {
                                        var destFile = Path.Combine(destDir, fname);
                                        var sFile = Path.Combine(cd, fname);
                                        
                                        if (File.Exists(destFile))
                                        {
                                            MessageBox.Show($"פיצול תיקיות - שם קובץ כפול - {destFile}");
                                            continue;
                                        }
                                        
                                        _fileService.MoveFiles(sFile, destFile);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show($"נכשל בהעברת קובץ {file}----{e.Message}");
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

                                var val = counter / files.Length * 100;
                                if (val > 100)
                                {
                                    val = 100;
                                }
                                _progressCallback?.Invoke(val);
                            }

                            var destFiles = Directory.GetFiles(destDir, "*.*", SearchOption.TopDirectoryOnly);
                            foreach (var df in destFiles)
                            {
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
                                    
                                    var destFile = Path.Combine(destDir, dnewName + dext);
                                    _fileService.MoveFiles(df, destFile);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"תקלה בפיצול תיקיות - {ex.Message}");
            }

            _progressCallback?.Invoke(100);
        }

        public bool IsContain888(string fname)
        {
            return fname.Contains("-888-") || fname.Contains("_888_") || fname.Contains("_888-") ||
                   fname.Contains("-888_") || fname.Contains("888_") || fname.Contains("_888") ||
                   fname.Contains("-888") || fname.Contains("888-");
        }
    }
}
