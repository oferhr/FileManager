# FileManager

A Windows Forms application for managing files with integrated Microsoft Office automation capabilities.

## Project Overview

FileManager is a desktop application built with .NET Framework 4.8 that provides file management functionality with special features for handling Excel files, generating PDFs, and automating Outlook email operations. The application includes Hebrew language support and is designed for document processing workflows.

## Architecture

### Service-Oriented Design

The application follows a **service-oriented architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│         Presentation Layer (UI)             │
│  Form1.cs, ResultGrid.cs, print.cs          │
│  - Windows Forms UI components              │
│  - Event handlers and user interactions     │
│  - Display and input logic                  │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│         Service Layer (Business Logic)      │
│  Services/ directory (14 service classes)   │
│  - Core services (File, Config, Logging)    │
│  - Business logic services                  │
│  - Interface-based design pattern           │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│      Utilities Layer (Security & Validation)│
│  Utilities/ directory (3 utility classes)   │
│  - PathValidator (path security)            │
│  - EmailValidator (email security)          │
│  - InputValidator (general validation)      │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│         External Integrations               │
│  - Microsoft Office Interop (Excel, Outlook)│
│  - File System operations                   │
│  - NLog logging infrastructure              │
└─────────────────────────────────────────────┘
```

### Architecture Principles

1. **Separation of Concerns**: UI logic separated from business logic
2. **Interface-Based Design**: Each service has an interface for testability and flexibility
3. **Single Responsibility**: Each service handles one specific domain of functionality
4. **SOLID Principles**: Architecture follows SOLID design principles
5. **Security by Design**: Comprehensive input validation and path security through dedicated utilities
6. **Maintainability**: Form1.cs reduced from 4051 lines to 888 lines (78% reduction)

### Key Architectural Components

- **Form1.cs**: Main UI coordinator that orchestrates service calls and manages user interactions
- **Services Layer**: 14 specialized services handling distinct business operations
- **Utilities Layer**: 3 security and validation utility classes providing centralized validation logic
- **Data Models**: FileCount.cs contains all data transfer objects and settings classes
- **Configuration**: JSON-based configuration managed by ConfigurationService
- **Logging**: Centralized logging through LoggingService using NLog with security event tracking

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
├── packages/                 # NuGet packages directory
└── FileManager/              # Main project directory
    ├── Services/             # Service layer (business logic)
    │   ├── IArchiveService.cs / ArchiveService.cs
    │   ├── IConfigurationService.cs / ConfigurationService.cs
    │   ├── IDuplicateManagementService.cs / DuplicateManagementService.cs
    │   ├── IEmailService.cs / EmailService.cs
    │   ├── IExcelExportService.cs / ExcelExportService.cs
    │   ├── IExcelService.cs / ExcelService.cs
    │   ├── IFileCopyService.cs / FileCopyService.cs
    │   ├── IFileCountService.cs / FileCountService.cs
    │   ├── IFileDeletionService.cs / FileDeletionService.cs
    │   ├── IFileNameManagementService.cs / FileNameManagementService.cs
    │   ├── IFileService.cs / FileService.cs
    │   ├── IFolderSplitService.cs / FolderSplitService.cs
    │   ├── ILoggingService.cs / LoggingService.cs
    │   └── IReportManagementService.cs / ReportManagementService.cs
    ├── Utilities/            # Security and validation utilities
    │   ├── PathValidator.cs  # Path security and traversal prevention
    │   ├── EmailValidator.cs # Email validation and sanitization
    │   └── InputValidator.cs # General input validation
    ├── Form1.cs              # Main application form (refactored, 888 lines)
    ├── Form1.Designer.cs     # Form designer generated code
    ├── Form1.resx            # Form resources
    ├── ResultGrid.cs         # Results display grid
    ├── ResultGrid.Designer.cs
    ├── ResultGrid.resx
    ├── FileCount.cs          # Data model classes
    ├── print.cs              # Print operations
    ├── Settings.cs           # Application settings
    ├── Program.cs            # Application entry point
    ├── App.config            # Application configuration
    ├── NLog.config           # Logging configuration
    ├── packages.config       # NuGet package references
    ├── FodyWeavers.xml       # Fody configuration
    ├── REFACTORING_README.md # Refactoring documentation
    └── Properties/           # Assembly properties
        ├── AssemblyInfo.cs
        ├── Resources.Designer.cs
        ├── Resources.resx
        ├── Settings.Designer.cs
        ├── Settings.settings
        └── app.manifest
```

