# PHASE 2 SEMANTICS AND CATALOG

## Purpose

Build repository line detection, semantic plugins, category/requirement extraction, and test catalog.

## Prerequisites

Phases 0-1 complete

## In scope

- Workspace analysis, plugin routing, capability matrix, test catalog
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Execution and UI behavior beyond contract mocks
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Semantics.Abstractions
- RavenMcp.Semantics.Raven.V62
- RavenMcp.Semantics.Raven.V71
- RavenMcp.Semantics.Raven.V72
- RavenMcp.Core

## Required contracts

- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/VERSIONING_AND_CAPABILITIES.md`
- `docs/contracts/ERROR_TAXONOMY.md`

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

May proceed in parallel with Phase 1 after Phase 0 is accepted.
