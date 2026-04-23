# WP C 006 catalog persistence and capability matrix

## Task ID
`WP_C_006_catalog_persistence_and_capability_matrix`

## Title
Persist semantic snapshots, category catalogs, and compatibility matrices in RavenDB Embedded.

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
- RavenDB.TestRunner.McpServer.Semantics.Abstractions
- RavenDB.TestRunner.McpServer.Semantics.Raven.V62
- RavenDB.TestRunner.McpServer.Semantics.Raven.V71
- RavenDB.TestRunner.McpServer.Semantics.Raven.V72

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/VERSIONING_AND_CAPABILITIES.md

## Expected outputs
- Persist semantic snapshots, category catalogs, and compatibility matrices in RavenDB Embedded.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- workspace fixture tests for v6.2/v7.1/v7.2
- capability matrix snapshot test

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
