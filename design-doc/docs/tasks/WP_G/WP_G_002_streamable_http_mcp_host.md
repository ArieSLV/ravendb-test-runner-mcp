# WP G 002 streamable http mcp host

## Task ID
`WP_G_002_streamable_http_mcp_host`

## Title
Implement the primary local Streamable HTTP MCP host with local-only posture.

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
- RavenDB.TestRunner.McpServer.Mcp.Host.Http
- RavenDB.TestRunner.McpServer.Mcp.Host.Stdio

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/MCP_TOOLS.md
- docs/contracts/EVENT_MODEL.md

## Expected outputs
- Implement the primary local Streamable HTTP MCP host with local-only posture.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Keep the Streamable HTTP host as primary.
- Do not log to stdout in the stdio bridge.

## Validation steps
- MCP schema tests
- stdio stdout purity test
- Streamable HTTP lifecycle tests

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
