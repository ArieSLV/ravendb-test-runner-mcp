# WP B STORAGE AND REGISTRY

## Objective

Implement RavenDB Embedded bootstrap, persistent run registry, artifact metadata, and retention-aware storage plumbing.

## Business / engineering purpose

This work package exists to make the system incrementally shippable while preserving frozen contracts and enabling parallel delivery.

## Exact scope

- embedded startup
- collections/indexes
- optimistic concurrency
- artifact metadata
- filesystem artifact root
- restart recovery

## Out of scope

- semantic parsing
- MCP hosts
- browser UI

## Dependencies

- WP_A complete

## Touched documents

- STORAGE_MODEL.md
- ARTIFACTS_AND_RETENTION.md
- SECURITY_AND_REDACTION.md

## Touched projects/modules

- RavenMcp.Storage.RavenEmbedded
- RavenMcp.Artifacts
- RavenMcp.Core

## Sub-deliverables

- implementation modules for this work package
- updated tests
- updated task statuses
- handoff notes
- contract or ADR updates if necessary

## Detailed TODO checkpoints

- Bootstrap RavenDB Embedded with license probing.
- Ensure database, collections, and indexes on startup.
- Implement mutable vs immutable document write patterns.
- Implement artifact metadata registration and filesystem mapping.
- Implement retention metadata and restart recovery checks.

## Acceptance criteria

- all listed checkpoints are satisfied
- required tests for this package pass
- no undocumented contract drift exists
- artifacts and outputs can be handed to the next package cleanly

## Required tests

- embedded bootstrap integration tests
- document id/index tests
- restart persistence tests
- retention routing tests

## Merge / handoff instructions

- merge only after passing the required tests
- update `docs/tasks/TASK_INDEX.md`
- include a handoff note using `docs/tasks/HANDOFF_TEMPLATE.md`
- call out all touched contracts explicitly
- note any deferred follow-up task

## Likely ADR touchpoints

- ADR_0002
- ADR_0003
