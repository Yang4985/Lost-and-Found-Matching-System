> Historical baseline documentation. For the current repair and retest status, see [fix-verification.md](fix-verification.md). Baseline failures remain preserved as before-fix evidence.

# Q01 Service performance baseline

Measured against the production PrototypeDataService used by MatchesPage, on the QA baseline descended from commit 27362e4. Full per-run timings and runtime metadata: evidence/performance.json. Harness: QA.Performance/Program.cs. AI assistance: OpenAI Codex; no Copilot usage claimed.

## Method

Release build with .NET SDK 10.0.302. Windows machine, AMD Ryzen 7 7840H. Runtime/OS/process architecture and logical processor count are recorded in the JSON. No dedicated machine isolation or memory profiling was performed.

Seed one lost report and 1,000 found reports in memory using deterministic index-based values. Six categories, two locations, two descriptions and ten dates are represented. All reports have Submitted status. Five warm-up calls precede twenty individually measured calls. Stopwatch includes scoring, explanation construction and materialising the sorted results. Fixture preparation, console output and post-call validation are outside the timed section. No forced garbage collection or outlier removal. Percentiles use the nearest-rank method; all twenty raw samples are retained.

Each run returns 700 candidates. Checks after timing confirm a stable candidate count, scores in 0-100 and descending order. These sanity checks do not establish that recommendations are semantically correct: D001 and D002 remain open.

## Results

| Measure | Milliseconds |
|---|---:|
| p50 | 2.827 |
| p95 | 4.492 |
| Maximum | 22.924 |

All twenty service calls were below the 2,000 ms reference threshold taken from the existing performance NFR. This is a service baseline, not end-to-end NFR acceptance: XAML rendering, database access, device startup, image handling and UI responsiveness are not measured. No portability or load-concurrency conclusion is justified.

The maximum is reported without removal; JIT/tiering, scheduling or GC could contribute, but no cause was profiled. Re-run after scoring and persistence changes and compare with this baseline. Complete a supported-device UI response test before marking the performance NFR verified.

## Reproduce

From the repository root:

```text
dotnet run --project QA.Performance/QA.Performance.csproj -c Release
```

The program writes qa/evidence/performance.json; preserve the earlier artifact before re-running if comparing revisions. An optional first argument chooses another output path.
