# PHASE 6 WEB API AND LIVE EVENTS

## Purpose

Provide browser-facing REST, SignalR, SSE, and live log/event access.

## Prerequisites

Phases 0-4 complete; API/event contracts frozen

## In scope

- BFF/API, hub, event replay, cursor logs
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Frontend UI
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Web.Api
- RavenMcp.Shared.Contracts
- RavenMcp.Results
- RavenMcp.Flaky

## Required contracts

- `docs/contracts/WEB_API.md`
- `docs/contracts/EVENT_MODEL.md`
- `docs/contracts/FRONTEND_VIEW_MODELS.md`
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

May overlap with Phase 5 and early Phase 7 once event and API contracts are stable.
