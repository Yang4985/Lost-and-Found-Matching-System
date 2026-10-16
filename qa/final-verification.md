Current final workflow and 100-case verification: [completed-workflow.md](completed-workflow.md). The following preserves the earlier 76-case validation milestone.

# Aggregated report validation and final verification

Verification owner: Zelong Liang. Local verification: 16 October 2026 (Pacific/Auckland).
Base: main `b7ec393f16c537a4a936fc29f42b0afb94b80ae4`.

## User-visible improvement

Lost, staff-found and public-found reporting now display every applicable mandatory-field error in one submission instead of a generic prompt or the first error only. Missing descriptions and missing staff storage locations are validated centrally as well as name, category, location and future dates. Optional brand/colour remain optional; public found reports keep the existing `Pending staff intake` value.

`ValidationService.GetLostReportErrors` / `GetFoundReportErrors` collect messages. Existing boolean/out-message methods remain compatible for a single error and join multiple errors with newlines. All three handlers construct reports safely when controls are empty, then call `PrototypeDataService.TrySubmitLostReport` / `TrySubmitFoundReport`. Invalid submissions return before assigning identity, mutating collections or saving. Inputs remain available for correction; existing I/O error handling keeps the form on write failure.

Low-level `AddLostReport` / `AddFoundReport` remain for controlled insertion and existing fixtures. They are not a validated public form submission API; new form callers must use TrySubmit. This change does not implement authentication or prevent every caller from bypassing validation.

## Results

| Evidence | Executed | Passed | Failed | Skipped |
|---|---:|---:|---:|---:|
| Main before this feature | 58 | 58 | 0 | 0 |
| Full regression after feature | 76 | 76 | 0 | 0 |

Fourteen new test methods expand to 18 instances through null/empty/whitespace data rows. They exercise all-field error aggregation, compatible messages, missing description/storage, null reports, unchanged file/ID state on invalid submission, corrected submission and reload, optional fields, public intake, fresh validation state and blocked-path write failure. The earlier 58 tests remain in the full run.

- Core Release build with warnings as errors: zero warnings/errors.
- Windows MAUI Debug build, net10.0-windows10.0.19041.0 / win-x64: zero warnings/errors. Official NuGet restored runtime packs absent from the offline cache.
- Matching benchmark: 1 lost / 1,000 found, 5 warmups, 20 measured calls; p50 0.3807 ms, p95 1.7341 ms, max 4.7179 ms, 167 candidates. Service-only; not a GUI latency result.
- Production-source linking in QA.Performance now includes ValidationService because PrototypeDataService uses it. The initial benchmark compile exposed that missing link; it was corrected and the benchmark passed.

Raw evidence is under `evidence/final-verification/`: pre-change baseline and final regression TRX, core/Windows build logs, performance.json and summary.json. Older 34-instance baseline and 46-instance repair evidence remain untouched.

## Requirement links and limits

FR-03's every-missing-field criterion now has service-level regression evidence and all three page handlers use that boundary. Real form rendering, keyboard/screen-reader behaviour and end-to-end GUI interaction were not executed during this verification. Current traceability is in [current-traceability.md](current-traceability.md).

D001-D003 remain protected by earlier tests. Ownership decisions, audit history, shared multiuser storage, authorisation and comprehensive filter acceptance remain outside this feature. Native compilation and 76 passing tests do not establish production approval. Review/merge and remote CI are tracked separately from these local results.

## Reproduce

```powershell
dotnet build QA.Tests/QA.Tests.csproj -c Release -warnaserror
dotnet test QA.Tests/QA.Tests.csproj -c Release --no-build --logger "trx;LogFileName=validation-regression.trx" --results-directory TestResults
dotnet run --project QA.Performance/QA.Performance.csproj -c Release -- TestResults/performance.json
dotnet build LostAndFound/LostAndFound.csproj -f net10.0-windows10.0.19041.0 -c Debug -r win-x64
```
