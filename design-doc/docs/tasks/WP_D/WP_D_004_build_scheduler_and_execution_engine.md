# WP D 004 build scheduler and execution engine

## Task ID
`WP_D_004_build_scheduler_and_execution_engine`

## Title
Implement build scheduler, restore/build/clean/rebuild orchestration, and process supervision.

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
- WP_B_001_embedded_bootstrap_and_database_init
- WP_C_001_workspace_and_repo_line_detection

## Touched modules/files
- RavenDB.TestRunner.McpServer.Build
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/BUILD_SUBSYSTEM.md
- docs/contracts/STORAGE_MODEL.md
- docs/contracts/MCP_TOOLS.md
- docs/contracts/WEB_API.md

## Expected outputs
- Implement build scheduler, restore/build/clean/rebuild orchestration, and process supervision.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Do not let test execution take ownership of build orchestration.
- Persist explicit build reuse decisions and readiness tokens.
- Capture binlog policy explicitly rather than assuming defaults.

## Validation steps
- build policy unit tests
- reuse/fingerprint integration tests
- build artifact/binlog smoke test

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
