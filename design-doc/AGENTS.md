# AGENTS.md

## Purpose

This file is the concise operating guide for AI coding agents working in this repository.
It is intentionally short. The authoritative implementation guidance lives in the execution pack documents under `docs/`.

## Read this first

Before making any code change, every agent MUST read in this order:

1. `docs/architecture/DECISION_FREEZE.md`
2. `docs/architecture/EXECUTION_PACK_INDEX.md`
3. `docs/contracts/DOMAIN_MODEL.md`
4. `docs/contracts/VERSIONING_AND_CAPABILITIES.md`
5. `docs/contracts/STORAGE_MODEL.md`
6. `docs/contracts/EVENT_MODEL.md`
7. The relevant phase brief
8. The relevant work package file
9. The exact task card(s) you are executing

## Non-negotiable rules

- Do not redesign the system from scratch.
- Do not bypass frozen contracts.
- Do not introduce SQLite.
- Do not replace RavenDB Embedded.
- Do not turn the product into a remote multi-tenant service in v1.
- Do not store large raw artifacts as the primary canonical payload in RavenDB by default.
- Do not treat MCP and browser APIs as the same surface.
- Do not break contract files without creating or updating an ADR and a migration note.
- Do not write non-protocol output to `stdout` in the stdio MCP host.
- Do not treat raw filter strings as the canonical internal selector model.

## Repository layout

```text

src/
  RavenMcp.Core.Abstractions/
  RavenMcp.Domain/
  RavenMcp.Core/
  RavenMcp.Semantics.Abstractions/
  RavenMcp.Semantics.Raven.V62/
  RavenMcp.Semantics.Raven.V71/
  RavenMcp.Semantics.Raven.V72/
  RavenMcp.Storage.RavenEmbedded/
  RavenMcp.Artifacts/
  RavenMcp.Execution/
  RavenMcp.Planning/
  RavenMcp.Results/
  RavenMcp.Flaky/
  RavenMcp.Mcp.Host.Common/
  RavenMcp.Mcp.Host.Stdio/
  RavenMcp.Mcp.Host.Http/
  RavenMcp.Web.Api/
  RavenMcp.Web.Ui/
  RavenMcp.Shared.Contracts/

tests/
  RavenMcp.UnitTests/
  RavenMcp.ContractTests/
  RavenMcp.IntegrationTests/
  RavenMcp.CrossBranchTests/
  RavenMcp.UiTests/

docs/
  architecture/
  contracts/
  phases/
  work-packages/
  adr/
  tasks/

```

## How to use the execution pack

- `docs/architecture/` contains the constitution, frozen decisions, navigation, and dependency graph.
- `docs/contracts/` contains normative interface, state, storage, API, event, and security contracts.
- `docs/phases/` contains delivery sequencing.
- `docs/work-packages/` contains parallelizable implementation slices.
- `docs/tasks/` contains small bounded tasks suitable for a focused coding-agent session.
- `docs/adr/` contains architectural decisions and permitted deviations.

## Before changing contracts

If your change affects:
- public DTOs,
- document schemas,
- event schemas,
- run state transitions,
- MCP request/response shapes,
- browser API shapes,
- version capability rules,

you MUST:

1. check the relevant contract file,
2. update the contract file first,
3. add or update an ADR if the change is architectural,
4. add validation and migration notes,
5. mention the contract delta in your handoff.

## Required reporting format

Every agent response that claims implementation work MUST include:

- Task ID(s)
- Scope completed
- Touched modules/files
- Touched contracts
- Validation executed
- Open risks
- Handoff notes

## Handoff rules

Use `docs/tasks/HANDOFF_TEMPLATE.md`.
If you changed behavior but not contracts, explicitly state: `No contract delta`.
If you changed contracts, explicitly list:
- old behavior
- new behavior
- migration impact
- tests added or updated

## When blocked

If you are blocked by ambiguity:
1. consult the authoritative contract file,
2. consult the relevant ADR,
3. check the relevant phase and work package,
4. if still blocked, produce a focused decision request instead of guessing.

## Success criteria for an agent task

A task is not complete until:
- the stated output exists,
- the acceptance criteria in the task card are satisfied,
- the referenced validations are run or explicitly documented as pending with reason,
- the handoff note is written.
