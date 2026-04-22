# WP A FOUNDATION AND CONTRACTS

## Objective

Create the solution scaffold, shared abstractions, frozen DTO/contracts package, and task/ADR operating structure.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- solution skeleton
- shared contracts package
- ID/naming conventions
- event/state baseline
- task system and handoff discipline

## Out of scope

- RavenDB bootstrap
- real execution
- UI implementation

## Dependencies

- docs/architecture/DECISION_FREEZE.md
- all contract files

## Touched documents

- AGENTS.md
- docs/contracts/*
- docs/tasks/*
- docs/adr/*

## Touched projects/modules

- RavenMcp.Core.Abstractions
- RavenMcp.Domain
- RavenMcp.Shared.Contracts

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Create solution/project layout and naming baseline.
- Create shared contracts package and reference graph.
- Freeze document ID conventions and entity names.
- Freeze event envelope and core state machines.
- Create tasking, handoff, and ADR operational structure.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- schema/serialization tests
- dependency graph lint or review
- documentation completeness checks

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0001
- ADR_0004
