# WP K 005 packaging and startup smoke

## Task ID
`WP_K_005_packaging_and_startup_smoke`

## Title
Implement packaging, startup smoke, and first-run embedded license flow validation.

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
- tests/*
- packaging/*
- docs/runbooks/*

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- all contracts
- all relevant phase briefs

## Expected outputs
- Implement packaging, startup smoke, and first-run embedded license flow validation.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- full system smoke
- restart persistence tests
- packaging validation

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
