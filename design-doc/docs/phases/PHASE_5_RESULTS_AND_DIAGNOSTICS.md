# Phase 5 — Results, Diagnostics, and Normalization

## Purpose
Implement canonical run/build result models, artifact harvesting, skip extraction, diagnostic capture, and failure classification.

## Prerequisites
Phases 1, 3, 4

## In scope
- normalized build/run results
- artifact harvesting
- failure taxonomy
- diagnostic capture

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Results
- RavenDB.TestRunner.McpServer.Artifacts

## Required contracts
- ARTIFACTS_AND_RETENTION.md
- ERROR_TAXONOMY.md
- EVENT_MODEL.md

## Deliverables
- normalized build/run results
- artifact harvesting
- failure taxonomy
- diagnostic capture

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
- WP_E
