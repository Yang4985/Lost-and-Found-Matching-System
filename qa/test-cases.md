> Historical baseline documentation. For the current repair and retest status, see [fix-verification.md](fix-verification.md). Baseline failures remain preserved as before-fix evidence.

# Executable test catalogue

Common preconditions: .NET 10, restored QA.Tests dependencies, isolated new objects per case. Run command is in qa/README.md. Default fixture: Electronics, Samsung, Galaxy, black cracked screen, Black, scratch; Library; 2026-10-01. PrototypeDataService seed collections are cleared for isolation. Tests do not execute XAML/UI actions.

Each TC ID maps to the identically prefixed method in QA.Tests/MatchingTests.cs. Data rows account for the difference between 18 scenarios and 34 instances. Boundary tests capture current scoring rules; these rules still need explicit team approval as final acceptance criteria.

## TC01 Full match

- Requirement or defect: FR04
- Priority: P1
- Test data: Default lost/found fixtures
- Steps: Calculate score once
- Expected result: 100
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC02 Different or missing category

- Requirement or defect: FR04
- Priority: P1
- Test data: Found category Wallet, empty, whitespace
- Steps: Calculate score for each value
- Expected result: 0 in all three cases
- Actual execution: Passed; 3 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC03 Case and whitespace

- Requirement or defect: FR04
- Priority: P2
- Test data: Lowercase/spaced category, brand, colour and location
- Steps: Calculate score against original fixture
- Expected result: 100
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC04 Date boundary

- Requirement or defect: FR04
- Priority: P1
- Test data: Found date offsets -2,-1,0,1,2 days
- Steps: Calculate score for each offset
- Expected result: 85,100,100,100,85; captures current symmetric date rule
- Actual execution: Passed; 5 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC05 Recommendation boundaries

- Requirement or defect: FR04
- Priority: P1
- Test data: Scores 0,39,40,59,60,79,80,100
- Steps: Call GetMatchLevel for each score
- Expected result: Not Recommended,Not Recommended,Low,Low,Medium,Medium,High,High
- Actual execution: Passed; 8 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC06 Null reports

- Requirement or defect: FR04
- Priority: P2
- Test data: Lost null then found null
- Steps: Call CalculateMatchScore
- Expected result: 0 without exception
- Actual execution: Passed; 2 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC07 Null nested items

- Requirement or defect: FR04
- Priority: P2
- Test data: Lost.Item null then Found.Item null
- Steps: Call CalculateMatchScore
- Expected result: 0 without exception
- Actual execution: Passed; 2 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC08 Empty descriptions

- Requirement or defect: FR04
- Priority: P2
- Test data: Both descriptions empty
- Steps: Calculate score
- Expected result: 90; no keyword bonus
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC09 Repeated keywords

- Requirement or defect: FR04
- Priority: P2
- Test data: Lost description black black black screen screen
- Steps: Calculate score
- Expected result: 100; keyword bonus not multiplied
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC10 Empty collections

- Requirement or defect: FR04
- Priority: P2
- Test data: Clear both seeded collections
- Steps: Generate UI-service matches
- Expected result: 0 candidates
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC11 Closed status

- Requirement or defect: FR04 FR06
- Priority: P1
- Test data: Close lost, then in separate instance close found
- Steps: Generate UI-service matches
- Expected result: 0 candidates in both cases
- Actual execution: Passed; 2 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC12 Lost record insertion

- Requirement or defect: FR01 FR06
- Priority: P1
- Test data: Two fresh lost reports
- Steps: Add both to empty service
- Expected result: Distinct IDs, two stored in memory, Submitted status
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC13 Found record insertion

- Requirement or defect: FR02 FR06
- Priority: P1
- Test data: Two fresh found reports
- Steps: Add both to empty service
- Expected result: Distinct IDs, two stored in memory, Submitted status
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC14 Sorting and explanation

- Requirement or defect: FR04
- Priority: P1
- Test data: One full and one weaker found report (Gym/unrelated/Different)
- Steps: Generate UI-service matches
- Expected result: Two results, strong first, nonempty explanation
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC15 Repeatability

- Requirement or defect: Reliability FR06
- Priority: P2
- Test data: One valid report pair
- Steps: Generate matches twice
- Expected result: Same score; both reports remain Submitted; counts remain one
- Actual execution: Passed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC16 UI and core agreement

- Requirement or defect: FR04 D001
- Priority: P1
- Test data: Full fixture except found category Wallet
- Steps: Check new service score; generate UI-service candidates
- Expected result: New score 0 and no UI candidates; ACTUAL one UI candidate
- Actual execution: Failed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC17 Returned item exclusion

- Requirement or defect: FR04 FR06 D002
- Priority: P1
- Test data: Valid pair; found status Returned after insertion
- Steps: Generate UI-service matches
- Expected result: No candidates; ACTUAL one candidate
- Actual execution: Failed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.

## TC18 Persistence on service recreation

- Requirement or defect: Reliability D003
- Priority: P1
- Test data: Lost name QA-PERSISTENCE-UNIQUE-2026
- Steps: Insert into empty service; create new service; query name
- Expected result: Name retained; ACTUAL absent
- Actual execution: Failed; 1 instance(s). Exact assertion failures and durations are in evidence/core-qa.trx.
