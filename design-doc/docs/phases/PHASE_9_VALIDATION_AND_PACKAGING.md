# PHASE 9 VALIDATION AND PACKAGING

## Purpose

Harden the system, validate all supported lines, and prepare packaging/runbooks.

## Prerequisites

All earlier phases substantially complete

## In scope

- CI, smoke tests, packaging, runbooks, upgrade notes
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- New feature expansion
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- tests/*
- packaging/
- docs/runbooks/
- RavenMcp.Mcp.Host.Http
- RavenMcp.Web.Api

## Required contracts

- All contract files

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

Runs throughout, but final sign-off happens last.
