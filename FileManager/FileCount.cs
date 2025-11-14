using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager
{
    /// <summary>
    /// Represents file counting information for display in the results grid.
    /// Used to track and display the number of occurrences of specific files.
    /// </summary>
    public class FileCount
    {
        /// <summary>
        /// Gets or sets the name of the file being counted.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the number of occurrences/count for this file.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the processing method used for this file.
        /// Examples: "איחוד-קצר", "בודד-זהה", "בודד-קצר", etc.
        /// </summary>
        public string method { get; set; }
    }

    /// <summary>
    /// Configuration settings for email directory operations.
    /// Stores information about directories to process and their associated email recipients.
    /// </summary>
    public class EmailDirSettings
    {
        /// <summary>
        /// Gets or sets the directory path to process.
        /// </summary>
        public string dir { get; set; }

        /// <summary>
        /// Gets or sets the email address for this directory.
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Gets or sets the check/validation method name.
        /// </summary>
        public string check { get; set; }

        /// <summary>
        /// Gets or sets the integer representation of the check method.
        /// Used for indexing into mail processing arrays.
        /// </summary>
        public int icheck { get; set; }

        /// <summary>
        /// Gets or sets the processing method for this email directory.
        /// Examples: "איחוד-קצר", "בודד-זהה", "בודד-קצר", "איחוד שמי", "איחוד לפי דוח"
        /// </summary>
        public string method { get; set; }
    }

    /// <summary>
    /// Configuration for various folder paths used throughout the application.
    /// Persists user-selected folders for different operations.
    /// </summary>
    public class FolderSettings
    {
        /// <summary>
        /// Gets or sets the selected folders for general operations.
        /// </summary>
        public string selectedFolders { get; set; }

        /// <summary>
        /// Gets or sets the folders used for duplicate file operations.
        /// </summary>
        public string duplicatesFolders { get; set; }

        /// <summary>
        /// Gets or sets the folders used for file name operations.
        /// </summary>
        public string fileNamesFolders { get; set; }

        /// <summary>
        /// Gets or sets the folders used for Excel file operations.
        /// </summary>
        public string ExcelFolders { get; set; }

        /// <summary>
        /// Gets or sets the folders designated for file deletion.
        /// </summary>
        public string deleteFolders { get; set; }

        /// <summary>
        /// Gets or sets the folders used for report generation.
        /// </summary>
        public string reportsFolders { get; set; }
    }

    /// <summary>
    /// Settings for file counting operations.
    /// Configures which directories to count files in and the counting method.
    /// </summary>
    public class CountSettings
    {
        /// <summary>
        /// Gets or sets the directory to count files in.
        /// </summary>
        public string dir { get; set; }

        /// <summary>
        /// Gets or sets the counting method/strategy to use.
        /// </summary>
        public string method { get; set; }

        /// <summary>
        /// Gets or sets whether this count setting is enabled/checked.
        /// </summary>
        public bool check { get; set; }
    }

    /// <summary>
    /// Settings for file splitting operations.
    /// Defines source and destination for splitting files.
    /// </summary>
    public class SplitSettings
    {
        /// <summary>
        /// Gets or sets the source directory containing files to split.
        /// </summary>
        public string dir { get; set; }

        /// <summary>
        /// Gets or sets the destination directory for split files.
        /// </summary>
        public string dest { get; set; }

        /// <summary>
        /// Gets or sets whether this split setting is enabled/checked.
        /// </summary>
        public bool check { get; set; }
    }

    /// <summary>
    /// Settings for file copy operations.
    /// Configures source, destination, and filtering for copy operations.
    /// </summary>
    public class CopySettings
    {
        /// <summary>
        /// Gets or sets the source directory to copy files from.
        /// </summary>
        public string dir { get; set; }

        /// <summary>
        /// Gets or sets the destination directory to copy files to.
        /// </summary>
        public string dest { get; set; }

        /// <summary>
        /// Gets or sets whether this copy setting is enabled/checked.
        /// </summary>
        public bool check { get; set; }

        /// <summary>
        /// Gets or sets the filter string for selecting files to copy.
        /// Can be a pattern or criteria for file selection.
        /// </summary>
        public string str { get; set; }
    }

    /// <summary>
    /// Groups files together by a common identifier.
    /// Used for operations that need to process related files together.
    /// </summary>
    public class Grouper
    {
        /// <summary>
        /// Gets or sets the unique identifier for this group.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the list of file paths belonging to this group.
        /// </summary>
        public List<string> files { get; set; }
    }

    /// <summary>
    /// Settings for archive operations.
    /// Configures source, destination, and original source directories for archiving.
    /// </summary>
    public class ArchiveSettings
    {
        /// <summary>
        /// Gets or sets the directory containing files to archive.
        /// </summary>
        public string dir { get; set; }

        /// <summary>
        /// Gets or sets the destination directory for archived files.
        /// </summary>
        public string dest { get; set; }

        /// <summary>
        /// Gets or sets the original source directory path.
        /// Used for reference or restoration purposes.
        /// </summary>
        public string sourceDir { get; set; }

        /// <summary>
        /// Gets or sets whether this archive setting is enabled/checked.
        /// </summary>
        public bool check { get; set; }
    }
}
