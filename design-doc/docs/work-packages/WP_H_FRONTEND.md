# WP H FRONTEND

## Objective

Implement the operator dashboard aligned with RavenDB Studio patterns.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- SPA shell
- runs list
- run details
- live console
- results explorer
- artifacts
- diagnostics
- flaky pages
- settings/profiles

## Out of scope

- remote multi-user auth
- cross-machine deployment

## Dependencies

- WP_G browser APIs and events available

## Touched documents

- FRONTEND_VIEW_MODELS.md
- WEB_API.md
- EVENT_MODEL.md

## Touched projects/modules

- RavenMcp.Web.Ui

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement app shell, routing, and global state.
- Implement runs list and run details pages.
- Implement live console and results explorer.
- Implement artifacts and diagnostics views.
- Implement flaky analysis and settings/profile pages aligned with Studio design patterns.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- view-model contract tests
- UI smoke tests
- reconnect tests
- virtualized log tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0005
- ADR_0006
