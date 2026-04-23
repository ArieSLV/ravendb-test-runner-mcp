# WP_J — FLAKY AND QUARANTINE

## Objective
Implement iterative execution, attempt comparison, stability classification, and explainable quarantine automation.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement iterative run planning modes and attempt sequencing.
- Implement attempt lifecycle persistence and historical rollups.
- Implement attempt/build/run comparison engine and signature drift detection.
- Implement stability signals, classification, and explainable scoring.
- Implement quarantine actions/proposals, reversibility, and audit trail requirements.
- Implement flaky reporting surfaces for MCP, web API, and browser UI.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_E
- WP_F
- WP_D

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_J/*

## Touched modules
- RavenDB.TestRunner.McpServer.Flaky
- RavenDB.TestRunner.McpServer.Results

## Detailed TODO checkpoints
- [1] 001 iterative run planner
- [2] 002 attempt lifecycle and history persistence
- [3] 003 comparison engine
- [4] 004 stability classification and scoring
- [5] 005 quarantine policy and audit trail
- [6] 006 reporting surfaces and notifications

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
- ADR_0009_FLAKY_AUTOMATION_AND_QUARANTINE_POLICY
