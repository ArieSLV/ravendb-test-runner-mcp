# WP_I — FRONTEND

## Objective
Implement RavenDB Studio-aligned operator UI for builds, tests, artifacts, diagnostics, and policy explainability.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement the UI shell and RavenDB Studio-aligned design baseline without hard-coupling to Studio internals.
- Implement combined runs/builds list and detail views with live state.
- Implement live console/output panes, results explorer, and build output inspectors.
- Implement artifact explorer, diagnostics views, and plan inspectors for builds and runs.
- Implement flaky analysis views, settings, and build/test policy screens.
- Implement keyboard navigation, reconnect handling, and degraded-mode UX.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- WP_H

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_I/*

## Touched modules
- RavenDB.TestRunner.McpServer.Web.Ui

## Detailed TODO checkpoints
- [1] 001 ui app shell and design baseline
- [2] 002 runs and builds list details
- [3] 003 live console results and build output
- [4] 004 artifacts diagnostics and plan views
- [5] 005 flaky settings and policy views
- [6] 006 accessibility and reconnect behavior

## Acceptance criteria
- all task cards in this work package are completed or explicitly deferred by ADR
- contract tests for the affected area pass
- handoff notes are recorded for integrator review
- no undocumented drift remains between architecture docs and implementation-facing docs

## Required tests
- UI view model tests
- live update tests
- reconnect behavior tests

## Merge / handoff instructions
- merge only after updating any changed contract references
- include a short handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- flag any uncovered risk explicitly for integrator review

## Likely ADR touchpoints
- none expected unless scope changes
