# CLAUDE.md Maintenance Hooks & Integration

This document provides integration methods for automatic CLAUDE.md maintenance across your development workflow.

## Quick Start

Choose one or more integration methods below:

1. **Manual PowerShell Script** (Easiest) - Run when you want
2. **Git Pre-commit Hook** (Recommended) - Runs before each commit
3. **Visual Studio Task Runner** (IDE Integration) - Run from VS
4. **Build Event Hook** (CI Integration) - Runs on build

---

## Method 1: Manual PowerShell Script

### Usage

```powershell
# Quick validation check
.\Maintenance-ClaudeValidator.ps1

# Full validation with report
.\Maintenance-ClaudeValidator.ps1 -FullValidation -GenerateReport

# Save report to file
.\Maintenance-ClaudeValidator.ps1 -FullValidation -GenerateReport -OutputFile "claude-check-$(Get-Date -Format 'yyyyMMdd').txt"
```

### When to Run

- After adding a new service
- After updating dependencies
- After major code refactoring
- Before committing changes
- Weekly during active development

### What It Checks

- CLAUDE.md file exists and is valid
- Service count matches (actual vs documented)
- All services are documented
- Markdown format is correct
- (Optional) Technology stack versions

### Expected Output

```
[2026-01-13 10:30:45] [SUCCESS] Found: C:\projects\ofer\filemanager\CLAUDE.md
[2026-01-13 10:30:45] [SUCCESS] Actual services in Services/: 14
[2026-01-13 10:30:45] [SUCCESS] Service count matches (baseline: 14)
[2026-01-13 10:30:45] [SUCCESS] CLAUDE.md format is valid
[2026-01-13 10:30:45] [SUCCESS] All services are documented
[2026-01-13 10:30:45] [SUCCESS] No issues detected - CLAUDE.md is in sync!
```

---

## Method 2: Git Pre-commit Hook

Automatically check CLAUDE.md before each commit.

### Setup Instructions

1. Create `.git/hooks/pre-commit` (Windows: `.git/hooks/pre-commit.ps1`)

```powershell
#!/usr/bin/env powershell
# .git/hooks/pre-commit.ps1
# Validates CLAUDE.md before commit

$PROJECT_ROOT = (git rev-parse --show-toplevel)
$validator = Join-Path $PROJECT_ROOT "Maintenance-ClaudeValidator.ps1"

Write-Host "Running CLAUDE.md validation..." -ForegroundColor Cyan

# Run validator
& $validator

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "CLAUDE.md validation issues detected!" -ForegroundColor Red
    Write-Host "Run: .\Maintenance-ClaudeValidator.ps1 -FullValidation" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To bypass this check: git commit --no-verify" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "CLAUDE.md validation passed!" -ForegroundColor Green
exit 0
```

2. Make it executable:

```powershell
# On Windows, ensure PowerShell can execute it
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

3. Configure Git to use PowerShell for hooks:

```bash
git config core.hooksPath .git/hooks
```

### Usage

Just commit normally - the hook runs automatically:

```bash
git add .
git commit -m "Add new service"
```

If issues are found, fix them and try again:

```bash
git commit --no-verify  # Skip validation (not recommended)
```

### When It Runs

- **Every commit**: Quick validation check
- **Commit with Services/ or packages.config changes**: Extra scrutiny
- **Skipped**: Only when using `--no-verify` flag

---

## Method 3: Visual Studio Task Runner

Integrate maintenance checks into Visual Studio.

### Setup Instructions (VS 2019+)

1. Create `Maintenance-Tasks.json` in project root:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Validate CLAUDE.md",
      "command": "powershell",
      "type": "process",
      "args": [
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        "${workspaceFolder}\\Maintenance-ClaudeValidator.ps1"
      ],
      "problemMatcher": [],
      "group": {
        "kind": "build",
        "isDefault": false
      }
    },
    {
      "label": "Full CLAUDE.md Validation with Report",
      "command": "powershell",
      "type": "process",
      "args": [
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        "${workspaceFolder}\\Maintenance-ClaudeValidator.ps1",
        "-FullValidation",
        "-GenerateReport"
      ],
      "problemMatcher": [],
      "group": {
        "kind": "build",
        "isDefault": false
      }
    }
  ]
}
```

