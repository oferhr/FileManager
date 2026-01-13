using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileManager;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class ReportManagementService : IReportManagementService
    {
        private readonly string _basePath;
        private readonly IFileService _fileService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;

        public ReportManagementService(string basePath, IFileService fileService, ILoggingService loggingService, Action<int> progressCallback = null)
        {
            _basePath = basePath;
            _fileService = fileService;
            _loggingService = loggingService;
            _progressCallback = progressCallback;
        }

        public void SetReportsNames(List<string> checkedItems, string destFolName)
        {
            var itemCount = checkedItems.Count;
            var percent = itemCount == 0 ? 100 : Convert.ToInt32(Math.Round(95.0 / itemCount, 0));

            foreach (var checkedItem in checkedItems)
            {
                var combinedPath = Path.Combine(_basePath, checkedItem);
                string basePath, errorMessage;
                if (!PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out errorMessage))
                {
                    _loggingService.LogSecurityEvent($"Path traversal attempt blocked in ReportManagementService: {errorMessage}");
                    continue;
                }

                var combinedDest = Path.Combine(basePath, destFolName);
                string dest, destError;
                if (!PathValidator.ValidateAndNormalize(combinedDest, _basePath, out dest, out destError))
                {
                    _loggingService.LogSecurityEvent($"Invalid destination path in ReportManagementService: {destError}");
                    continue;
                }

                if (!Directory.Exists(dest))
                {
                    _loggingService.LogFileOperation("CREATE_DIR", dest);
                    Directory.CreateDirectory(dest);
                }

                var allAvailDirs = Directory.GetDirectories(basePath);
                var availDirs = allAvailDirs.Where(w =>
                {
                    var dn = Path.GetFileName(w);
                    return dn != null && dn.Length < 3 && dn != destFolName;
                }).ToArray();

                if (availDirs.Length > 0)
                {
                    foreach (var cd in availDirs)
                    {
                        var checkdir = Path.GetFileNameWithoutExtension(cd);
                        if (checkdir == null || checkdir.Length > 2 || !Directory.Exists(cd))
                        {
                            continue;
                        }

                        var source = cd;
                        var sourceName = Path.GetFileName(basePath) + " -- " + Path.GetFileName(source);
                        var reportFiles = Directory.GetFiles(source, "*.*", SearchOption.TopDirectoryOnly);
                        
                        if (!reportFiles.Any())
                        {
                            continue;
                        }

                        var grouper = new List<Grouper>();
                        try
                        {
                            foreach (var file in reportFiles)
                            {
                                var parts = _fileService.GetParts(file);
                                var id = parts[parts.Length - 2];

                                if (grouper.Exists(f => f.id == id))
                                {
                                    grouper.Find(f => f.id == id).files.Add(file);
                                }
                                else
                                {
                                    grouper.Add(new Grouper
                                    {
                                        id = id,
                                        files = new List<string> { file }
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _loggingService.LogError($"Error reading source files in report management: {source}", ex);
                            MessageBox.Show($"בעיה נמצאה בקריאת קבצי המקור, {ex.Message}");
                            continue;
                        }

                        var max = 1;
                        var destFiles = Directory.GetFiles(dest, "*.*", SearchOption.TopDirectoryOnly);
                        try
                        {
                            foreach (var df in destFiles)
                            {
                                int num;
                                var destParts = _fileService.GetParts(df);
                                if (int.TryParse(destParts[destParts.Length - 1], out num))
                                {
                                    if (num > max)
                                    {
                                        max = num;
                                    }
                                }
                            }
                            if (max > 1)
                            {
                                max++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _loggingService.LogError($"Error checking destination files in report management: {dest}", ex);
                            MessageBox.Show($"בעיה נמצאה בבדיקת קבצי היעד, {ex.Message}");
                            continue;
                        }

                        try
                        {
                            foreach (var grp in grouper)
                            {
                                var grpId = grp.id;
                                foreach (var gf in grp.files)
                                {
                                    var ext = Path.GetExtension(gf);
                                    var grpParts = _fileService.GetParts(gf);
                                    grpParts[grpParts.Length - 1] = max.ToString("D3");
                                    
                                    if (grpId == "00000000")
                                    {
                                        max++;
                                    }

                                    var newFile = string.Empty;
                                    foreach (var grpPart in grpParts)
                                    {
                                        newFile += grpPart + "-";
                                    }
                                    newFile = newFile.TrimEnd('-') + ext;

                                    var combinedDestFile = Path.Combine(dest, Path.GetFileName(newFile));
                                    string destFilePath, destFileError;
                                    if (!PathValidator.ValidateAndNormalize(combinedDestFile, _basePath, out destFilePath, out destFileError))
                                    {
                                        _loggingService.LogSecurityEvent($"Invalid destination file in ReportManagementService: {destFileError}");
                                        continue;
                                    }

                                    _loggingService.LogFileOperation("MOVE", $"{gf} -> {destFilePath}");
                                    _fileService.MoveFiles(gf, destFilePath);
                                }
                                if (grpId != "00000000")
                                {
                                    max++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _loggingService.LogError("Error moving files in report management", ex);
                            MessageBox.Show($"בעיה בהעברת הקובץ, {ex.Message}");
                        }
                    }
                }

                _progressCallback?.Invoke(percent);
            }

            _progressCallback?.Invoke(100);
        }
    }
}
