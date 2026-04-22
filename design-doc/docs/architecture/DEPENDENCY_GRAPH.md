# Dependency Graph

## Purpose

This document defines:
- documentation dependency order,
- implementation dependency order,
- parallelization boundaries,
- merge risk hotspots.

## Document dependency graph

```text
DECISION_FREEZE
  -> IMPLEMENTATION_SPEC
  -> DOMAIN_MODEL
  -> VERSIONING_AND_CAPABILITIES
  -> STORAGE_MODEL
  -> EVENT_MODEL
  -> STATE_MACHINES
  -> MCP_TOOLS
  -> WEB_API
  -> FRONTEND_VIEW_MODELS
  -> ERROR_TAXONOMY
  -> SECURITY_AND_REDACTION

Contract files
  -> Phase briefs
  -> Work packages
  -> Task cards
```

## Implementation dependency graph

```text
Phase 0: Foundation and contract freeze
  -> Phase 1: Storage and registry
  -> Phase 2: Semantics and catalog
  -> Phase 3: Planning and execution
  -> Phase 4: Results and diagnostics
  -> Phase 5: MCP surface
  -> Phase 6: Web API and live events
  -> Phase 7: Frontend UI
  -> Phase 8: Flaky subsystem
  -> Phase 9: Validation and packaging
```

## Subsystem dependency graph

```text
Core.Abstractions
  -> Domain
  -> Shared.Contracts

Shared.Contracts
  -> Storage.RavenEmbedded
  -> Planning
  -> Execution
  -> Results
  -> Flaky
  -> MCP Hosts
  -> Web.Api
  -> Web.Ui

Semantics.Abstractions
  -> Semantics.Raven.V62
  -> Semantics.Raven.V71
  -> Semantics.Raven.V72

Storage.RavenEmbedded
  -> Core
  -> Planning
  -> Execution
  -> Results
  -> Flaky
  -> Web.Api
  -> MCP Hosts

Artifacts
  -> Execution
  -> Results
  -> Web.Api

Planning
  -> Execution

Execution
  -> Results
  -> Flaky

Results
  -> Flaky
  -> Web.Api
  -> MCP Hosts

Web.Api
  -> Web.Ui
```

## Parallelization opportunities

The following may proceed in parallel after Phase 0 contract freeze:

- WP-B Storage and Registry
- WP-C Semantics and Catalog
- WP-F MCP Surface skeleton work

The following may start after event and API contracts are frozen:

- WP-G Web API and Streams
- WP-H Frontend

The following should start after run/result/attempt contracts are stable:

- WP-I Flaky Analytics

## Merge risk hotspots

High-risk merge zones:

- `docs/contracts/DOMAIN_MODEL.md`
- `docs/contracts/EVENT_MODEL.md`
- `docs/contracts/STORAGE_MODEL.md`
- shared DTO package
- run/attempt lifecycle code
- event publication pipeline
- browser live state contracts

## Integrator responsibilities

The integrator MUST:
- enforce contract freeze before parallel coding
- sequence shared DTO changes
- serialize event model changes
- review ADRs before merging architectural changes
- maintain the task index status
