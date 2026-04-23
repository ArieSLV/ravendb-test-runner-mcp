# WP_C — SEMANTICS V62 V71 V72

## Objective
Implement branch-aware semantics plugins, test catalog building, capability routing, and repo-line discovery.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement workspace detection, branch line routing, and capability discovery for v6.2, v7.1, and v7.2.
- Create semantic plugin interfaces and shared capability routing abstractions.
- Implement the v6.2 plugin, including xUnit v2 assumptions and no-AI capability baseline.
- Implement the v7.1 plugin, including transitional AI capabilities and xUnit v2-era behavior.
- Implement the v7.2 plugin, including xUnit v3-era capabilities and modern test topology.
- Persist semantic snapshots, category catalogs, and compatibility matrices in RavenDB Embedded.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_A

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_C/*

## Touched modules
- RavenDB.TestRunner.McpServer.Semantics.Abstractions
- RavenDB.TestRunner.McpServer.Semantics.Raven.V62
- RavenDB.TestRunner.McpServer.Semantics.Raven.V71
- RavenDB.TestRunner.McpServer.Semantics.Raven.V72

## Detailed TODO checkpoints
- [1] 001 workspace and repo line detection
- [2] 002 semantic plugin contracts
- [3] 003 v62 semantics plugin
- [4] 004 v71 semantics plugin
- [5] 005 v72 semantics plugin
- [6] 006 catalog persistence and capability matrix

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
