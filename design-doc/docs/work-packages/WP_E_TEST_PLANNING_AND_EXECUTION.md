# WP_E — TEST PLANNING AND EXECUTION

## Objective
Implement selector normalization, preflight, build-to-test handoff, run planning, execution, and reproducible command generation.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement structured selector normalization and expert-mode raw filter isolation.
- Implement preflight evaluation, deterministic skip prediction, and runtime unknown reporting.
- Implement run planning with explicit build dependency resolution and artifact path generation.
- Implement run scheduling, single-workspace process discipline, cancellation, and timeout handling.
- Implement explicit build-to-test handoff so test execution never performs chaotic hidden rebuilds.
- Implement exact repro commands and execution summaries for builds and runs.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_B
- WP_C
- WP_D

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_E/*

## Touched modules
- RavenDB.TestRunner.McpServer.TestExecution
- RavenDB.TestRunner.McpServer.Build

## Detailed TODO checkpoints
- [1] 001 selector normalization engine
- [2] 002 preflight evaluator
- [3] 003 test run planner
- [4] 004 scheduler and process supervisor
- [5] 005 build to test handoff
- [6] 006 repro commands and execution summaries

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
