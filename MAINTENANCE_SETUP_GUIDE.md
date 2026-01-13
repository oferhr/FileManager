# CLAUDE.md Maintenance System - Complete Setup Guide

Complete instructions for setting up and activating automatic CLAUDE.md maintenance for your FileManager project.

**Status**: Ready to Deploy
**Version**: 1.0.0
**Date**: 2026-01-13

---

## Table of Contents

1. [System Overview](#system-overview)
2. [What Was Set Up](#what-was-set-up)
3. [Quick Start (5 minutes)](#quick-start-5-minutes)
4. [Full Setup (20 minutes)](#full-setup-20-minutes)
5. [Verification Checklist](#verification-checklist)
6. [Daily Usage](#daily-usage)
7. [Troubleshooting](#troubleshooting)

---

## System Overview

Your FileManager project now has a complete automated maintenance system for CLAUDE.md that ensures your documentation stays synchronized with the 14-service architecture.

### What Gets Maintained

The system monitors and maintains:
- **Service Layer Documentation** - Keeps 14 services documented
- **Technology Stack** - Tracks NuGet package versions
- **Project Structure** - Validates file organization
- **Architecture Section** - Ensures design patterns are current
- **Code Documentation** - Tracks documentation quality

### How It Works

1. **Detection**: Monitors project files for changes
2. **Validation**: Checks if CLAUDE.md is in sync
3. **Notification**: Alerts you if updates needed
4. **Synchronization**: Can auto-update via `/enhance-claude-md`

---

## What Was Set Up

Three maintenance documents have been created in your project:

### 1. `CLAUDE_MAINTENANCE.md` (Comprehensive Guide)
- Detailed explanation of what gets monitored
- Update triggers and conditions
- Quality assurance procedures
- Common update scenarios
- Troubleshooting guide

**Location**: `C:\projects\ofer\filemanager\CLAUDE_MAINTENANCE.md`

**Read this when**:
- You need to understand why CLAUDE.md should be updated
- You want to manually update a specific section
- You're troubleshooting out-of-sync issues

### 2. `CLAUDE_MAINTENANCE_HOOKS.md` (Integration Guide)
- 5 integration methods (manual to fully automated)
- Step-by-step setup for each method
- Git hooks, Visual Studio, and CI/CD integration
- Workflow recommendations

**Location**: `C:\projects\ofer\filemanager\CLAUDE_MAINTENANCE_HOOKS.md`

**Read this when**:
- You want to set up automated checks
- You need to integrate with your build process
- You're configuring pre-commit hooks

### 3. `Maintenance-ClaudeValidator.ps1` (Validation Script)
- PowerShell script that validates CLAUDE.md
- Checks service count, format, documentation
- Generates detailed reports
- Zero configuration needed

**Location**: `C:\projects\ofer\filemanager\Maintenance-ClaudeValidator.ps1`

**Use this when**:
- You want to check if CLAUDE.md is in sync
- You need a detailed validation report
- You're about to commit changes

---

## Quick Start (5 minutes)

### Step 1: Verify the Setup (30 seconds)

All files are now in your FileManager directory:

```powershell
cd C:\projects\ofer\filemanager
ls -Name | grep -i "maintenance\|validator"
```

Expected output:
```
CLAUDE.md
CLAUDE_MAINTENANCE.md
CLAUDE_MAINTENANCE_HOOKS.md
Maintenance-ClaudeValidator.ps1
MAINTENANCE_SETUP_GUIDE.md
```

### Step 2: Run Your First Validation (1 minute)

```powershell
cd C:\projects\ofer\filemanager
.\Maintenance-ClaudeValidator.ps1
```

You should see output like:
```
[2026-01-13 10:30:45] [SUCCESS] Found: CLAUDE.md
[2026-01-13 10:30:45] [SUCCESS] Actual services in Services/: 14
[2026-01-13 10:30:45] [SUCCESS] Service count matches (baseline: 14)
[2026-01-13 10:30:45] [SUCCESS] CLAUDE.md format is valid
[2026-01-13 10:30:45] [SUCCESS] All services are documented
```

### Step 3: Choose Your Integration Method (3 minutes)

Pick **ONE** of these based on your preference:

**Option A: Manual (No Setup)**
- Just run the PowerShell script when you want to check
- Best for: Solo developers, occasional checks
- Time to first use: Now

**Option B: Pre-Commit Hook (Recommended)**
- Runs validation before each git commit
- Best for: All development teams, ensures consistency
- Time to setup: 5 minutes (see "Full Setup" below)

**Option C: Visual Studio Integration**
- Run from VS Task Runner with keyboard shortcut
- Best for: VS-centric workflow
- Time to setup: 3 minutes (see "Full Setup" below)

**Option D: Build Event**
- Runs automatically after each build
- Best for: CI/CD pipelines
- Time to setup: 2 minutes (see "Full Setup" below)

---

## Full Setup (20 minutes)

Choose your integration method and follow the steps:

### Method A: Pre-Commit Hook Setup (Recommended)

**Time**: 5 minutes | **Expertise**: Beginner

This runs validation automatically before every commit.

#### Step 1: Create the hook script

Create a file `.git/hooks/pre-commit.ps1`:

```powershell
#!/usr/bin/env powershell
# Validates CLAUDE.md before each commit

$PROJECT_ROOT = (git rev-parse --show-toplevel)
$validator = Join-Path $PROJECT_ROOT "Maintenance-ClaudeValidator.ps1"

Write-Host "Validating CLAUDE.md before commit..." -ForegroundColor Cyan

& powershell -NoProfile -ExecutionPolicy Bypass -File $validator

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "CLAUDE.md validation found issues!" -ForegroundColor Red
    Write-Host "Fix the issues or run: git commit --no-verify" -ForegroundColor Yellow
    exit 1
}

exit 0
```

#### Step 2: Configure Git to use it

```bash
git config core.hooksPath .git/hooks
```

#### Step 3: Test it

Make a small change and try to commit:

```bash
git add .
git commit -m "test: verify pre-commit hook"
```

You should see validation output before the commit completes.

### Method B: Visual Studio Integration

**Time**: 3 minutes | **Expertise**: Beginner

Add a task you can run from VS with a hotkey.

#### Step 1: Enable Task Runner

In Visual Studio:
- Tools → Options → Preview Features → Task Runner Enabled (enable if available)

#### Step 2: Create task file

Create `Maintenance-Tasks.json` in your project root:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Check CLAUDE.md",
      "type": "process",
      "command": "powershell",
      "args": [
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        "${workspaceFolder}\\Maintenance-ClaudeValidator.ps1"
      ],
      "group": "build"
    }
  ]
}
```

#### Step 3: Access from VS

- View → Task Runner Explorer
- Refresh
- Double-click "Check CLAUDE.md" to run

### Method C: Build Event Integration

**Time**: 2 minutes | **Expertise**: Intermediate

Validation runs after every build in Visual Studio.

#### Step 1: Unload project

Right-click on FileManager project → Unload Project

#### Step 2: Edit project file

Right-click on unloaded project → Edit Project File

#### Step 3: Add validation hook

Find the closing `</Project>` tag and add before it:

```xml
<Target Name="ValidateClaude" AfterTargets="Build">
  <Exec Command="powershell -NoProfile -ExecutionPolicy Bypass -File &quot;$(ProjectDir)..\Maintenance-ClaudeValidator.ps1&quot;" ContinueOnError="true" />
</Target>
```

#### Step 4: Reload project

Right-click → Reload Project

Now validation runs after every build!

### Method D: GitHub Actions (Teams Only)

**Time**: 5 minutes | **Expertise**: Intermediate

Automatically validates on GitHub push/PR.

#### Step 1: Create workflow directory

```bash
mkdir -p .github/workflows
```

#### Step 2: Create workflow file

Create `.github/workflows/claude-check.yml`:

```yaml
name: CLAUDE.md Check

on:
  push:
    paths:
      - 'FileManager/Services/**'
      - 'CLAUDE.md'
  pull_request:
    paths:
      - 'FileManager/Services/**'
      - 'CLAUDE.md'

jobs:
  validate:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Validate CLAUDE.md
        shell: powershell
        run: ./Maintenance-ClaudeValidator.ps1 -FullValidation
```

#### Step 3: Commit and push

```bash
git add .github/workflows/claude-check.yml
git commit -m "ci: Add CLAUDE.md validation workflow"
git push
```

Validation now runs on every push!

---

## Verification Checklist

After setup, verify everything works:

- [ ] All 4 maintenance files exist in project root:
  - [ ] CLAUDE.md
  - [ ] CLAUDE_MAINTENANCE.md
  - [ ] CLAUDE_MAINTENANCE_HOOKS.md
  - [ ] Maintenance-ClaudeValidator.ps1

- [ ] PowerShell script works:
  - [ ] Run: `.\Maintenance-ClaudeValidator.ps1`
  - [ ] Shows "CLAUDE.md is in sync" or lists issues

- [ ] If you chose pre-commit hook:
  - [ ] Hook file exists: `.git/hooks/pre-commit.ps1`
  - [ ] Git knows about hooks: `git config core.hooksPath`
  - [ ] Make a test commit - validation runs

- [ ] If you chose VS integration:
  - [ ] Task file exists: `Maintenance-Tasks.json`
  - [ ] Task appears in Task Runner Explorer
  - [ ] Can double-click to run validation

- [ ] If you chose build integration:
  - [ ] `.csproj` file has `<Target>` section
  - [ ] Build runs without errors
  - [ ] Validation output appears in build log

- [ ] CLAUDE.md is current:
  - [ ] All 14 services are documented
  - [ ] Architecture section is accurate
  - [ ] Technology Stack matches packages.config

---

## Daily Usage

### Before Committing Code

```powershell
# Quick check (30 seconds)
.\Maintenance-ClaudeValidator.ps1

# If issues found, fix CLAUDE.md and try again
```

### When Adding a New Service

1. Create IMyService.cs and MyService.cs
2. Integrate into Form1.cs
3. Run validation
4. Update CLAUDE.md Service Layer section
5. Commit both service code and CLAUDE.md updates

### When Updating Dependencies

1. Update packages via NuGet
2. Test the build
3. Run validation
4. Update Technology Stack in CLAUDE.md if needed
5. Commit changes

### Weekly Check (If Active Development)

```powershell
# Full validation with report
.\Maintenance-ClaudeValidator.ps1 -FullValidation -GenerateReport

# Review maintenance-report.txt for any issues
```

### Monthly Audit

1. Run full validation
2. Review CLAUDE_MAINTENANCE.md quality checklist
3. Verify architecture still matches code
4. Check for any documentation gaps
5. Use `/enhance-claude-md` if needed

---

## Troubleshooting

### Issue: PowerShell script won't run

**Error**: `File cannot be loaded because running scripts is disabled`

**Fix**:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue: Pre-commit hook not running

**Error**: Git commits without running validation

**Fix**:
```bash
git config core.hooksPath .git/hooks
# Then delete your last commit and try again
```

### Issue: Validator says service count is wrong

**Error**: "Service count mismatch! Expected 14..."

**Fix**:
1. Count actual services:
```powershell
(dir FileManager\Services\I*Service.cs).Count
```

2. Count documented services in CLAUDE.md
3. If counts differ, use `/enhance-claude-md` to sync

### Issue: CLAUDE.md has issues but validator passes

**Cause**: Validator checks structure, not detailed content

**Fix**:
Use `/enhance-claude-md` command for detailed AI-assisted review

### Issue: Can't find Maintenance-ClaudeValidator.ps1

**Fix**:
Make sure you're in the FileManager project root:
```powershell
cd C:\projects\ofer\filemanager
# Now run validator
.\Maintenance-ClaudeValidator.ps1
```

---

## Next Steps

### Immediate (Today)

1. ✓ Read this document (you're doing it!)
2. Run the PowerShell script once to verify setup
3. Choose your integration method (or pick Method A: Manual for now)

### This Week

1. Set up your chosen integration method
2. Get comfortable running validation
3. Update CLAUDE.md if validator finds issues
4. Test your integration (make a commit, etc.)

### Ongoing

1. Run validation before commits
2. Update CLAUDE.md when you add services or change dependencies
3. Weekly full validation if actively developing
4. Monthly comprehensive audit of CLAUDE.md quality

---

## Quick Reference

### Essential Commands

```powershell
# Quick validation
.\Maintenance-ClaudeValidator.ps1

# Full validation with report
.\Maintenance-ClaudeValidator.ps1 -FullValidation -GenerateReport

# View maintenance guide
Get-Content CLAUDE_MAINTENANCE.md -Head 50

# View integration options
Get-Content CLAUDE_MAINTENANCE_HOOKS.md -Head 50
```

### File Quick Reference

| File | Purpose | When to Read |
|------|---------|--------------|
| CLAUDE.md | Your project documentation | Always - main project doc |
| CLAUDE_MAINTENANCE.md | Maintenance procedures | Before updating CLAUDE.md |
| CLAUDE_MAINTENANCE_HOOKS.md | Setup integration methods | When setting up automation |
| Maintenance-ClaudeValidator.ps1 | Validation script | Run regularly, read only if issues |
| MAINTENANCE_SETUP_GUIDE.md | This file | For reference during setup |

### Key Baselines

- **Service Count**: 14 services expected
- **Form1.cs Lines**: ~888 (78% reduction from 4051)
- **Framework**: .NET Framework 4.8
- **Language**: C# 7.3

---

## Success Criteria

Your maintenance system is working well when:

✓ Validator runs without errors
✓ All 14 services documented in CLAUDE.md
✓ No "out of sync" warnings
✓ Technology Stack matches packages.config
✓ Project Structure diagram is accurate
✓ CLAUDE.md updated within 2 weeks of major changes

---

## Support & Questions

For help with specific issues:

1. **Understanding what to update**: Read `CLAUDE_MAINTENANCE.md`
2. **Setting up automation**: Read `CLAUDE_MAINTENANCE_HOOKS.md`
3. **Diagnosing issues**: Run `Maintenance-ClaudeValidator.ps1 -FullValidation`
4. **Complex changes**: Use `/enhance-claude-md` slash command

---

## Document Hierarchy

```
MAINTENANCE_SETUP_GUIDE.md (START HERE)
├── Quick Start (5 min)
├── Full Setup (20 min)
│   ├── Method A: Pre-Commit Hook
│   ├── Method B: VS Integration
│   ├── Method C: Build Event
│   └── Method D: GitHub Actions
└── Points to CLAUDE_MAINTENANCE_HOOKS.md for details

CLAUDE_MAINTENANCE.md (COMPREHENSIVE REFERENCE)
├── What Gets Monitored
├── Update Triggers
├── Sections to Review
├── Quality Assurance
└── Common Scenarios

CLAUDE_MAINTENANCE_HOOKS.md (INTEGRATION DETAILS)
├── Method 1: Manual Script
├── Method 2: Pre-Commit Hook
├── Method 3: VS Task Runner
├── Method 4: Build Event
└── Method 5: GitHub Actions

Maintenance-ClaudeValidator.ps1 (AUTOMATION)
└── Run directly or via hooks/tasks
```

---

## Maintenance System Status

**Current Status**: ✓ Ready for Use

**Components Active**:
- [x] CLAUDE.md (Service-oriented architecture, 14 services documented)
- [x] Maintenance documentation created
- [x] Validation script implemented
- [x] Integration guides provided

**Next Check**: Run `Maintenance-ClaudeValidator.ps1` now to verify

**Recommended First Integration**: Pre-Commit Hook (Method A in Full Setup)

---

**Setup Date**: 2026-01-13
**Documentation Version**: 1.0.0
**Status**: Complete and Ready
**Estimated Time to First Validation**: 2 minutes
**Estimated Time to Full Integration**: 20 minutes

Let me know if you need clarification on any part of the setup!
