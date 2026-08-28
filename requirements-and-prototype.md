# Lost-and-Found Matching System: Requirements and Prototype Scope

## Purpose

The system is intended to provide a centralised workflow for students to report lost property, staff to register found property, and authorised staff to review possible matches and ownership claims.

## Functional requirements

- **FR-01:** A student can submit a lost-item report containing the item name, category, description, date lost, location, distinguishing features, and an optional image.
- **FR-02:** An authorised staff member can register a found item containing its category, description, date found, location, storage location, and an optional image.
- **FR-03:** The system rejects a report when a mandatory field is empty or invalid.
- **FR-04:** The system ranks possible matches using category, description keywords, location, and date information.
- **FR-05:** An authorised staff member can approve or reject a possible match.
- **FR-06:** Each report has a status: Submitted, Under Review, Matched, Claimed, Returned, or Closed.
- **FR-07:** An ownership claim records the claimant, supporting evidence, review decision, and review time.
- **FR-08:** Important status changes are stored in an audit history.
- **FR-09:** Users can search and filter reports by category, status, date, and location.
- **FR-10:** Role-based access prevents students from using staff-only review functions.

## Non-functional requirements

- **Usability:** Forms use clear labels and show validation messages beside invalid fields.
- **Performance:** Search and matching results should appear within two seconds for up to 1,000 active reports on a supported device.
- **Reliability:** Valid data remains available after the application is restarted, and failed submissions do not create partial records.
- **Security and privacy:** Claim evidence and private claimant information are visible only to authorised staff.
- **Maintainability:** Views, page models, domain models, and persistence logic remain separated.
- **Portability:** The first supported prototype platform is Windows, while shared .NET MAUI code supports later mobile builds.
- **Accessibility:** Interactive controls have meaningful labels, readable text, and sufficient contrast.

## Acceptance criteria

### Lost-item submission

1. A valid form creates exactly one stored report with a unique identifier.
2. A new report receives the Submitted status.
3. An invalid form is not stored and identifies every missing mandatory field.

### Matching

1. Only active found-item reports are considered.
2. Each result includes a score and an explanation.
3. A report with matching category, location, date, and keywords ranks above an unrelated report.

### Staff review

1. An authorised staff user can approve or reject a suggested match.
2. The decision updates the match and report status.
3. The decision is recorded in status history.
4. A student user cannot perform the review action.

## Initial prototype scope

The application uses .NET MAUI, XAML, MVVM, and SQLite. The current repository provides a technical scaffold with navigation, local persistence, dependency injection, reusable controls, error handling, and light/dark themes. Core lost-and-found domain models have been introduced for items, users, match results, ownership claims, and status history.

The current interface and repositories still contain parts of the original project-and-task sample. Lost-item submission, found-item registration, matching, claim review, and role-based access therefore remain planned prototype functions until their pages, repositories, and workflow logic are implemented.

## Out of scope for the initial prototype

- Live university authentication integration
- Trained AI image-recognition models
- GPS tracking
- Production cloud deployment

These features require external services, training data, institutional approval, or additional security work. The initial prototype prioritises a complete and testable core workflow.
