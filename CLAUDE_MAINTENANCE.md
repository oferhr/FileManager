# CLAUDE.md Maintenance Guide

Automated maintenance system for keeping CLAUDE.md synchronized with FileManager project changes.

## Overview

This guide establishes an automatic maintenance process to ensure your CLAUDE.md file stays current as the FileManager project evolves. The system monitors key project areas and triggers targeted updates when significant changes occur.

## Version Information

- **Maintenance Version**: 1.0.0
- **Last Configured**: 2026-01-13
- **Service Count Baseline**: 14 services
- **Form1.cs Baseline**: 888 lines

## What Gets Monitored

The maintenance system watches for changes in these critical areas:

### 1. Service Layer (HIGHEST PRIORITY)
- **Path**: `FileManager/Services/`
- **Triggers**:
  - New service files added (either interface or implementation)
  - Existing service modifications
  - Service deletion
- **Action**: Update "Service Layer Documentation" section with new service details
- **Check**: Is the Services directory listing current? Do all 14 services have documentation?

### 2. Core Files (HIGH PRIORITY)
- **Paths**: `FileManager/Form1.cs`, `FileManager/FileCount.cs`, `FileManager/*.cs`
- **Triggers**:
  - Major code modifications (>100 lines changed)
  - New public classes or significant API changes
  - Refactoring events
- **Action**: Review and update affected sections in CLAUDE.md
- **Check**: Does documentation match current implementation?

### 3. Dependencies (MEDIUM PRIORITY)
- **Path**: `FileManager/packages.config`
- **Triggers**:
  - Package version updates
  - New NuGet packages added
  - Package removal
- **Action**: Update "Technology Stack" section with new dependency versions
- **Check**: Are all NuGet packages listed? Are versions current?

### 4. Solution Structure (MEDIUM PRIORITY)
- **Path**: `FileManager.sln`
- **Triggers**:
  - Project reference changes
  - Solution file modifications
  - New project directories
- **Action**: Update "Project Structure" diagram and file listing
- **Check**: Does the directory tree match actual structure?

### 5. Configuration (LOW PRIORITY)
- **Paths**: `App.config`, `NLog.config`, `FodyWeavers.xml`
- **Triggers**:
  - Configuration file changes
  - New settings added
  - Logging configuration updates
- **Action**: Update "Configuration" section if significant
- **Check**: Are configuration options documented?

## Update Triggers & Actions

### Automatic Update Conditions

Update CLAUDE.md when:

1. **New Service Added**
   - Detect: New `.cs` file in `Services/` matching pattern `I[ServiceName]Service.cs` or `[ServiceName]Service.cs`
   - Action: Add new service documentation to "Service Layer Documentation" section
   - Template:
     ```
     #### [N]. I[ServiceName] / [ServiceName]
     **Purpose**: [One-line description]
     **Responsibilities**:
     - [Responsibility 1]
     - [Responsibility 2]
     ```
   - Update service count at beginning of section if changed

2. **Service Modification**
   - Detect: Significant changes to existing service files (interface or implementation)
   - Action: Review and update service documentation
   - Check: Do method signatures match documentation? Are responsibilities current?

3. **New Dependencies**
   - Detect: Changes to `packages.config`
   - Action: Update Technology Stack section with new dependencies and versions
   - Example:
     ```
     - **[Package Name]** ([Version number])
     ```

4. **Project Structure Changes**
   - Detect: New directories or major file additions/deletions
   - Action: Update Project Structure diagram and file tree
   - Ensure all major directories are represented

5. **Form1.cs Modifications**
   - Detect: Line count changes >5% or major method additions/removals
   - Action: Review and update line count references in CLAUDE.md
   - Update any documentation referencing Form1 structure

6. **Code Documentation Improvements**
   - Detect: XML doc comments added or enhanced
   - Action: Note in "Code Documentation" section
   - Update documentation coverage statistics if applicable

## Monitored Sections in CLAUDE.md

These sections should be reviewed for accuracy after detected changes:

### Always Check After Service Changes
- Section: "Service Layer Documentation" (Lines 116-226)
- Verify: Count of services, service descriptions, responsibilities
- Update: Service number if new service added

### Always Check After Dependency Changes
- Section: "Technology Stack" (Lines 58-68)
- Verify: Version numbers, dependency list completeness
- Update: Add/remove/update package references

### Always Check After Structure Changes
- Section: "Project Structure" (Lines 70-114)
- Verify: Directory tree accuracy
- Update: New directories, moved files, renamed files

### Always Check After Code Changes
- Section: "Code Documentation" (Lines 275-316)
- Verify: Accuracy of class/method documentation references
- Update: New documentation features, improved coverage

### Reference When Refactoring
- Section: "Refactoring History" (Lines 328-377)
- Update: Add notes if refactoring activity occurred
- Check: Line count references still accurate

## How to Trigger Manual Update

### Using the Slash Command

