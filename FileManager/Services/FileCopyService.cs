using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class FileCopyService : IFileCopyService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;

        public FileCopyService(string basePath, IFileService fileService, ILoggingService loggingService, Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _loggingService = loggingService;
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
                    // Validate source path
                    var combinedPath = Path.Combine(_basePath, checkedItem.dir);
                    string basePath, errorMessage;
                    if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                    {
                        _loggingService.LogSecurityEvent($"Path traversal attempt blocked in FileCopyService source: {errorMessage}");
                        continue;
                    }

                    // Validate destination path
                    string destPath, destError;
                    if (!PathValidator.ValidateAndNormalize(checkedItem.dest, null, out destPath, out destError))
                    {
                        _loggingService.LogSecurityEvent($"Invalid destination path in FileCopyService: {destError}");
                        continue;
                    }
                    // Note: destPath can be outside _basePath as it's a copy destination

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

                            var combinedDestDir = Path.Combine(destPath, checkdir);
                            string destDir, destDirError;
                            if (!PathValidator.ValidateAndNormalize(combinedDestDir, null, out destDir, out destDirError))
                            {
                                _loggingService.LogSecurityEvent($"Invalid destination directory: {destDirError}");
                                continue;
                            }

                            if (!Directory.Exists(destDir))
                            {
                                try
                                {
                                    Directory.CreateDirectory(destDir);
                                    _loggingService.LogFileOperation("CREATE_DIR", destDir, true);
                                }
                                catch (Exception ex)
                                {
                                    _loggingService.LogFileOperation("CREATE_DIR", destDir, false, ex.Message);
                                    _loggingService.LogError($"Failed to create directory: {destDir}", ex);
                                    continue; // Skip this directory and continue with others
                                }
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
                                    string destFile = string.Empty, destFileError = string.Empty, sFile = string.Empty, sFileError = string.Empty;
                                    try
                                    {
                                        
                                        bool destValid = PathValidator.ValidateAndNormalize(Path.Combine(destDir, fname), null, out destFile, out destFileError);
                                        bool sourceValid = PathValidator.ValidateAndNormalize(Path.Combine(cd, fname), _basePath, out sFile, out sFileError);

                                        if (!destValid || !sourceValid)
                                        {
                                            var errorMsg = !destValid ? destFileError : sFileError;
                                            _loggingService.LogSecurityEvent($"Path validation failed for file copy: {errorMsg}");
                                            continue;
                                        }

                                        if (File.Exists(destFile))
                                        {
                                            _loggingService.LogInfo($"Duplicate file name detected during copy: {destFile}");
                                            MessageBox.Show($"שיכפול קבצים - שם קובץ כפול - {destFile}");
                                            continue;
                                        }

                                        File.Copy(sFile, destFile);
                                        _loggingService.LogFileOperation("COPY", $"{sFile} -> {destFile}", true);
                                    }
                                    catch (Exception e)
                                    {
                                        _loggingService.LogFileOperation("COPY", $"{sFile} -> {destFile}", false, e.Message);
                                        _loggingService.LogError($"Failed to copy file {file}", e);
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
                _loggingService.LogError("General error in file copy operation", ex);
                MessageBox.Show($"תקלה בשיכפול קבצים - {ex.Message}");
            }

            _progressCallback?.Invoke(100);
        }
    }
}
