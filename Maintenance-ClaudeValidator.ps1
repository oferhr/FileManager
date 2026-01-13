# FileManager CLAUDE.md Maintenance Validator
# Purpose: Monitor project changes and validate CLAUDE.md synchronization
# Version: 1.0.0
# Usage: .\Maintenance-ClaudeValidator.ps1

param(
    [switch]$QuickCheck = $false,
    [switch]$FullValidation = $false,
    [switch]$GenerateReport = $false,
    [string]$OutputFile = "maintenance-report.txt"
)

# Configuration
$PROJECT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Path
$CLAUDE_MD = Join-Path $PROJECT_ROOT "CLAUDE.md"
$SERVICES_DIR = Join-Path $PROJECT_ROOT "FileManager" "Services"
$PACKAGES_CONFIG = Join-Path $PROJECT_ROOT "FileManager" "packages.config"
$SOLUTION_FILE = Join-Path $PROJECT_ROOT "FileManager.sln"

# Initialize report
$report = @()
$issues = @()
$warnings = @()

function Write-ReportLine {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$timestamp] [$Level] $Message"
    $report += $line

    switch ($Level) {
        "ERROR" { Write-Host $line -ForegroundColor Red }
        "WARNING" { Write-Host $line -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $line -ForegroundColor Green }
        default { Write-Host $line -ForegroundColor White }
    }
}

function Test-FileExists {
    param([string]$Path)
    if (Test-Path $Path) {
        Write-ReportLine "Found: $Path" "SUCCESS"
        return $true
    } else {
        Write-ReportLine "Missing: $Path" "ERROR"
        $issues += "File not found: $Path"
        return $false
    }
}

function Get-ServiceCount {
    param([string]$ServicesPath)

    if (-not (Test-Path $ServicesPath)) {
        return 0
    }

    $interfaceCount = (Get-ChildItem -Path $ServicesPath -Filter "I*Service.cs" -ErrorAction SilentlyContinue).Count
    return $interfaceCount
}

function Get-DocumentedServiceCount {
    param([string]$ClaudemdPath)

    if (-not (Test-Path $ClaudemdPath)) {
        return 0
    }

    $content = Get-Content $ClaudemdPath -Raw
    $servicePattern = '#### \d+\.\s+I\w+Service\s+/'
    $matches = [System.Text.RegularExpressions.Regex]::Matches($content, $servicePattern)
    return $matches.Count
}

function Get-PackageVersions {
    param([string]$PackagesPath)

    if (-not (Test-Path $PackagesPath)) {
        return @{}
    }

    [xml]$xml = Get-Content $PackagesPath
    $packages = @{}
    foreach ($package in $xml.packages.package) {
        $packages[$package.id] = $package.version
    }
    return $packages
}

function Validate-ClaudemdFormat {
    param([string]$ClaudemdPath)

    if (-not (Test-Path $ClaudemdPath)) {
        return $false
    }

    $content = Get-Content $ClaudemdPath -Raw
    $errors = @()

    # Check for required sections
    $requiredSections = @(
        "^# FileManager",
        "^## Project Overview",
        "^## Architecture",
        "^## Technology Stack",
        "^## Project Structure",
        "^## Service Layer Documentation",
        "^## Key Features",
        "^## Build & Run"
    )

    foreach ($section in $requiredSections) {
        if (-not ($content -match $section)) {
            $errors += "Missing section: $section"
        }
    }

    # Check for proper markdown syntax
    $lines = $content -split "`n"
    $headingCount = ($lines | Where-Object { $_ -match "^#+\s" }).Count

    if ($headingCount -lt 5) {
        $errors += "Insufficient headings detected (found $headingCount, expected at least 5)"
    }

    return @{
        Valid = $errors.Count -eq 0
        Errors = $errors
    }
}

function Check-ServiceDocumentation {
    param([string]$ServicesPath, [string]$ClaudemdPath)

    if (-not (Test-Path $ServicesPath) -or -not (Test-Path $ClaudemdPath)) {
        return $null
    }

    # Get actual services
    $actualServices = @()
    $services = Get-ChildItem -Path $ServicesPath -Filter "I*Service.cs" -ErrorAction SilentlyContinue
    foreach ($service in $services) {
        $actualServices += $service.BaseName -replace "^I", ""
    }

    # Get documented services
    $content = Get-Content $ClaudemdPath -Raw
    $documentedServices = @()
    $servicePattern = 'I(\w+Service)\s+/'
    $matches = [System.Text.RegularExpressions.Regex]::Matches($content, $servicePattern)
    foreach ($match in $matches) {
        $documentedServices += $match.Groups[1].Value
    }

    $missing = @()
    $undocumented = @()

    foreach ($service in $actualServices) {
        if ($service -notin $documentedServices) {
            $undocumented += $service
        }
    }

    foreach ($service in $documentedServices) {
        if ($service -notin $actualServices) {
            $missing += $service
        }
    }

    return @{
        Actual = $actualServices
        Documented = $documentedServices
        Undocumented = $undocumented
        Missing = $missing
    }
}

