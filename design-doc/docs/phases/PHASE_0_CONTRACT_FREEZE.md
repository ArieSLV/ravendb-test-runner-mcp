# Phase 0 — Contract Freeze and Naming Consolidation

## Purpose
Freeze shared contracts, naming, module boundaries, storage conventions, build/test lifecycles, and event schemas before any production code lands.

## Prerequisites
none

## In scope
- approved contract set
- approved naming/module policy
- approved state machines
- solution skeleton

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Core.Abstractions
- RavenDB.TestRunner.McpServer.Domain
- RavenDB.TestRunner.McpServer.Shared.Contracts

## Required contracts
- NAMING_AND_MODULE_POLICY.md
- DOMAIN_MODEL.md
- EVENT_MODEL.md
- STATE_MACHINES.md

## Deliverables
- approved contract set
- approved naming/module policy
- approved state machines
- solution skeleton

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
- WP_B
- WP_C
