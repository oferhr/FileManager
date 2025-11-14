using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FileManager
{
    /// <summary>
    /// Main program entry point for the FileManager application.
    /// This class initializes and starts the Windows Forms application.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Configures application-wide settings and launches the main form.
        /// </summary>
        /// <remarks>
        /// The STAThread attribute is required for COM interop with Microsoft Office applications
        /// (Excel and Outlook) used throughout the application.
        /// </remarks>
        [STAThread]
        static void Main()
        {
            // Enable visual styles for modern Windows UI rendering
            Application.EnableVisualStyles();

            // Set text rendering to use GDI+ for better compatibility
            Application.SetCompatibleTextRenderingDefault(false);

            // Start the application with Form1 as the main window
            Application.Run(new Form1());
        }
    }
}
