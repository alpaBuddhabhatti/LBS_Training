# V3 release notes

Prepared from `student-pack-v2`, preserving the existing `V2` tree unchanged.
The current edition is in `V3` on branch `v3`; this is a branch update, not a
GitHub Release/tag. Dates below refer to the 29 October 2026 training work.

## Main changes

- Retains the V2-style Day 1 / Day 2 / Labs student folder layout.
- Aligns Day 2 to the newly uploaded Service Bus / Azure AI agenda, rather than
  the earlier cost/monitoring-first afternoon.
- Updates the student Day 2 PowerPoint and regenerates its PDF from that deck.
  Core slides 1-33 include a short working tracker demo followed by AI teaching
  and practical material. Slides 34-42 are optional technical/demo appendices.
- Supplies an aligned instructor PowerPoint/PDF with speaker notes under
  `Instructor Materials/Day 2`; removes instructor notes from the student deck.
- Adds the separate Azure-backed tracker source and its architecture/identity/
  networking/testing guide. The original local tracker remains separate.
- Includes original tracker samples 05-10 and matching checks/manifest.
- Adds external-guest invitation and Training RG access guidance, including a
  student slide demo and the distinction between RG roles and data permissions.
- Documents Function SQL external-user creation and least-privileged grants,
  queue roles and the broad SQL Azure-services networking exception.
- Distinguishes observed successful jobs, earlier failed tests and future work.
- Retains supporting operations/cost/KQL material without displacing AI sessions.
- Adds an original fictional one-page PDF for the separate Read/Layout AI
  exercise; no live AI analysis is claimed.
- Aligns Day 1's Day 2 preview, repairs a SQL-verification hyperlink and fixes
  slide numbering, with a matching regenerated Day 1 PDF.

## Validation performed for this branch

| Check | Result / boundary |
|---|---|
| Azure tracker solution build | Passed; initial Functions extension restore emitted warning AZFW0108, not a build error |
| Azure tracker offline checks | Passed all ten outcomes and existing upload/retry/failure checks; no Azure connection used |
| Original local tracker checks | Passed all ten sample outcomes, validation and worker shutdown |
| AppServiceSqlSample build | Passed, zero warnings/errors |
| Standalone HelloAzureFunction build | Passed, zero warnings/errors |
| Dummy website build and existing local HTTP fixture checks | Passed index and all three fixture records; no cloud endpoint or browser-button check |
| Updated Day 2 PowerPoints | Opened using installed PowerPoint; 42 slides each; no detected vertical text overflow |
| Updated Day 2 PDFs | Regenerated from their corresponding PowerPoints |
| Release hygiene | Office ZIP integrity, relative Markdown links/code fences and credential-pattern/file-exclusion checks performed before upload |

The first attempt to run the publish-basics test omitted its mandatory WebsiteUrl
argument; it was rerun successfully against a responsive, temporary local host.
Only that test-owned process was stopped afterward.

## Evidence and limits

- No new live Azure acceptance run, AI analysis, guest invitation, role assignment,
  resource creation or deployment was performed for this publication.
- Latest supplied cloud evidence is four sample-01 Completed records around
  22:05 UTC after the owner enabled SQL's Azure-services exception.
- A new full ten-case cloud regression after that change and old-job DLQ recovery
  remain follow-up work. Do not claim an empty dead-letter queue.
- Document Intelligence is a separate approved-resource practical. The tracker
  uses deterministic format checks; AI integration is not implemented.
- Attendee permissions, resource availability, AI model availability and tenant
  policies must be checked by the instructor before live delivery.
- Existing local UI authentication/hosting limitations remain intentional.
- No exhaustive security assessment, performance certification or production
  readiness certification is claimed.

## Exclusions

No archives, previous deployment packages, private local settings, local logs,
rehearsal records, generated bin/obj trees or unrelated instructor source drafts
are added to V3. V2 is preserved rather than rewritten. Refer to the current
V3 README instead of older schedules or handbooks.
