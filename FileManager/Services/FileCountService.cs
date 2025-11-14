using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileManager;
using Newtonsoft.Json;

namespace FileManager.Services
{
    public class FileCountService : IFileCountService
    {
        private readonly string _basePath;
        private readonly string _configPath;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly Action<int> _progressCallback;
        private readonly Action<string> _logCallback;
        private readonly Action _refreshDataGridViewCallback;

        public FileCountService(
            string basePath, 
            string configPath,
            IFileService fileService, 
            IConfigurationService configurationService,
            Action<int> progressCallback = null,
            Action<string> logCallback = null,
            Action refreshDataGridViewCallback = null)
        {
            _basePath = basePath;
            _configPath = configPath;
            _fileService = fileService;
            _configurationService = configurationService;
            _progressCallback = progressCallback;
            _logCallback = logCallback;
            _refreshDataGridViewCallback = refreshDataGridViewCallback;
        }

        public List<FileCount> CountFilesInDirectories(List<CountSettings> checkedItems, bool useProgressBar = false)
        {
            var list = new List<FileCount>();
            var percent = 0;
            
            if (useProgressBar)
            {
                var itemCount = checkedItems.Count;
                percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(100.0 / itemCount, 0));
            }

            foreach (var checkedItem in checkedItems)
            {
                var checkedPath = Path.Combine(_basePath, checkedItem.dir);
                if (!Directory.Exists(checkedPath))
                {
                    continue;
                }

                var lfiles = Directory.GetFiles(checkedPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));
                
                var files = lfiles as IList<string> ?? lfiles.ToList();
                var isThumbs = files.Any(_fileService.IsThumbsInPath);

                int fileCount;
                if (isThumbs)
                {
                    fileCount = files.Count() - 1;
                }
                else
                {
                    fileCount = files.Count();
                }

                if (fileCount > 0)
                {
                    list.Add(new FileCount
                    {
                        FileName = checkedItem.dir,
                        method = checkedItem.method,
                        Count = fileCount
                    });
                }

                if (useProgressBar && _progressCallback != null)
                {
                    _progressCallback(percent);
                }
            }

            return list;
        }

        public void InitializeGrid(List<CountSettings> countSettings, List<string> foldersList)
        {
            // This method is called from Form1 to initialize the grid data
            // The actual grid binding is handled in Form1, but we can prepare the data here
            var countDs = new List<CountSettings>();
            
            foreach (var fol in foldersList)
            {
                var curdir = countSettings.Find(f => f.dir == fol);
                if (curdir != null)
                {
                    countDs.Add(new CountSettings
                    {
                        dir = fol,
                        method = curdir.method,
                        check = curdir.check
                    });
                }
                else
                {
                    countDs.Add(new CountSettings
                    {
                        dir = fol,
                        method = null,
                        check = false
                    });
                }
            }
        }

        public void HandleGridCellValueChanged(int rowIndex, int columnIndex, string columnName, object value)
        {
            if (rowIndex == -1)
                return;

            // Handle checkbox column changes
            if (columnName == "chb")
            {
                var check = (bool)value;
                var dirSettings = GetCountSettings();
                var ddir = GetCellValue(rowIndex, "dirs");
                var method = GetCellValue(rowIndex, "methods");
                
                var dirObj = dirSettings.Find(f => f.dir == ddir);
                if (dirObj != null)
                {
                    dirObj.check = check;
                }
                else
                {
                    dirSettings.Add(new CountSettings
                    {
                        dir = ddir,
                        method = method,
                        check = check
                    });
                }
                SetCountSettings(dirSettings);
            }
            // Handle methods column changes
            else if (columnName == "methods")
            {
                var method = value?.ToString() ?? string.Empty;
                var fol = GetCellValue(rowIndex, "dirs");
                
                // Update email settings
                var emailConfigList = _configurationService.GetEmailDirSettings();
                var curMailDir = emailConfigList.Find(f => f.dir == fol);
                if (curMailDir != null)
                {
                    curMailDir.method = string.IsNullOrEmpty(method) ? null : method;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method))
                    {
                        emailConfigList.Add(new EmailDirSettings
                        {
                            dir = fol,
                            method = method,
                            check = "איחוד-קצר", // mailCheck[0]
                            icheck = 0,
                            email = null
                        });
                    }
                }
                _configurationService.SetEmailDirSettings(emailConfigList);
                
                // Refresh dataGridView1 to reflect the method changes
                _refreshDataGridViewCallback?.Invoke();
            }
        }

        public void HandleGridCellEndEdit(int rowIndex, string method, string folder, bool check)
        {
            var countsConfigList = GetCountSettings();
            
            var curCountDir = countsConfigList.Find(f => f.dir == folder);
            if (curCountDir != null)
            {
                if (string.IsNullOrEmpty(method) && check == false)
                {
                    countsConfigList.Remove(curCountDir);
                }
                else
                {
                    curCountDir.method = method;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(method))
                {
                    countsConfigList.Add(new CountSettings
                    {
                        dir = folder,
                        method = method,
                        check = check
                    });
                }
            }

            SetCountSettings(countsConfigList);

            var emailConfigList = _configurationService.GetEmailDirSettings();
            var curMailDir = emailConfigList.Find(f => f.dir == folder);
            if (curMailDir != null)
            {
                curMailDir.method = string.IsNullOrEmpty(method) ? null : method;
            }
            else
            {
                if (!string.IsNullOrEmpty(method))
                {
                    emailConfigList.Add(new EmailDirSettings
                    {
                        dir = folder,
                        method = method,
                        check = "איחוד-קצר", // mailCheck[0]
                        icheck = 0,
                        email = null
                    });
                }
            }
            _configurationService.SetEmailDirSettings(emailConfigList);
            
            // Refresh dataGridView1 to reflect the method changes
            _refreshDataGridViewCallback?.Invoke();
        }

        public List<CountSettings> GetCountSettings()
        {
            return _configurationService.GetCountSettings();
        }

        public void SetCountSettings(List<CountSettings> settings)
        {
            _configurationService.SetCountSettings(settings);
        }

        // public bool IsThumbsInPath(string filePath)
        // {
        //     if (filePath.Contains("Thumbs"))
        //         return true;
        //     return false;
        // }

        public void SaveSelectedFoldersToSettings(List<CountSettings> checkedItems)
        {
            var items = string.Empty;
            foreach (var checkedItem in checkedItems)
            {
                items += checkedItem.dir + ",";
            }

            var folderSettings = _configurationService.GetFolderSettings();
            if (folderSettings.Count > 0)
            {
                folderSettings.First().selectedFolders = items.TrimEnd(',');
            }
            else
            {
                folderSettings.Add(new FolderSettings
                {
                    duplicatesFolders = string.Empty,
                    fileNamesFolders = string.Empty,
                    reportsFolders = string.Empty,
                    selectedFolders = items.TrimEnd(','),
                    deleteFolders = string.Empty
                });
            }
            _configurationService.SetFolderSettings(folderSettings);
        }

        private string GetCellValue(int rowIndex, string columnName)
        {
            // This is a placeholder - in the actual implementation, this would get the value from the grid
            // For now, we'll return empty string as this method will be called from Form1 with actual values
            return string.Empty;
        }
    }
}
