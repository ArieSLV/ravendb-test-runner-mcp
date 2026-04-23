# AGENTS.md

## Purpose
This repository contains the execution pack for **RavenDB Test Runner MCP Server**.

This file is intentionally concise. It tells you how to work safely with the pack. It is not a substitute for the detailed documents in `docs/`.

## Start here
1. Read `docs/architecture/DECISION_FREEZE.md`
2. Read `IMPLEMENTATION_PROGRESS.md` to see current task state
3. Read these contracts in order:
   - `docs/contracts/NAMING_AND_MODULE_POLICY.md`
   - `docs/contracts/DOMAIN_MODEL.md`
   - `docs/contracts/BUILD_SUBSYSTEM.md`
   - `docs/contracts/STORAGE_MODEL.md`
   - `docs/contracts/EVENT_MODEL.md`
   - `docs/contracts/MCP_TOOLS.md`
   - `docs/contracts/WEB_API.md`
4. Read the relevant phase brief
5. Read the relevant work package
6. Execute exactly one task card unless the integrator explicitly asks for more

## Non-negotiable rules
- Do not rename the product away from **RavenDB Test Runner MCP Server**
- Do not reintroduce legacy `RavenMcp*` naming as the primary identity
- Do not treat build as a hidden part of `tests.run`
- Do not change contracts without updating the relevant contract documents and ADRs
- Do not let stdio hosts write non-protocol data to stdout
- Do not add SQLite

## Reporting requirements
Before major work, report:
- scope summary
- touched modules
- touched contracts
- risks
- acceptance criteria
- selected task status update in `IMPLEMENTATION_PROGRESS.md`

After work, report:
- what changed
- what contracts changed
- how it was validated
- handoff notes
- final task status update in `IMPLEMENTATION_PROGRESS.md`

## Progress tracking
- `docs/tasks/TASK_INDEX.md` is the static backlog
- `IMPLEMENTATION_PROGRESS.md` is the mutable status ledger
- Set a task to `In Progress` before implementation starts
- Set a task to `Partial`, `Done`, `Blocked`, or `Deferred` when work stops
- Reflect every accepted worker handoff in `IMPLEMENTATION_PROGRESS.md`
- Do not mark a task `Done` without validation notes or an explicit reason validation was blocked

## Change control
If your work changes any architecture rule, add or update an ADR and include a design delta note in your handoff.

## Where to look
- Architecture: `docs/architecture/`
- Contracts: `docs/contracts/`
- Phases: `docs/phases/`
- Work packages: `docs/work-packages/`
- Tasks: `docs/tasks/`
- ADRs: `docs/adr/`
- Runbooks: `docs/runbooks/`
