# Phase 6 — MCP Surfaces

## Purpose
Expose build and test orchestration through Streamable HTTP MCP and stdio bridge hosts with progress, cancellation, and contract-safe schemas.

## Prerequisites
Phases 0-5

## In scope
- Streamable HTTP host
- stdio bridge
- build tools
- test tools

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Mcp.Host.Http
- RavenDB.TestRunner.McpServer.Mcp.Host.Stdio

## Required contracts
- MCP_TOOLS.md
- EVENT_MODEL.md
- SECURITY_AND_REDACTION.md

## Deliverables
- Streamable HTTP host
- stdio bridge
- build tools
- test tools

## Acceptance criteria
- phase outputs are stored in the expected modules and registries
- phase-specific contracts remain satisfied
- no unresolved critical TODOs remain inside this phase’s declared scope
- human integrator can approve handoff to dependent phases

## Validation gates
- unit and contract tests for touched contracts
- integration smoke for touched subsystem(s)
- update docs/tasks if new constraints are discovered

## Main risks
- contract drift
- insufficient validation
- parallel work misalignment

## Handoff conditions
- all required deliverables complete
- no contract-breaking change left undocumented
- ADRs added for any meaningful deviation

## May start in parallel with
- WP_H
