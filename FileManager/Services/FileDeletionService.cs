using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class FileDeletionService : IFileDeletionService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;

        public FileDeletionService(string basePath, IFileService fileService, ILoggingService loggingService, Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _loggingService = loggingService;
            _progressCallback = progressCallback;
        }

        public void DeleteFiles(List<string> checkedItems, int daysToDelete)
        {
            var filesToDelete = new List<string>();
            var dtDeleteFiles = DateTime.Now.AddDays(daysToDelete * -1);
            var tts = new TimeSpan(0, 0, 0);
            dtDeleteFiles = dtDeleteFiles.Date + tts;

            var itemCount = checkedItems.Count;
            var percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(95.0 / itemCount, 0));

            foreach (var checkedItem in checkedItems)
            {
                var combinedPath = Path.Combine(_basePath, checkedItem);
                string basePath, errorMessage;
                if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                {
                    _loggingService.LogSecurityEvent($"Path validation failed in FileDeletionService: {errorMessage}");
                    continue; // Skip this item
                }

                var fileParts = new List<string>();

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
                            .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                        
                        var files = lfiles as IList<string> ?? lfiles.ToList();
                        if (files.Any())
                        {
                            foreach (var file in files)
                            {
                                var creationDate = File.GetLastWriteTime(file);
                                if (creationDate >= dtDeleteFiles)
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

                    foreach (var fileToDelete in filesToDelete)
                    {
                        try
                        {
                            File.Delete(fileToDelete);
                            _loggingService.LogFileOperation("DELETE", fileToDelete, true);
                        }
                        catch (Exception ex)
                        {
                            _loggingService.LogFileOperation("DELETE", fileToDelete, false, ex.Message);
                            _loggingService.LogError($"Failed to delete file: {fileToDelete}", ex);
                        }
                    }

                    var dirsToDelete = new List<string>();
                    foreach (var curdir in dirs1)
                    {
                        if (!Directory.Exists(curdir))
                        {
                            continue;
                        }

                        var lfiles = Directory.GetFiles(curdir, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                        
                        var files = lfiles as IList<string> ?? lfiles.ToList();
                        if (!files.Any())
                        {
                            dirsToDelete.Add(curdir);
                        }
                    }

                    foreach (var dtd in dirsToDelete)
                    {
                        try
                        {
                            // Safety check: validate path before deletion
                            string normalizedPath, validationError;
                            if (!PathValidator.ValidateAndNormalize(dtd, _basePath, out normalizedPath, out validationError))
                            {
                                _loggingService.LogSecurityEvent($"Attempted to delete directory outside boundary: {validationError}");
                                continue;
                            }

                            Directory.Delete(normalizedPath, recursive: false); // Only delete empty directories
                            _loggingService.LogFileOperation("DELETE_DIR", normalizedPath, true);
                        }
                        catch (Exception ex)
                        {
                            _loggingService.LogFileOperation("DELETE_DIR", dtd, false, ex.Message);
                            _loggingService.LogError($"Failed to delete directory: {dtd}", ex);
                        }
                    }

                    filesToDelete.Clear();
                    dirsToDelete.Clear();
                }

                _progressCallback?.Invoke(percent);
            }

            _progressCallback?.Invoke(100);
        }
    }
}
