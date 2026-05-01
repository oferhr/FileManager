# Email send — silent failure fix plan

**Date:** 2026-04-29
**Symptom (reported by client):** When sending emails from the app, some recipients receive their messages and some do not. The UI reports `המיילים נשלחו בהצלחה` ("emails sent successfully") in every case. No log file is created on the failed run because no exception is thrown.
**Last known-good version (per client):** v1.2.44 (pre-refactor build).

This document captures the full set of regressions identified between v1.2.44 and `master @ 4563ea5`, prioritises the fixes, and defines test/acceptance criteria for each phase. Nothing in this document changes code — execution is gated on owner approval per phase.

---

## 1. Root causes (ordered by likely contribution to the symptom)

| # | Root cause | Origin | File / line |
|---|-----------|--------|-------------|
| R1 | `AllowedMailDomains` allowlist silently drops rows whose domain is not `@ERAN-MOR.CO.IL`. For multi-address rows, one disallowed address blocks the whole row. Rejection is logged only to `logs/security-YYYY-MM-DD.log`. | post-refactor (commit `13f4e01`, refined in `256607c`) | `App.config:15`, `EmailService.cs:80-102`, `Form1.cs:826` |
| R2 | Success MessageBox is unconditional. The pre-refactor `fOK` flag was lost when the loop moved into `EmailService.SendEmails` (returns `void`). | refactor (commit `9b52875`) | `Form1.cs:719-722` (was `Form1.cs:872-881` pre-refactor) |
| R3 | Per-email exceptions are caught inside the foreach over `arfiles` and swallowed with `LogError + continue`. Pre-refactor, any COM error bubbled out of the loop and skipped the success message. | refactor | `EmailService.cs:365-447` |
| R4 | The `dnames` dictionary that mapped each attachment file → its group-name-without-extension was deleted during refactor. Subject is now the raw first-attachment filename including extension instead of the group name. May cause server-side throttling on Exchange anti-spam policies. | refactor | `EmailService.cs:380-401` |
| R5 | The verbose `log.txt` diagnostic trail (numbered `Log("000000…")` … `Log("777777…")` calls in pre-refactor `btnMail_Click`) is gone. The grouping logic is now opaque after the fact. | refactor | `EmailService.cs:291-359` |
| R6 | `oMsg.Send()` is fire-and-forget — Outlook only queues to Outbox. Server-side rejections (NDR, attachment too large, throttling, security prompt time-out) never throw. | pre-existing (true in v1.2.44 too) | `EmailService.cs:413` |
| R7 | `CleanupCopiedFiles` and `ArchiveProcessedFiles` run unconditionally after the send loop, so the visible folder state matches "all sent" even when nothing went out. | pre-existing | `EmailService.cs:139-141` |
| R8 | Several silent skips: empty email → no log; missing folder → no log; no `*.tif/*.pdf` → no log; no subdir literally named `"1"` → no log; empty `arfiles` → no log. | mostly pre-existing, masked by R5 | `EmailService.cs:80-141`, `EmailService.cs:298-315` |
| R9 | Substring matching in `ProcessFilesForEmail` (`lpaths.Where(ln => ln.Contains(xduplicateKey))`) can mis-group files when one filename is a substring of another (`1.pdf` ⊂ `11.pdf`, `9876789` ⊂ temp folder name). Also: `xduplicateKey = Path.GetFileNameWithoutExtension(...)` is dead code, immediately overwritten on the next line. | pre-existing in pre-refactor too | `EmailService.cs:343-356` |

---

## 2. Phased fix plan

Each phase is independently shippable and independently revertable. Phases 1–3 together restore the v1.2.44 user-visible behaviour. Phases 4–5 are quality improvements layered on top.

### Phase 1 — Remove the domain allowlist  *(client request)*

**Why first:** Highest-impact, lowest-risk single change. Restores the pre-refactor "any valid email goes through" behaviour. v1.2.44 had no allowlist — removing it is not a regression vs the known-good baseline.

