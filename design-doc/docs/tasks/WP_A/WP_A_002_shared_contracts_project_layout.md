# WP A 002 shared contracts project layout

## Task ID
`WP_A_002_shared_contracts_project_layout`

## Title
Create the shared contracts/package layout and map each contract document to a target project/module.

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
- None beyond the current work package prerequisites

## Touched modules/files
- RavenDB.TestRunner.McpServer.Core.Abstractions
- RavenDB.TestRunner.McpServer.Domain
- RavenDB.TestRunner.McpServer.Shared.Contracts

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/NAMING_AND_MODULE_POLICY.md
- docs/contracts/EVENT_MODEL.md
- docs/contracts/STATE_MACHINES.md

## Expected outputs
- Create the shared contracts/package layout and map each contract document to a target project/module.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- document review against frozen naming
- cross-link validation
- contract completeness review

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
