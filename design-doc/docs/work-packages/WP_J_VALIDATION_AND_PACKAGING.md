# WP J VALIDATION AND PACKAGING

## Objective

Harden the system, validate supported lines, package the standalone app, and publish runbooks.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- unit/contract/integration tests
- cross-branch tests
- UI tests
- smoke tests
- packaging
- runbooks

## Out of scope

- major new features
- distributed deployment model

## Dependencies

- all earlier work packages substantially complete

## Touched documents

- all contract files
- phase briefs
- runbook docs

## Touched projects/modules

- tests/*
- packaging/*
- docs/runbooks/*

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement contract, unit, and integration test suites.
- Implement cross-branch validation for v6.2, v7.1, and v7.2.
- Implement UI smoke and reconnect tests.
- Implement packaging scripts and standalone runtime validation.
- Write developer/operator runbooks and upgrade notes.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- full matrix CI
- packaging smoke tests
- runbook validation
- cold-start and restart tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0001
- ADR_0002
- ADR_0003
- ADR_0007