**Files touched:**
- `FileManager/App.config:14-15` — drop the `AllowedMailDomains` key + comment.
- `FileManager/Form1.cs:116-119` — drop `_allowedMailDomains` field + XML doc.
- `FileManager/Form1.cs:173-176` — drop the `ConfigurationManager.AppSettings["AllowedMailDomains"]` parsing block.
- `FileManager/Form1.cs:218` — drop the constructor argument when wiring `EmailService`.
- `FileManager/Form1.cs:823-833` — drop the `IsEmailDomainAllowed` check in `dataGridView1_CellEndEdit`.
- `FileManager/Services/EmailService.cs:22, 32, 40` — drop field, ctor parameter, ctor assignment.
- `FileManager/Services/EmailService.cs:44-73` — delete `IsEmailDomainAllowed` method.
- `FileManager/Services/EmailService.cs:92-99` — delete the per-row domain filter in `SendEmails`.
- `FileManager/Services/IEmailService.cs:9` — drop `IsEmailDomainAllowed` from the interface.
- `FileManager/Utilities/EmailValidator.cs:277-310` — delete `IsEmailFromAllowedDomain` helper (no other callers).

**Keep:**
- `EmailValidator.IsValidEmail` / `IsValidEmailList` / `SanitizeEmailList` — these are format/CRLF-injection guards and are still useful. They are NOT the allowlist.

**Risk:** Very low. The allowlist was added 2026-04 and never existed in v1.2.44.
**Effort:** ~30 min including build.
**Acceptance:**
1. App.config no longer contains `AllowedMailDomains`.
2. `grep -r AllowedMailDomains FileManager/` returns no production-code matches (history docs may still reference it).
3. Solution compiles (Debug + Release).
4. Email send to any valid recipient succeeds end-to-end (manual smoke test).

---

### Phase 2 — Restore "real success/failure" reporting (fixes R2 + R3)

**Why:** Single biggest cause of "system reported all OK" — the UI cannot distinguish 0/10 success from 10/10 success.

**Design:**
- Introduce a result type used as the `SendEmails` return value:
  ```csharp
  public class EmailSendResult
  {
      public int Attempted;
      public int Succeeded;
      public int SkippedNoEmail;
      public int SkippedInvalidFormat;
      public int SkippedMissingFolder;
      public int SkippedNoFiles;
      public List<string> FailedRecipients;   // recipient + brief reason
  }
  ```
- `IEmailService.SendEmails` signature changes from `void` to `EmailSendResult`.
- Inside `SendEmailAttachments`, count successes and failures. Per-email try/catch keeps swallowing exceptions (so one bad message does not nuke the batch) but increments `FailedRecipients`.
- `Form1.btnMail_Click` shows `המיילים נשלחו: X הצליחו, Y נכשלו, Z דולגו — בדוק קובץ הלוג`. Only show "all succeeded" when `Succeeded == Attempted && SkippedAll == 0`.

**Files touched:**
- `FileManager/Services/IEmailService.cs` — change return type.
- `FileManager/Services/EmailService.cs:75-143, 361-447` — populate the result.
- `FileManager/Form1.cs:699-731` — use the result for the MessageBox.

**Risk:** Low. Caller signature change, all callers in this repo.
**Effort:** ~2 hours.
**Acceptance:**
1. Force a failure (point one row at a non-existent folder, another at a malformed email, send a third successfully). MessageBox reports `1 הצליח, 1 נכשל, 1 דולג`.
2. Force a Send() exception (e.g. detach Outlook profile or kill outlook.exe mid-batch). MessageBox does NOT say all succeeded.
3. `logs/YYYY-MM-DD.log` contains one line per attempted send and one error line per failure.

---

### Phase 3 — Restore subject behaviour (fixes R4)

**Why:** Pre-refactor subject was the **group name without extension** (`111_222`). Post-refactor it is the **first attachment's full filename** (`111_222_1.pdf`). For `icheck == 2`, the pre-refactor pipeline applied `GetMailFileName` twice, post-refactor only once. Either may trigger anti-spam quarantines on Exchange.

