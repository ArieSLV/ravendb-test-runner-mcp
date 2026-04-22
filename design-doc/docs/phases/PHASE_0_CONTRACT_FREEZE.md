# PHASE 0 CONTRACT FREEZE

## Purpose

Freeze solution skeleton, contracts, IDs, event model, state machines, and tasking structure before code implementation broadens.

## Prerequisites

None

## In scope

- Solution skeleton, contract package, task system, ADR baseline
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- All production behavior implementation
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Core.Abstractions
- RavenMcp.Domain
- RavenMcp.Shared.Contracts
- docs/contracts/*
- docs/tasks/*
- docs/adr/*

## Required contracts

- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/VERSIONING_AND_CAPABILITIES.md`
- `docs/contracts/STORAGE_MODEL.md`
- `docs/contracts/EVENT_MODEL.md`
- `docs/contracts/STATE_MACHINES.md`
- `docs/contracts/MCP_TOOLS.md`
- `docs/contracts/WEB_API.md`
- `docs/contracts/FRONTEND_VIEW_MODELS.md`
- `docs/contracts/ERROR_TAXONOMY.md`
- `docs/contracts/SECURITY_AND_REDACTION.md`

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

No parallel implementation phases should begin before this phase is accepted.
