# WP_B — STORAGE AND REGISTRY

## Objective
Implement RavenDB Embedded registry, collections, indexes, attachments policy, and filesystem artifact metadata integration.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Bootstrap RavenDB Embedded, database initialization, and mandatory licensed startup checks.
- Implement collection creation, indexes, revisions policy decisions, and optimistic concurrency baseline.
- Implement artifact metadata documents and attachment threshold policy for compact artifacts.
- Implement canonical filesystem layout, hashing, and path registration for large artifacts.
- Persist event checkpoints and stream resume cursors for build and run streams.
- Implement restart recovery, retention metadata, and cleanup job journal design.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_A

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_B/*

## Touched modules
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded
- RavenDB.TestRunner.McpServer.Artifacts

## Detailed TODO checkpoints
- [1] 001 embedded bootstrap and database init
- [2] 002 collections indexes and optimistic concurrency
- [3] 003 artifact metadata and attachment thresholds
- [4] 004 filesystem artifact layout and hashing
- [5] 005 event checkpoint and resume persistence
- [6] 006 restart recovery cleanup and retention

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
