using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileManager.Utilities
{
    /// <summary>
    /// Provides email validation and sanitization for Outlook automation.
    /// Prevents email header injection and ensures RFC 5322 compliance.
    /// </summary>
    public static class EmailValidator
    {
        // RFC 5322 compliant email regex pattern (simplified but robust)
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Characters that could be used for email header injection
        private static readonly char[] InjectionChars = { '\r', '\n', '\0' };

        /// <summary>
        /// Validates a single email address.
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if email is valid, false otherwise</returns>
        public static bool IsValidEmail(string email, out string errorMessage)
        {
            errorMessage = null;

            // Check for null or empty
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email address cannot be null or empty";
                return false;
            }

            // Trim whitespace
            email = email.Trim();

            // Check length (RFC 5321: local part max 64, domain max 255)
            if (email.Length > 320)
            {
                errorMessage = "Email address exceeds maximum length (320 characters)";
                return false;
            }

            // Check for header injection characters
            if (email.IndexOfAny(InjectionChars) >= 0 ||
                email.Contains("%0d") ||
                email.Contains("%0a") ||
                email.Contains("%0D") ||
                email.Contains("%0A"))
            {
                errorMessage = "Email address contains invalid control characters (potential header injection)";
                return false;
            }

            // Validate email format using regex
            if (!EmailRegex.IsMatch(email))
            {
                errorMessage = "Email address format is invalid";
                return false;
            }

            // Additional validation: check local and domain parts
            var parts = email.Split('@');
            if (parts.Length != 2)
            {
                errorMessage = "Email address must contain exactly one @ symbol";
                return false;
            }

            string localPart = parts[0];
            string domainPart = parts[1];

            // Validate local part length
            if (localPart.Length > 64)
            {
                errorMessage = "Email local part exceeds maximum length (64 characters)";
                return false;
            }

            // Validate domain part length
            if (domainPart.Length > 255)
            {
                errorMessage = "Email domain part exceeds maximum length (255 characters)";
                return false;
            }

            // Validate domain has at least one dot
            if (!domainPart.Contains('.'))
            {
                errorMessage = "Email domain must contain at least one dot";
                return false;
            }

            // Validate domain doesn't start or end with dot or hyphen
            if (domainPart.StartsWith(".") || domainPart.EndsWith(".") ||
                domainPart.StartsWith("-") || domainPart.EndsWith("-"))
            {
                errorMessage = "Email domain has invalid format (starts or ends with dot/hyphen)";
                return false;
            }

            // Validate TLD length (at least 2 characters)
            string[] domainParts = domainPart.Split('.');
            string tld = domainParts[domainParts.Length - 1];
            if (tld.Length < 2)
            {
                errorMessage = "Email top-level domain must be at least 2 characters";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a list of email addresses (comma or semicolon separated).
        /// </summary>
        /// <param name="emails">The email list to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if all emails are valid, false otherwise</returns>
        public static bool IsValidEmailList(string emails, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(emails))
            {
                errorMessage = "Email list cannot be null or empty";
                return false;
            }

            var emailList = ParseEmailList(emails);

            if (emailList.Count == 0)
            {
                errorMessage = "No valid email addresses found in list";
                return false;
            }

            // Validate each email in the list
            for (int i = 0; i < emailList.Count; i++)
            {
                if (!IsValidEmail(emailList[i], out string emailError))
                {
                    errorMessage = $"Email {i + 1} is invalid: {emailError} ('{emailList[i]}')";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sanitizes an email address by removing potentially dangerous characters.
        /// </summary>
        /// <param name="email">The email address to sanitize</param>
        /// <returns>Sanitized email address</returns>
        public static string SanitizeEmailAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            // Trim whitespace
            email = email.Trim();

            // Remove injection characters
            foreach (char c in InjectionChars)
            {
                email = email.Replace(c.ToString(), string.Empty);
            }

            // Remove URL-encoded injection patterns (case-insensitive)
            email = Regex.Replace(email, "%0d", string.Empty, RegexOptions.IgnoreCase);
            email = Regex.Replace(email, "%0a", string.Empty, RegexOptions.IgnoreCase);

            // Remove any remaining control characters
            email = Regex.Replace(email, @"[\x00-\x1F\x7F]", string.Empty);

            return email;
        }

        /// <summary>
        /// Parses a string containing multiple email addresses separated by commas or semicolons.
        /// </summary>
        /// <param name="emails">The email list string to parse</param>
        /// <returns>List of individual email addresses</returns>
        public static List<string> ParseEmailList(string emails)
        {
            if (string.IsNullOrWhiteSpace(emails))
            {
                return new List<string>();
            }

            // Split by comma or semicolon
            var separators = new[] { ',', ';' };
            var emailList = emails
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            return emailList;
        }

        /// <summary>
        /// Parses an email list, sanitizes each address, drops any that collapse to empty,
        /// and returns a canonical semicolon-delimited string (e.g. "a@x.com; b@y.com").
        /// </summary>
        /// <param name="emails">The email list string to sanitize</param>
        /// <returns>Canonical semicolon-delimited list of sanitized addresses; empty string if none remain</returns>
        public static string SanitizeEmailList(string emails)
        {
            return string.Join("; ",
                ParseEmailList(emails)
                    .Select(SanitizeEmailAddress)
                    .Where(a => !string.IsNullOrWhiteSpace(a)));
        }

        /// <summary>
        /// Validates and sanitizes an email address in one operation.
        /// </summary>
        /// <param name="email">The email address to validate and sanitize</param>
        /// <param name="sanitizedEmail">The sanitized email if validation succeeds</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if email is valid after sanitization, false otherwise</returns>
        public static bool ValidateAndSanitize(string email, out string sanitizedEmail, out string errorMessage)
        {
            sanitizedEmail = null;
            errorMessage = null;

            // First sanitize
            sanitizedEmail = SanitizeEmailAddress(email);

            // Then validate the sanitized version
            if (!IsValidEmail(sanitizedEmail, out errorMessage))
            {
                sanitizedEmail = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates and sanitizes a list of email addresses.
        /// </summary>
        /// <param name="emails">The email list to validate and sanitize</param>
        /// <param name="sanitizedEmails">List of sanitized emails if validation succeeds</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if all emails are valid after sanitization, false otherwise</returns>
        public static bool ValidateAndSanitizeList(string emails, out List<string> sanitizedEmails, out string errorMessage)
        {
            sanitizedEmails = new List<string>();
            errorMessage = null;

            var emailList = ParseEmailList(emails);

            if (emailList.Count == 0)
            {
                errorMessage = "No email addresses found in list";
                return false;
            }

            // Validate and sanitize each email
            for (int i = 0; i < emailList.Count; i++)
            {
                if (!ValidateAndSanitize(emailList[i], out string sanitized, out string error))
                {
                    errorMessage = $"Email {i + 1} is invalid: {error} ('{emailList[i]}')";
                    sanitizedEmails = null;
                    return false;
                }
                sanitizedEmails.Add(sanitized);
            }

            return true;
        }
    }
}
