namespace FileManager.Services
{
    /// <summary>
    /// Canonical action names used as keys in <c>actions.json</c>.
    /// </summary>
    public static class ActionNames
    {
        public const string CopyFiles = "CopyFiles";
        public const string SplitFolders = "SplitFolders";
        public const string FixFileNames = "FixFileNames";
        public const string CountFiles = "CountFiles";
        public const string FixDuplicates = "FixDuplicates";
        public const string SetExcelNames = "SetExcelNames";
        public const string SetReportsNames = "SetReportsNames";
        public const string SendEmails = "SendEmails";
        public const string Archive = "Archive";
        public const string DeleteFiles = "DeleteFiles";

        /// <summary>
        /// All known action names. Used to validate <c>actions.json</c> keys at load time.
        /// </summary>
        public static readonly string[] All =
        {
            CopyFiles,
            SplitFolders,
            FixFileNames,
            CountFiles,
            FixDuplicates,
            SetExcelNames,
            SetReportsNames,
            SendEmails,
            Archive,
            DeleteFiles,
        };
    }
}
