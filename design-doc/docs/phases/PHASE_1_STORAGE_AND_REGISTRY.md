# Phase 1 — RavenDB Embedded Storage and Registry Foundation

## Purpose
Stand up RavenDB Embedded, define collections/indexes/IDs, artifact metadata, event checkpoints, and restart-safe registries for builds and runs.

## Prerequisites
Phase 0

## In scope
- embedded bootstrap
- collections/indexes
- artifact metadata persistence
- cleanup journal

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded
- RavenDB.TestRunner.McpServer.Artifacts

## Required contracts
- STORAGE_MODEL.md
- ARTIFACTS_AND_RETENTION.md
- SECURITY_AND_REDACTION.md

## Deliverables
- embedded bootstrap
- collections/indexes
- artifact metadata persistence
- cleanup journal

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
- WP_C
