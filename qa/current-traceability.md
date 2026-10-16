# Current requirement verification

Base: main b7ec393 plus the aggregated submission-validation feature. This matrix describes evidence boundaries, not full system acceptance. Historical before-fix traceability remains in traceability.md.

| Requirement | Current implementation and executable evidence | Remaining acceptance |
|---|---|---|
| FR01 Lost reporting | LostReportPage -> TrySubmitLostReport; TC12/18/21/23; invalid/corrected submission tests | Optional image and actual GUI submission acceptance |
| FR02 Found reporting | Staff/public pages -> TrySubmitFoundReport; TC13/21/23; intake/persistence tests | Staff permissions and optional image acceptance |
| FR03 Reject invalid input and identify all missing fields | Aggregated validation; all three handlers; SubmissionValidationTests plus eight original validation tests | Rendered message visibility and field-level accessibility still need GUI checks |
| FR04 Rank matches | Shared MatchingService; TC01-10/14-16/20 | GUI acceptance and labelled real-world usefulness |
| FR05 Approve/reject match | No complete decision test linked | Unverified |
| FR06 Status | Active allowlist, returned update; TC11/17/19/21 and ReturnedItemTests | Full authorised transition/decision workflow |
| FR07 Ownership evidence | Model exists | Complete claim workflow and privacy unverified |
| FR08 Audit history | Returned timestamp is stored | Full actor/decision audit unverified |
| FR09 Search/filter | Ranking is not complete filter evidence | Category/status/date/location filter acceptance unverified |
| FR10 Staff access | Demo session and page guards | Service-level permissions unverified |
| Reliability | Real-file reload, corrupt-file and failure rollback; invalid submissions leave file/IDs unchanged | App restart, crash recovery and shared storage |
| Performance | New raw 1/1000 service benchmark, 20 calls | End-to-end supported-device two-second target |
| Usability | Complete field-specific message list and retained input | Real user interaction |
| Accessibility | Text-based error list | Keyboard, focus, screen reader, contrast and scaling |
| Maintainability | Shared validator replaces duplicated page guards; backward-compatible wrapper; source-link fix | Further shared-library extraction |
| Portability | Windows app compiles locally; core suite is portable | Device and other platform acceptance |
| Security/privacy | No new identity or permission mechanism | Real authorisation and private claim-evidence protection |

All 76 current automated instances passed locally. Windows Debug compilation passed. This is not a production release recommendation. See final-verification.md for exact evidence and limits.
