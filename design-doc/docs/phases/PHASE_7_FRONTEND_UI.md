# PHASE 7 FRONTEND UI

## Purpose

Build the operator dashboard aligned with RavenDB Studio patterns.

## Prerequisites

Phases 0-6 complete

## In scope

- SPA shell, live pages, diagnostics/flaky screens
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Distributed deployment or team auth
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Web.Ui
- RavenMcp.Web.Api
- tests/RavenMcp.UiTests

## Required contracts

- `docs/contracts/FRONTEND_VIEW_MODELS.md`
- `docs/contracts/WEB_API.md`
- `docs/contracts/EVENT_MODEL.md`
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

May overlap with late Phase 6 once view-model and live-event contracts are stable.
