# EXECUTION_PACK_INDEX.md

## Purpose
This index explains how to use the execution pack for RavenDB Test Runner MCP Server.

## Normative hierarchy
1. `docs/architecture/IMPLEMENTATION_SPEC.md`
2. `docs/architecture/DECISION_FREEZE.md`
3. `docs/contracts/*`
4. `docs/adr/*`
5. `docs/phases/*`
6. `docs/work-packages/*`
7. `docs/tasks/*`
8. runbooks and informative summaries

## How to navigate the pack
- Start with `AGENTS.md`
- Read `DECISION_FREEZE.md`
- Read `DOMAIN_MODEL.md`, `STORAGE_MODEL.md`, `BUILD_SUBSYSTEM.md`, `EVENT_MODEL.md`, `MCP_TOOLS.md`, and `WEB_API.md`
- Read the relevant phase brief
- Read the relevant work package
- Execute one task card at a time

## Main document groups
- **Architecture:** frozen decisions, dependency graph, execution summaries
- **Contracts:** authoritative interfaces, schemas, states, events, storage rules
- **Phases:** coarse implementation sequencing
- **Work packages:** bounded implementation tracks
- **Tasks:** individual agent-sized work units
- **ADRs:** design decisions and change governance
- **Runbooks:** human operational guidance

## New in this execution pack revision
- canonical product naming consolidated around `RavenDB Test Runner MCP Server`
- first-class build subsystem introduced across the entire pack
- separate build surfaces added for MCP, web API, events, UI, phases, work packages, and tasks
