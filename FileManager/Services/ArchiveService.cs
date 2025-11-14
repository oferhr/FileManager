using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FileManager;

namespace FileManager.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly IFileService _fileService;
        private readonly Action<int> _progressCallback;

        public ArchiveService(IFileService fileService, Action<int> progressCallback = null)
        {
            _fileService = fileService;
            _progressCallback = progressCallback;
        }

        public void ArchiveFiles(List<ArchiveSettings> checkedItems, string parentPath, string destPath)
        {
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
                    var dirs = Directory.GetDirectories(Path.Combine(parentPath, asettings.dir));
                    foreach (var dirPath in dirs)
                    {
                        var dir = Path.GetFileName(dirPath);
                        if (dir.Length > 2 && Regex.IsMatch(dir, @"\d"))
                        {
                            var midPath = Path.Combine(destPath, asettings.dest);
                            if (!Directory.Exists(midPath))
                            {
                                Directory.CreateDirectory(midPath);
                            }

                            var dirDest = Path.Combine(midPath, dir);
                            if (!Directory.Exists(dirDest))
                            {
                                Directory.CreateDirectory(dirDest);
                            }

                            foreach (string ddir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories))
                            {
                                Directory.CreateDirectory(Path.Combine(dirDest, ddir.Substring(dirPath.Length + 1)));
                            }

                            foreach (string file_name in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                            {
                                _fileService.MoveFiles(file_name, Path.Combine(dirDest, file_name.Substring(dirPath.Length + 1)));
                            }

                            Directory.Delete(dirPath, true);
                        }
                    }
                }
            }

            _progressCallback?.Invoke(100);
        }
    }
}
