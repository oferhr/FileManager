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
        public string check { get; set; }
        public int icheck { get; set; }
        public string method { get; set; }
    }

    public class FolderSettings
    {
        public string selectedFolders { get; set; }
        public string duplicatesFolders { get; set; }
        public string fileNamesFolders { get; set; }
        public string ExcelFolders { get; set; }
        public string deleteFolders { get; set; }
        public string reportsFolders { get; set; }
    }

    public class CountSettings
    {
        public string dir { get; set; }
        public string method { get; set; }
        public bool check { get; set; }
    }
    public class SplitSettings
    {
        public string dir { get; set; }
        public string dest { get; set; }
        public bool check { get; set; }
    }
    public class Grouper
    {
        public string id { get; set; }
        public List<string> files { get; set; }
    }

    public class ArchiveSettings
    {
        public string dir { get; set; }
        public string dest { get; set; }
        public string sourceDir { get; set; }
        public bool check { get; set; }
    }
}