function Generate-Report {
    param([string]$OutputPath, [array]$ReportLines)

    $header = @(
        "==============================================="
        "FileManager CLAUDE.md Maintenance Report"
        "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        "==============================================="
        ""
    )

    $fullReport = $header + $ReportLines

    if ($OutputPath) {
        $fullReport | Out-File -FilePath $OutputPath -Encoding UTF8
        Write-ReportLine "Report saved to: $OutputPath" "SUCCESS"
    }
}

# Main Execution
Write-ReportLine "Starting CLAUDE.md Maintenance Validator" "INFO"
Write-ReportLine "Project Root: $PROJECT_ROOT" "INFO"
Write-ReportLine "" "INFO"

# Check critical files
Write-ReportLine "=== Critical Files Check ===" "INFO"
Test-FileExists $CLAUDE_MD
Test-FileExists $SERVICES_DIR
Test-FileExists $PACKAGES_CONFIG
Write-ReportLine "" "INFO"

# Service Count Validation
Write-ReportLine "=== Service Count Validation ===" "INFO"
$actualServices = Get-ServiceCount $SERVICES_DIR
$documentedServices = Get-DocumentedServiceCount $CLAUDE_MD
Write-ReportLine "Actual services in Services/: $actualServices" "INFO"
Write-ReportLine "Documented services in CLAUDE.md: $documentedServices" "INFO"

if ($actualServices -eq $documentedServices) {
    Write-ReportLine "Service count matches (baseline: 14)" "SUCCESS"
} else {
    Write-ReportLine "Service count mismatch! Expected 14, found Actual=$actualServices, Documented=$documentedServices" "WARNING"
    $warnings += "Service count mismatch - may indicate undocumented services"
}
Write-ReportLine "" "INFO"

# Format Validation
Write-ReportLine "=== Format Validation ===" "INFO"
$formatCheck = Validate-ClaudemdFormat $CLAUDE_MD
if ($formatCheck.Valid) {
    Write-ReportLine "CLAUDE.md format is valid" "SUCCESS"
} else {
    Write-ReportLine "CLAUDE.md format issues detected:" "WARNING"
    foreach ($error in $formatCheck.Errors) {
        Write-ReportLine "  - $error" "WARNING"
        $issues += $error
    }
}
Write-ReportLine "" "INFO"

# Service Documentation Check
Write-ReportLine "=== Service Documentation Check ===" "INFO"
$serviceCheck = Check-ServiceDocumentation $SERVICES_DIR $CLAUDE_MD
if ($serviceCheck) {
    if ($serviceCheck.Undocumented.Count -gt 0) {
        Write-ReportLine "Undocumented services found:" "WARNING"
        foreach ($service in $serviceCheck.Undocumented) {
            Write-ReportLine "  - $service" "WARNING"
            $issues += "Undocumented service: $service"
        }
    } else {
        Write-ReportLine "All services are documented" "SUCCESS"
    }

    if ($serviceCheck.Missing.Count -gt 0) {
        Write-ReportLine "References to missing services:" "WARNING"
        foreach ($service in $serviceCheck.Missing) {
            Write-ReportLine "  - $service (documented but not in Services/)" "WARNING"
            $warnings += "Reference to missing service: $service"
        }
    }
}
Write-ReportLine "" "INFO"

# Package Version Check (if FullValidation)
if ($FullValidation) {
    Write-ReportLine "=== Technology Stack Validation ===" "INFO"
    $packages = Get-PackageVersions $PACKAGES_CONFIG
    $report += "Installed packages:"
    foreach ($pkg in $packages.GetEnumerator() | Sort-Object Name) {
        $report += "  $($pkg.Key): $($pkg.Value)"
        Write-ReportLine "  $($pkg.Key): $($pkg.Value)" "INFO"
    }
    Write-ReportLine "" "INFO"
}

# Summary
Write-ReportLine "=== Summary ===" "INFO"
Write-ReportLine "Total Issues: $($issues.Count)" "INFO"
Write-ReportLine "Total Warnings: $($warnings.Count)" "INFO"

if ($issues.Count -gt 0) {
    Write-ReportLine "Issues found:" "ERROR"
    foreach ($issue in $issues) {
        Write-ReportLine "  - $issue" "ERROR"
    }
}

if ($warnings.Count -gt 0) {
    Write-ReportLine "Warnings:" "WARNING"
    foreach ($warning in $warnings) {
        Write-ReportLine "  - $warning" "WARNING"
    }
}

if ($issues.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-ReportLine "No issues detected - CLAUDE.md is in sync!" "SUCCESS"
}

# Generate Report File
if ($GenerateReport) {
    Generate-Report $OutputFile $report
}

Write-ReportLine "" "INFO"
Write-ReportLine "Validation Complete" "INFO"

# Exit with appropriate code
exit $issues.Count
