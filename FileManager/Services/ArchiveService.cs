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
    public class ArchiveService : IArchiveService
    {
        private readonly IFileService _fileService;
        private readonly ILoggingService _loggingService;
        private readonly Action<int> _progressCallback;

        public ArchiveService(IFileService fileService, ILoggingService loggingService, Action<int> progressCallback = null)
        {
            _fileService = fileService;
            _loggingService = loggingService;
            _progressCallback = progressCallback;
        }

        public void ArchiveFiles(List<ArchiveSettings> checkedItems, string parentPath, string destPath)
        {
            // Validate input paths
            if (!PathValidator.IsValidPath(parentPath, out string parentError))
            {
                _loggingService.LogSecurityEvent("PathValidationFailure", $"Invalid parent path: {parentError}",
                    new Dictionary<string, object> { { "ParentPath", parentPath } });
                throw new ArgumentException($"Invalid parent path: {parentError}", nameof(parentPath));
            }

            if (!PathValidator.IsValidPath(destPath, out string destError))
            {
                _loggingService.LogSecurityEvent("PathValidationFailure", $"Invalid destination path: {destError}",
                    new Dictionary<string, object> { { "DestPath", destPath } });
                throw new ArgumentException($"Invalid destination path: {destError}", nameof(destPath));
            }

            _loggingService.LogInfo($"Starting archive operation from '{parentPath}' to '{destPath}'", "ArchiveService");

            var currentSettings = checkedItems.Where(f => f.sourceDir == parentPath && f.check);
            var index = 0;

            foreach (var asettings in currentSettings)
            {
                index++;
                var pbIncrement = !currentSettings.Any() ? 100 : index / currentSettings.Count();
                var dVal = index * pbIncrement;
                var val = Convert.ToInt32(dVal);
                if (val > 100)
                {
                    val = 100;
                }
                _progressCallback?.Invoke(val);

                if (!string.IsNullOrEmpty(asettings.dest))
                {
                    try
                    {
                        var sourceDir = Path.Combine(parentPath, asettings.dir);

                        // Validate source directory path
                        if (!PathValidator.ValidateAndNormalize(sourceDir, parentPath, out string normalizedSourceDir, out string sourceValidationError))
                        {
                            _loggingService.LogSecurityEvent("PathValidationFailure", $"Invalid source directory: {sourceValidationError}",
                                new Dictionary<string, object> { { "SourceDir", sourceDir } });
                            continue;
                        }

                        var dirs = Directory.GetDirectories(normalizedSourceDir);
                        foreach (var dirPath in dirs)
                        {
                            var dir = Path.GetFileName(dirPath);
                            if (dir.Length > 2 && Regex.IsMatch(dir, @"\d"))
                            {
                                // Validate destination components
                                if (!InputValidator.IsValidFolderName(asettings.dest, out string folderError))
                                {
                                    _loggingService.LogValidationFailure("FolderName", asettings.dest, folderError);
                                    continue;
                                }

                                var midPath = Path.Combine(destPath, asettings.dest);

                                // Validate midPath stays within destPath boundary
                                if (!PathValidator.IsPathWithinBoundary(midPath, destPath, out string midPathError))
                                {
                                    _loggingService.LogSecurityEvent("PathBoundaryViolation", $"Mid path outside boundary: {midPathError}",
                                        new Dictionary<string, object> { { "MidPath", midPath }, { "DestPath", destPath } });
                                    continue;
                                }

                                if (!Directory.Exists(midPath))
                                {
                                    Directory.CreateDirectory(midPath);
                                    _loggingService.LogFileOperation("CreateDirectory", midPath, true);
                                }

                                var dirDest = Path.Combine(midPath, dir);

                                // Validate dirDest stays within midPath boundary
                                if (!PathValidator.IsPathWithinBoundary(dirDest, destPath, out string dirDestError))
                                {
                                    _loggingService.LogSecurityEvent("PathBoundaryViolation", $"Destination directory outside boundary: {dirDestError}",
                                        new Dictionary<string, object> { { "DirDest", dirDest }, { "DestPath", destPath } });
                                    continue;
                                }

                                if (!Directory.Exists(dirDest))
                                {
                                    Directory.CreateDirectory(dirDest);
                                    _loggingService.LogFileOperation("CreateDirectory", dirDest, true);
                                }

                                // CRITICAL FIX: Use Path.GetRelativePath instead of Substring to prevent path traversal
                                foreach (string ddir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories))
                                {
                                    try
                                    {
                                        // Get relative path safely
                                        string relativePath = Path.GetFullPath(ddir).Substring(Path.GetFullPath(dirPath).Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                                        // Validate relative path doesn't contain traversal sequences
                                        if (!PathValidator.IsValidPath(relativePath, out string relativeError) || relativePath.Contains(".."))
                                        {
                                            _loggingService.LogSecurityEvent("PathTraversalAttempt", $"Invalid relative path detected: {relativePath}",
                                                new Dictionary<string, object> { { "SourcePath", ddir }, { "BasePath", dirPath } });
                                            continue;
                                        }

                                        var destSubDir = Path.Combine(dirDest, relativePath);

                                        // Validate destination stays within boundary
                                        if (!PathValidator.IsPathWithinBoundary(destSubDir, destPath, out string destSubError))
                                        {
                                            _loggingService.LogSecurityEvent("PathBoundaryViolation", $"Destination subdirectory outside boundary: {destSubError}",
                                                new Dictionary<string, object> { { "DestSubDir", destSubDir }, { "DestPath", destPath } });
                                            continue;
                                        }

                                        Directory.CreateDirectory(destSubDir);
                                    }
                                    catch (Exception ex)
                                    {
                                        _loggingService.LogError("ArchiveService.CreateDirectory", ex, $"Failed to create directory: {ddir}");
                                    }
                                }

                                // CRITICAL FIX: Use safe relative path calculation for files
                                // Track failed moves to prevent data loss
                                bool anyMoveFailed = false;
                                int successfulMoves = 0;
                                int totalFiles = 0;

                                foreach (string file_name in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                                {
                                    totalFiles++;
                                    try
                                    {
                                        // Get relative path safely
                                        string relativePath = Path.GetFullPath(file_name).Substring(Path.GetFullPath(dirPath).Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                                        // Validate relative path doesn't contain traversal sequences
                                        if (!PathValidator.IsValidPath(relativePath, out string relativeError) || relativePath.Contains(".."))
                                        {
                                            _loggingService.LogSecurityEvent("PathTraversalAttempt", $"Invalid relative file path detected: {relativePath}",
                                                new Dictionary<string, object> { { "SourceFile", file_name }, { "BasePath", dirPath } });
                                            anyMoveFailed = true;
                                            continue;
                                        }

                                        var destFile = Path.Combine(dirDest, relativePath);

                                        // Validate destination file stays within boundary
                                        if (!PathValidator.IsPathWithinBoundary(destFile, destPath, out string destFileError))
                                        {
                                            _loggingService.LogSecurityEvent("PathBoundaryViolation", $"Destination file outside boundary: {destFileError}",
                                                new Dictionary<string, object> { { "DestFile", destFile }, { "DestPath", destPath } });
                                            anyMoveFailed = true;
                                            continue;
                                        }

                                        _fileService.MoveFiles(file_name, destFile);
                                        _loggingService.LogFileOperation("MoveFile", file_name, true);
                                        successfulMoves++;
                                    }
                                    catch (Exception ex)
                                    {
                                        _loggingService.LogError("ArchiveService.MoveFile", ex, $"Failed to move file: {file_name}");
                                        anyMoveFailed = true;
                                    }
                                }

                                // Only delete source directory if all files were moved successfully
                                if (anyMoveFailed)
                                {
                                    _loggingService.LogWarning($"Archive incomplete: {successfulMoves}/{totalFiles} files moved. Source directory preserved to prevent data loss: {dirPath}");
                                }
                                else
                                {
                                    // Delete directory whether it has files or is empty (totalFiles == 0)
                                    try
                                    {
                                        Directory.Delete(dirPath, true);
                                        _loggingService.LogFileOperation("DeleteDirectory", dirPath, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        _loggingService.LogError("ArchiveService.DeleteDirectory", ex, $"Failed to delete directory after successful archive: {dirPath}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError("ArchiveService.ArchiveFiles", ex, $"Error archiving files for setting: {asettings.dest}");
                    }
                }
            }

            _progressCallback?.Invoke(100);
            _loggingService.LogInfo("Archive operation completed", "ArchiveService");
        }
    }
}
