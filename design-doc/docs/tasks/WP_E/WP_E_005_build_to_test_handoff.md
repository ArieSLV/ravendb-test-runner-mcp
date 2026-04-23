# WP E 005 build to test handoff

## Task ID
`WP_E_005_build_to_test_handoff`

## Title
Implement explicit build-to-test handoff so test execution never performs chaotic hidden rebuilds.

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
- WP_D_003_build_fingerprint_and_reuse_engine

## Touched modules/files
- RavenDB.TestRunner.McpServer.TestExecution
- RavenDB.TestRunner.McpServer.Build

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/BUILD_SUBSYSTEM.md
- docs/contracts/ERROR_TAXONOMY.md

## Expected outputs
- Implement explicit build-to-test handoff so test execution never performs chaotic hidden rebuilds.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- plan synthesis tests
- build-to-test handshake tests
- run scheduler tests

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
