using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;

namespace FileManager.Services
{
    public class FileCopyService : IFileCopyService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly Action<int> _progressCallback;

        public FileCopyService(string basePath, IFileService fileService, Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _progressCallback = progressCallback;
        }

        public void CopyFiles(List<CopySettings> checkedItems)
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
                    var str = checkedItem.str;
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

                                if (fname.Contains(str))
                                {
                                    try
                                    {
                                        var destFile = Path.Combine(destDir, fname);
                                        var sFile = Path.Combine(cd, fname);

                                        if (File.Exists(destFile))
                                        {
                                            MessageBox.Show($"שיכפול קבצים - שם קובץ כפול - {destFile}");
                                            continue;
                                        }

                                        File.Copy(sFile, destFile);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show($"נכשל בהעברת קובץ {file}----{e.Message}");
                                    }
                                }

                                var val = counter / files.Length * 100;
                                if (val > 100)
                                {
                                    val = 100;
                                }
                                _progressCallback?.Invoke(val);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"תקלה בשיכפול קבצים - {ex.Message}");
            }

            _progressCallback?.Invoke(100);
        }
    }
}
