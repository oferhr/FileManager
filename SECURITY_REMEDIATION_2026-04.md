# FileManager — Security Remediation Report

**Project:** FileManager (.NET Framework 4.8 Windows Forms application)
**Original audit date:** 2026-04-11
**Remediation started:** 2026-04-24
**Document status:** Complete — all seven items verified closed by `/cso` re-audit on 2026-04-24
**Prepared for:** Client delivery

---

## 1. Executive Summary

An AI-assisted security audit (`/cso`, daily mode, 8/10 confidence gate) was conducted on the FileManager repository on 2026-04-11. A verification re-audit on 2026-04-24 confirms all findings and concerns are closed:

> **Re-audit result: 0 critical / 0 high / 0 medium / 0 tentative. Trend: IMPROVING — 7 resolved, 0 persistent, 0 new.**

The original audit reported:

| Severity | Count |
|----------|-------|
| CRITICAL | 0 |
| HIGH     | 3 |
| MEDIUM   | 0 |
| TENTATIVE| 0 |

All three HIGH findings are concentrated in the email/Excel COM interop boundary — the same surfaces the application uses to read the `Codes.xlsx` lookup workbook and dispatch mail via Outlook. The audit also flagged two operational concerns (NLog logging targets unwired in production configuration, and a dormant `SecurityHelper` class with no active callers) that are not themselves vulnerabilities but materially weaken forensic visibility and code hygiene.

The audit confirmed the following areas are clean:
- No SQL, shell execution, untrusted deserialization, weak cryptography, XXE, zip extraction, or TLS bypass
- No secrets tracked in git history across any branch
- Supply chain is minimal and current (Newtonsoft.Json 13.0.3; NLog 4.6.8 has no known CVEs)
- `ArchiveService`, `FileDeletionService`, and `EmailValidator.IsValidEmail` are verified well-hardened

This document tracks the remediation of all reported findings. Each finding is described below with its technical root cause, exploit scenario, impact, and planned fix. Fix evidence (commit hashes and code diffs) will be filled in as the work is completed, followed by a re-run of the `/cso` audit to verify closure.

---

## 2. Scope of Remediation

| # | Item | Severity | Status |
|---|------|----------|--------|
| 1 | Dormant email domain allowlist — Outlook exfiltration path | HIGH | FIXED |
| 2 | `EmailService` missing path boundary check on `dirSetting.dir` | HIGH | FIXED |
| 3 | Excel COM opens workbooks with macros enabled by default | HIGH | FIXED |
| 4 | NLog logging targets unwired — no forensic trail at runtime | Concern | FIXED |
| 5 | `SecurityHelper` class is dead code — wire in or remove | Concern | FIXED (removed) |
| 6 | `PathValidator.IsValidPath` contains empty-body dead code block | Minor | FIXED |
| 7 | `.gstack/` directory not listed in `.gitignore` | Hygiene | FIXED |

---

## 3. Findings

### Finding #1 — Dormant email domain allowlist (HIGH)

**File:** `FileManager/Services/EmailService.cs:324-325`
**Severity:** HIGH · **Confidence:** 9/10 · **Category:** Data exfiltration (OWASP A01/A08)

**Description**

Two independent email domain allowlist functions exist in the codebase, and neither is ever called:

1. `SecurityHelper.IsEmailAllowed` at `FileManager/Security/SecurityHelper.cs:137`, with the inline doc comment *"Prevents data exfiltration to unauthorized external addresses."*
2. `EmailValidator.IsEmailFromAllowedDomain` at `FileManager/Utilities/EmailValidator.cs:277`, a cleaner reimplementation of the same check.

A repository-wide grep for `IsEmailAllowed|SecurityHelper\.IsEmail|IsEmailFromAllowedDomain` returns only the method definitions — zero call sites. `EmailService.SendEmails` validates email *format* (`EmailValidator.IsValidEmail`) and strips CRLF injection (`SanitizeEmailAddress`), then assigns the address directly to `oMsg.To` on line 325 and calls `oMsg.Send()` on line 360 without any domain check.

