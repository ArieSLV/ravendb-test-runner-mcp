# PHASE 3 PLANNING AND EXECUTION

## Purpose

Implement selector normalization, preflight, run planning, scheduler, process supervision, and execution pipeline.

## Prerequisites

Phases 0-2 complete

## In scope

- RunRequest -> RunPlan -> RunExecution pipeline
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Result normalization, full browser UI
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Planning
- RavenMcp.Execution
- RavenMcp.Artifacts
- RavenMcp.Core

## Required contracts

- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/STATE_MACHINES.md`
- `docs/contracts/ERROR_TAXONOMY.md`
- `docs/contracts/VERSIONING_AND_CAPABILITIES.md`

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

May begin once storage and semantics minimum interfaces are stable.
