# WP_H — WEB API AND STREAMS

## Objective
Implement browser-facing APIs, SignalR/SSE streams, log cursors, and localhost security for local standalone mode.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement query APIs for builds, runs, catalogs, capabilities, and settings.
- Implement command APIs for planning, launching, cancelling, and cleaning builds/runs.
- Implement SignalR hubs and event mapping for build/run/attempt streams.
- Implement SSE endpoints and cursor-based log playback endpoints.
- Implement dedicated build status, build history, and build policy endpoints.
- Implement localhost binding, origin validation, and local browser safety rules.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_D
- WP_E
- WP_F

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_H/*

## Touched modules
- RavenDB.TestRunner.McpServer.Web.Api

## Detailed TODO checkpoints
- [1] 001 query api surface
- [2] 002 command api surface
- [3] 003 signalr hub and event mapping
- [4] 004 sse and log cursor endpoints
- [5] 005 build status and policy endpoints
- [6] 006 localhost security posture

## Acceptance criteria
- all task cards in this work package are completed or explicitly deferred by ADR
- contract tests for the affected area pass
- handoff notes are recorded for integrator review
- no undocumented drift remains between architecture docs and implementation-facing docs

## Required tests
- API contract tests
- SignalR/SSE tests
- cursor replay tests

## Merge / handoff instructions
- merge only after updating any changed contract references
- include a short handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- flag any uncovered risk explicitly for integrator review

## Likely ADR touchpoints
- ADR_0007_SIGNALR_PRIMARY_BROWSER_TRANSPORT
