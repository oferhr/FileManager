using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace FileManager.Services
{
    public class ExcelService : IExcelService
    {
        private readonly string _basePath;
        private readonly string _excelPath;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly Action<int> _progressCallback;

        public ExcelService(
            string basePath, 
            string excelPath, 
            IFileService fileService, 
            IConfigurationService configurationService,
            Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _excelPath = excelPath;
            _fileService = fileService;
            _configurationService = configurationService;
            _progressCallback = progressCallback;
        }

        public void SetExcelNames(List<string> checkedItems)
        {
            if (checkedItems.Count == 0)
            {
                return;
            }

            var arr = GetExcelValues();
            var itemCount = checkedItems.Count;
            var itemParts = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(100.0 / itemCount, 0));
            var progress = 0;
            var index = 0;

            foreach (var checkedItem in checkedItems)
            {
                var basePath = Path.Combine(_basePath, checkedItem);
                if (!Directory.Exists(basePath))
                {
                    continue;
                }

                var lfiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".pdf"));

                var files = lfiles as IList<string> ?? lfiles.ToList();
                var percent = files.Count == 0 ? 100 : itemParts / files.Count;

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName == null)
                    {
                        continue;
                    }

                    var ext = Path.GetExtension(fileName);
                    if (string.IsNullOrEmpty(ext))
                    {
                        continue;
                    }

                    var hyphenSplits = Path.GetFileNameWithoutExtension(fileName).Split('-');
                    var parts = new List<string>();
                    
                    foreach (var split in hyphenSplits)
                    {
                        var splits = split.Split('_');
                        foreach (var s in splits)
                        {
                            parts.Add(s);
                        }
                    }

                    foreach (var part in parts)
                    {
                        var result = CheckExcelForItems(arr, part);
                        if (!string.IsNullOrEmpty(result))
                        {
                            string newFileName;
                            if (Regex.IsMatch(fileName, @"[\p{IsHebrew}]+"))
                            {
                                newFileName = SetExcelFileName(parts, part, result) + ext;
                            }
                            else
                            {
                                newFileName = SetExcelFileName2(parts, part, result) + ext;
                            }

                            var ndir = Path.GetDirectoryName(file);
                            if (ndir != null)
                            {
                                var newFilePath = Path.Combine(ndir, newFileName);
                                if (File.Exists(newFilePath))
                                {
                                    File.Delete(newFilePath);
                                }
                                _fileService.MoveFiles(file, newFilePath);
                            }
                            break;
                        }
                    }

                    progress += (index * itemParts) + percent;
                    if (progress > 100) progress = 100;
                    _progressCallback?.Invoke(progress);
                }
                index++;
            }

            _progressCallback?.Invoke(100);
        }

        public void SetSelectedExcelFolders(List<string> checkedItems)
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
                folderSettings.First().ExcelFolders = items.TrimEnd(',');
            }
            else
            {
                folderSettings.Add(new FolderSettings
                {
                    ExcelFolders = items.TrimEnd(','),
                    reportsFolders = string.Empty,
                    fileNamesFolders = string.Empty,
                    selectedFolders = string.Empty,
                    duplicatesFolders = string.Empty
                });
            }
            _configurationService.SetFolderSettings(folderSettings);
        }

        public void LoadExcelFolders(CheckedListBox dirList)
        {
            var folderSettings = _configurationService.GetFolderSettings();
            var excelFolderSettings = folderSettings.Count == 0 ? String.Empty : folderSettings.First().ExcelFolders;
            if (!String.IsNullOrEmpty(excelFolderSettings))
            {
                var folders = excelFolderSettings.Split(',');
                for (var i = 0; i <= (dirList.Items.Count - 1); i++)
                {
                    foreach (var fol in folders)
                    {
                        if (dirList.Items[i].ToString() == fol)
                        {
                            dirList.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }

        public object[,] GetExcelValues()
        {
            var xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = null;
            Excel._Worksheet xlWorksheet = null;
            Excel.Range xlRange = null;
            
            try
            {
                xlWorkbook = xlApp.Workbooks.Open(_excelPath);
                xlApp.Visible = false;
                xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets[1];
                xlRange = xlWorksheet.UsedRange;

                var vals = (object[,])xlRange.Cells.Value2;
                xlWorkbook.Close();
                xlApp.Quit();
                return vals;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Excel file: {ex.Message}");
                xlWorkbook?.Close();
                xlApp?.Quit();
                return new object[0, 0];
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (xlWorksheet != null)
                {
                    Marshal.ReleaseComObject(xlWorksheet);
                }
                if (xlWorkbook != null)
                {
                    Marshal.ReleaseComObject(xlWorkbook);
                }
                if (xlApp != null)
                {
                    Marshal.ReleaseComObject(xlApp);
                }
            }
        }

        public string CheckExcelForItems(object[,] arr, string part)
        {
            var lst = new List<string>();
            for (var x = 1; x <= arr.GetLength(0); x++)
            {
                if (part == arr[x, 1].ToString())
                {
                    for (var y = 2; y <= arr.GetLength(1); y++)
                    {
                        var val = arr[x, y]?.ToString();
                        if (!string.IsNullOrEmpty(val))
                        {
                            lst.Add(val);
                        }
                    }
                }
            }
            if (lst.Count > 0)
            {
                return string.Join(" ", lst.ToArray());
            }
            return null;
        }

        public string SetExcelFileName(List<string> parts, string part, string result)
        {
            if (parts == null)
            {
                return string.Empty;
            }
            var arPart = parts.IndexOf(part);
            if (parts.Count > arPart && arPart >= 0)
            {
                parts[arPart] = result;
            }

            if (parts.Count != 3)
            {
                string name = string.Empty;
                foreach (var part1 in parts)
                {
                    name += part1 + '-';
                }
                return name.TrimEnd('-');
            }
            return parts[2] + "_" + parts[0] + "-" + parts[1];
        }

        public string SetExcelFileName2(List<string> parts, string part, string result)
        {
            var arPart = parts.IndexOf(part);
            parts[arPart] = result;
            string name = string.Empty;
            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    if (i < parts.Count - 1)
                    {
                        name += "_";
                    }
                    else
                    {
                        name += "-";
                    }
                }
                name += parts[i];
            }
            return name;
        }
    }
}
