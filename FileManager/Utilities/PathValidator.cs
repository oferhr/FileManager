using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileManager.Utilities
{
    /// <summary>
    /// Provides centralized path validation to prevent path traversal attacks and ensure path safety.
    /// </summary>
    public static class PathValidator
    {
        private const int MaxPathLength = 260; // Windows MAX_PATH
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        // Path traversal patterns to detect
        private static readonly string[] TraversalPatterns = new[]
        {
            "..",
            "/../",
            "\\..\\",
            "/..",
            "\\..",
            "%2e%2e",
            "%252e%252e",
            "..%2f",
            "..%5c"
        };

        /// <summary>
        /// Validates a path for security and correctness.
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if path is valid, false otherwise</returns>
        public static bool IsValidPath(string path, out string errorMessage)
        {
            errorMessage = null;

            // Check for null or empty
            if (string.IsNullOrWhiteSpace(path))
            {
                errorMessage = "Path cannot be null or empty";
                return false;
            }

            // Check for null byte injection
            if (path.Contains('\0'))
            {
                errorMessage = "Path contains null byte character";
                return false;
            }

            // Check path length
            if (path.Length > MaxPathLength)
            {
                errorMessage = $"Path length ({path.Length}) exceeds maximum allowed length ({MaxPathLength})";
                return false;
            }

            // Check for invalid characters
            if (path.IndexOfAny(InvalidPathChars) >= 0)
            {
                errorMessage = "Path contains invalid characters";
                return false;
            }

            // Check for path traversal patterns
            string normalizedPath = path.Replace('/', '\\').ToLowerInvariant();
            foreach (var pattern in TraversalPatterns)
            {
                if (normalizedPath.Contains(pattern.ToLowerInvariant()))
                {
                    errorMessage = $"Path contains traversal pattern: {pattern}";
                    return false;
                }
            }

            // Call Path.GetFullPath purely to validate the path format; it throws for malformed paths.
            try
            {
                Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                errorMessage = $"Path format is invalid: {ex.Message}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a path stays within a specified base directory boundary.
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <param name="basePath">The base directory that path must stay within</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if path is within boundary, false otherwise</returns>
        public static bool IsPathWithinBoundary(string path, string basePath, out string errorMessage)
        {
            errorMessage = null;

            // Validate both paths first
            if (!IsValidPath(path, out string pathError))
            {
                errorMessage = $"Invalid path: {pathError}";
                return false;
            }

            if (!IsValidPath(basePath, out string baseError))
            {
                errorMessage = $"Invalid base path: {baseError}";
                return false;
            }

            try
            {
                // Get full absolute paths
                string fullPath = Path.GetFullPath(path);
                string fullBasePath = Path.GetFullPath(basePath);

                // Ensure both paths end with directory separator for accurate comparison
                if (!fullBasePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                    !fullBasePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    fullBasePath += Path.DirectorySeparatorChar;
                }

                // Check if path starts with base path (case-insensitive for Windows)
                if (!fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = $"Path is outside allowed boundary. Path: {fullPath}, Boundary: {fullBasePath}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error validating path boundary: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Sanitizes a path by removing potentially dangerous characters and patterns.
        /// </summary>
        /// <param name="path">The path to sanitize</param>
        /// <returns>Sanitized path</returns>
        public static string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            // Remove null bytes
            path = path.Replace("\0", string.Empty);

            // Remove any invalid path characters
            foreach (char c in InvalidPathChars)
            {
                path = path.Replace(c.ToString(), string.Empty);
            }

            // Remove path traversal patterns
            foreach (var pattern in TraversalPatterns)
            {
                path = path.Replace(pattern, string.Empty);
            }

            return path;
        }

        /// <summary>
        /// Validates and normalizes a path, ensuring it's within a base directory boundary.
        /// </summary>
        /// <param name="path">The path to validate and normalize</param>
        /// <param name="basePath">The base directory that path must stay within</param>
        /// <param name="normalizedPath">The normalized absolute path if validation succeeds</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if validation succeeds, false otherwise</returns>
        public static bool ValidateAndNormalize(string path, string basePath, out string normalizedPath, out string errorMessage)
        {
            normalizedPath = null;
            errorMessage = null;

            // Validate the path
            if (!IsValidPath(path, out string pathError))
            {
                errorMessage = pathError;
                return false;
            }

            try
            {
                // Get normalized absolute path
                normalizedPath = Path.GetFullPath(path);

                // If base path is provided, validate boundary
                if (!string.IsNullOrWhiteSpace(basePath))
                {
                    if (!IsPathWithinBoundary(normalizedPath, basePath, out string boundaryError))
                    {
                        errorMessage = boundaryError;
                        normalizedPath = null;
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error normalizing path: {ex.Message}";
                normalizedPath = null;
                return false;
            }
        }

        /// <summary>
        /// Validates a file name (not a full path) for invalid characters.
        /// </summary>
        /// <param name="fileName">The file name to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if file name is valid, false otherwise</returns>
        public static bool IsValidFileName(string fileName, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                errorMessage = "File name cannot be null or empty";
                return false;
            }

            // Check for invalid file name characters
            if (fileName.IndexOfAny(InvalidFileNameChars) >= 0)
            {
                errorMessage = "File name contains invalid characters";
                return false;
            }

            // Check for reserved names (Windows)
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5",
                                      "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4",
                                      "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
            if (reservedNames.Contains(nameWithoutExtension))
            {
                errorMessage = $"File name uses reserved Windows name: {nameWithoutExtension}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a directory exists and is accessible.
        /// </summary>
        /// <param name="directoryPath">The directory path to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if directory exists and is accessible, false otherwise</returns>
        public static bool IsAccessibleDirectory(string directoryPath, out string errorMessage)
        {
            errorMessage = null;

            if (!IsValidPath(directoryPath, out string pathError))
            {
                errorMessage = pathError;
                return false;
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    errorMessage = "Directory does not exist";
                    return false;
                }

                // Try to enumerate to check accessibility
                Directory.EnumerateFileSystemEntries(directoryPath).Take(1).ToList();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Access denied to directory";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error accessing directory: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Validates that a file exists and is accessible.
        /// </summary>
        /// <param name="filePath">The file path to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if file exists and is accessible, false otherwise</returns>
        public static bool IsAccessibleFile(string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (!IsValidPath(filePath, out string pathError))
            {
                errorMessage = pathError;
                return false;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    errorMessage = "File does not exist";
                    return false;
                }

                // Try to get file info to check accessibility
                var fileInfo = new FileInfo(filePath);
                var _ = fileInfo.Length; // Access a property to verify readability
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Access denied to file";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error accessing file: {ex.Message}";
                return false;
            }
        }
    }
}