```bash
/enhance-claude-md
```

This command will:
1. Analyze current project state
2. Compare with CLAUDE.md
3. Identify discrepancies
4. Propose updates
5. Apply targeted changes

### Manual Verification Checklist

After any significant project changes, verify:

- [ ] Service count matches (14 services baseline)
- [ ] All services in Services/ directory documented
- [ ] Technology Stack versions match packages.config
- [ ] Project Structure matches actual file layout
- [ ] File line counts referenced are current
- [ ] No broken section links
- [ ] All code examples are still valid
- [ ] Architecture diagram is accurate

### When to Manually Update

Update CLAUDE.md immediately when:

1. **New service added** - Update service count and add documentation
2. **Service removed** - Remove documentation and update count
3. **Major dependencies changed** - Update Technology Stack
4. **Project structure reorganized** - Update diagram and file listing
5. **Form1.cs refactored** - Update line count and structure references
6. **New language support added** - Update Language Support section
7. **New features introduced** - Update Key Features list
8. **Build/configuration changed** - Update Development Configuration

## Monitoring Schedule

### Recommended Check Frequency

- **Per Development Session**: Quick visual check that structure matches
- **After Each Major Feature**: Full review of affected sections
- **Weekly (if active development)**: Comprehensive review of all sections
- **Monthly**: Full quality audit and completeness check
- **After Release**: Update Version History and document changes

### Automated Check Points

The system will trigger checks at these points:

1. **Session Start**: Quick structural validation
2. **After 5+ files modified**: Check if any affect CLAUDE.md sections
3. **After dependency changes**: Auto-check Technology Stack
4. **After Services/ changes**: Auto-check Service Layer Documentation
5. **After major refactoring**: Full review suggested

## Quality Assurance

### Validation Rules

Before considering CLAUDE.md update complete:

1. **Service Documentation Completeness**
   - Minimum 14 service descriptions required
   - Each service has Purpose and Responsibilities
   - Service numbering is sequential

2. **Project Structure Accuracy**
   - All major directories present
   - File listing reflects actual structure
   - No obsolete files listed

3. **Technology Stack Accuracy**
   - All NuGet packages listed with versions
   - Versions match packages.config
   - No duplicate entries

4. **Code Documentation Validity**
   - Referenced classes exist in codebase
   - Referenced methods are accurate
   - Documentation coverage statistics realistic

