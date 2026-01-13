using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using FileManager.Utilities;

namespace FileManager.Services
{
    public class FileService : IFileService
    {
        private readonly bool _isMove;
        private readonly ILoggingService _loggingService;
        private const string LtrMark = "\u200E";

        public FileService(bool isMove = true, ILoggingService loggingService = null)
        {
            _isMove = isMove;
            _loggingService = loggingService;
        }

        public bool IsThumbsInPath(string filePath)
        {
            return filePath.Contains("Thumbs");
        }

        public void CopyFiles(string src, string dest)
        {
            // Validate source path
            if (!PathValidator.IsValidPath(src, out string srcError))
            {
                var errorMsg = $"Invalid source path: {srcError}";
                _loggingService?.LogSecurityEvent("PathValidationFailure", errorMsg,
                    new Dictionary<string, object> { { "SourcePath", src } });
                throw new ArgumentException(errorMsg, nameof(src));
            }

            // Validate destination path
            if (!PathValidator.IsValidPath(dest, out string destError))
            {
                var errorMsg = $"Invalid destination path: {destError}";
                _loggingService?.LogSecurityEvent("PathValidationFailure", errorMsg,
                    new Dictionary<string, object> { { "DestinationPath", dest } });
                throw new ArgumentException(errorMsg, nameof(dest));
            }

            if (_isMove)
            {
                MoveFiles(src, dest);
            }
            else
            {
                try
                {
                    File.Copy(src, dest);
                    _loggingService?.LogFileOperation("CopyFile", $"{src} -> {dest}", true);
                }
                catch (Exception ex)
                {
                    _loggingService?.LogError("FileService.CopyFiles", ex, $"Failed to copy file from {src} to {dest}");
                    _loggingService?.LogFileOperation("CopyFile", $"{src} -> {dest}", false, ex.Message);
                    MessageBox.Show($"Error while copying file:\r{src}\rTo:\r{dest}\rError Message:\r{ex.Message}");
                }
            }
        }

        public void MoveFiles(string src, string dest)
        {
            // Validate source path
            if (!PathValidator.IsValidPath(src, out string srcError))
            {
                var errorMsg = $"Invalid source path: {srcError}";
                _loggingService?.LogSecurityEvent("PathValidationFailure", errorMsg,
                    new Dictionary<string, object> { { "SourcePath", src } });
                throw new ArgumentException(errorMsg, nameof(src));
            }

            // Validate destination path
            if (!PathValidator.IsValidPath(dest, out string destError))
            {
                var errorMsg = $"Invalid destination path: {destError}";
                _loggingService?.LogSecurityEvent("PathValidationFailure", errorMsg,
                    new Dictionary<string, object> { { "DestinationPath", dest } });
                throw new ArgumentException(errorMsg, nameof(dest));
            }

            try
            {
                File.SetAttributes(src, FileAttributes.Normal);
                File.Move(src, dest);
                _loggingService?.LogFileOperation("MoveFile", $"{src} -> {dest}", true);
            }
            catch (Exception ex)
            {
                _loggingService?.LogError("FileService.MoveFiles", ex, $"Failed to move file from {src} to {dest}");
                _loggingService?.LogFileOperation("MoveFile", $"{src} -> {dest}", false, ex.Message);
                // Rethrow to allow callers to handle the failure (e.g., ArchiveService data loss protection)
                // Callers are responsible for showing UI to users - don't show MessageBox here to avoid duplicates
                throw;
            }
        }

        public string GetNewFileName(string name, int num)
        {
            var fnwe = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            if (!string.IsNullOrEmpty(ext))
            {
                var re = new Regex(@"\d+");
                var m = re.Match(name);

                if (m.Success)
                {
                    return fnwe + "_" + num + ext;
                }

                var newname = fnwe + LtrMark + " " + num + ext;
                return newname;
            }

            return null;
        }

        public string[] GetParts(string file)
        {
            return Path.GetFileNameWithoutExtension(file).Split('-');
        }

        public string GetMailFileName(string fileName, int isCheck, bool shortenName = false)
        {
            var type = 1;
            var fne = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var splits = fne.Split('_');
            
            if (splits.Length == 1)
            {
                type = 2;
                splits = fne.Split('-');
                if (splits.Length == 1)
                {
                    type = 3;
                    splits = fne.Split(' ');
                }
            }

            if (isCheck == 0 || shortenName)
            {
                var first = splits[0];
                var last = splits[splits.Length - 1];
                string[] newName;
                var sep = type == 1 ? "_" : type == 2 ? "-" : " ";
                
                if (!first.Any(ch => ch < '0' || ch > '9'))
                {
                    newName = splits.Skip(1).Take(splits.Count()).ToArray();
                }
                else if (!last.Any(ch => ch < '0' || ch > '9'))
                {
                    newName = splits.Take(splits.Count() - 1).ToArray();
                }
                else
                {
                    newName = splits;
                }

                var name = string.Join(sep, newName);
                return ext.Length == 1 ? name : name + ext;
            }
            else if (isCheck == 3)
            {
                for (int i = 0; i < splits.Length; i++)
                {
                    var sp = splits[i];
                    if (sp.Split(' ').Length > 1)
                    {
                        return sp;
                    }
                }
                return splits[0];
            }
            else if (isCheck == 4)
            {
                var sep = type == 1 ? "_" : type == 2 ? "-" : " ";
                if (splits.Length == 4)
                {
                    return splits[0] + sep + splits[1] + sep + splits[3];
                }
                else
                {
                    return splits[0] + sep + splits[1] + sep + splits[2];
                }
            }
            else
            {
                return fileName;
            }
        }
    }
}
