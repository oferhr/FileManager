using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager
{
    public class FileCount
    {
        public string FileName { get; set; }
        public int Count { get; set; }
        public string method { get; set; }
    }

    public class EmailDirSettings
    {
        public string dir { get; set; }
        public string email { get; set; }
        public bool check { get; set; }
    }

    public class FolderSettings
    {
        public string selectedFolders { get; set; }
        public string duplicatesFolders { get; set; }
        public string fileNamesFolders { get; set; }
    }

    public class CountSettings
    {
        public string dir { get; set; }
        public string method { get; set; }
        public bool check { get; set; }
    }
}
