# PHASE 8 FLAKY SUBSYSTEM

## Purpose

Implement iterative run planning, attempt comparison, scoring, and quarantine workflow.

## Prerequisites

Phases 0-7 complete

## In scope

- Iterative runs, signals, classification, compare views
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Advanced remote policy service
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Flaky
- RavenMcp.Results
- RavenMcp.Web.Api
- RavenMcp.Web.Ui

## Required contracts

- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/EVENT_MODEL.md`
- `docs/contracts/ERROR_TAXONOMY.md`
- `docs/contracts/MCP_TOOLS.md`
- `docs/contracts/WEB_API.md`
- `docs/contracts/FRONTEND_VIEW_MODELS.md`

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

May overlap with late Phase 7 after attempt/result contracts are stable.
