# WP I 004 artifacts diagnostics and plan views

## Task ID
`WP_I_004_artifacts_diagnostics_and_plan_views`

## Title
Implement artifact explorer, diagnostics views, and plan inspectors for builds and runs.

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
- WEB_API and FRONTEND_VIEW_MODELS contracts approved

## Touched modules/files
- RavenDB.TestRunner.McpServer.Web.Ui

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/FRONTEND_VIEW_MODELS.md
- docs/contracts/WEB_API.md

## Expected outputs
- Implement artifact explorer, diagnostics views, and plan inspectors for builds and runs.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Follow RavenDB Studio design language where useful without coupling to Studio internals.

## Validation steps
- UI view model tests
- live update tests
- navigation smoke tests

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