The send sequence uses `GetInspector.Activate()` followed by `Send()` — this briefly surfaces an Outlook inspector window but does not wait for user confirmation; the send is programmatic and effectively silent.

**Technical root cause**

Email recipients are read from `fileManager_emailDirConfig.json` via `EmailDirSettings.email`. There is no defence-in-depth check that the recipient belongs to an approved domain before the mail is dispatched through the user's authenticated Outlook profile.

**Exploit scenario**

1. Attacker (insider, compromised JSON file, or via the email column in the DataGridView) sets `EmailDirSettings.email = "attacker@external.example"` in `fileManager_emailDirConfig.json`.
2. User launches FileManager and clicks Start. `SendEmails` enumerates `.tif`/`.tiff`/`.pdf` files in the configured directory.
3. `oMsg.To = "attacker@external.example"`. All matching files are attached. `oMsg.Send()` fires.
4. Outlook delivers the mail from the user's authenticated profile to the external recipient. No preview, no second-channel confirmation.

**Impact**

The application is documented to process confidential customer records. This finding is a one-line-configuration-change exfiltration path to any RFC-5322-valid address on the public internet. A mitigation was written twice already (the two dormant allowlist functions) — the gap is that neither copy was wired in.

**Remediation plan**

1. Add a new configuration key `AllowedMailDomains` to `App.config` (comma-separated list of permitted domain suffixes — `@ERAN-MOR.CO.IL`).
2. In `EmailService.SendEmails`, after the existing format validation, call `EmailValidator.IsEmailFromAllowedDomain` against the configured allowlist. If the list is empty or the email's domain is not matched, log a `EmailDomainNotAllowed` security event and skip that recipient.
3. Mirror the same check in `Form1.dataGridView1_CellEndEdit` so disallowed domains are rejected at input time in the UI, not only at send time.
4. Delete the unused `SecurityHelper.IsEmailAllowed` method to remove the duplicate implementation (addressed in Finding #5).

**Status:** FIXED

**Fix details:** Commit `13f4e01` (*"Add email domain allowlist to block outbound exfiltration"*)

- **App.config**: added `AllowedMailDomains` key with value `@ERAN-MOR.CO.IL` (single authoritative organisational domain).
- **Form1.cs** constructor: parses the config value into `_allowedMailDomains` (a `List<string>`). Parsing tolerates both comma and semicolon delimiters and strips the optional leading `@` so config values like `@ERAN-MOR.CO.IL` and `ERAN-MOR.CO.IL` behave identically.
- **EmailService.cs**: new constructor parameter `List<string> allowedMailDomains`; new public method `IsEmailDomainAllowed(string email, out string errorMessage)` that wraps `EmailValidator.IsEmailFromAllowedDomain` and treats an empty allowlist as fail-closed (blocks the send rather than silently allowing everything).
- **EmailService.SendEmails**: the pre-send filter now runs the domain check after format validation; disallowed recipients are dropped and logged as structured `EmailDomainNotAllowed` security events with `Email` and `Dir` properties.
- **Form1.dataGridView1_CellEndEdit**: same check on the email column input; disallowed domains show a Hebrew MessageBox, keep the cell in edit mode, and log the security event — bad values never reach the JSON config.
- **IEmailService.cs**: added `IsEmailDomainAllowed` to the interface contract so the Form1 input-time check goes through the interface, not a concrete-class cast.

Fail-closed behaviour is deliberate: if `AllowedMailDomains` is missing or empty in `App.config`, `SendEmails` blocks every recipient and logs a security event. This prevents a deployment where the allowlist was forgotten from silently shipping as "allow all".

The duplicate dormant allowlist in `SecurityHelper.IsEmailAllowed` was removed in a separate commit — see Concern #5.

**Verification:** *(awaiting `/cso` re-run — see Section 7)*

---

### Finding #2 — `EmailService` missing path boundary check (HIGH)

**File:** `FileManager/Services/EmailService.cs:73`
**Severity:** HIGH · **Confidence:** 8/10 · **Category:** Path traversal (OWASP A01)

**Description**

`EmailService.SendEmails` constructs a directory path from user-influenced configuration and enumerates files inside it recursively, with no boundary validation:

```csharp
var basePath = Path.Combine(_basePath, dirSetting.dir);
if (!Directory.Exists(basePath)) { continue; }
var lfiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
    .Where(s => s.ToLower().EndsWith(".tif") || ... || s.ToLower().EndsWith(".pdf"));
```

Every peer service that takes a user-influenced directory name validates the same pattern through `PathValidator.ValidateAndNormalize` (see `ExcelService.cs:55`, `FileDeletionService.cs:40`, `ArchiveService.cs:66`, `ReportManagementService`, `FileNameManagementService`, `DuplicateManagementService`, `FolderSplitService`). `EmailService` is the single outlier.

**Technical root cause**

`Path.Combine(_basePath, dirSetting.dir)` returns `dirSetting.dir` unchanged if that value is an absolute path (documented .NET behaviour). It also does not prevent relative traversal sequences such as `..\..\some-other-folder`. Without a post-combine boundary check, `basePath` can resolve anywhere on disk the user's process has read access to.

**Exploit scenario**

1. Attacker sets `EmailDirSettings.dir = "..\..\some-sensitive-folder"` (or an absolute path) in the config.
2. `Directory.GetFiles` recursively enumerates `.tif`/`.pdf` files outside `_basePath`. Those files are attached to the outbound mail.
3. After sending, `ArchiveProcessedFiles` (line ~423) calls `Directory.Delete(Path.Combine(basePath, "1"), true)` — a recursive delete operating *outside* the allowed base directory.

This compounds with Finding #1: the same tainted JSON record controls both *what* is read and *who* receives it.

**Impact**

Combined read-and-exfiltrate plus recursive-delete primitive outside the intended directory scope. Potential for data loss and data exposure.

**Remediation plan**

Insert a `PathValidator.ValidateAndNormalize` call immediately after the `Path.Combine` call, mirroring the exact pattern already used in `ExcelService.cs:55`:

```csharp
if (!PathValidator.ValidateAndNormalize(basePath, _basePath, out basePath, out var pathError))
{
    _loggingService.LogSecurityEvent("PathValidationFailure",
        $"EmailService rejected directory: {pathError}",
        new Dictionary<string, object> { { "dir", dirSetting.dir } });
    continue;
}
```

The auditor also suggested validating `dirSetting.dir` upstream in `HandleGridCellEndEdit` as defence-in-depth. That was attempted and then reverted — see the note under "Fix details" below for why.

**Status:** FIXED

**Fix details:** Commit `5079086` (*"Add path boundary validation to EmailService"*)

- **EmailService.SendEmails**: `Path.Combine(_basePath, dirSetting.dir)` is now passed through `PathValidator.ValidateAndNormalize(combinedPath, _basePath, out basePath, out pathError)`. Failures are logged as structured `PathValidationFailure` security events with `dir` and `basePath` properties, and the iteration continues to the next recipient — the current recipient is skipped rather than failing the whole batch. The normalised `basePath` returned by the validator is what subsequent `Directory.GetFiles` and `ArchiveProcessedFiles` calls operate on, so any downstream recursive delete is also boundary-constrained.

This brings `EmailService` in line with the validation pattern already used by `ExcelService`, `ArchiveService`, `FileDeletionService`, `FileCopyService`, `FileNameManagementService`, `DuplicateManagementService`, `FolderSplitService`, and `ReportManagementService`.

An earlier iteration of this fix also added an `InputValidator.IsValidFolderName` check inside `EmailService.HandleGridCellEndEdit` as defence-in-depth at the UI layer. That check was removed during PR review: `HandleGridCellEndEdit` runs on every cell edit in a row (email, method, etc.) and reads the folder value from the row regardless of which column was edited — an invalid folder value would therefore silently block unrelated edits with no user feedback. The `PathValidator.ValidateAndNormalize` check in `SendEmails` is the security boundary and covers the same threat with structured logging; the UI-layer check was belt-and-suspenders that turned out buggy, and has been dropped.

**Verification:** *(awaiting `/cso` re-run — see Section 7)*

---

### Finding #3 — Excel COM runs macros by default (HIGH)

**File:** `FileManager/Services/ExcelService.cs:215-222` (also `ExcelExportService.cs:12`)
**Severity:** HIGH · **Confidence:** 8/10 · **Category:** Code execution (STRIDE EoP)

**Description**

`ExcelService.GetExcelValues` creates an Excel COM instance and opens the configured lookup workbook without setting `AutomationSecurity`:

```csharp
var xlApp = new Excel.Application();
...
xlWorkbook = xlApp.Workbooks.Open(_excelPath);
```

A grep for `AutomationSecurity|msoAutomationSecurity` across the entire repository returns zero hits. The default value for a COM-created `Excel.Application` is `msoAutomationSecurityLow`, which executes VBA macros in opened workbooks **without any prompt, without Protected View, and bypassing the Trust Center**. This is a well-documented Office Interop pitfall — the protections a user relies on when opening an Excel file by double-clicking do *not* apply to interop-opened workbooks.

`_excelPath` is read from `App.config` key `ExcelPath` and defaults to `C:\Projects\Anatoly\FileManager\Codes.xlsx`. This is a local or possibly network-shared lookup table. It is not user input at runtime, but it is reachable by any attacker with write access to the file system location.

**Technical root cause**

Missing hardening step: `xlApp.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityForceDisable;` is not set before `Workbooks.Open`.

**Exploit scenario**

1. Attacker gains write access to `Codes.xlsx` — for example via a shared folder, a cloud-sync compromise, lateral movement on the LAN, or an insider threat.
2. Attacker replaces the workbook with a file containing a `Workbook_Open` VBA macro.
3. A legitimate user runs FileManager → `SetExcelNames` → `GetExcelValues` → `Workbooks.Open(_excelPath)`.
4. The macro executes with the user's full privileges. It can drop payloads, exfiltrate data, invoke the already-authenticated Outlook profile to send mail, or establish persistence.

**Impact**

Arbitrary code execution in the user's session every time the file-renaming or export flow runs. Combined with Finding #1 (unrestricted Outlook send), a single compromised `Codes.xlsx` becomes a pivot for mass data exfiltration.

**Remediation plan**

1. Set `xlApp.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityForceDisable;` immediately before `Workbooks.Open` in `ExcelService.GetExcelValues`.
2. Apply the same setting in `ExcelExportService.ExportToExcel` as defence in depth, even though that method creates a new empty workbook rather than opening an existing one.
3. *(Deferred — tracked as future hardening, not part of this remediation scope):* store a known SHA-256 of `Codes.xlsx` in `App.config` and verify at startup, or migrate the lookup to a plain CSV, or parse `.xlsx` via the OpenXML SDK (`DocumentFormat.OpenXml`) which reads the zip+XML directly and cannot execute macros.

**Status:** FIXED

**Fix details:** Commit `4f0da82` (*"Harden Excel COM against macro execution"*)

- **ExcelService.GetExcelValues**: inserted `xlApp.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityForceDisable;` inside the `try` block immediately before `xlApp.Workbooks.Open(_excelPath)`. Any macros embedded in the opened workbook are now disabled rather than silently executed.
- **ExcelExportService.ExportToExcel**: the same `AutomationSecurity` assignment is applied before `xlApp.Workbooks.Add()` as defence in depth. `ExportToExcel` creates a new empty workbook (not a high-risk read surface), but consistent hardening across both Excel call sites protects against future code paths that might later call `Open` here.

The `Microsoft.Office.Core` namespace was already available via a pre-existing COM reference in `FileManager.csproj:228`, so no project reference change was required.

Item #3 in the original remediation plan (SHA-256 verification of `Codes.xlsx` or migration to a macro-incapable format such as CSV or the OpenXML SDK) is tracked as *deferred future hardening* and is out of scope for this remediation round.

**Verification:** *(awaiting `/cso` re-run — see Section 7)*

---

## 4. Operational Concerns

### Concern #4 — NLog logging targets unwired

**File:** `FileManager/NLog.config`

**Description**

`NLog.config` contains `<targets>` and `<rules>` sections, but every concrete `<target>` and `<logger>` declaration inside them is commented out. As a result, every call in the codebase to `LoggingService.LogSecurityEvent`, `LoggingService.LogFileOperation`, `LoggingService.LogValidationFailure`, `LoggingService.LogError`, and similar methods silently no-ops at runtime — the audit trail that the application claims to produce does not exist on disk.

This is not itself a vulnerability under standard security audit exclusion rules (absence of logging is not a vulnerability), but it defeats the audit-trail argument supporting the mitigations for Findings #1 and #2. If one of those paths were exploited today, post-incident forensics would have nothing to work with.

**Remediation plan**

Add a `<target xsi:type="File" ...>` element to `NLog.config` writing to `${basedir}/logs/${shortdate}.log` (or equivalent), plus a `<logger name="*" minlevel="Info" writeTo="..." />` rule. Confirm that log files are produced at runtime.

**Status:** FIXED

**Fix details:** Commit `f76305e` (*"Wire NLog file targets so the audit trail is persisted"*)

Two file targets are now configured in `NLog.config`, both rolling daily and sitting next to the executable under `logs/`:

- `mainFile` — `${basedir}/logs/${shortdate}.log`, all events at Info level and above, 10 MB archive threshold, 30-file archive retention. Layout includes timestamp, level, logger, message, exception detail if present, and a dump of all structured event-properties.
- `securityFile` — `${basedir}/logs/security-${shortdate}.log`, filtered via `<when condition="'${event-properties:item=SecurityEvent}' == 'True'" action="Log" />` to only include the events marked by `LoggingService.LogSecurityEvent` (which sets the `SecurityEvent=true` property on its `LogEventInfo`). 90-file archive retention so the security trail outlives the main log if the main log rolls over faster.

The rule for `securityFile` does not set `final="true"`, so security events also appear in `mainFile` — they are duplicated to give forensics a focused file plus full chronological context.

With these targets wired, the audit-trail arguments supporting the mitigations for Findings #1 and #2 are now materially true: `EmailDomainNotAllowed`, `PathValidationFailure`, file-operation, and validation-failure events are persisted to disk on every run.

---

### Concern #5 — `SecurityHelper` class is dead code

**File:** `FileManager/Security/SecurityHelper.cs`

**Description**

The `SecurityHelper` class is referenced only by the `<Compile Include="Security\SecurityHelper.cs" />` entry in `FileManager.csproj:170`. None of its methods (`ValidatePath`, `Encrypt`, `Decrypt`, `SanitizeFileName`, `IsEmailAllowed`, `CalculateFileHash`, `HasAllowedExtension`, `CreateBackup`, `IsDirectoryAccessible`) have any call sites in the application code. The equivalent functionality already lives in the active `Utilities/` folder (`PathValidator`, `EmailValidator`, `InputValidator`).

**Remediation plan**

Delete `SecurityHelper.cs` and the corresponding `<Compile>` entry in `FileManager.csproj`. This removes the duplicate allowlist from Finding #1 in a single stroke and eliminates ~290 lines of unmaintained code that could diverge from the active validators.

**Status:** FIXED (removed)

**Fix details:** Commit `9c7dd72` (*"Delete dead SecurityHelper class"*)

- Deleted `FileManager/Security/SecurityHelper.cs` (290 lines).
- Removed the `<Compile Include="Security\SecurityHelper.cs" />` entry from `FileManager.csproj`.
- Removed the empty `FileManager/Security/` directory.

A repository-wide grep confirms no remaining source references to `SecurityHelper` in `.cs` files. References that do remain are in historical documentation files (`SECURITY_AUDIT_REPORT.md`, the original audit PDF/JSON/HTML under `.gstack/`, and this document) — those are records of the situation at the time of the audit and are correct to retain as-is.

---

### Concern #6 — `PathValidator.IsValidPath` empty dead code block

**File:** `FileManager/Utilities/PathValidator.cs:87-91`

**Description**

An `if` block with an empty body and the inline comment *"We'll allow it but could be tightened based on requirements"* remains in the code. This is dead code that signals an unfinished check.

**Remediation plan**

Either remove the block entirely (preferred — the method already performs the needed traversal-pattern checks earlier) or complete the intended tightening. This item was flagged by the auditor as confidence 5/10 and suppressed from the primary findings; it is included here for completeness.

**Status:** FIXED

**Fix details:** Commit `0749ba5` (*"Clean PathValidator dead code block and gitignore security reports"*)

The empty-body `if` and its unused `fullPath` local were removed. The surrounding `try` / `catch` now retains only the `Path.GetFullPath(path)` call, whose purpose is to throw for malformed paths so the catch clause can return a structured `errorMessage`. The preceding checks in `IsValidPath` (null/empty guard, null-byte guard, length cap, invalid-character scan, explicit traversal-pattern list) remain in place — they were already performing the work the dead block pretended to.

---

### Concern #7 — `.gstack/` not in `.gitignore`

**File:** `.gitignore`

**Description**

The `/cso` tooling writes reports to `.gstack/security-reports/`. This directory currently appears as untracked in `git status`. Without a `.gitignore` entry, a future `git add -A` or `git add .` could accidentally commit historical security reports (including this one) into the repository history.

**Remediation plan**

Add `.gstack/` to `.gitignore`.

**Status:** FIXED

**Fix details:** Commit `0749ba5` (bundled with Concern #6).

Added the line `.gstack/` under a new `## /cso security audit reports — keep out of repo history` heading in `.gitignore`. The historical `2026-04-11-172456-report.pdf`, `.html`, and `.json` files under `.gstack/security-reports/` remain on the local filesystem for reference but will not be staged by `git add -A` or `git add .` going forward.

---

## 5. Clean Areas (Audit Findings — No Action Required)

The audit confirmed the following areas of the codebase are well-hardened or demonstrate strong security hygiene. These require no action and are reported here for completeness of the client deliverable.

- **No classic OWASP vulnerabilities** in the .NET code. Targeted sweeps for `SqlCommand`, `Process.Start`, `BinaryFormatter` (in .cs files), weak cryptography (MD5, SHA1, DES, RC4, ECB), `XmlResolver`/`DtdProcessing`, `ExtractToDirectory`, and `ServerCertificateValidationCallback` all returned zero hits.
- **Secrets hygiene is strong.** `git log -S` across all branches for common credential patterns (AWS keys, GitHub tokens, Stripe live keys, RSA private key headers) returned zero matches. No `.env`, `.pem`, `.key`, or `credentials*` files are tracked.
- **`ArchiveService` is well-hardened.** Validates source, mid, and destination paths at every step; explicitly rejects relative paths containing `..`; preserves source on partial failure to prevent data loss. The commit history claim of a "critical path traversal vulnerability fixed" in this service holds up under review.
- **`FileDeletionService` is well-hardened.** Path validation before each delete; boundary check before directory delete; `recursive: false` on empty-directory removal.
- **`EmailValidator.IsValidEmail` correctly blocks CRLF and URL-encoded injection.** Commit `52ad420` ("Fix email validation bypass") remediation has held.
- **Supply chain is minimal and current.** Costura.Fody, Fody, Newtonsoft.Json 13.0.3 (current), NLog 4.6.8 (older but no known CVEs at time of audit). No install scripts. COM Interop assemblies are provided by the installed Office suite, so there is no transitive NuGet supply chain to scan.

---

## 6. Remediation Timeline

| Order | Item | Commit | Date | Build status | Verified by `/cso` |
|-------|------|--------|------|--------------|--------------------|
| 1 | Finding #3 — Excel `AutomationSecurity` hardening | `4f0da82` | 2026-04-24 | PASS | **CLOSED** |
| 2 | Finding #2 — `EmailService` path boundary check | `5079086` | 2026-04-24 | PASS | **CLOSED** |
| 3 | Finding #1 — Email domain allowlist | `13f4e01` → `f53af3f` | 2026-04-24 | PASS | **CLOSED** |
| 4 | Concern #4 — NLog.config targets wired | `f76305e` | 2026-04-24 | PASS | **CLOSED** |
| 5 | Concern #5 — Delete `SecurityHelper.cs` | `9c7dd72` | 2026-04-24 | PASS | **CLOSED** |
| 6 | Concern #6 — `PathValidator` dead code removed | `0749ba5` | 2026-04-24 | PASS | **CLOSED** |
| 7 | Concern #7 — Added `.gstack/` to `.gitignore` | `0749ba5` | 2026-04-24 | PASS | **CLOSED** |

Fixes were applied and committed in the order shown above — the order matches the auditor's recommended triage sequence (smallest-blast-radius one-liner first; configuration+logic change last). Finding #1 received a follow-up review commit (`f53af3f`) to remove a placeholder domain from the allowlist and drop a buggy belt-and-suspenders check; see the Finding #1 section for detail.

**Build verification:** the full solution was built with MSBuild 17.14 (Visual Studio 2022 Community) in Debug configuration after all seven commits landed. Result: build succeeded, no new warnings introduced by the remediation. Pre-existing warnings (unused local `ex` in five catch blocks; one assigned-but-unread field `Form1.CopiedFilesDirectory`) are unchanged and out of remediation scope.

---

## 7. Verification

Verification has two parts: the static-analysis re-audit (completed) and a runtime check of the newly-wired log files (pending deployment).

### 7.1 Static re-audit via `/cso` — COMPLETE

**Re-audit date:** 2026-04-24 15:00 UTC
**Re-audit report:** `.gstack/security-reports/2026-04-24-150033.json`
**Mode:** daily (8/10 confidence gate)
**Scope:** full — all 15 phases run

**Totals:**

| Severity | Original audit (2026-04-11) | Re-audit (2026-04-24) |
|----------|----------------------------:|----------------------:|
| CRITICAL | 0 | 0 |
| HIGH     | 3 | **0** |
| MEDIUM   | 0 | 0 |
| TENTATIVE| 0 | 0 |
| Trend    | — | **IMPROVING — 7 resolved, 0 persistent, 0 new** |

**Filter statistics (re-audit):** 12 candidates scanned → 4 hard-excluded → 8 filtered by the 8/10 confidence gate → **0 reported**.

**Per-item closure evidence (from the re-audit report):**

| # | Item | Status | Evidence cited by re-audit |
|---|------|--------|----------------------------|
| 1 | Dormant email domain allowlist | CLOSED | `AllowedMailDomains` in `App.config`; `EmailService.IsEmailDomainAllowed` with fail-closed on empty allowlist; enforced at `Form1.dataGridView1_CellEndEdit` AND `EmailService.SendEmails`; `IsEmailDomainAllowed` in the `IEmailService` contract. |
| 2 | `EmailService` missing path boundary | CLOSED | `PathValidator.ValidateAndNormalize(combinedPath, _basePath, …)` invoked in `EmailService.SendEmails` before `Directory.Exists` and `GetFiles`; drop-and-log on failure. |
| 3 | Excel COM opens workbooks with macros | CLOSED | `xlApp.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityForceDisable` set before `Workbooks.Open` in `ExcelService`. |
| 4 | NLog targets unwired | CLOSED | `NLog.config` declares `mainFile` and `securityFile` File targets with 30/90-file archive retention and 10 MB archive threshold; security-event filter routes `SecurityEvent=true` properties to the security log. |
| 5 | `SecurityHelper` dead class | CLOSED | Repository-wide grep returns no source matches under `FileManager/**/*.cs`; only stale entries remain in gitignored `bin/`/`obj/` artefacts. |
| 6 | `PathValidator` empty-body dead block | CLOSED | `PathValidator.IsValidPath` body contains only active checks (null/byte/length/chars/traversal/`GetFullPath`). |
| 7 | `.gstack/` not in `.gitignore` | CLOSED | `.gitignore` contains `.gstack/`; `git ls-files .gstack/` returns no tracked files. |

**STRIDE summary (excerpt from re-audit, `EmailService`):**

- *Spoofing:* email address format + domain allowlist block spoofed recipients.
- *Tampering:* JSON config tampering mitigated by path boundary + allowlist fail-closed.
- *Repudiation:* security event log (`securityFile` target) provides audit trail.
- *Information Disclosure:* domain allowlist is the defence; no plaintext credentials in logs.
- *Denial of Service:* Outlook throttling via `MailSleepSeconds`.
- *Elevation of Privilege:* not applicable (no auth system).

### 7.2 Below-gate hygiene notes (for tracking — not findings)

The re-audit flagged two items below the 8/10 daily-mode confidence gate. They are not findings and are not in the scope of the original 2026-04-11 remediation. They are recorded here for awareness and can be addressed in a follow-up change.

1. **`FileManager_TemporaryKey.pfx` tracked in git** (severity: LOW/HYGIENE, confidence: 5/10)
   Visual Studio auto-generated ClickOnce signing PKCS#12 committed to the repository. The project sets `SignManifests=false` and no runtime code references the file, so the key is inert in the current build configuration. It is still a private-key artefact in version control.
   *Recommendation:* add `*.pfx` to `.gitignore`, delete the tracked file, and regenerate locally if ClickOnce signing is ever re-enabled.

2. **Silent catch in `EmailService.ArchiveProcessedFiles`** (severity: LOW/HYGIENE, confidence: 4/10)
   The `catch` for `Directory.Delete(newDir, true)` on an empty archive directory has an empty body with a `// Log error if needed` comment. This is a post-archive cleanup path and not security-critical, but it swallows IO errors contrary to the project's stated "never silent catch" discipline.
   *Recommendation:* add a `LoggingService.LogWarning` call with the caught exception message.

### 7.3 Runtime log-file verification (deployment-time)

After the next production run of the application:

- `logs/YYYY-MM-DD.log` should exist next to the executable and contain the `LogInfo("Sending email to…")` entries already present in `EmailService.SendEmails`.
- `logs/security-YYYY-MM-DD.log` should exist and contain the structured `EmailDomainNotAllowed`, `PathValidationFailure`, and validation-failure events whenever the corresponding defensive paths are triggered by user input or tampered config.

If either file is missing, NLog is likely blocked by filesystem permissions on the deployment target — in which case the target's `fileName` attribute should be changed to a writable location (e.g. `%LOCALAPPDATA%\FileManager\logs\`).

---

## 8. Appendix A — References

### Original audit (baseline)

- **Report:** `.gstack/security-reports/2026-04-11-172456.json` (also `…-report.pdf`, `…-report.html`)
- **Mode:** daily, 8/10 confidence gate
- **Filter stats:** 13 candidates scanned → 4 hard-excluded → 1 confidence-gated → 3 reported
- **Status line:** `DONE_WITH_CONCERNS`

### Verification re-audit

- **Report:** `.gstack/security-reports/2026-04-24-150033.json`
- **Mode:** daily, 8/10 confidence gate
- **Filter stats:** 12 candidates scanned → 4 hard-excluded → 8 confidence-gated → 0 reported
- **Totals:** 0 critical / 0 high / 0 medium / 0 tentative
- **Trend vs prior:** IMPROVING — 7 resolved, 0 persistent, 0 new

---

## 9. Appendix B — Disclaimer

The source audit is an AI-assisted scan that catches common vulnerability patterns. It is not comprehensive, not guaranteed, and not a replacement for a professional penetration test. LLM-based audits can miss subtle vulnerabilities, misunderstand complex authorisation flows, and produce false negatives.

For production systems handling confidential customer data — especially regulated data — a professional security firm engagement is recommended in addition to ongoing `/cso` passes. `/cso` is appropriate as a first-pass to catch low-hanging fruit between professional audits, not as the sole line of defence.

---

*Document version: 1.0 (verified closed by `/cso` re-audit 2026-04-24)*
*Last updated: 2026-04-24*
