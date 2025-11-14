using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ============================================================================
// Assembly Information
// ============================================================================
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

/// <summary>
/// Assembly title displayed in Windows file properties.
/// </summary>
[assembly: AssemblyTitle("FileManager")]

/// <summary>
/// Description of the assembly's purpose and capabilities.
/// Key features: File counting, name modification, deletion, automated mailing,
/// Excel/PDF processing, and Outlook integration.
/// </summary>
[assembly: AssemblyDescription("count files, change names, delete files, send mails, etc..")]

/// <summary>
/// Configuration instructions for the assembly.
/// Important: Set the base path in App.config before running the application.
/// </summary>
[assembly: AssemblyConfiguration("Set base path in config file.")]

/// <summary>
/// Company information for the assembly.
/// Private development by Ofer Harari.
/// Contact: offa23@gmail.com
/// </summary>
[assembly: AssemblyCompany("Private - Ofer Harari. offa23@gmail.com")]

/// <summary>
/// Product name for this assembly.
/// </summary>
[assembly: AssemblyProduct("FileManager")]

/// <summary>
/// Copyright information for the assembly.
/// </summary>
[assembly: AssemblyCopyright("Copyright ©  2017")]

/// <summary>
/// Trademark information (none specified).
/// </summary>
[assembly: AssemblyTrademark("")]

/// <summary>
/// Culture setting for the assembly (neutral culture).
/// The application supports Hebrew language in the UI.
/// </summary>
[assembly: AssemblyCulture("")]

// ============================================================================
// COM Visibility
// ============================================================================
/// <summary>
/// Setting ComVisible to false makes the types in this assembly not visible
/// to COM components. If you need to access a type in this assembly from
/// COM, set the ComVisible attribute to true on that type.
/// Note: The application uses COM interop for Microsoft Office (Excel and Outlook).
/// </summary>
[assembly: ComVisible(false)]

/// <summary>
/// The following GUID is for the ID of the typelib if this project is exposed to COM.
/// This GUID uniquely identifies this assembly when accessed via COM.
/// </summary>
[assembly: Guid("ded4b8f8-1625-495c-bbb6-30681d14389e")]

// ============================================================================
// Version Information
// ============================================================================
// Version information for an assembly consists of the following four values:
//
//      Major Version    - Major feature updates (1.x.x.x)
//      Minor Version    - Minor feature additions (x.0.x.x)
//      Build Number     - Build/revision number (x.x.50.x)
//      Revision         - Hot fixes and patches (x.x.x.0)
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

/// <summary>
/// Assembly version used for referencing.
/// Format: Major.Minor.Build.Revision
/// </summary>
[assembly: AssemblyVersion("1.0.0.0")]

/// <summary>
/// File version displayed in Windows file properties.
/// Current version: 1.2.50
/// Recent updates include: .NET 4.8 upgrade, enhanced documentation,
/// copy file functionality, mail archive persistence.
/// </summary>
[assembly: AssemblyFileVersion("1.2.50")]
