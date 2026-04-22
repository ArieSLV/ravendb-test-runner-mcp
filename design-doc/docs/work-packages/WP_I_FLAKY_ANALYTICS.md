# WP I FLAKY ANALYTICS

## Objective

Implement iterative execution, attempt comparison, scoring, and quarantine workflow.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- iterative planning
- attempt lifecycle
- comparison engine
- signals
- classification
- quarantine proposals/acceptance
- history rollups

## Out of scope

- team-wide quarantine policy service

## Dependencies

- WP_D, WP_E, WP_G available

## Touched documents

- DOMAIN_MODEL.md
- ERROR_TAXONOMY.md
- WEB_API.md
- MCP_TOOLS.md
- FRONTEND_VIEW_MODELS.md

## Touched projects/modules

- RavenMcp.Flaky
- RavenMcp.Results
- RavenMcp.Web.Api
- RavenMcp.Web.Ui

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement iterative run request and attempt planning.
- Implement attempt persistence and comparison engine.
- Implement stability signals and scoring.
- Implement quarantine proposal/accept/revoke workflow.
- Implement flaky history/report surfaces for MCP and browser UI.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- iterative execution tests
- attempt comparison tests
- classification tests
- quarantine audit tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0007
