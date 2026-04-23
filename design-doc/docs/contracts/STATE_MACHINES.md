# STATE_MACHINES.md

## Purpose
Define lifecycle state machines for builds, runs, attempts, and quarantine actions.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Build lifecycle
```text
created
  -> queued
  -> analyzing_graph
  -> resolving_reuse
  -> restoring (optional)
  -> building
  -> harvesting
  -> ready
  -> completed

active
  -> cancelling
  -> cancelled

active
  -> timeout_kill_pending
  -> timed_out

analyzing_graph/restoring/building/harvesting
  -> failed_terminal
```

## Build reuse branch
```text
created
  -> queued
  -> analyzing_graph
  -> resolving_reuse
  -> reused_existing_ready_build
  -> completed
```

## Run lifecycle
```text
created
  -> queued
  -> resolving_build_dependency
  -> preflighting
  -> executing
  -> harvesting
  -> normalizing
  -> completed

active
  -> cancelling
  -> cancelled

active
  -> timeout_kill_pending
  -> timed_out

executing/harvesting/normalizing
  -> failed_terminal
```

## Attempt lifecycle
```text
planned
  -> waiting_for_build
  -> executing
  -> analyzing
  -> completed

executing/analyzing
  -> failed
  -> cancelled
  -> timed_out
```

## Quarantine action lifecycle
```text
proposed
  -> approved
  -> applied
  -> reverted

proposed
  -> rejected
```

## Invariants
- A run MUST reference either a build readiness token, a linked build execution, or an explicit expert-mode skip-build decision.
- A reused build still emits a distinct build execution/result record with `status=reused` or equivalent reuse decision metadata.
- Quarantine actions MUST remain auditable and reversible.