## Service Layer Documentation

The application's business logic is organized into 14 specialized services, each with a corresponding interface for testability and maintainability.

### Core Infrastructure Services

#### 1. IFileService / FileService
**Purpose**: Handles basic file operations
**Responsibilities**:
- File copying and moving operations
- File name manipulation and validation
- Thumbs.db detection and handling
- Mail file name generation

#### 2. IConfigurationService / ConfigurationService
**Purpose**: Manages all JSON configuration files
**Responsibilities**:
- Reading/writing email directory settings
- Reading/writing folder settings
- Reading/writing count, split, copy, and archive settings
- Centralized configuration persistence

#### 3. ILoggingService / LoggingService
**Purpose**: Centralized logging functionality
**Responsibilities**:
- Error logging with stack traces
- Info and warning logging for operations
- Security event logging with structured properties
- File operation logging (create, delete, move, copy)
- Validation failure logging
- File-based log persistence via NLog

### Business Logic Services

#### 4. IFileCountService / FileCountService
**Purpose**: Handles file counting operations
**Responsibilities**:
- Counting files in directories with progress reporting
- Thumbs.db exclusion logic
- Directory traversal and file enumeration

#### 5. IDuplicateManagementService / DuplicateManagementService
**Purpose**: Manages duplicate file operations
**Responsibilities**:
- Detecting and fixing duplicate files
- Checking folder duplication status
- Cleaning up empty folders after deduplication

#### 6. IFileNameManagementService / FileNameManagementService
**Purpose**: Handles file naming operations
**Responsibilities**:
- Fixing file names according to patterns
- Migdal-specific naming conventions
- Batch file renaming with progress reporting

#### 7. IExcelService / ExcelService
**Purpose**: Handles Excel file operations via COM Interop
**Responsibilities**:
- Reading values from Excel cells
- Setting Excel file names based on content
- File name transformations for Excel documents
- Excel automation and data extraction

#### 8. IFileDeletionService / FileDeletionService
**Purpose**: Handles file deletion operations
**Responsibilities**:
- Deleting old files based on criteria
- Age-based file deletion policies
- Pattern-based file deletion

#### 9. IReportManagementService / ReportManagementService
**Purpose**: Handles report file operations
**Responsibilities**:
- Setting report file names
- File grouping and organization
- Report processing with progress tracking

#### 10. IFolderSplitService / FolderSplitService
**Purpose**: Handles folder splitting operations
**Responsibilities**:
- Splitting folders based on file count criteria
- 888 pattern detection (special splitting logic)
- File reorganization across directories

#### 11. IFileCopyService / FileCopyService
**Purpose**: Handles file copying operations
**Responsibilities**:
- Copying files with specific patterns
- Destination directory management
- Progress reporting for bulk copy operations

#### 12. IArchiveService / ArchiveService
**Purpose**: Handles archive operations
**Responsibilities**:
- Archiving files to designated locations
- Directory structure management for archives
- Archive operation progress reporting

#### 13. IEmailService / EmailService
**Purpose**: Handles email operations via Outlook COM Interop
**Responsibilities**:
- Sending emails with file attachments
- Processing files for email distribution
- Outlook integration and automation
- Support for Hebrew mail types (איחוד-קצר, בודד-זהה, etc.)

#### 14. IExcelExportService / ExcelExportService
**Purpose**: Handles Excel export operations
**Responsibilities**:
- Exporting data collections to Excel workbooks
- Creating multiple worksheets within workbooks
- Data formatting and styling in Excel
- CSV to Excel conversion

## Utilities Layer Documentation

