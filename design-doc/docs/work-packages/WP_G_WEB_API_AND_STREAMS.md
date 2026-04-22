# WP G WEB API AND STREAMS

## Objective

Provide browser-facing REST, SignalR, SSE, log cursors, and run/event query surfaces.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- query endpoints
- command endpoints
- SignalR hub
- SSE endpoints
- cursor log APIs
- localhost defaults

## Out of scope

- full UI implementation details inside the browser

## Dependencies

- WP_E results available
- event contracts frozen

## Touched documents

- WEB_API.md
- EVENT_MODEL.md
- FRONTEND_VIEW_MODELS.md

## Touched projects/modules

- RavenMcp.Web.Api
- RavenMcp.Shared.Contracts

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement query endpoints for runs, artifacts, results, and capabilities.
- Implement write endpoints for runs, cancellation, iterative execution, and quarantine actions.
- Implement SignalR hub and event publication mapping.
- Implement SSE and cursor-based log stream endpoints.
- Implement localhost-only defaults and trusted-local posture.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- API contract tests
- SignalR event tests
- SSE tests
- cursor log tests
- localhost binding tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0005
- ADR_0006
