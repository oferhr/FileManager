using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;
using FileManager.Utilities;
using NLog;

namespace FileManager.Services
{
    public class FileNameManagementService : IFileNameManagementService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;
        private readonly Action<string> _logCallback;
        private readonly Action<bool> _progressVisibilityCallback;
        private readonly Action<string> _progressMessageCallback;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public FileNameManagementService(
            string basePath,
            IFileService fileService,
            IConfigurationService configurationService,
            ILoggingService loggingService,
            Action<int> progressCallback = null,
            Action<string> logCallback = null,
            Action<bool> progressVisibilityCallback = null,
            Action<string> progressMessageCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _configurationService = configurationService;
            _loggingService = loggingService;
            _progressCallback = progressCallback;
            _logCallback = logCallback;
            _progressVisibilityCallback = progressVisibilityCallback;
            _progressMessageCallback = progressMessageCallback;
        }

        public void FixFileNames(List<string> checkedItems, bool isMigdal)
        {
            string curdir = null, fileName = null, newName = null, copyName = null;
            try
            {
                _progressVisibilityCallback?.Invoke(true);
                _progressCallback?.Invoke(0);
                _progressMessageCallback?.Invoke("מתקן שמות");
                
                if (checkedItems.Count == 0)
                {
                    return;
                }

                // Save selected items to Settings
                SetSelectedFileNameFolders(checkedItems);

                var itemCount = checkedItems.Count;
                var percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(95.0 / itemCount, 0));
                var progress = 0;

                // Loop over the checked items
                foreach (var checkedItem in checkedItems)
                {
                    var combinedPath = Path.Combine(_basePath, checkedItem.ToString());
                    string basePath, errorMessage;
                    if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                    {
                        _loggingService.LogSecurityEvent($"Path traversal attempt blocked in FileNameManagementService: {errorMessage}");
                        continue;
                    }

                    var dirs = Directory.GetDirectories(basePath);
                    
                    if (dirs.Length > 0)
                    {
                        // Run over the array to get the dir/1 folder path where all the files are
                        foreach (var cd in dirs)
                        {
                            curdir = cd;
                            if (!Directory.Exists(curdir))
                            {
                                continue;
                            }

                            var checkdir = Path.GetFileNameWithoutExtension(curdir);
                            if (checkdir == null || checkdir.Length > 2)
                            {
                                continue;
                            }

                            var lfiles = Directory.GetFiles(curdir, "*.*", SearchOption.AllDirectories)
                                .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));

                            var files = lfiles as IList<string> ?? lfiles.ToList();

                            foreach (var file in files)
                            {
                                fileName = Path.GetFileName(file);

                                if (fileName == null || fileName.Contains("-"))
                                {
                                    continue;
                                }

                                var fnwe = Path.GetFileNameWithoutExtension(fileName);
                                var ext = Path.GetExtension(fileName);
                                var splits = fnwe.Split('_');
                                
                                if (splits.Length != 3)
                                {
                                    continue;
                                }

                                newName = string.Empty;
                                if (splits[1] == "9999999999")
                                {
                                    for (var i = 0; i < splits.Length; i++)
                                    {
                                        if (i == 1) continue;
                                        var part = splits[i];
                                        newName += part + "_";
                                    }
                                }
                                else
                                {
                                    for (var i = 0; i < splits.Length; i++)
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

                                var miniCounter = 1;
                                var isCopy = false;
                                while (!isCopy)
                                {
                                    var based = Directory.GetParent(curdir).FullName;
                                    var combinedNewDir = Path.Combine(based, miniCounter.ToString());
                                    string newdir, newdirError;
                                    if (!PathValidator.ValidateAndNormalize(combinedNewDir, _basePath, out newdir, out newdirError))
                                    {
                                        _loggingService.LogSecurityEvent($"Invalid directory in FileNameManagementService: {newdirError}");
                                        break;
                                    }

                                    var combinedCopyName = Path.Combine(newdir, newName.TrimEnd('_') + ext);
                                    string tempCopyName, copyError;
                                    if (!PathValidator.ValidateAndNormalize(combinedCopyName, _basePath, out tempCopyName, out copyError))
                                    {
                                        _loggingService.LogSecurityEvent($"Invalid file path in FileNameManagementService: {copyError}");
                                        break;
                                    }
                                    copyName = tempCopyName;

                                    if (File.Exists(copyName))
                                    {
                                        miniCounter++;
                                    }
                                    else
                                    {
                                        isCopy = true;
                                        try
                                        {
                                            if (!Directory.Exists(newdir))
                                            {
                                                Directory.CreateDirectory(newdir);
                                                _loggingService.LogFileOperation("CREATE_DIR", newdir, true);
                                            }
                                            _fileService.MoveFiles(file, copyName);
                                            _loggingService.LogFileOperation("MOVE", $"{file} -> {copyName}", true);
                                        }
                                        catch (Exception ex)
                                        {
                                            _loggingService.LogFileOperation("MOVE", $"{file} -> {copyName}", false, ex.Message);
                                            _loggingService.LogError($"Failed to move file in FileNameManagementService: {file}", ex);
                                            // Don't rethrow - continue processing remaining files
                                            // The error is logged, and we want to process other files
                                            break; // Exit the while loop for this file, continue with next file
                                        }
                                    }
                                }
                            }
                        }
                    }

                    progress += percent;
                    _progressCallback?.Invoke(progress);
                }

                _progressCallback?.Invoke(100);
            }
            catch (System.Exception ex)
            {
                var errorMessage = "FixFileNames Error :\r" + ex.Message;
                _logCallback?.Invoke(errorMessage);
                
                var props = new
                {
                    method = "FixFileNames",
                    fileName = fileName,
                    destFile = copyName
                };
                
                var eventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Exception = ex,
                    Message = "FixFileNames Error",
                    Properties = { { "Files", props } }
                };
                
                Logger.Log(eventInfo);
                throw;
            }
        }

        public void SetSelectedFileNameFolders(List<string> checkedItems)
        {
            var items = string.Empty;
            foreach (var checkedItem in checkedItems)
            {
                // Set a comma delimited string to be saved in Settings
                items += checkedItem + ",";
            }

            var folderSettings = _configurationService.GetFolderSettings();
            if (folderSettings.Count > 0)
            {
                folderSettings.First().fileNamesFolders = items.TrimEnd(',');
            }
            else
            {
                folderSettings.Add(new FolderSettings
                {
                    duplicatesFolders = string.Empty,
                    fileNamesFolders = items.TrimEnd(','),
                    reportsFolders = string.Empty,
                    selectedFolders = string.Empty,
                    deleteFolders = string.Empty
                });
            }

            _configurationService.SetFolderSettings(folderSettings);
        }

        public void LoadFileNameFolders(CheckedListBox filenamesListBox)
        {
            var folderSettings = _configurationService.GetFolderSettings();
            var fileNamesFolderSettings = folderSettings.Count == 0 ? string.Empty : folderSettings.First().fileNamesFolders;
            
            if (!string.IsNullOrEmpty(fileNamesFolderSettings))
            {
                var folders = fileNamesFolderSettings.Split(',');
                for (var i = 0; i <= (filenamesListBox.Items.Count - 1); i++)
                {
                    foreach (var fol in folders)
                    {
                        if (filenamesListBox.Items[i].ToString() == fol)
                        {
                            filenamesListBox.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }
    }
}