**Design:**
- Reintroduce a `Dictionary<string,string> dnames` (keyed by the new sanitized filename, value = `Path.GetFileNameWithoutExtension(GetMailFileName(...))`) inside `ProcessFilesForEmail`.
- Pass `dnames` to `SendEmailAttachments` as part of the per-row state OR change `arfiles` from `List<List<string>>` to `List<MailGroup>` where `MailGroup { string SubjectKey; List<string> Files; }` — cleaner and avoids the lookup.

**Files touched:**
- `FileManager/Services/EmailService.cs:291-401` — restructure grouping output and subject computation.

**Risk:** Medium — touches the grouping data model. Must be tested on real folder layouts.
**Effort:** ~3 hours including a manual matrix on `icheck` ∈ {0, 1, 2, 3, 4}.
**Acceptance:**
1. Send with `icheck=0,1,3,4` — subject equals group name without extension, matches v1.2.44 byte-for-byte on a sample folder.
2. Send with `icheck=2` — subject equals `GetMailFileName(groupKey, 2, true)`, matches v1.2.44.
3. Diff captured Outlook drafts (use `Display` instead of `Send` in a debug branch) against pre-refactor expectations.

---

### Phase 4 — Restore diagnostic logging (fixes R5 + R8)

**Why:** Even after Phases 1–3, when something does go wrong we still cannot see *which file ended up in which group, sent to which recipient*. The pre-refactor `log.txt` numbered traces gave us that.

**Design:**
- Replace pre-refactor `Log("000…", w)` style with structured `LoggingService.LogInfo` calls at the same checkpoints, gated by a config flag (`EmailDiagnosticsEnabled`, default `true`):
  - per file added to `lpaths` — log filename + computed mail name + computed copied path
  - per duplicate key — log key + matched paths
  - per `arfiles` group — log subject + recipient + attachment count + attachment names
- Add explicit `LogWarning` for every silent skip identified in R8 (empty email, missing folder, no matching files, no `1` subdir, empty `arfiles`).

**Files touched:**
- `FileManager/Services/EmailService.cs` (multiple sites)
- `FileManager/App.config` — add `EmailDiagnosticsEnabled`

**Risk:** Very low. Pure logging.
**Effort:** ~1 hour.
**Acceptance:**
1. Send batch with one row missing email, one with no `1` subdir, one valid. Main log shows three distinct warning lines explaining each skip plus one info line for the success.
2. With `EmailDiagnosticsEnabled=false`, log volume returns to current baseline (one line per send).

---

### Phase 5 — Detect actual delivery (mitigates R6 + R7)

**Why:** This is the real fix to "Outlook said it sent and it didn't." Optional — only needed if Phases 1–4 do not fully eliminate client complaints.

**Two options, ranked by client impact:**

**Option A — Verify via `Sent Items` and `Outbox`** *(stay on Outlook automation)*
- After `oMsg.Send()`, capture `oMsg.EntryID`.
- Poll `oApp.Session.GetDefaultFolder(olFolderSentMail)` for that EntryID with a timeout (5–10 s).
- If still in Outbox after timeout, treat as failure. Log + add to `FailedRecipients`.
- Pros: no architectural change, no SMTP credentials needed.
- Cons: depends on Exchange caching; some configurations move sent mail too fast for polling.

**Option B — Move to SMTP** *(`System.Net.Mail.SmtpClient`)*
- Send directly to the Exchange SMTP gateway. Exceptions surface synchronously per recipient.
- Pros: full per-recipient feedback, no Outlook security prompts, no Inspector window flicker, no COM resource issues.
- Cons: needs SMTP host, port, auth credentials configured. Lose user-attached signatures unless we render them server-side. Existing `oMsg.GetInspector.Activate()` signature behaviour cannot be reproduced — we would need a fixed signature template.

**Recommendation:** Defer until Phases 1–4 ship and the client validates whether the symptom is gone. If still reported, prefer Option B for clean semantics; Option A only if SMTP is administratively blocked.

**Effort:** A — ~4 hours; B — 1–2 days plus client coordination on signature handling.

---

