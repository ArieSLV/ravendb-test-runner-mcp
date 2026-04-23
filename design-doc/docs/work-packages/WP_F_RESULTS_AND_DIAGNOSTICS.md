# WP_F — RESULTS AND DIAGNOSTICS

## Objective
Implement canonical result normalization, console/TRX/binlog harvesting, diagnostics capture, and failure taxonomy mapping.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement stdout/stderr/merged transcript capture for build and test processes.
- Implement TRX/JUnit harvesting for tests and binlog harvesting for builds.
- Implement canonical failure classifications for build and test execution outcomes.
- Implement normalized build/run result builders and persistence.
- Implement diagnostic hooks, blame-style capture, and artifact indexing.
- Implement reconciliation between predicted preflight outcomes and actual execution outcomes.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_D
- WP_E

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_F/*

## Touched modules
- RavenDB.TestRunner.McpServer.Results
- RavenDB.TestRunner.McpServer.Artifacts

## Detailed TODO checkpoints
- [1] 001 console capture pipeline
- [2] 002 trx junit and binlog harvesting
- [3] 003 failure taxonomy mapper
- [4] 004 normalized result builder
- [5] 005 diagnostic hooks and blame artifacts
- [6] 006 predicted vs actual reconciliation

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
