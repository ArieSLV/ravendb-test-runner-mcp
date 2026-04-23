# WP H 006 localhost security posture

## Task ID
`WP_H_006_localhost_security_posture`

## Title
Implement localhost binding, origin validation, and local browser safety rules.

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
- core domain and event contracts approved

## Touched modules/files
- RavenDB.TestRunner.McpServer.Web.Api

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/WEB_API.md
- docs/contracts/EVENT_MODEL.md
- docs/contracts/FRONTEND_VIEW_MODELS.md

## Expected outputs
- Implement localhost binding, origin validation, and local browser safety rules.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- API contract tests
- SignalR stream tests
- SSE cursor tests

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
