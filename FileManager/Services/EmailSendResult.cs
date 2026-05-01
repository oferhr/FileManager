using System.Collections.Generic;

namespace FileManager.Services
{
    /// <summary>
    /// Captures the outcome of a <see cref="IEmailService.SendEmails"/> call so callers
    /// can distinguish full success, partial success, and complete failure without relying
    /// on thrown exceptions.
    /// </summary>
    public class EmailSendResult
    {
        /// <summary>Total number of emails for which an Outlook send was attempted.</summary>
        public int Attempted { get; set; }

        /// <summary>Number of emails for which <c>oMsg.Send()</c> completed without exception.</summary>
        public int Succeeded { get; set; }

        /// <summary>Rows skipped because the email field was empty or whitespace.</summary>
        public int SkippedNoEmail { get; set; }

        /// <summary>Rows skipped because the email address(es) failed RFC 5322 format validation.</summary>
        public int SkippedInvalidFormat { get; set; }

        /// <summary>Rows skipped because the resolved directory path was invalid or did not exist on disk.</summary>
        public int SkippedMissingFolder { get; set; }

        /// <summary>Rows skipped because no .tif/.tiff/.pdf files were found in the directory.</summary>
        public int SkippedNoFiles { get; set; }

        /// <summary>
        /// Recipient strings (plus brief reason) for every email where <c>oMsg.Send()</c> threw.
        /// Format: &quot;recipient@domain.com: exception message&quot;.
        /// </summary>
        public List<string> FailedRecipients { get; set; } = new List<string>();

        /// <summary>Sum of all skip counters.</summary>
        public int SkippedTotal
        {
            get { return SkippedNoEmail + SkippedInvalidFormat + SkippedMissingFolder + SkippedNoFiles; }
        }

        /// <summary>
        /// <c>true</c> when at least one send was attempted, every attempt succeeded,
        /// and nothing was skipped.
        /// </summary>
        public bool AllSucceeded
        {
            get { return Attempted > 0 && Succeeded == Attempted && SkippedTotal == 0 && FailedRecipients.Count == 0; }
        }
    }
}
