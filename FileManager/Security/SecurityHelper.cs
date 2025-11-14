using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager.Security
{
    /// <summary>
    /// Provides security utilities for the FileManager application
    /// Including path validation, encryption, and input sanitization
    /// </summary>
    public static class SecurityHelper
    {
        // Allowed email domains for the organization
        private static readonly string[] DefaultAllowedDomains = { "@company.com" };

        /// <summary>
        /// Validates that a user-provided path does not escape the base directory
        /// Prevents path traversal attacks (CWE-22)
        /// </summary>
        /// <param name="basePath">The trusted base directory path</param>
        /// <param name="userPath">The user-provided relative path</param>
        /// <returns>The validated full path</returns>
        /// <exception cref="SecurityException">Thrown if path traversal is detected</exception>
        public static string ValidatePath(string basePath, string userPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException(nameof(basePath));
            if (string.IsNullOrEmpty(userPath))
                throw new ArgumentNullException(nameof(userPath));

            // Combine and resolve to absolute path
            var combined = Path.Combine(basePath, userPath);
            var fullPath = Path.GetFullPath(combined);
            var fullBasePath = Path.GetFullPath(basePath);

            // Ensure fullBasePath ends with directory separator to prevent bypass
            // This prevents "C:\base-evil\" from matching "C:\base"
            if (!fullBasePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fullBasePath += Path.DirectorySeparatorChar;
            }

            // Ensure the result is within the base directory
            if (!fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error($"Path traversal attempt detected: userPath={userPath}, basePath={basePath}, resolved={fullPath}");

                throw new SecurityException(
                    $"Path traversal detected: {userPath} escapes base directory"
                );
            }

            return fullPath;
        }

        /// <summary>
        /// Encrypts data using Windows Data Protection API (DPAPI) with user scope
        /// Suitable for configuration files containing sensitive data
        /// </summary>
        /// <param name="plainText">Plain text to encrypt</param>
        /// <returns>Encrypted bytes</returns>
        public static byte[] Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            var bytes = Encoding.UTF8.GetBytes(plainText);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        }

        /// <summary>
        /// Decrypts DPAPI-encrypted data
        /// </summary>
        /// <param name="encryptedData">Encrypted bytes from Encrypt method</param>
        /// <returns>Decrypted plain text</returns>
        public static string Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length == 0)
                throw new ArgumentNullException(nameof(encryptedData));

            var bytes = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Sanitizes filename for safe use in email subjects and file operations
        /// Removes control characters and potentially dangerous characters
        /// </summary>
        /// <param name="fileName">Filename to sanitize</param>
        /// <returns>Sanitized filename</returns>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            const int MaxLength = 255;
            if (fileName.Length > MaxLength)
            {
                fileName = fileName.Substring(0, MaxLength);
            }

            // Remove control characters (0x00-0x1F, 0x7F)
            fileName = Regex.Replace(fileName, @"[\x00-\x1F\x7F]", "");

            // Remove potentially dangerous characters for email subjects
            fileName = Regex.Replace(fileName, @"[<>\""]", "");

            return fileName.Trim();
        }

        /// <summary>
        /// Validates email address against allowed domains
        /// Prevents data exfiltration to unauthorized external addresses
        /// </summary>
        /// <param name="email">Email address to validate</param>
        /// <param name="allowedDomains">Array of allowed domain suffixes (e.g., "@company.com")</param>
        /// <returns>True if email is in an allowed domain</returns>
        public static bool IsEmailAllowed(string email, string[] allowedDomains = null)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // Use default domains if none provided
            var domains = allowedDomains ?? DefaultAllowedDomains;

            // Check if email ends with any allowed domain
            var isAllowed = domains.Any(domain =>
                email.EndsWith(domain, StringComparison.OrdinalIgnoreCase)
            );

            if (!isAllowed)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Warn($"Email address rejected (not in allowed domains): {email}");
            }

            return isAllowed;
        }

        /// <summary>
        /// Calculates SHA256 hash of a file for verification
        /// Useful for audit trails and backup verification
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>Hexadecimal hash string</returns>
        public static string CalculateFileHash(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Cannot calculate hash of non-existent file", filePath);

            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        /// <summary>
        /// Checks if a file has an allowed extension
        /// Uses culture-invariant comparison for file system operations
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="allowedExtensions">Array of allowed extensions (e.g., ".tif", ".pdf")</param>
        /// <returns>True if extension is allowed</returns>
        public static bool HasAllowedExtension(string filePath, string[] allowedExtensions)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (allowedExtensions == null || allowedExtensions.Length == 0)
                return false;

            var extension = Path.GetExtension(filePath);
            return allowedExtensions.Any(ext =>
                ext.Equals(extension, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Creates a backup of a file before deletion
        /// Returns the backup file path for audit logging
        /// </summary>
        /// <param name="sourceFile">File to backup</param>
        /// <param name="backupDirectory">Directory for backups</param>
        /// <returns>Path to backup file</returns>
        public static string CreateBackup(string sourceFile, string backupDirectory)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException("Cannot backup non-existent file", sourceFile);

            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            var fileName = Path.GetFileName(sourceFile);
            var backupPath = Path.Combine(backupDirectory, fileName);

            // If backup already exists, append timestamp with milliseconds and ensure uniqueness
            if (File.Exists(backupPath))
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);

                // Loop until we find a unique filename (prevents timestamp collisions)
                int attempt = 0;
                do
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    backupPath = Path.Combine(backupDirectory, $"{fileNameWithoutExt}_{timestamp}{ext}");

                    // Safety limit to prevent infinite loop
                    if (++attempt > 1000)
                    {
                        throw new IOException($"Failed to create unique backup filename after {attempt} attempts");
                    }

                    // Small delay if we're looping to ensure timestamp changes
                    if (attempt > 1)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                } while (File.Exists(backupPath));
            }

            File.Copy(sourceFile, backupPath, false); // Don't overwrite - we ensured uniqueness above
            return backupPath;
        }

        /// <summary>
        /// Validates that a directory path exists and is accessible
        /// </summary>
        /// <param name="path">Directory path to validate</param>
        /// <returns>True if directory exists and is accessible</returns>
        public static bool IsDirectoryAccessible(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return false;

                // Try to enumerate files to verify read access
                Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
