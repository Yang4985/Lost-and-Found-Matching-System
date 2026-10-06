# Member 3 QA baseline

This is a development baseline, not a final submission or release approval.
Baseline: PR #1 commit `0af3c51564bc909b3220fcc17e0c877c1e06826f`.
Local working branch: `qa/automation-baseline`. Work prepared on 7 October 2026 (Pacific/Auckland).

## Run

Install .NET 10 SDK, then from the repository root:

```text
dotnet restore QA.Tests/QA.Tests.csproj
dotnet build QA.Tests/QA.Tests.csproj -c Release --no-restore -warnaserror
dotnet test QA.Tests/QA.Tests.csproj -c Release --no-build --logger "trx;LogFileName=core-qa.trx" --results-directory qa/evidence
```

The all-tests command currently returns exit code 1 because three real defects remain open.
Do not remove these tests, change their assertions to match faulty behaviour, or exclude them from the release gate.
`OpenDefect` is a diagnostic category only; CI runs every test.

## Test architecture and scope

QA.Tests is a separate, portable MSTest project. It compiles six production source files by reference, without copying/reimplementing their algorithms. It checks the PR's new MatchingService AND the PrototypeDataService used by MatchesPage. This makes core behaviour testable without installing MAUI platform workloads. Source linking is a temporary seam; the longer-term design should extract shared production logic into a class library used by both the app and tests.

The original NUnit project is unchanged. It remains a separate suite requiring application dependencies; its placeholder Assert.Pass is not included in our metrics. Choosing a separate MSTest harness uses the available local toolchain without rewriting Brighton's tests. The team should consolidate frameworks when extracting the shared library.

These results do NOT establish that XAML bindings, Windows packaging, MAUI startup, SQLite native binaries, Android/iOS builds, permissions or real UI workflows work. Service recreation in TC18 is evidence of absent service persistence, not an actual desktop restart test.

## Evidence and metrics

18 named scenarios expand into 34 executed instances using data rows. Planned/executed: 34/34; passed: 31; failed: 3; blocked: 0; not run: 0 within this automated suite. Instance pass rate: 91.18%. Scenario-level result: 15 pass and 3 fail (83.33%). Do not confuse either percentage with code coverage or overall requirements coverage.

`evidence/core-qa.trx` is actual local runner output. `evidence/results.md` is derived from that file. `test-cases.md` records scenario details, inputs, priorities, steps and results. Runtime: Windows, .NET SDK 10.0.302, Release, MSTest 4.0.2, Test SDK 18.0.1. Dependencies were restored from a local cache with NuGet audit disabled for that offline restore; no vulnerability-free claim is made.

## CI configuration

`.github/workflows/qa.yml` restores, builds with warnings as errors, runs all 34 instances and uploads TRX results even on failure. It has read-only repository permission and a 10-minute timeout. The configuration is prepared locally; no GitHub Actions run has occurred for this work yet. Its expected current outcome is FAILED due to D001-D003. No branch protection rule has been installed. The workflow verifies core logic, not a MAUI build.

Release recommendation: NOT READY, because matching behaviour is inconsistent and persistence is absent. Final approval also requires UI acceptance, at least two quality areas with real evidence, dependency/security checks and team review.

## Integration handoff

1. Ask the matching owner to agree one scoring specification, integrate it into the UI path, and fix D001/D002. Preserve explainable scores. Review missing brand/colour inputs before wiring the new service directly into the forms.
2. Ask the database owner to implement persistence for lost/found records and supply an isolated database setup so TC18 can become a real storage integration test.
3. Re-run this suite after each correction; record the fix commit and both failing and passing evidence. Do not mark a defect closed until a reviewer verifies the correction.
4. Run Windows MAUI smoke tests and save screenshots. Verify SQLite advisory/package remediation independently, including package restore and vulnerability scanning.
5. Push the reviewed branch and execute CI. A follow-up QA PR can target Brighton's branch, or be rebased after PR #1 is reviewed and merged. Do not merge PR #1 solely because this harness builds.

## Authorship and evidence

OpenAI Codex prepared this local QA baseline. Zelong must review, understand and run the work before describing it as his contribution. No GitHub Copilot usage, team review, remote commit, remote CI result or historical weekly contribution is claimed. Record real Copilot suggestions and human decisions during subsequent work if required by the course. Never backdate contributions.
