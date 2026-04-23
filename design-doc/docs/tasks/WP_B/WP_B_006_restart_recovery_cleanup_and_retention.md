# WP B 006 restart recovery cleanup and retention

## Task ID
`WP_B_006_restart_recovery_cleanup_and_retention`

## Title
Implement restart recovery, retention metadata, and cleanup job journal design.

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

## Touched modules/files
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded
- RavenDB.TestRunner.McpServer.Artifacts

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/STORAGE_MODEL.md
- docs/contracts/ARTIFACTS_AND_RETENTION.md
- docs/contracts/SECURITY_AND_REDACTION.md

## Expected outputs
- Implement restart recovery, retention metadata, and cleanup job journal design.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- embedded startup integration test
- document persistence test
- artifact metadata routing test

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
