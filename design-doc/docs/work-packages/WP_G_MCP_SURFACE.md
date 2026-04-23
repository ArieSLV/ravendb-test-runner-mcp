# WP_G — MCP SURFACE

## Objective
Implement Streamable HTTP MCP host, stdio bridge host, and build/test tools/resources over the shared core.

## Engineering purpose
This work package exists to deliver a bounded part of RavenDB Test Runner MCP Server without blurring responsibilities with adjacent work packages.



## Exact scope
- Implement the shared MCP handler layer over the orchestration core.
- Implement the primary local Streamable HTTP MCP host with local-only posture.
- Implement the optional stdio bridge host with stdout protocol purity.
- Implement the tests.* MCP tools over the shared core.
- Implement the build.* MCP tools as a first-class sibling surface.
- Implement MCP progress, cancellation, and resumability-friendly behavior.

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
- docs/tasks/WP_G/*

## Touched modules
- RavenDB.TestRunner.McpServer.Mcp.Host.Http
- RavenDB.TestRunner.McpServer.Mcp.Host.Stdio

## Detailed TODO checkpoints
- [1] 001 mcp common handler layer
- [2] 002 streamable http mcp host
- [3] 003 stdio bridge host
- [4] 004 tests toolset
- [5] 005 build toolset
- [6] 006 progress cancellation and resume

## Acceptance criteria
- all task cards in this work package are completed or explicitly deferred by ADR
- contract tests for the affected area pass
- handoff notes are recorded for integrator review
- no undocumented drift remains between architecture docs and implementation-facing docs

## Required tests
- MCP schema tests
- stdio stdout purity tests
- Streamable HTTP session tests

## Merge / handoff instructions
- merge only after updating any changed contract references
- include a short handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- flag any uncovered risk explicitly for integrator review

## Likely ADR touchpoints
- ADR_0002_STANDALONE_APP_AND_MCP_SURFACES