2. In Visual Studio:
   - Tools → Tasks → Task Runner Explorer
   - Refresh
   - Double-click "Validate CLAUDE.md" to run

### Keyboard Shortcuts

Add to your VS key bindings for quick access:

```json
[
    {
        "key": "ctrl+alt+d",
        "command": "workbench.action.tasks.runTask",
        "args": "Validate CLAUDE.md"
    }
]
```

---

## Method 4: Build Event Hook

Integrate into your Visual Studio build process.

### Setup Instructions

1. Open FileManager.csproj in a text editor
2. Add this before the closing `</Project>` tag:

```xml
<!-- CLAUDE.md Maintenance Hook -->
<Target Name="ValidateClaude" AfterTargets="Build">
  <Message Text="Validating CLAUDE.md..." Importance="high" />
  <Exec
    Command="powershell -NoProfile -ExecutionPolicy Bypass -File &quot;$(ProjectDir)..\Maintenance-ClaudeValidator.ps1&quot;"
    ContinueOnError="true"
  />
</Target>
```

3. Save the file and reload the project

### Usage

CLAUDE.md validation now runs after every build:

```
Build started 1/13/2026 10:30:45 AM
...
Validating CLAUDE.md...
[2026-01-13 10:30:50] [SUCCESS] All validation checks passed
Build succeeded.
```

### Customization

To make it fail the build on issues:

```xml
<Exec
  Command="powershell -NoProfile -ExecutionPolicy Bypass -File &quot;$(ProjectDir)..\Maintenance-ClaudeValidator.ps1&quot;"
  ContinueOnError="false"
/>
```

---

## Method 5: GitHub Actions Workflow (Optional)

For teams using GitHub, automatically validate on push:

### Setup Instructions

1. Create `.github/workflows/claude-maintenance.yml`:

```yaml
name: CLAUDE.md Maintenance

on:
  push:
    paths:
      - 'FileManager/Services/**'
      - 'FileManager/packages.config'
      - 'CLAUDE.md'
  pull_request:
    paths:
      - 'FileManager/Services/**'
      - 'FileManager/packages.config'
      - 'CLAUDE.md'

jobs:
  validate-claude:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3

      - name: Validate CLAUDE.md
        shell: powershell
        run: |
          ./Maintenance-ClaudeValidator.ps1 -FullValidation -GenerateReport

      - name: Upload Report
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: maintenance-report
          path: maintenance-report.txt
```

2. Push to GitHub - validation runs automatically on PR

---

## Integration Workflow

### Recommended Development Process

#### Adding a New Service

1. **Create the service**
   ```
   Services/IMyService.cs
   Services/MyService.cs
   ```

2. **Integrate into Form1.cs**
   ```csharp
   private IMyService _myService;
   // ... in constructor
   _myService = new MyService();
   ```

3. **Run validation**
   ```powershell
   .\Maintenance-ClaudeValidator.ps1
   ```

4. **If validation fails**:
   - Update CLAUDE.md Service Layer Documentation
   - Add the new service entry
   - Run validation again

5. **Commit changes**
   ```bash
   git add FileManager/Services/IMyService.cs FileManager/Services/MyService.cs CLAUDE.md
   git commit -m "Add MyService to handle [domain]"
   ```

#### Updating Dependencies

1. **Update packages.config** via NuGet
2. **Build and test**
3. **Run validation**
   ```powershell
   .\Maintenance-ClaudeValidator.ps1 -FullValidation
   ```
4. **Update Technology Stack in CLAUDE.md** if needed
5. **Commit both files**

#### Major Refactoring

1. **Complete refactoring**
2. **Build and test thoroughly**
3. **Run full validation**
   ```powershell
   .\Maintenance-ClaudeValidator.ps1 -FullValidation -GenerateReport
   ```
4. **Update affected CLAUDE.md sections**:
   - Architecture (if design changed)
   - Project Structure (if organized differently)
   - Code Documentation (if improved)
   - Version History (note the refactoring)
5. **Use slash command** to verify sync:
   ```bash
   /enhance-claude-md
   ```
6. **Commit all changes**

---

## Troubleshooting Integration Issues

### PowerShell Execution Policy Issue

**Error**: `File cannot be loaded because running scripts is disabled`

