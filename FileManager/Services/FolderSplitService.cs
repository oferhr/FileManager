using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class FolderSplitService : IFolderSplitService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;

        public FolderSplitService(string basePath, IFileService fileService, ILoggingService loggingService, Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _loggingService = loggingService;
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
                    // Validate source path
                    var combinedPath = Path.Combine(_basePath, checkedItem.dir);
                    string basePath, errorMessage;
                    if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                    {
                        _loggingService.LogSecurityEvent($"Path traversal attempt blocked in FolderSplitService source: {errorMessage}");
                        continue;
                    }

                    // Validate destination path
                    string destPath, destError;
                    if (!PathValidator.ValidateAndNormalize(checkedItem.dest, null, out destPath, out destError))
                    {
                        _loggingService.LogSecurityEvent($"Invalid destination path in FolderSplitService: {destError}");
                        continue;
                    }
                    // Note: destPath can be outside _basePath as it's a split destination

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

                            var combinedDestDir = Path.Combine(destPath, checkdir);
                            string destDir, destDirError;
                            if (!PathValidator.ValidateAndNormalize(combinedDestDir, null, out destDir, out destDirError))
                            {
                                _loggingService.LogSecurityEvent($"Invalid destination directory in FolderSplitService: {destDirError}");
                                continue;
                            }

                            if (!Directory.Exists(destDir))
                            {
                                _loggingService.LogFileOperation("CREATE_DIR", destDir);
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
                                        string destFile, destFileError, sFile, sFileError;
                                        bool destValid = PathValidator.ValidateAndNormalize(Path.Combine(destDir, fname), null, out destFile, out destFileError);
                                        bool sourceValid = PathValidator.ValidateAndNormalize(Path.Combine(cd, fname), _basePath, out sFile, out sFileError);

                                        if (!destValid || !sourceValid)
                                        {
                                            var errorMsg = !destValid ? destFileError : sFileError;
                                            _loggingService.LogSecurityEvent($"Path validation failed for folder split: {errorMsg}");
                                            continue;
                                        }

                                        if (File.Exists(destFile))
                                        {
                                            _loggingService.LogInfo($"Duplicate file name detected during folder split: {destFile}");
                                            MessageBox.Show($"פיצול תיקיות - שם קובץ כפול - {destFile}");
                                            continue;
                                        }

                                        _loggingService.LogFileOperation("MOVE", $"{sFile} -> {destFile}");
                                        _fileService.MoveFiles(sFile, destFile);
                                    }
                                    catch (Exception e)
                                    {
                                        _loggingService.LogError($"Failed to move file during folder split: {file}", e);
                                        MessageBox.Show($"נכשל בהעברת קובץ {file}----{e.Message}");
                                    }
                                }

                                if (fname.Contains("777"))
                                {
                                    if (File.Exists(file))
                                    {
                                        _loggingService.LogFileOperation("DELETE", file);
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

                                    var combinedDestFile = Path.Combine(destDir, dnewName + dext);
                                    string destFile, destFileError;
                                    if (!PathValidator.ValidateAndNormalize(combinedDestFile, null, out destFile, out destFileError))
                                    {
                                        _loggingService.LogSecurityEvent($"Invalid rename destination: {destFileError}");
                                        continue;
                                    }

                                    _loggingService.LogFileOperation("RENAME", $"{df} -> {destFile}");
                                    _fileService.MoveFiles(df, destFile);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("General error in folder split operation", ex);
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
