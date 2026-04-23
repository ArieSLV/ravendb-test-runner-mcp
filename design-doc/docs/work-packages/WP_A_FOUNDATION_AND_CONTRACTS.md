# WP_A — FOUNDATION AND CONTRACTS

## Objective
Freeze product naming, module boundaries, shared contracts, event schemas, and solution layout for multi-agent implementation.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Create the solution scaffold and rename the implementation surface to the canonical product/module names.
- Create the shared contracts/package layout and map each contract document to a target project/module.
- Freeze document ID patterns, collection names, and module ownership tables.
- Freeze the event envelope, ordering rules, cursors, and replay semantics across build and test subsystems.
- Freeze build/run/attempt lifecycle state machines and optimistic concurrency expectations.
- Create validation checklists and contract approval gates required before any production implementation starts.

## Out of scope
- changing frozen product decisions without ADR
- opportunistic renaming outside the approved naming policy
- hidden cross-cutting changes that are not reflected in contracts/tasks

## Dependencies
- none

## Touched documents
- docs/contracts/*
- docs/phases/*
- docs/tasks/WP_A/*

## Touched modules
- RavenDB.TestRunner.McpServer.Core.Abstractions
- RavenDB.TestRunner.McpServer.Domain
- RavenDB.TestRunner.McpServer.Shared.Contracts

## Detailed TODO checkpoints
- [1] 001 solution scaffold and name freeze
- [2] 002 shared contracts project layout
- [3] 003 document id and collection conventions
- [4] 004 event contract baseline
- [5] 005 state machine baseline
- [6] 006 phase0 validation harness

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
- ADR_0001_PRODUCT_NAMING_AND_MODULE_POLICY
