# Completed Windows prototype verification

The final workflow builds on the integrated main revision and the 18-instance aggregated-validation addition. The full suite now executes 100 instances, all passing, with zero skips. Core Release and Windows MAUI Debug builds complete with zero warnings and errors.

## Implemented workflow

Staff approves or rejects a compatible suggested pair with a reason. Approval reserves both reports as Matched. Staff records private claimant evidence, approves or rejects ownership, records handover and closes the reports. Review, claim, return and closure changes persist with actor, reason and UTC history in the same atomic snapshot. A failed write restores prior states and collections. Duplicate actions and handover before approved ownership are rejected. Rejected matches do not reappear. Private queries and staff changes require the demo staff session. Public listing records omit claim and claimant information.

Reporting forms also show each mandatory-field error beside its input, retain the aggregate message, and preserve input for correction. Photo validation errors are caught and shown without leaving the form.

Native search combines name, category, status, inclusive dates and location. Optional PNG/JPEG images are copied under unique names, limited to five MB and retained by path in item storage. Existing snapshots remain compatible.

## Verification

`WorkflowAcceptanceTests` covers the complete persisted flow, permissions and private queries, rejected pairs, exclusive assignment, mandatory reasons/evidence, duplicate claims/decisions/returns, invalid IDs, legacy stores, combined filters, date ranges, failed review/claim/handover writes and public intake identity normalisation. `PhotoAndStaffBoundaryTests` covers copied photo bytes, safe filenames, stored image paths, invalid/oversized data and staff-only registration.

Evidence: `evidence/completed-workflow/workflow-final.trx`, `core-build.log`, `windows-build.log`, `performance.json`, `verification.json`. The 100 instances include the earlier 76 plus 24 new workflow/photo/boundary instances. The final service benchmark records one lost and 1,000 found reports, five warmups and twenty measurements. Its timing is service-only.

Native observations cover launch, search navigation, existing records, an unmatched-query response and retained category totals after close/relaunch. Staff workflow is executed through production-service acceptance tests, not through a native login session. Screen-reader/scaling evaluations and institutional deployment are separate future evaluations. The supported delivery is a local course prototype with the existing demonstration staff account.

## Run

```powershell
dotnet test QA.Tests/QA.Tests.csproj -c Release -warnaserror --logger trx
dotnet build LostAndFound/LostAndFound.csproj -f net10.0-windows10.0.19041.0 -c Debug -r win-x64 -warnaserror
dotnet run --project QA.Performance/QA.Performance.csproj -c Release -- TestResults/performance.json
```

The existing demo account is STAFF001 / Campus123!. Staff Dashboard opens Review matches and ownership claims. Search Reports is in the public navigation menu. Optional `LOSTFOUND_STORE` selects a separate local snapshot for controlled test launches. Runtime files and photos are not committed.
