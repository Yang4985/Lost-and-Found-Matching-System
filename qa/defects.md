# Defect register and root cause analysis

All defects below are OPEN at the baseline commit. Discovery: local automated service tests on 7 October 2026. Owner assignments are proposed for team confirmation, not completed assignments.

Severity: Critical = data exposure or catastrophic loss; High = core workflow unreliable; Medium = limited function or usability problem; Low = cosmetic. Priority P1 = before release; P2 = next iteration; P3 = backlog. Lifecycle: New -> Triaged -> In progress -> Fixed pending verification -> Closed; failed retest -> Reopened. Evidence, reproduction, fix commit and independent retest must accompany closure.

## D001 Two matching algorithms produce inconsistent recommendations

- Severity High; priority P1; proposed owner matching developer; verification Member 3.
- Reproduce: TC16 creates Electronics and Wallet reports at Library on 2026-10-01, with otherwise matching fields. Call both MatchingService.CalculateMatchScore and PrototypeDataService.GenerateMatches.
- Expected: the UI respects the category exclusion explicitly specified by MatchingService; no cross-category candidate.
- Actual: the new service returns 0 but the UI service returns one candidate. See failed TC16 in TRX.
- Root cause: MatchesPage calls PrototypeDataService.GenerateMatches, which independently awards category/location/keyword/date points. The new MatchingService category gate is not used by the page. The existing NUnit test only tests the new service, leaving integration unverified.
- Correction: agree the authoritative business rule and route the page through one shared scoring component; retain explanations. If category exclusion is intentionally rejected, document the approved requirement change rather than silently weakening this test.
- Prevention: page-service integration tests; one scoring specification; code review of call sites when adding services.
- Verification: TC16 passes plus manual UI test using the same data; confirm ranking and explanations for valid categories. Fix commit and retest: not yet available.

## D002 Returned found items remain matching candidates

- Severity High; priority P1; proposed owner matching/workflow developer; verification Member 3.
- Reproduce: TC17 adds matching reports then sets the found report status to Returned; generate matches.
- Expected: returned property is no longer available for matching. This operational interpretation of active reports should be confirmed in the final status-transition specification.
- Actual: one candidate remains.
- Root cause: both loops filter only Closed. There is no shared eligibility policy separating terminal and active states.
- Correction: define eligible statuses and transitions with the team, apply the same policy to both report types and queries.
- Prevention: table-driven tests for every state; transition acceptance criteria and audit assertions.
- Verification: TC17 passes; extend regression tests to Returned/Closed lost reports and relevant Claimed cases after policy confirmation. Fix commit/retest: pending.

## D003 Lost/found workflow has no persistent storage

- Severity High; priority P1; proposed owner Yang/database; verification Member 3.
- Reproduce: TC18 inserts a report named QA-PERSISTENCE-UNIQUE-2026 into an empty PrototypeDataService, creates a new service and searches for that name.
- Expected: saved reports survive recreation/restart, as specified by the reliability NFR.
- Actual: the report is absent; the new instance contains seed data only.
- Root cause: the service writes only ObservableCollection instances and reconstructs seed records in its constructor. Existing SQLite dependencies and unrelated sample repositories are not connected to this report workflow.
- Correction: inject a persistent report repository, perform durable writes before reporting success, reload records on startup and make seed data explicit/test-only.
- Prevention: isolated temporary-database tests for create/read/restart, failed writes and duplicate submissions.
- Verification: replace the baseline recreation check with a real repository recreation test using the same database file; separately relaunch the app and capture evidence. Fix commit/retest: pending.

## Additional observations not counted as executed defects

- LostReportPage.xaml has an unused extra Entry in PR #1; inspect visually and remove if accidental.
- Original NUnit UnitTest1 always passes and provides no behavioural evidence.
- Form code hardcodes user IDs; authentication/authorisation has not been validated.
- SQLite security remediation is not verified by this suite. Do not state that the advisory is resolved based only on a package comment.
