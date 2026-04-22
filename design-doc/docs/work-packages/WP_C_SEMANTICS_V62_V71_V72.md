# WP C SEMANTICS V62 V71 V72

## Objective

Implement repository line detection, semantic plugin routing, capability inference, and test catalog construction.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- workspace detection
- capability matrix
- plugin interfaces
- v62 plugin
- v71 plugin
- v72 plugin
- test catalog

## Out of scope

- execution runtime
- browser pages

## Dependencies

- WP_A complete
- WP_B storage basics available

## Touched documents

- DOMAIN_MODEL.md
- VERSIONING_AND_CAPABILITIES.md

## Touched projects/modules

- RavenMcp.Semantics.Abstractions
- RavenMcp.Semantics.Raven.V62
- RavenMcp.Semantics.Raven.V71
- RavenMcp.Semantics.Raven.V72

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement workspace/repo-line detection pipeline.
- Implement semantic plugin interface and router.
- Implement `RavenV62Semantics`.
- Implement `RavenV71Semantics`.
- Implement `RavenV72Semantics` and capability matrix generation.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- repo-line detection tests
- plugin routing tests
- capability matrix tests
- catalog tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0004
