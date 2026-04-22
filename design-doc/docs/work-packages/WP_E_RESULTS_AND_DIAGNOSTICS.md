# WP E RESULTS AND DIAGNOSTICS

## Objective

Harvest artifacts, normalize results, classify failures, and expose diagnostics-friendly outputs.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- console capture
- trx/junit harvesting
- failure taxonomy mapping
- predicted vs actual reconciliation
- diagnostics escalation metadata

## Out of scope

- browser UI pages beyond contracts
- policy automation

## Dependencies

- WP_D execution path available
- WP_B artifact metadata available

## Touched documents

- ARTIFACTS_AND_RETENTION.md
- ERROR_TAXONOMY.md
- EVENT_MODEL.md

## Touched projects/modules

- RavenMcp.Results
- RavenMcp.Artifacts
- RavenMcp.Execution

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Capture console and stream outputs.
- Harvest TRX/JUnit and diagnostics artifacts.
- Implement normalized result model mapping.
- Implement failure taxonomy classification.
- Implement predicted-vs-actual reconciliation and diagnostics escalation hooks.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- artifact harvest tests
- result normalization tests
- failure taxonomy tests
- predicted-vs-actual tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0003
- ADR_0007
