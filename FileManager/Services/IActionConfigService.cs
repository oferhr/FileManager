namespace FileManager.Services
{
    /// <summary>
    /// Reads the <c>actions.json</c> toggle file that controls which actions are allowed to run.
    /// </summary>
    /// <remarks>
    /// Fail-open semantics: if the JSON file is missing, unreadable, or the action key is
    /// not present in the file, the action is treated as enabled. Lookups are case-insensitive
    /// for both the action name and the boolean string value.
    /// </remarks>
    public interface IActionConfigService
    {
        /// <summary>
        /// Returns true when the action is enabled (or when the file/key is missing).
        /// </summary>
        bool IsActionEnabled(string actionName);
    }
}
