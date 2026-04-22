# WP F MCP SURFACE

## Objective

Expose the shared core through local Streamable HTTP and optional stdio MCP hosts.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- tool handlers
- progress
- cancellation
- stdio protocol hygiene
- HTTP MCP surface
- shared handler reuse

## Out of scope

- browser API
- UI

## Dependencies

- WP_D and WP_E minimum behavior available

## Touched documents

- MCP_TOOLS.md
- STATE_MACHINES.md
- SECURITY_AND_REDACTION.md

## Touched projects/modules

- RavenMcp.Mcp.Host.Common
- RavenMcp.Mcp.Host.Stdio
- RavenMcp.Mcp.Host.Http

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Implement shared MCP handler layer.
- Implement local Streamable HTTP MCP host.
- Implement stdio bridge host with stdout discipline.
- Implement long-running progress and cancellation.
- Implement expert-mode filter handling with normalization safeguards.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- tool schema tests
- stdio stdout purity tests
- progress/cancel tests
- host parity tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0001
