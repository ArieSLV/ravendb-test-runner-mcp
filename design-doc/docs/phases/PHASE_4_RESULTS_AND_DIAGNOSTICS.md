# PHASE 4 RESULTS AND DIAGNOSTICS

## Purpose

Normalize results, classify failures, harvest artifacts, and reconcile predicted vs actual outcomes.

## Prerequisites

Phases 0-3 complete

## In scope

- TRX/junit handling, console capture, diagnostics metadata
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Full MCP/browser surfaces
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Results
- RavenMcp.Artifacts
- RavenMcp.Execution
- tests/RavenMcp.ContractTests

## Required contracts

- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/ARTIFACTS_AND_RETENTION.md`
- `docs/contracts/ERROR_TAXONOMY.md`
- `docs/contracts/EVENT_MODEL.md`

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

May overlap late Phase 3 integration work after execution artifacts exist.