### Phase 6 — Stop archiving on partial failure (mitigates R7)

**Why:** Even with delivery detection, the current code archives the source files (moves out of `1` subdir) regardless of send outcome. After Phase 2 we will *know* which sends failed; we should keep their source files in place for retry.

**Design:**
- `ArchiveProcessedFiles` accepts the list of *successfully-sent* filenames and only archives those. Failed-send sources stay in `1`.
- `CleanupCopiedFiles` (the temp `9876789` folder) is fine to run unconditionally — those are throwaway copies.

**Files touched:**
- `FileManager/Services/EmailService.cs:135-143, 468-523`

**Risk:** Medium — file-state semantics. Need to confirm with client that "leave failed sends in `1` for retry" matches their workflow expectation.
**Effort:** ~2 hours.
**Dependency:** Phase 2 (we need per-recipient success/failure) and ideally Phase 5 (we need *real* per-recipient success/failure).

---

### Phase 7 — Fix the `Contains` mis-grouping bug  (R9)

**Why:** Pre-existing bug — same issue exists in v1.2.44. Low priority unless filename collisions are observed in the field.

**Design:** Track grouping by `(lCopiedNames[i], lpaths[i])` index pairs rather than substring matching across `lpaths`. Drop the dead `Path.GetFileNameWithoutExtension` overwrite.

**Effort:** ~1 hour.
**Risk:** Low.
**Acceptance:** Folder containing `1.pdf` and `11.pdf` produces two distinct groups, not one.

---

## 3. Recommended sequencing

```
Phase 1  ── ship immediately (client request, low risk)
   │
Phase 2  ── ship next sprint  (real success/failure)
   │
Phase 3  ── ship with Phase 2 (subject regression)
   │
Phase 4  ── ship with Phase 2 (diagnostic logging)
   │
   ├─→ Validate with client. If symptom persists ↓
   │
Phase 5  ── ship later if needed (delivery detection)
   │
Phase 6  ── follows Phase 5  (don't archive failed sources)
   │
Phase 7  ── opportunistic    (mis-grouping bug)
```

**Minimum viable fix to close the client ticket:** Phases 1 + 2 + 4. Phase 3 is highly recommended to avoid Exchange quarantine. Phase 5+ is strategic.

---

## 4. What this plan deliberately does NOT change

- **The service-oriented architecture.** Form1.cs is not being merged back. We are restoring v1.2.44 *behaviour*, not v1.2.44 *file layout*.
- **Path validation.** `PathValidator.ValidateAndNormalize` in `EmailService` stays — it is a real security guard, unrelated to the symptom.
- **Email format validation / CRLF stripping.** `EmailValidator.IsValidEmail`, `IsValidEmailList`, `SanitizeEmailList` stay. These are not the allowlist.
- **NLog targets.** Both `mainFile` and `securityFile` targets in `NLog.config` stay. Phase 4 makes the main log informative again.
- **The `1` subfolder convention.** Files outside `1` are still skipped — that is intentional product behaviour from v1.2.41 (commit `a3d9815` — "parse only directory '1'"). Phase 4 will at least log the skip.

---

## 5. Open questions for the client

1. **Allowlist confirmed gone:** removing `AllowedMailDomains` means any valid-format email may be sent. Confirm there is no compliance requirement that would prefer a config-driven *warning* (instead of a hard block) on external recipients.
2. **Retry behaviour (Phase 6):** when a send fails, do you want the source files to stay in `1` for an automatic retry on the next "send" click, or be moved to a separate `failed-YYYY-MM-DD-HH-MM` folder for manual triage?
3. **Subject regression (Phase 3):** is the v1.2.44 subject format (`group name without extension`) still the desired format, or has the client expectation changed since? A quick screenshot of an old vs new received email would settle this in five minutes.
4. **Delivery verification (Phase 5):** if we go with Option B (SMTP), the client must provide the Exchange SMTP host/port and confirm whether automated sends should use a service account or the logged-in user's credentials. Also: how should signatures be handled (fixed template, none, or HTML pulled from a config file)?
