using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileManager.Utilities
{
    /// <summary>
    /// Provides general input validation framework for UI controls and user inputs.
    /// </summary>
    public static class InputValidator
    {
        // Windows reserved file/folder names
        private static readonly string[] ReservedNames = {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Validates a string for null, empty, and length constraints.
        /// </summary>
        /// <param name="input">The input string to validate</param>
        /// <param name="minLength">Minimum required length (0 for no minimum)</param>
        /// <param name="maxLength">Maximum allowed length (int.MaxValue for no maximum)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if string is valid, false otherwise</returns>
        public static bool IsValidString(string input, int minLength, int maxLength, out string errorMessage)
        {
            errorMessage = null;

            // Check for null
            if (input == null)
            {
                errorMessage = "Input cannot be null";
                return false;
            }

            // Check minimum length
            if (input.Length < minLength)
            {
                errorMessage = $"Input length ({input.Length}) is less than minimum required ({minLength})";
                return false;
            }

            // Check maximum length
            if (input.Length > maxLength)
            {
                errorMessage = $"Input length ({input.Length}) exceeds maximum allowed ({maxLength})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a string is not null or whitespace.
        /// </summary>
        /// <param name="input">The input string to validate</param>
        /// <param name="fieldName">Name of the field for error messages</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if string is valid, false otherwise</returns>
        public static bool IsNonEmptyString(string input, string fieldName, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = $"{fieldName} cannot be empty";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a folder name for invalid characters and reserved names.
        /// </summary>
        /// <param name="folderName">The folder name to validate (not a full path)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if folder name is valid, false otherwise</returns>
        public static bool IsValidFolderName(string folderName, out string errorMessage)
        {
            errorMessage = null;

            // Check for null or empty
            if (string.IsNullOrWhiteSpace(folderName))
            {
                errorMessage = "Folder name cannot be null or empty";
                return false;
            }

            // Trim for validation
            folderName = folderName.Trim();

            // Check for invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (folderName.IndexOfAny(invalidChars) >= 0)
            {
                errorMessage = $"Folder name contains invalid characters";
                return false;
            }

            // Check for reserved names
            string upperName = folderName.ToUpperInvariant();
            if (ReservedNames.Contains(upperName))
            {
                errorMessage = $"Folder name '{folderName}' is a reserved Windows name";
                return false;
            }

            // Check for names ending with space or period (Windows restriction)
            if (folderName.EndsWith(" ") || folderName.EndsWith("."))
            {
                errorMessage = "Folder name cannot end with a space or period";
                return false;
            }

            // Check length (Windows has 255 char limit for folder names)
            if (folderName.Length > 255)
            {
                errorMessage = "Folder name exceeds maximum length (255 characters)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a file extension against a list of allowed extensions.
        /// </summary>
        /// <param name="extension">The file extension to validate (with or without leading dot)</param>
        /// <param name="allowedExtensions">Array of allowed extensions (e.g., [".txt", ".pdf"])</param>
        /// <returns>True if extension is allowed, false otherwise</returns>
        public static bool IsValidFileExtension(string extension, string[] allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(extension) || allowedExtensions == null || allowedExtensions.Length == 0)
            {
                return false;
            }

            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            // Case-insensitive comparison
            return allowedExtensions.Any(allowed =>
                string.Equals(extension, allowed, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Validates that a numeric value is within a specified range.
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="min">Minimum allowed value (inclusive)</param>
        /// <param name="max">Maximum allowed value (inclusive)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if value is within range, false otherwise</returns>
        public static bool IsValidNumericRange(int value, int min, int max, out string errorMessage)
        {
            errorMessage = null;

            if (value < min)
            {
                errorMessage = $"Value ({value}) is less than minimum allowed ({min})";
                return false;
            }

            if (value > max)
            {
                errorMessage = $"Value ({value}) exceeds maximum allowed ({max})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a numeric value is within a specified range (long version).
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="min">Minimum allowed value (inclusive)</param>
        /// <param name="max">Maximum allowed value (inclusive)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if value is within range, false otherwise</returns>
        public static bool IsValidNumericRange(long value, long min, long max, out string errorMessage)
        {
            errorMessage = null;

            if (value < min)
            {
                errorMessage = $"Value ({value}) is less than minimum allowed ({min})";
                return false;
            }

            if (value > max)
            {
                errorMessage = $"Value ({value}) exceeds maximum allowed ({max})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a string against a regular expression pattern.
        /// </summary>
        /// <param name="input">The input string to validate</param>
        /// <param name="pattern">Regular expression pattern</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if string matches pattern, false otherwise</returns>
        public static bool MatchesPattern(string input, string pattern, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Input cannot be null or empty";
                return false;
            }

            try
            {
                var regex = new Regex(pattern);
                if (!regex.IsMatch(input))
                {
                    errorMessage = $"Input does not match required pattern";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error validating pattern: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Validates that a string contains only alphanumeric characters and optionally spaces/dashes.
        /// </summary>
        /// <param name="input">The input string to validate</param>
        /// <param name="allowSpaces">Whether to allow spaces</param>
        /// <param name="allowDashes">Whether to allow dashes</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if string is alphanumeric, false otherwise</returns>
        public static bool IsAlphanumeric(string input, bool allowSpaces, bool allowDashes, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Input cannot be null or empty";
                return false;
            }

            string pattern = @"^[a-zA-Z0-9";
            if (allowSpaces) pattern += @"\s";
            if (allowDashes) pattern += @"\-";
            pattern += "]+$";

            return MatchesPattern(input, pattern, out errorMessage);
        }

        /// <summary>
        /// Validates a list of items against a maximum count.
        /// </summary>
        /// <param name="items">The list of items to validate</param>
        /// <param name="maxCount">Maximum allowed number of items</param>
        /// <param name="itemType">Description of item type for error messages</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if list count is valid, false otherwise</returns>
        public static bool IsValidListCount<T>(ICollection<T> items, int maxCount, string itemType, out string errorMessage)
        {
            errorMessage = null;

            if (items == null)
            {
                errorMessage = $"{itemType} list cannot be null";
                return false;
            }

            if (items.Count > maxCount)
            {
                errorMessage = $"Number of {itemType} ({items.Count}) exceeds maximum allowed ({maxCount})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a string contains only safe characters (no control characters).
        /// </summary>
        /// <param name="input">The input string to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if string contains only safe characters, false otherwise</returns>
        public static bool ContainsOnlySafeCharacters(string input, out string errorMessage)
        {
            errorMessage = null;

            if (input == null)
            {
                errorMessage = "Input cannot be null";
                return false;
            }

            // Check for control characters (except whitespace like tab, newline, carriage return)
            foreach (char c in input)
            {
                if (char.IsControl(c) && c != '\t' && c != '\n' && c != '\r')
                {
                    errorMessage = $"Input contains control character at position {input.IndexOf(c)}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sanitizes a string by removing control characters.
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>Sanitized string with control characters removed</returns>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Remove control characters except whitespace
            return Regex.Replace(input, @"[\x00-\x08\x0B-\x0C\x0E-\x1F\x7F]", string.Empty);
        }

        /// <summary>
        /// Validates that a date/time value is within a specified range.
        /// </summary>
        /// <param name="value">The date/time value to validate</param>
        /// <param name="minDate">Minimum allowed date (inclusive)</param>
        /// <param name="maxDate">Maximum allowed date (inclusive)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if date is within range, false otherwise</returns>
        public static bool IsValidDateRange(DateTime value, DateTime minDate, DateTime maxDate, out string errorMessage)
        {
            errorMessage = null;

            if (value < minDate)
            {
                errorMessage = $"Date ({value:yyyy-MM-dd}) is earlier than minimum allowed ({minDate:yyyy-MM-dd})";
                return false;
            }

            if (value > maxDate)
            {
                errorMessage = $"Date ({value:yyyy-MM-dd}) is later than maximum allowed ({maxDate:yyyy-MM-dd})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates multiple inputs and combines error messages.
        /// </summary>
        /// <param name="validations">Dictionary of field names to validation results</param>
        /// <param name="combinedErrorMessage">Combined error message if any validation fails</param>
        /// <returns>True if all validations pass, false otherwise</returns>
        public static bool ValidateAll(Dictionary<string, (bool isValid, string errorMessage)> validations, out string combinedErrorMessage)
        {
            combinedErrorMessage = null;

            var errors = validations
                .Where(v => !v.Value.isValid)
                .Select(v => $"{v.Key}: {v.Value.errorMessage}")
                .ToList();

            if (errors.Count > 0)
            {
                combinedErrorMessage = string.Join(Environment.NewLine, errors);
                return false;
            }

            return true;
        }
    }
}
