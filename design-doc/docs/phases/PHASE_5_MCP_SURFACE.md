# PHASE 5 MCP SURFACE

## Purpose

Expose the shared core through Streamable HTTP and stdio MCP hosts.

## Prerequisites

Phases 0-4 complete

## In scope

- Tool handlers, progress, cancellation, protocol-safe stdio host
- contract-aligned implementation and validation artifacts
- update of task statuses and handoff notes for this phase

## Out of scope

- Browser UI implementation
- architectural redesign beyond ADR-approved deltas

## Touched projects/modules

- RavenMcp.Mcp.Host.Common
- RavenMcp.Mcp.Host.Stdio
- RavenMcp.Mcp.Host.Http
- RavenMcp.Shared.Contracts

## Required contracts

- `docs/contracts/MCP_TOOLS.md`
- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/STATE_MACHINES.md`
- `docs/contracts/SECURITY_AND_REDACTION.md`

## Deliverables

- phase-specific implementation artifacts
- updated task statuses
- phase completion note
- validation evidence

## Acceptance criteria

- all mandatory deliverables for the phase exist
- referenced contracts are honored
- phase-level tests pass or are explicitly marked as blocked with reason
- no undocumented architectural drift is introduced

## Validation gates

- unit and/or contract tests relevant to this phase
- integration tests where the phase introduces runtime behavior
- review by the integrator for shared contract impact

## Main risks

- shared contract drift
- hidden dependency on unfinished neighboring work package
- missing validation coverage
- branch-specific behavior leakage into shared core

## Handoff conditions

- all touched documents updated
- all task cards completed or explicitly split
- handoff note written for each merged task
- unresolved issues listed with owner and next step

## Parallelization note

May overlap with Phase 6 after core tool contracts are frozen.
