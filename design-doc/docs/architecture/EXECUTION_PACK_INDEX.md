# Execution Pack Index

## Purpose

This document explains how to navigate the execution pack and which files are normative versus supportive.

## Reading order by role

### Human architect / integrator

1. `docs/architecture/DECISION_FREEZE.md`
2. `docs/architecture/IMPLEMENTATION_SPEC.md`
3. `docs/architecture/DEPENDENCY_GRAPH.md`
4. `docs/contracts/DOMAIN_MODEL.md`
5. `docs/contracts/VERSIONING_AND_CAPABILITIES.md`
6. `docs/contracts/STORAGE_MODEL.md`
7. `docs/contracts/EVENT_MODEL.md`
8. `docs/contracts/MCP_TOOLS.md`
9. `docs/contracts/WEB_API.md`
10. phase briefs and work packages

### Coding agent

1. `AGENTS.md`
2. `docs/architecture/DECISION_FREEZE.md`
3. the specific contract files referenced by the task
4. the phase brief
5. the work package
6. the task card
7. related ADRs

### Reviewer / QA agent

1. `docs/contracts/ERROR_TAXONOMY.md`
2. `docs/contracts/STATE_MACHINES.md`
3. `docs/contracts/EVENT_MODEL.md`
4. relevant phase brief
5. relevant task cards
6. `docs/architecture/HIGH_RISK_AREAS.md`

## Normative documents

The following are normative:

- `AGENTS.md`
- `docs/architecture/DECISION_FREEZE.md`
- `docs/architecture/IMPLEMENTATION_SPEC.md`
- every file under `docs/contracts/`
- every ADR under `docs/adr/`

## Delivery-control documents

These are execution-control documents:

- `docs/phases/`
- `docs/work-packages/`
- `docs/tasks/`

## Informative documents

These are strongly recommended but not directly contract-authoritative:

- `docs/architecture/DEPENDENCY_GRAPH.md`
- `docs/architecture/IMPLEMENTATION_ORDER_SUMMARY.md`
- `docs/architecture/PARALLELIZATION_STRATEGY.md`
- `docs/architecture/MAIN_OPEN_QUESTIONS.md`
- `docs/architecture/HIGH_RISK_AREAS.md`
- `docs/architecture/FIRST_10_TASKS_TO_EXECUTE.md`

## Authoritative conflict resolution

When documents disagree:

1. `DECISION_FREEZE.md` wins
2. then `IMPLEMENTATION_SPEC.md`
3. then contract files
4. then ADRs if they explicitly supersede a prior decision
5. then phase/work package/task docs

## Pack contents summary

This execution pack contains:

- 1 short agent guide
- 4 architecture/navigation files
- 10 contract files
- 10 phase briefs
- 10 work package briefs
- 3 task framework files
- 50 task cards
- 7 ADRs
- 5 execution summary files

## Required process

Before parallel coding starts:
- Phase 0 must be complete
- core contracts must be frozen
- dependency order must be acknowledged
- the integrator must assign work package ownership