The application includes a comprehensive security and validation infrastructure through three specialized utility classes. These utilities are used throughout all services to ensure secure file operations, email handling, and input validation.

### PathValidator (Static Utility Class)

**File**: `FileManager\Utilities\PathValidator.cs`

**Purpose**: Provides centralized path validation and security to prevent path traversal attacks and ensure safe file operations.

**Key Methods**:

1. **`IsValidPath(string path, out string errorMessage)`**
   - Validates a path for basic safety checks
   - Checks for null/empty/whitespace
   - Validates against path traversal sequences (`..`, `.\`, etc.)
   - Checks for invalid characters
   - Validates path length (Windows MAX_PATH = 260 characters)
   - Prevents null byte injection
   - Returns: `true` if valid, `false` with error message if invalid

2. **`IsPathWithinBoundary(string path, string basePath, out string errorMessage)`**
   - Ensures a path stays within allowed base directory boundaries
   - Normalizes both paths using `Path.GetFullPath()`
   - Verifies the full path starts with the base path
   - Prevents directory traversal outside allowed boundaries
   - Returns: `true` if within boundary, `false` with error message if outside

3. **`SanitizePath(string path)`**
   - Sanitizes a path by removing dangerous characters
   - Removes null bytes
   - Normalizes path separators
   - Trims whitespace
   - Returns: Sanitized path string

4. **`ValidateAndNormalize(string path, string basePath, out string normalizedPath, out string errorMessage)`**
   - Comprehensive validation combining all checks
   - Validates path format
   - Normalizes using `Path.GetFullPath()`
   - Validates against base path boundary
   - Returns: `true` with normalized path if valid, `false` with error message if invalid

**Security Features**:
- Path traversal prevention (`..`, `.\`, UNC paths)
- Invalid character detection
- Path length validation (260 character limit)
- Null byte injection prevention
- Boundary enforcement for directory operations
- Path normalization for consistent comparisons

**Usage Example**:
```csharp
// Validate and normalize a path before file operations
if (!PathValidator.ValidateAndNormalize(userPath, basePath, out var normalizedPath, out var error))
{
    LoggingService.LogSecurityEvent("PathValidationFailure", $"Invalid path: {error}",
        new Dictionary<string, object> { { "Path", userPath } });
    throw new SecurityException($"Invalid path: {error}");
}

// Use the validated normalizedPath for file operations
File.Copy(sourceFile, normalizedPath);
```

### EmailValidator (Static Utility Class)

**File**: `FileManager\Utilities\EmailValidator.cs`

**Purpose**: Validates and sanitizes email addresses before use in Outlook automation to prevent email header injection and ensure RFC 5322 compliance.

**Key Methods**:

1. **`IsValidEmail(string email, out string errorMessage)`**
   - Validates single email address format
   - Uses RFC 5322 compliant regex pattern
   - Checks for email header injection attempts
   - Validates control characters
   - Checks for URL-encoded injection patterns
   - Returns: `true` if valid, `false` with error message if invalid

2. **`IsValidEmailList(string emails, out string errorMessage)`**
   - Validates multiple email addresses (comma or semicolon separated)
   - Parses list and validates each email individually
   - Returns: `true` if all emails valid, `false` with error message if any invalid

3. **`SanitizeEmailAddress(string email)`**
   - Sanitizes an email address by removing dangerous characters
   - Removes control characters (`\r`, `\n`, `\t`)
   - Trims whitespace
   - Returns: Sanitized email string

4. **`ParseEmailList(string emails)`**
   - Parses a delimited email list into individual addresses
   - Supports comma and semicolon delimiters
   - Trims whitespace from each address
   - Returns: `List<string>` of individual email addresses

**Security Features**:
- RFC 5322 email format compliance
- Email header injection prevention (detects `\r\n`, `\n`, control chars)
- URL-encoded injection pattern detection (`%0a`, `%0d`)
- Control character filtering
- Multi-email list validation support

**Usage Example**:
```csharp
// Validate email before sending through Outlook
if (!EmailValidator.IsValidEmail(recipientEmail, out var emailError))
{
    LoggingService.LogSecurityEvent("InvalidEmail", $"Invalid email address: {emailError}",
        new Dictionary<string, object> { { "Email", recipientEmail } });
    return;
}

var sanitizedEmail = EmailValidator.SanitizeEmailAddress(recipientEmail);
outlookMailItem.To = sanitizedEmail;
```

### InputValidator (Static Utility Class)

**File**: `FileManager\Utilities\InputValidator.cs`

**Purpose**: Provides general input validation framework for UI controls and user inputs.

**Key Methods**:

1. **`IsValidString(string input, int minLength, int maxLength, out string errorMessage)`**
   - Validates string length within specified bounds
   - Checks for null, empty, or whitespace
   - Returns: `true` if valid length, `false` with error message if invalid

2. **`IsValidFolderName(string folderName, out string errorMessage)`**
   - Validates folder name format
   - Checks for null/empty/whitespace
   - Validates against invalid filename characters
   - Checks for reserved Windows names (CON, PRN, AUX, etc.)
   - Validates length constraints
   - Returns: `true` if valid, `false` with error message if invalid

3. **`IsValidFileExtension(string extension, string[] allowedExtensions)`**
   - Validates file extension against allowed list
   - Case-insensitive comparison
   - Handles extensions with or without leading dot
   - Returns: `true` if extension in allowed list, `false` otherwise

4. **`IsValidNumericRange(int value, int min, int max, out string errorMessage)`**
   - Validates numeric value within specified range
   - Inclusive range checking
   - Returns: `true` if in range, `false` with error message if out of range

**Security Features**:
- Reserved Windows filename detection
- Invalid character filtering
- Length constraint enforcement
- Extension whitelist validation
- Range boundary checking

**Usage Example**:
```csharp
// Validate folder name from user input
if (!InputValidator.IsValidFolderName(userFolderName, out var error))
{
    LoggingService.LogValidationFailure("FolderNameValidation", userFolderName, error);
    MessageBox.Show($"Invalid folder name: {error}", "Validation Error",
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}

// Proceed with folder creation
Directory.CreateDirectory(Path.Combine(basePath, userFolderName));
```

### Integration with Services

All 14 services have been enhanced to use these validation utilities:

**Services with Path Validation**:
- ArchiveService (critical path traversal vulnerability fixed)
- ConfigurationService (all configuration paths validated)
- FileService (copy and move operations)
- FileDeletionService (deletion operations with comprehensive logging)
- FileCopyService (source and destination validation)
- FolderSplitService (split operations)
- ExcelService (Excel file operations)
- ReportManagementService (report operations)
- FileNameManagementService (renaming operations)
- DuplicateManagementService (duplicate detection)

**Services with Email Validation**:
- EmailService (Outlook automation with email validation)
- Form1.cs (email grid input validation)

**Logging Integration**:
- All validation failures are logged using `LoggingService.LogSecurityEvent()`
- All file operations are logged using `LoggingService.LogFileOperation()`
- Structured properties provide audit trail for security analysis

### Security Benefits

1. **Path Traversal Prevention**: All file operations validate paths to prevent `../` attacks
2. **Email Header Injection Prevention**: Email validation prevents control character injection
3. **Input Sanitization**: User inputs are validated before processing
4. **Centralized Validation Logic**: DRY principle applied across all services
5. **Comprehensive Logging**: Security events logged for audit and forensic analysis
6. **Boundary Enforcement**: File operations constrained to allowed directories
7. **Defense in Depth**: Multiple validation layers at utilities, services, and UI levels

## Key Features

1. **Service-Oriented Architecture**: 14 specialized services with interface-based design
2. **Security Infrastructure**: Comprehensive path validation, email validation, and input sanitization
3. **File Management**: File operations including copying, moving, archiving, and deduplication with security validation
4. **Excel Integration**: XML to Excel conversion, Excel data processing, and automated Excel operations
5. **PDF Operations**: PDF creation and merging capabilities
6. **Email Automation**: Outlook integration for automated mailing with Hebrew language support and email validation
7. **Data Archiving**: Archive functionality with directory structure management and path security
8. **Logging**: Comprehensive NLog-based logging system with security event tracking
9. **Configuration Management**: JSON-based configuration with dedicated service

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
- **Form1.cs**: Main form with comprehensive documentation:
  - All private fields with detailed purpose descriptions
  - Constructor with full initialization workflow explanation
  - Event handlers documented (Close, Start, Delete, Resize, Paint, etc.)
  - Main workflow method (bStart_Click) with complete processing sequence
  - Email automation method (btnMail_Click) with detailed workflow steps
  - Inline comments throughout constructor for grid initialization
  - Configuration loading and data binding logic explained
- **AssemblyInfo.cs**: Assembly metadata with XML documentation:
  - Assembly attributes explained
  - Version information documented
  - COM visibility settings with notes on Office interop

### Documentation Features

- XML documentation comments for all public classes and methods
- Inline comments explaining complex logic blocks
- Parameter and return value descriptions
- Code examples and usage notes where applicable
- Explanation of Hebrew language support and RTL rendering
- Thread safety and COM interop considerations

## Version History

### Version 1.3.0 (Security Hardening Update)
**Major Security Infrastructure Enhancements:**
- **NEW: Utilities Layer** - Added comprehensive security and validation infrastructure
  - PathValidator: Path traversal prevention and boundary enforcement
  - EmailValidator: RFC 5322 compliance and header injection prevention
  - InputValidator: General input validation framework
- **Enhanced Logging**: LoggingService now includes security event tracking, file operation logging, and validation failure logging
- **Service Layer Security**: All 14 services enhanced with path validation and security logging
- **Critical Fixes**:
  - Fixed path traversal vulnerability in ArchiveService
  - Added comprehensive logging to FileDeletionService (previously had none)
  - Fixed email validation in EmailService and Form1 email grid
- **Security Features**:
  - Path validation before all file operations
  - Email validation before Outlook automation
  - Boundary enforcement for directory operations
  - Comprehensive audit trail through structured logging
- **Documentation**: Complete security validation patterns and usage guidelines

### Version 1.2.43
- Copy file tab added
- XML to Excel conversion improvements
- PDF creation and merging functionality
- Mail archive with data persistence
- Excel export capabilities
- .NET Framework updated to 4.8

## Refactoring History

### Major Architecture Refactoring (Completed)

The application underwent a comprehensive refactoring to transform from a monolithic architecture to a service-oriented design.

**Before Refactoring:**
- Single Form1.cs file with **4051 lines** of code
- All business logic mixed with UI logic
- Difficult to test individual components
- Hard to maintain and extend
- Violation of Single Responsibility Principle

**After Refactoring:**
- Form1.cs reduced to **888 lines** (78% reduction)
- **14 specialized service classes** with clear responsibilities
- Interface-based design for all services
- Clear separation between UI and business logic
- Follows SOLID principles

### Refactoring Benefits

1. **Maintainability**
   - Each service has a single, well-defined responsibility
   - Easy to locate and fix issues in specific domains
   - Clear separation of concerns across the application

2. **Testability**
   - Each service can be unit tested independently
   - Mock interfaces enable isolated testing
   - Business logic decoupled from UI dependencies

3. **Extensibility**
   - Easy to add new functionality without affecting existing code
   - Services can be extended independently
   - Clear interfaces define contracts for new features

4. **Readability**
   - Much smaller, focused classes
   - Clear method names and responsibilities
   - Better code organization and navigation

5. **Reusability**
   - Services can be reused across different parts of the application
   - Common functionality extracted to shared services
   - Consistent patterns across the codebase

### Migration Details

For detailed information about the refactoring process, service responsibilities, and migration guide, see `REFACTORING_README.md` in the FileManager directory.

## Development Guide

### Working with Services

1. **Adding New Features**
   - Create a new service interface in Services/I[ServiceName].cs
   - Implement the interface in Services/[ServiceName].cs
   - Inject the service into Form1.cs constructor
   - Add necessary UI event handlers to call service methods

2. **Modifying Existing Features**
   - Locate the appropriate service based on the domain (file operations, email, Excel, etc.)
   - Modify the service implementation
   - Update the interface if method signatures change
   - Test the changes independently before UI integration

3. **Service Dependencies**
   - Services are injected into Form1.cs at construction time
   - Common dependencies: ILoggingService, IConfigurationService
   - Keep service dependencies minimal for better testability

### Common Development Tasks

- **Adding a new configuration setting**: Modify ConfigurationService and update the corresponding data model in FileCount.cs
- **Adding file operations**: Extend FileService or create a new specialized service if the domain is distinct
  - ALWAYS use PathValidator.ValidateAndNormalize() before file operations
  - ALWAYS use LoggingService.LogFileOperation() to log operations
  - ALWAYS validate paths against base directory boundaries
- **Excel automation**: Use ExcelService for reading/manipulation, ExcelExportService for data export
- **Email functionality**: Extend EmailService with new mail types or sending patterns
  - ALWAYS use EmailValidator.IsValidEmail() before sending emails
  - ALWAYS sanitize email addresses with EmailValidator.SanitizeEmailAddress()
- **Logging**: Use ILoggingService injected into services; avoid direct NLog calls
- **Input validation**: Use InputValidator for UI inputs, PathValidator for paths, EmailValidator for emails

### Security Validation Patterns

When implementing new features, always follow these security patterns:

#### Pattern 1: File Operation Validation
```csharp
// Before any file operation, validate and normalize the path
if (!PathValidator.ValidateAndNormalize(userPath, basePath, out var normalizedPath, out var error))
{
    LoggingService.LogSecurityEvent("PathValidationFailure", $"Invalid path: {error}",
        new Dictionary<string, object> { { "Path", userPath }, { "Source", "ServiceName" } });
    throw new SecurityException($"Invalid path: {error}");
}

// Use the validated normalizedPath for file operations
File.Copy(sourceFile, normalizedPath);
LoggingService.LogFileOperation("Copy", normalizedPath, true);
```

#### Pattern 2: Email Validation
```csharp
// Before sending emails, validate the email address
if (!EmailValidator.IsValidEmail(recipientEmail, out var emailError))
{
    LoggingService.LogSecurityEvent("InvalidEmail", $"Invalid email: {emailError}",
        new Dictionary<string, object> { { "Email", recipientEmail } });
    return; // or throw exception
}

var sanitizedEmail = EmailValidator.SanitizeEmailAddress(recipientEmail);
outlookMailItem.To = sanitizedEmail;
```

#### Pattern 3: Boundary Validation
```csharp
// When working with user-provided paths, enforce directory boundaries
if (!PathValidator.IsPathWithinBoundary(targetPath, allowedBaseDirectory, out var error))
{
    LoggingService.LogSecurityEvent("PathBoundaryViolation",
        $"Path outside allowed boundary: {error}",
        new Dictionary<string, object> {
            { "Path", targetPath },
            { "BasePath", allowedBaseDirectory }
        });
    throw new SecurityException("Path outside allowed directory");
}
```

#### Pattern 4: Input Validation in UI
```csharp
// Validate user inputs in event handlers
if (!InputValidator.IsValidFolderName(userInput, out var error))
{
    LoggingService.LogValidationFailure("FolderNameValidation", userInput, error);
    MessageBox.Show($"Invalid folder name: {error}", "Validation Error",
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}
```

#### Pattern 5: Comprehensive Error Logging
```csharp
// Never use silent catch blocks - always log exceptions
try
{
    // Operation
    LoggingService.LogFileOperation("Operation", path, true);
}
catch (Exception ex)
{
    LoggingService.LogError("Operation failed", "ServiceName", ex);
    LoggingService.LogFileOperation("Operation", path, false, ex.Message);
    // Handle or rethrow as appropriate
}
```

### Prerequisites for Development

**Required Software:**
- Visual Studio 2013 or later (project uses ToolsVersion 12.0)
- .NET Framework 4.8 SDK
- Microsoft Office installation (Excel and Outlook) for COM Interop development

**Optional:**
- NuGet Package Manager (for dependency restoration)
- Git (for version control)
