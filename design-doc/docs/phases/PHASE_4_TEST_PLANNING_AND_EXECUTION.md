# Phase 4 — Test Planning and Execution

## Purpose
Implement test selectors, preflight, build-to-test handoff, deterministic run planning, scheduling, process supervision, and run execution.

## Prerequisites
Phases 1-3

## In scope
- selector engine
- preflight
- build-to-test handshake
- run planner
- run execution

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.TestExecution
- RavenDB.TestRunner.McpServer.Build
- RavenDB.TestRunner.McpServer.Core

## Required contracts
- DOMAIN_MODEL.md
- BUILD_SUBSYSTEM.md
- MCP_TOOLS.md
- ERROR_TAXONOMY.md

## Deliverables
- selector engine
- preflight
- build-to-test handshake
- run planner
- run execution

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
- WP_D
- WP_C