5. **Syntax & Formatting**
   - Valid Markdown syntax throughout
   - Proper heading hierarchy (# ## ### ####)
   - Code blocks properly formatted with ```

### Quality Metrics

Track these metrics to ensure CLAUDE.md quality:

- **Service Coverage**: Should always be 14/14 (or updated number)
- **Documentation Freshness**: CLAUDE.md should be <2 weeks old
- **Structure Accuracy**: Should match actual project structure 100%
- **Broken Links**: Should be 0
- **Syntax Errors**: Should be 0

## Integration Points

### With Git Workflow

If using git, consider:

1. **Pre-commit Hook** (optional)
   ```bash
   # Check if services/ or packages.config changed
   if git diff --cached --name-only | grep -E "(Services/|packages.config)"; then
     echo "Reminder: Check if CLAUDE.md needs updating"
   fi
   ```

2. **Commit Message Convention**
   When updating CLAUDE.md, use:
   ```
   docs: Update CLAUDE.md - [reason]
   - Updated [section name]
   - Added/removed [service/dependency/feature]
   ```

3. **Branch Naming**
   For documentation updates:
   ```
   docs/claude-md-update-YYYY-MM-DD
   ```

### With CI/CD Pipeline

If implementing CI/CD:

1. **Validation Job**
   - Verify CLAUDE.md Markdown syntax
   - Check service count matches Services/ directory count
   - Validate project structure matches actual files

2. **Notification**
   - Alert if Services/ changed but CLAUDE.md unchanged
   - Alert if packages.config changed but CLAUDE.md unchanged

### With Development Workflow

Suggested integration:

1. **Feature Branch**: Developer works on feature
2. **Service Added/Modified**: Developer updates service docs in CLAUDE.md
3. **Pull Request**: Include CLAUDE.md update if applicable
4. **Review**: Reviewer checks CLAUDE.md accuracy
5. **Merge**: Feature + CLAUDE.md updates merged together

## Maintenance Tasks by Role

### For Developers

When working on code:

1. **If adding a new service**:
   - Create IServiceName.cs and ServiceName.cs
   - Update CLAUDE.md Service Layer Documentation
   - Add to line count in service count reference
   - Update Architecture section if adding new domain

2. **If modifying existing service**:
   - Review documentation for accuracy
   - Update if method signatures changed
   - Update responsibilities if scope changed

3. **If changing dependencies**:
   - Update packages.config
   - Update Technology Stack in CLAUDE.md
   - Note version updates in commit message

### For Team Leads / Architects

Review quarterly:

1. Check overall architecture documentation matches implementation
2. Verify refactoring history is up-to-date
3. Ensure development guide reflects current patterns
4. Review service layer for organization and clarity
5. Check for outdated version information

### For Project Managers

Track and report:

1. Service count (should remain 14 unless architecture changes)
2. Form1.cs line count trend (should stay ~888 with minimal variance)
3. Project structure stability (major changes indicate significant work)
4. Documentation freshness (should be updated within 2 weeks of major changes)

## Common Update Scenarios

### Scenario 1: Adding a New Service

When you create a new service:

1. Create `INewService.cs` and `NewService.cs` in Services/
2. Implement the interface in Form1.cs
3. Update CLAUDE.md:
   - Increment service count in "Service Layer Documentation"
   - Add service documentation entry with:
     - Interface and class names
     - Purpose statement
     - List of 3-5 key responsibilities
   - Update Project Structure if new service affects diagram
4. Verify no conflicts with numbering

### Scenario 2: Major Dependency Update

When upgrading a NuGet package:

1. Update packages.config with new version
2. Test that application still builds and runs
3. Update CLAUDE.md Technology Stack:
   - Update version number
   - Add any breaking change notes if applicable
4. Check if update requires configuration changes in App.config
5. Update Development Configuration if needed

### Scenario 3: Project Structure Reorganization

When reorganizing files/directories:

1. Move files to new locations
2. Update project references if needed
3. Rebuild to verify no broken imports
4. Update CLAUDE.md Project Structure:
   - Redraw directory tree
   - Update all file path references in sections
   - Verify diagram matches actual structure

### Scenario 4: Major Refactoring

When refactoring code (like the 4051→888 line Form1.cs refactor):

1. Complete refactoring
2. Document new patterns in Architecture section
3. Update line count references
4. Add entry to Refactoring History
5. Review all service documentation for accuracy
6. Update Development Guide if patterns changed

### Scenario 5: Performance or Security Fix

When fixing issues:

1. Note in Version History if significant
2. Check if affects service documentation
3. Update Development Guide if affects patterns
4. Check if requires configuration changes
5. Note in Code Documentation section if improves coverage

## Troubleshooting

### Issue: CLAUDE.md is out of sync after major changes

**Solution**:
1. Run `/enhance-claude-md` slash command
2. Review all proposed changes
3. Accept updates that are accurate
4. Manually fix any inaccurate sections
5. Verify all 14 services are documented

### Issue: Service count is wrong

**Solution**:
1. Count actual files in `FileManager/Services/`
2. Count services documented in "Service Layer Documentation"
3. If count differs, use `/enhance-claude-md` to sync
4. Verify each service has both interface and implementation

### Issue: Technology Stack versions are outdated

**Solution**:
1. Open `FileManager/packages.config`
2. Extract all package versions
3. Update Technology Stack section with current versions
4. Run through /enhance-claude-md for validation

### Issue: Project Structure diagram is wrong

**Solution**:
1. Run `dir /s /b` on FileManager directory
2. Compare with diagram in CLAUDE.md
3. Update diagram to match actual structure
4. Ensure all major directories are represented

## Quick Reference

### Service Count Validation

```powershell
# In FileManager\Services directory
dir /b I*Service.cs | wc -l  # Should show 14
dir /b *Service.cs | wc -l   # Should show 28 (14 interfaces + 14 implementations)
```

### File Structure Validation

Verify these core files exist:
- Form1.cs (should be ~888 lines)
- FileCount.cs (data models)
- Program.cs (entry point)
- Services/ directory with 14 service pairs

### CLAUDE.md Validation Checklist

Print this and check after updates:

- [ ] Service count stated correctly (14 services)
- [ ] All services in Services/ directory documented
- [ ] Service numbering sequential (1-14)
- [ ] Tech Stack versions match packages.config
- [ ] Project Structure tree is accurate
- [ ] No broken markdown formatting
- [ ] No outdated file references
- [ ] Architecture diagram is current
- [ ] Code documentation section is complete
- [ ] Version history is recent

## Next Steps

1. **Review this document** and understand the monitoring approach
2. **Save this maintenance guide** in your FileManager directory
3. **Use `/enhance-claude-md`** to verify current CLAUDE.md state
4. **Establish a schedule** for regular checks (weekly recommended)
5. **Update CLAUDE.md** whenever you add new services or major features
6. **Keep this guide updated** as your maintenance process evolves

## Support & Updates

This maintenance system is designed to be:
- **Automated**: Triggers checks on code changes
- **Flexible**: Works with manual verification workflow
- **Scalable**: Adapts as project grows
- **Low-overhead**: Doesn't require complex tooling

For any questions about specific updates needed, use the `/enhance-claude-md` command with a description of recent changes.

---

**Document Version**: 1.0.0
**Last Updated**: 2026-01-13
**Maintenance Active**: Yes
**Project Status**: Service-oriented architecture with 14 services
