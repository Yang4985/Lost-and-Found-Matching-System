# D001–D003 correction and verification — 7 October 2026

## Evidence and release decision

Original local baseline: 34 executed, 31 passed, 3 failed, 0 skipped. Original GitHub run 37571171796 also reported 31/34; preserved baseline TRX is unchanged.

Current local Release build: 0 warnings, 0 errors. Retest: 46 executed, 46 passed, 0 failed, 0 skipped (24 named cases with data-driven expansions). Evidence: `evidence/regression/regression.trx`. This verifies the linked production services and models, not rendered MAUI controls. CI results must be reported separately after the pushed commit is executed.

Release decision: suitable for service-level integration review; NOT yet ready for a campus production release. No real authentication/authorisation, staff claims workflow, audit trail, multiuser database, UI accessibility or device compatibility verification is established by these tests. MAUI workloads are not installed on this verification machine. XML parsing passed for the changed forms but is not a MAUI compilation or UI execution.

## Corrections and root cause outcome

| Defect | Correction | Automated verification | Status / remaining validation |
|---|---|---|---|
| D001 independent scoring algorithms | GenerateMatches delegates to the existing MatchingService; preserves its 30/15/15/15/15/10 weights and category gate. Explanations come from that same component. Optional brand/colour form fields expose inputs previously absent. Display uses score/100, not probability. | TC16 exclusion; TC20 actual UI data service versus core score across date boundaries; TC14 ranking/explanation | Fixed at service level; manual page integration verification pending |
| D002 filtering only Closed | Shared active-status allowlist: Submitted, Available, UnderReview. Matched, Claimed, Returned and Closed are excluded on both sides. | TC17; TC19 all seven statuses on both report types; TC21 returned status survives reload | Fixed at service level; full status transition/claim workflow still outside this patch |
| D003 collections only, no persistence | App singleton explicitly opens lost-found-reports.json in FileSystem.AppDataDirectory. Saved records are loaded; next IDs derive from saved maxima; all Item fields are serializable. Writes use flushed temporary file plus same-directory replacement; insertion is rolled back when saving fails. Fake seed records removed. Form retains input on save error. | TC18 same-file recreation; TC21 complete serialized record equality, both report types, next IDs and a second restart; TC22 corrupt file preservation; TC23 write failure; TC24 empty initial store | Fixed for single-process local storage; manual close/reopen of MAUI app pending |

The user authorised Zelong to take over these repairs. Repository commits attribute this work to his configured identity. Team acceptance of the consolidated scoring/status specification should be recorded during integration review.

## Scope and architecture change

This is a local JSON snapshot repository for the report workflow, not SQLite integration. Existing SQLite packages/sample repositories were unrelated to these reports. No old persistent report data existed to migrate. The application path is supplied explicitly; the parameterless service is intentionally isolated in-memory mode for tests and the benchmark. Future changes to existing records must call Save; the current UI only inserts records. One process owns the file: there is no cross-process concurrency, transaction database, encryption or shared server. Corrupt JSON raises an error instead of overwriting evidence; a user-facing recovery flow remains future work. Preserve the same approved lost-property problem and document this storage choice in the final report.

## Added execution cases

| ID | Scenario / expected result | Outcome |
|---|---|---|
| TC19 | Every status, both sides; only three active states yield candidates | 7 data rows passed, each checking both sides |
| TC20 | Date-boundary and missing-brand results match production scorer; score label accurate | Passed |
| TC21 | Save both types, all fields, terminal status; reload twice and preserve increasing IDs | Passed |
| TC22 | Invalid JSON fails without destroying file | Passed |
| TC23 | Blocked parent path fails save without retaining unsaved lost/found entries | Passed |
| TC24 | New store contains no demonstration data | Passed |

TC18 now exercises an isolated real file and opens that same file on recreation. Its persistence expectation remains unchanged. None of the original defect assertions were skipped, removed or inverted. Test data uses unique temporary paths; each test cleans up its own file.

## Performance retest

One lost / 1000 found reports, 5 warmups and 20 measured service calls: p50 0.417 ms, p95 1.169 ms, max 5.083 ms; 167 candidates. See regression/performance.json for every sample and runtime metadata. Baseline yielded 700 candidates; current category exclusion yields 167. Timings are not directly equivalent because the corrected candidate set differs. These service-only results do not verify the UI's end-to-end 2-second requirement or matching accuracy.

## Manual demonstration checklist (not yet executed)

1. Build/run MAUI on a supported device with the requisite workloads.
2. Register matching Electronics items with brand Samsung, colour Black, same location/date and description. Show a 100/100 candidate with explained weights.
3. Register a Wallet with otherwise matching fields; verify it is excluded from the Electronics match.
4. Close/reopen the application; verify both saved lists and their fields remain. Add another report and verify IDs are distinct.
5. Verify save-error handling retains the form; evaluate usability, keyboard access, scaling and privacy separately.
6. Capture screenshots and the exact commit/device; retain GitHub regression artifacts and compare to the original three failures.
