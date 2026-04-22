# PHASE 1 STORAGE AND REGISTRY

## Purpose

Establish RavenDB Embedded bootstrap, filesystem artifact root, and restart-safe run registry.

## Prerequisites

Phase 0 complete

## In scope

- Embedded startup, collections, indexes, artifact index, retention metadata
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Semantics parsing, execution, MCP, browser UI
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Storage.RavenEmbedded
- RavenMcp.Artifacts
- RavenMcp.Core
- tests/RavenMcp.IntegrationTests

## Required contracts

- `docs/contracts/STORAGE_MODEL.md`
- `docs/contracts/ARTIFACTS_AND_RETENTION.md`
- `docs/contracts/SECURITY_AND_REDACTION.md`
- `docs/contracts/DOMAIN_MODEL.md`

## Deliverables

- phase-specific implementation artifacts
- updated task statuses
- phase completion note
- validation evidence

## Acceptance criteria

- all mandatory deliverables for the phase exist
- referenced contracts are honored
- phase-level tests pass or are explicitly marked as blocked with reason
- no undocumented architectural drift is introduced

## Validation gates

- unit and/or contract tests relevant to this phase
- integration tests where the phase introduces runtime behavior
- review by the integrator for shared contract impact

## Main risks

- shared contract drift
- hidden dependency on unfinished neighboring work package
- missing validation coverage
- branch-specific behavior leakage into shared core

## Handoff conditions

- all touched documents updated
- all task cards completed or explicitly split
- handoff note written for each merged task
- unresolved issues listed with owner and next step

## Parallelization note

May proceed in parallel with Phase 2 after Phase 0 is accepted.
