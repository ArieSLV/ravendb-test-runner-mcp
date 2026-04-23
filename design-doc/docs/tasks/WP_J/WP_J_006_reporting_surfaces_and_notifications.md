# WP J 006 reporting surfaces and notifications

## Task ID
`WP_J_006_reporting_surfaces_and_notifications`

## Title
Implement flaky reporting surfaces for MCP, web API, and browser UI.

## Purpose
Deliver one bounded step of RavenDB Test Runner MCP Server without changing frozen architecture implicitly.

## Scope
- implement the task-specific capability described by the title
- update only the contracts/docs/modules required by this task
- preserve the naming and build-subsystem invariants

## Out of scope
- unrelated refactors
- opportunistic architecture changes without ADR
- undocumented contract drift

## Prerequisites
- Phase 0 contract freeze approved
- Attempt and result contracts approved

## Touched modules/files
- RavenDB.TestRunner.McpServer.Flaky

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/ERROR_TAXONOMY.md
- docs/contracts/EVENT_MODEL.md

## Expected outputs
- Implement flaky reporting surfaces for MCP, web API, and browser UI.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Do not classify deterministic build/license/platform failures as flaky.

## Validation steps
- iterative-run tests
- flaky scoring tests
- quarantine audit tests

## Definition of done
- the task output exists in the declared module(s)
- the relevant contract references remain accurate
- validation steps were executed or explicitly blocked with reasons
- handoff note completed using `HANDOFF_TEMPLATE.md`

## Handoff expectations
- summarize exactly what changed
- mention any contract/doc updates
- mention risks and follow-ups
- mention any ADR impact
