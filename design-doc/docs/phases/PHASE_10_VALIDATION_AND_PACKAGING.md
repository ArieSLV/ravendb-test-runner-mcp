# Phase 10 — Validation, Packaging, and Runbooks

## Purpose
Finalize contract tests, cross-branch integration tests, packaging, startup smoke tests, and operator/developer documentation.

## Prerequisites
All prior phases

## In scope
- test matrix
- startup smoke
- packaging docs
- runbooks

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- tests/*
- packaging/*
- docs/runbooks/*

## Required contracts
- all contracts

## Deliverables
- test matrix
- startup smoke
- packaging docs
- runbooks

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
- WP_J
