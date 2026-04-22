# WP D PLANNING AND EXECUTION

## Objective

Turn selectors and profiles into deterministic run plans and real execution flows.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- selector normalization
- preflight
- run planning
- scheduler
- process supervision
- timeouts
- cancellation
- repro commands

## Out of scope

- result normalization
- full MCP/browser integration

## Dependencies

- WP_B and WP_C minimum contracts available

## Touched documents

- DOMAIN_MODEL.md
- STATE_MACHINES.md
- ERROR_TAXONOMY.md

## Touched projects/modules

- RavenMcp.Planning
- RavenMcp.Execution
- RavenMcp.Core

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement structured selector normalization.
- Implement preflight and skip prediction.
- Implement deterministic run planning and step synthesis.
- Implement scheduler and process supervisor.
- Implement cancellation, timeout, and reproducible command generation.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- selector normalization tests
- preflight tests
- command synthesis golden tests
- scheduler/cancel/timeout tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0001
- ADR_0004
