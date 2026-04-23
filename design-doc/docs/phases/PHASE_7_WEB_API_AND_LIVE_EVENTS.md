# Phase 7 — Web API and Live Event Surfaces

## Purpose
Expose browser-facing query/command APIs, SignalR event streams, SSE log streams, and local-host security posture.

## Prerequisites
Phases 0-5

## In scope
- REST/BFF endpoints
- SignalR hubs
- SSE endpoints
- log cursor model

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Web.Api

## Required contracts
- WEB_API.md
- EVENT_MODEL.md
- FRONTEND_VIEW_MODELS.md

## Deliverables
- REST/BFF endpoints
- SignalR hubs
- SSE endpoints
- log cursor model

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
- WP_G
