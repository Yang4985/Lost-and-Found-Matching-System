> Historical baseline documentation. For the current repair and retest status, see [fix-verification.md](fix-verification.md). Baseline failures remain preserved as before-fix evidence.

# Requirements traceability and remaining verification

Source: existing requirements-and-prototype.md. IDs retain the team's FR numbering. These are partial verification links, not claims of final compliance.

| Requirement | Implemented path / test | Current verification |
|---|---|---|
| FR01 lost report | LostReportPage -> AddLostReport; TC12, TC18 | Service add passes; persistence fails D003; UI and optional image not verified |
| FR02 found report | FoundReportPage -> AddFoundReport; TC13 | Service add passes; durable storage and authorised registration unverified |
| FR03 validation | Form handlers; TC02/06/07 are matching guards only | Required-field UI validation NOT verified by these guards |
| FR04 matching | Both matching services; TC01-05,08-11,14-17 | New scorer/unit boundaries pass; UI consistency fails D001; active eligibility fails D002 |
| FR05 approve/reject | No executed tests | Unverified |
| FR06 report status | TC11-13,15,17 | Initial status/Closed exclusion pass; Returned exclusion fails; transitions unverified |
| FR07 ownership claims | No executed tests | Unverified |
| FR08 audit history | No executed tests | Unverified |
| FR09 search/filter | No executed tests | Unverified; match ordering is not search/filter verification |
| FR10 role-based access | No executed tests | Unverified; also record as security NFR following mid-project feedback |
| Reliability NFR | TC15,18 | Repeat calculation stable; persistence failure D003 |
| Performance NFR | Planned Q01 | Not run |
| Security/privacy NFR | Planned Q02 | Not run |
| Usability/accessibility NFR | Planned Q03 | Not run |
| Compatibility NFR | Planned Q04 | Not run |

## Planned quality tests beyond this automated suite

Q01 Performance: supported Windows device, Release build, record CPU/RAM/runtime and data volume. Seed 1,000 active found reports, search/match one lost report, warm up and record 20 measured runs with p50/p95/max. Candidate acceptance target: results within the existing 2-second NFR; clarify whether UI rendering is included. Preserve raw timings. All-pairs batch matching is a different workload and needs a separate threshold.

Q02 Security/privacy: student and staff identities; attempt staff review through both navigation and service calls; verify denied operations leave data unchanged and private claim evidence inaccessible. Include SQLite dependency audit. Preconditions: real role enforcement and isolated test users. Not run; no security assurance claimed.

Q03 Accessibility/usability: keyboard-only completion of lost/found forms; focus order, labels, validation visibility and large-text layout. Use explicit device/display settings; document screenshots and observer results. Not run.

Q04 Compatibility/smoke: start Windows MAUI application, open three report/match pages, submit valid and invalid forms, refresh matches and restart. Check optional-image behaviour and unexplained empty Entry. Record OS, SDK/workloads and exact commit. Not run because MAUI workload is absent locally.

These four planned scenarios are not included in the 34-instance automated pass rate. At least two relevant quality areas must actually be executed before the final report can claim this course requirement is met.
