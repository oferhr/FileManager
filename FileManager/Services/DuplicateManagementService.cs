using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FileManager;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class DuplicateManagementService : IDuplicateManagementService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;
        private const string LtrMark = "\u200E";

        public DuplicateManagementService(
            string basePath,
            IFileService fileService,
            IConfigurationService configurationService,
            ILoggingService loggingService,
            Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _configurationService = configurationService;
            _loggingService = loggingService;
            _progressCallback = progressCallback;
        }

        // CheckedListBox management methods
        public void AddFolderToDuplicateFolders(string folder)
        {
            // This method will be called from Form1 to add folders to the CheckedListBox
            // The actual UI update will be handled in Form1
        }

        public void LoadCheckedDuplicateFolders(CheckedListBox duplicateFolders)
        {
            var folderSettings = _configurationService.GetFolderSettings();
            var duplicatesFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First().duplicatesFolders;
            
            if (!String.IsNullOrEmpty(duplicatesFolderSettings))
            {
                var folders = duplicatesFolderSettings.Split(',');
                for (var i = 0; i <= (duplicateFolders.Items.Count - 1); i++)
                {
                    foreach (var fol in folders)
                    {
                        if (duplicateFolders.Items[i].ToString() == fol)
                        {
                            duplicateFolders.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }

        public void SaveSelectedDuplicateFolders(CheckedListBox.CheckedItemCollection checkedItems)
        {
            var items = String.Empty;
            foreach (var checkedItem in checkedItems)
            {
                //set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }

            var folderSettings = _configurationService.GetFolderSettings();
            if (folderSettings.Count > 0)
            {
                folderSettings.First().duplicatesFolders = items.TrimEnd(',');
            }
            else
            {
                folderSettings.Add(new FolderSettings
                {
                    duplicatesFolders = items.TrimEnd(','),
                    fileNamesFolders = string.Empty,
                    selectedFolders = string.Empty,
                    deleteFolders = string.Empty,
                    reportsFolders = string.Empty
                });
            }
            _configurationService.SetFolderSettings(folderSettings);
        }

        public int FixDuplicates(List<string> checkedItems)
        {
            var numOfDuplicates = 0;
            var percent = checkedItems.Count == 0 ? 100 : Convert.ToInt32(Math.Round(95.0 / checkedItems.Count, 0));

            foreach (var checkedItem in checkedItems)
            {
                var combinedPath = Path.Combine(_basePath, checkedItem);
                string basePath, errorMessage;
                if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                {
                    _loggingService.LogSecurityEvent($"Path traversal attempt blocked in DuplicateManagementService.FixDuplicates: {errorMessage}");
                    continue;
                }

                var allAvailDirs = Directory.GetDirectories(basePath);
                var availDirs = allAvailDirs.Where(w =>
                {
                    var dn = Path.GetFileName(w);
                    return dn != null && dn.Length < 3;
                }).ToArray();

                if (availDirs.Length > 0)
                {
                    try
                    {
                        var allFilesPath = ProcessFirstFolder(availDirs);
                        if (!string.IsNullOrEmpty(allFilesPath))
                        {
                            numOfDuplicates += ProcessDuplicateFolders(availDirs, allFilesPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("FixDuplicates", ex, "בעיה בשינוי שם הקובץ");
                    }
                }

                UpdateProgress(percent);
            }

            // Clean up empty folders
            CleanupEmptyFolders(checkedItems);

            return numOfDuplicates;
        }

        private string ProcessFirstFolder(string[] availDirs)
        {
            var counter = 1;
            foreach (var cd in availDirs)
            {
                var checkdir = Path.GetFileName(cd);
                if (checkdir == null || checkdir.Length > 2 || !Directory.Exists(cd))
                {
                    continue;
                }

                var folder = Path.GetFileName(cd);
                if (folder == counter.ToString())
                {
                    if (counter == 1)
                    {
                        var lfiles = Directory.GetFiles(cd, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                        var files = lfiles as IList<string> ?? lfiles.ToList();
                        
                        if (files.Any())
                        {
                            if (CheckFolderIfNotFirstDuplication(files, cd))
                            {
                                return cd;
                            }

                            foreach (var file in files)
                            {
                                var fileName = Path.GetFileName(file);
                                if (fileName == null || _fileService.IsThumbsInPath(file))
                                {
                                    continue;
                                }

                                var newName = _fileService.GetNewFileName(fileName, 1);
                                if (string.IsNullOrEmpty(newName))
                                {
                                    continue;
                                }

                                if (!File.Exists(Path.Combine(cd, newName)))
                                {
                                    _fileService.MoveFiles(Path.Combine(cd, fileName), Path.Combine(cd, newName));
                                }
                            }
                        }
                        return cd;
                    }
                }
            }
            return null;
        }

        private int ProcessDuplicateFolders(string[] availDirs, string allFilesPath)
        {
            var numOfDuplicates = 0;
            var counter = 1;

            for (var i = 0; i < availDirs.Length; i++)
            {
                var curAvailDir = availDirs[i];
                if (!Directory.Exists(curAvailDir))
                {
                    continue;
                }

                var checkdir = Path.GetFileName(curAvailDir);
                if (checkdir == "1")
                {
                    counter++;
                    continue;
                }

                if (checkdir == null || checkdir.Length > 2)
                {
                    continue;
                }

                var llfiles = Directory.GetFiles(curAvailDir, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                var ffiles = llfiles as IList<string> ?? llfiles.ToList();

                if (ffiles.Any())
                {
                    try
                    {
                        foreach (var xfile in ffiles)
                        {
                            var fileName = Path.GetFileName(xfile);
                            if (fileName == null || _fileService.IsThumbsInPath(xfile))
                            {
                                continue;
                            }

                            var newName = _fileService.GetNewFileName(fileName, 1);
                            if (string.IsNullOrEmpty(newName))
                            {
                                continue;
                            }

                            if (File.Exists(Path.Combine(allFilesPath, newName)))
                            {
                                var miniCounter = 2;
                                var isCopy = false;
                                while (!isCopy)
                                {
                                    var copyName = _fileService.GetNewFileName(fileName, miniCounter);
                                    if (File.Exists(Path.Combine(allFilesPath, copyName)))
                                    {
                                        miniCounter++;
                                    }
                                    else
                                    {
                                        isCopy = true;
                                        numOfDuplicates++;
                                        _fileService.CopyFiles(xfile, Path.Combine(allFilesPath, copyName));
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"file name : \r{fileName}\r" +
                                                "exist in duplicated folder but not in base folder");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("FixDuplicates", ex, "בעיה בהעתקת הקובץ");
                    }
                }
                counter++;
            }

            return numOfDuplicates;
        }

        private void CleanupEmptyFolders(List<string> checkedItems)
        {
            foreach (var checkedItem in checkedItems)
            {
                var combinedPath = Path.Combine(_basePath, checkedItem);
                string basePath, errorMessage;
                if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                {
                    _loggingService.LogSecurityEvent($"Path traversal attempt blocked in DuplicateManagementService.CleanupEmptyFolders: {errorMessage}");
                    continue;
                }

                var allAvailDirs = Directory.GetDirectories(basePath);
                var availDirs = allAvailDirs.Where(w =>
                {
                    var dn = Path.GetFileName(w);
                    return dn != null && dn.Length < 3;
                }).ToArray();

                if (availDirs.Length > 1)
                {
                    try
                    {
                        for (var i = 0; i < availDirs.Length; i++)
                        {
                            if (!Directory.Exists(availDirs[i]))
                            {
                                continue;
                            }

                            var fileName = Path.GetFileName(availDirs[i]);
                            if (fileName == "1" || fileName.Length > 2)
                            {
                                continue;
                            }

                            var lfiles = Directory.GetFiles(availDirs[i], "*.*", SearchOption.TopDirectoryOnly);
                            var files = lfiles as IList<string> ?? lfiles.ToList();

                            if (!files.Any())
                            {
                                DeleteDirectory(availDirs[i]);
                            }
                            else if (files.Count == 1 && _fileService.IsThumbsInPath(files[0]))
                            {
                                try
                                {
                                    _loggingService.LogFileOperation("DELETE", files[0]);
                                    File.SetAttributes(files[0], FileAttributes.Normal);
                                    File.Delete(files[0]);
                                    DeleteDirectory(availDirs[i]);
                                }
                                catch (Exception ex)
                                {
                                    _loggingService.LogError($"Error deleting thumbs file: {files[0]}", ex);
                                    LogError("FixDuplicates", ex, "בעיה במחיקת הקובץ");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("FixDuplicates", ex, "בעיה במחיקת הקובץ");
                    }
                }
            }
        }

        private void DeleteDirectory(string path)
        {
            // Validate path outside try block so normalizedPath is accessible in catch blocks
            string normalizedPath, errorMessage;
            if (!PathValidator.ValidateAndNormalize(path, _basePath, out normalizedPath, out errorMessage))
            {
                _loggingService.LogSecurityEvent($"Attempted to delete directory outside boundary: {errorMessage}");
                return;
            }

            try
            {
                _loggingService.LogFileOperation("DELETE_DIR", normalizedPath);
                Directory.Delete(normalizedPath);
            }
            catch (IOException ex)
            {
                _loggingService.LogInfo($"Directory not empty, attempting recursive delete: {normalizedPath}");
                Directory.Delete(normalizedPath, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                _loggingService.LogWarning($"Unauthorized access, attempting recursive delete: {normalizedPath}");
                Directory.Delete(normalizedPath, true);
            }
        }

        public bool CheckFolderIfNotFirstDuplication(IEnumerable<string> files, string dir)
        {
            var isDuplicated = false;
            var newFiles = new List<string>();
            var duplicatedFiles = new List<string>();
            var r = new Regex(@".+?[_][0-9]{1,2}$");

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

                    var matcher = new Regex(@"^" + noExt + "[_][0-9]{1,2}$");
                    var matcher2 = new Regex(@"^" + noExt + LtrMark + @"[ \d]{1,2}$");
                    var sameFiles = new List<string>();

                    sameFiles.AddRange(duplicatedFiles.FindAll(matcher.IsMatch));
                    sameFiles.AddRange(duplicatedFiles.FindAll(matcher2.IsMatch));

                    if (sameFiles.Count > 0)
                    {
                        var miniCounter = 2;
                        var isCopy = false;
                        while (!isCopy)
                        {
                            var copyName = _fileService.GetNewFileName(fileName, miniCounter);
                            if (File.Exists(Path.Combine(dir, copyName)))
                            {
                                miniCounter++;
                            }
                            else
                            {
                                isCopy = true;
                                _fileService.MoveFiles(file, Path.Combine(dir, copyName));
                            }
                        }
                    }
                    else
                    {
                        var newName = _fileService.GetNewFileName(fileName, 1);
                        _fileService.MoveFiles(file, Path.Combine(dir, newName));
                    }
                }
            }

            return isDuplicated;
        }

        private void UpdateProgress(int percent)
        {
            _progressCallback?.Invoke(percent);
        }

        private void LogError(string method, Exception ex, string message)
        {
            // This would typically use a logging service
            // For now, we'll just show a message box
            MessageBox.Show($"Error in {method}: {ex.Message}");
        }
    }
}
