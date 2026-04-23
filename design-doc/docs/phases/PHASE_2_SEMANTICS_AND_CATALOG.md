# Phase 2 — Branch-Aware Semantics and Catalog

## Purpose
Implement workspace detection, repo line routing, capability matrix, and semantic plugins for v6.2, v7.1, and v7.2.

## Prerequisites
Phase 0

## In scope
- line detection
- plugin router
- semantic plugins
- catalog persistence

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Semantics.Abstractions
- RavenDB.TestRunner.McpServer.Semantics.Raven.V62
- RavenDB.TestRunner.McpServer.Semantics.Raven.V71
- RavenDB.TestRunner.McpServer.Semantics.Raven.V72

## Required contracts
- DOMAIN_MODEL.md
- VERSIONING_AND_CAPABILITIES.md

## Deliverables
- line detection
- plugin router
- semantic plugins
- catalog persistence

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
