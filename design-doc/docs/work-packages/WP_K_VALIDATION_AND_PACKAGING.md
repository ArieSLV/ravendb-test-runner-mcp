# WP_K — VALIDATION AND PACKAGING

## Objective
Implement test matrices, startup smoke, packaging, runbooks, and durability checks for the standalone application.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Define and implement unit and contract test matrix for all subsystems.
- Implement real workspace fixtures for v6.2, v7.1, and v7.2.
- Implement UI, SignalR, SSE, and reconnect validation suites.
- Implement build determinism, reuse, and no-chaotic-rebuild validation suites.
- Implement packaging, startup smoke, and first-run embedded license flow validation.
- Finalize runbooks, developer setup docs, and operational recovery guidance.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- all prior work packages

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_K/*

## Touched modules
- tests/*
- packaging/*
- docs/runbooks/*

## Detailed TODO checkpoints
- [1] 001 unit and contract test matrix
- [2] 002 cross branch integration fixtures
- [3] 003 ui and live transport validation
- [4] 004 build subsystem validation
- [5] 005 packaging and startup smoke
- [6] 006 runbooks and operator docs

## Acceptance criteria
- all task cards in this work package are completed or explicitly deferred by ADR
- contract tests for the affected area pass
- handoff notes are recorded for integrator review
- no undocumented drift remains between architecture docs and implementation-facing docs

## Required tests
- unit tests
- contract tests
- integration smoke tests

## Merge / handoff instructions
- merge only after updating any changed contract references
- include a short handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- flag any uncovered risk explicitly for integrator review

## Likely ADR touchpoints
- none expected unless scope changes
