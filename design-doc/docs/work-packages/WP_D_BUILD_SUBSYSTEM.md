# WP_D — BUILD SUBSYSTEM

## Objective
Implement build graph, build planning, build reuse/caching, build execution, build status APIs, and build artifacts.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.

This work package establishes build as a sibling to test execution, with its own persistence, statuses, tools, and APIs.

## Exact scope
- Implement build domain contracts, build policy enums, and the explicit build ownership model.
- Implement build graph analysis for solution/project scopes and deterministic build target enumeration.
- Implement build fingerprints, reuse decisions, readiness tokens, and stale-build invalidation logic.
- Implement build scheduler, restore/build/clean/rebuild orchestration, and process supervision.
- Implement binlog/text output capture, build artifacts, live build status, and build result documents.
- Expose build readiness and reuse decisions to the test planning subsystem and browser/MCP surfaces.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_A
- WP_B
- WP_C

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_D/*

## Touched modules
- RavenDB.TestRunner.McpServer.Build
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded
- RavenDB.TestRunner.McpServer.Core

## Detailed TODO checkpoints
- [1] 001 build domain contracts and policies
- [2] 002 build graph analyzer
- [3] 003 build fingerprint and reuse engine
- [4] 004 build scheduler and execution engine
- [5] 005 build artifacts status and binlog capture
- [6] 006 build readiness integration

## Acceptance criteria
- all task cards in this work package are completed or explicitly deferred by ADR
- contract tests for the affected area pass
- handoff notes are recorded for integrator review
- no undocumented drift remains between architecture docs and implementation-facing docs

## Required tests
- build reuse contract tests
- binlog/artifact tests
- readiness token persistence tests
- no-chaotic-rebuild regression tests

## Merge / handoff instructions
- merge only after updating any changed contract references
- include a short handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- flag any uncovered risk explicitly for integrator review

## Likely ADR touchpoints
- ADR_0006_BUILD_SUBSYSTEM_AS_FIRST_CLASS_CONCERN