**Solution**:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Hook Not Running

**Cause**: Git hooks path not configured

**Solution**:
```bash
git config core.hooksPath .git/hooks
```

### False Positives in Validation

**Cause**: Service count doesn't match due to interface-only files

**Solution**: Verify both interface and implementation files exist:
```powershell
dir FileManager\Services\I*Service.cs | wc -l  # Should be 14
dir FileManager\Services\*Service.cs | wc -l   # Should be 28
```

### Validation Passes but CLAUDE.md Still Out of Sync

**Cause**: Validator checks structure, not content accuracy

**Solution**: Use `/enhance-claude-md` for detailed content validation

---

## Monitoring Dashboard

Create a simple dashboard to track CLAUDE.md health:

### Create `CLAUDE_Status.txt` After Validation

```powershell
# Add this to Maintenance-ClaudeValidator.ps1

$status = @"
CLAUDE.md Status Report
Generated: $(Get-Date)

Service Documentation: PASS
  - Actual services: $actualServices
  - Documented services: $documentedServices
  - Status: $(if ($actualServices -eq $documentedServices) { 'IN SYNC' } else { 'OUT OF SYNC' })

Format Validation: $(if ($formatCheck.Valid) { 'PASS' } else { 'FAIL' })

Last Updated: $(Get-Item $CLAUDE_MD | Select-Object -ExpandProperty LastWriteTime)

Next Recommended Check: $(
    $daysSinceUpdate = ((Get-Date) - (Get-Item $CLAUDE_MD).LastWriteTime).Days
    if ($daysSinceUpdate -lt 7) { 'Low priority' }
    elseif ($daysSinceUpdate -lt 14) { 'Medium priority' }
    else { 'HIGH PRIORITY - Update overdue' }
)
"@

$status | Out-File -FilePath "CLAUDE_Status.txt"
```

---

## Advanced Automation

### Automatic CLAUDE.md Update Script (Optional)

For teams wanting full automation, create an auto-update script:

```powershell
# Auto-update.ps1
# WARNING: Use with caution - always review changes

param([string]$ServiceName)

$CLAUDE_MD = "CLAUDE.md"
$content = Get-Content $CLAUDE_MD -Raw

if ($ServiceName) {
    # Extract service documentation from source
    $servicePath = "FileManager\Services\I$ServiceName`Service.cs"

    if (Test-Path $servicePath) {
        # Parse service file for documentation
        # Add automatic entry to CLAUDE.md
        # Validate and save
    }
}

# After any auto-update, ALWAYS:
# 1. Review changes manually
# 2. Run validation
# 3. Test build
# 4. Commit with clear message
```

---

## Maintenance Check List

### Daily Development
- [ ] Run PowerShell validation before committing
- [ ] Check validator output for warnings

### Weekly (If Active Development)
- [ ] Run full validation with `-FullValidation` flag
- [ ] Generate and review report
- [ ] Check Technology Stack for outdated versions

### Monthly
- [ ] Full CLAUDE.md quality audit
- [ ] Review Refactoring History entry
- [ ] Verify Architecture matches actual code
- [ ] Check service documentation completeness

### Before Release
- [ ] Run full validation
- [ ] Generate final report
- [ ] Update Version History
- [ ] Get code review including CLAUDE.md review
- [ ] Verify no broken references
- [ ] Commit final version

---

## Success Metrics

A well-maintained CLAUDE.md achieves:

- ✓ 100% service documentation (all 14 services documented)
- ✓ 0 validator issues
- ✓ Updated within 2 weeks of major changes
- ✓ Accurate Architecture section
- ✓ Current Technology Stack versions
- ✓ Matching file structure and directory tree
- ✓ No broken Markdown syntax

---

## Getting Help

For issues or questions about CLAUDE.md maintenance:

1. **Check CLAUDE_MAINTENANCE.md** - Comprehensive guide
2. **Run validation** - Identifies specific issues
3. **Use `/enhance-claude-md`** - AI-assisted synchronization
4. **Review recent commits** - See what changed

---

**Document Version**: 1.0.0
**Last Updated**: 2026-01-13
**Status**: Active
**Recommended Integration**: Pre-commit Hook (Method 2) + Manual Validation (Method 1)
