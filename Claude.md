# FileManager

A Windows Forms application for managing files with integrated Microsoft Office automation capabilities.

## Project Overview

FileManager is a desktop application built with .NET Framework 4.8 that provides file management functionality with special features for handling Excel files, generating PDFs, and automating Outlook email operations. The application includes Hebrew language support and is designed for document processing workflows.

## Technology Stack

- **Framework**: .NET Framework 4.8
- **Language**: C# 7.3
- **UI**: Windows Forms
- **Office Integration**:
  - Microsoft.Office.Interop.Excel (Excel automation)
  - Microsoft.Office.Interop.Outlook (Email automation)
- **Logging**: NLog 4.6.8
- **JSON**: Newtonsoft.Json 13.0.3
- **Dependency Embedding**: Costura.Fody 3.3.3

## Project Structure

```
FileManager/
├── FileManager.sln           # Solution file
└── FileManager/              # Main project directory
    ├── Form1.cs              # Main application form
    ├── ResultGrid.cs         # Results display grid
    ├── FileCount.cs          # File counting functionality
    ├── print.cs              # Print operations
    ├── Settings.cs           # Application settings
    ├── Program.cs            # Application entry point
    ├── App.config            # Application configuration
    ├── NLog.config           # Logging configuration
    └── Properties/           # Assembly properties
```

## Key Features

1. **File Management**: File operations including copying, moving, and archiving
2. **Excel Integration**: XML to Excel conversion, Excel data processing
3. **PDF Operations**: PDF creation and merging capabilities
4. **Email Automation**: Outlook integration for automated mailing
5. **Data Archiving**: Archive functionality for data storage
6. **Logging**: Comprehensive NLog-based logging system

## Development Configuration

- **Build Platform**: AnyCPU (with x86 configuration available)
- **Language Version**: C# 7.3
- **Output Type**: WinExe (Windows executable)
- **Solution**: Visual Studio project (ToolsVersion 12.0)

## Configuration

The application uses App.config for runtime configuration with settings including:
- `basePath`: Base path for file operations
- Additional configuration settings stored in App.config

## Build & Run

1. Open `FileManager.sln` in Visual Studio
2. Restore NuGet packages (Fody, Costura.Fody, NLog, Newtonsoft.Json)
3. Build the solution (Debug or Release configuration)
4. Run the executable from `bin/Debug/` or `bin/Release/`

## Dependencies

External dependencies are managed through NuGet and embedded using Costura.Fody:
- NLog (logging framework)
- Newtonsoft.Json (JSON serialization)
- Microsoft Office Interop assemblies (Excel, Outlook)
- Costura.Fody (dependency embedding)

## Language Support

The application includes Hebrew language support for UI elements and mail operations, with specific mail types:
- איחוד-קצר
- בודד-זהה
- בודד-קצר
- איחוד שמי
- איחוד לפי דוח

## Code Documentation

The codebase includes comprehensive XML documentation comments and inline explanations:

### Documentation Coverage

- **Program.cs**: Application entry point with XML docs explaining STAThread requirement for Office automation
- **Settings.cs**: Settings manager with event handler documentation
- **FileCount.cs**: All data model classes fully documented, including:
  - FileCount: File counting results
  - EmailDirSettings: Email directory configuration
  - FolderSettings: Folder path management
  - CountSettings, SplitSettings, CopySettings: Operation-specific settings
  - Grouper: File grouping functionality
  - ArchiveSettings: Archive operation configuration
- **print.cs**: Complete documentation of the DataGridView printing system:
  - RTL (Right-to-Left) support for Hebrew
  - Page layout and pagination logic
  - Column width calculations
  - Header and cell rendering
- **ResultGrid.cs**: Results display form with CSV export and print capabilities
- **Form1.cs**: Main form with comprehensive field documentation and constructor explanation

### Documentation Features

- XML documentation comments for all public classes and methods
- Inline comments explaining complex logic blocks
- Parameter and return value descriptions
- Code examples and usage notes where applicable
- Explanation of Hebrew language support and RTL rendering
- Thread safety and COM interop considerations

## Version History

Recent version: 1.2.43
- Copy file tab added
- XML to Excel conversion improvements
- PDF creation and merging functionality
- Mail archive with data persistence
- Excel export capabilities
- .NET Framework updated to 4.8
