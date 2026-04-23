# Phase 3 — Build Subsystem as a First-Class Concern

## Purpose
Implement explicit build graph analysis, build planning, build reuse decisions, build execution, build output streaming, and build status surfaces.

## Prerequisites
Phase 0 + storage registry baseline

## In scope
- build graph analysis
- build plan and execution lifecycle
- build readiness tokens
- build result persistence
- build status surfaces baseline

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Build
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded

## Required contracts
- BUILD_SUBSYSTEM.md
- DOMAIN_MODEL.md
- STORAGE_MODEL.md
- EVENT_MODEL.md
- MCP_TOOLS.md
- WEB_API.md

## Deliverables
- build graph analysis
- build plan and execution lifecycle
- build readiness tokens
- build result persistence
- build status surfaces baseline

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
- incorrect reuse decisions
- hidden rebuild behavior creeping back in
- build graph oversimplification

## Handoff conditions
- all required deliverables complete
- no contract-breaking change left undocumented
- ADRs added for any meaningful deviation

## May start in parallel with
- WP_C
- WP_B
