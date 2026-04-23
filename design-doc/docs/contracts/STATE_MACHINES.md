# STATE_MACHINES.md

## Purpose
Define lifecycle state machines for builds, runs, attempts, and quarantine actions.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Vocabulary rule
This file distinguishes three related but different concepts for builds:
- `BuildExecution.state` expresses lifecycle progression
- `BuildResult.status` expresses the final execution outcome
- `BuildReadinessToken.status` expresses whether outputs remain reusable for future work

Readers MUST NOT collapse these into a single state vocabulary.

## Build lifecycle
```text
created
  -> queued
  -> analyzing_graph
  -> resolving_reuse
  -> restoring (optional)
  -> building
  -> harvesting
  -> finalizing_readiness
  -> completed

active
  -> cancelling
  -> cancelled

active
  -> timeout_kill_pending
  -> timed_out

analyzing_graph/restoring/building/harvesting/finalizing_readiness
  -> failed_terminal
```

## Build reuse branch
```text
created
  -> queued
  -> analyzing_graph
  -> resolving_reuse
  -> finalizing_reuse
  -> completed
```

## Build lifecycle-to-result mapping
| `BuildExecution.state` terminal path | `BuildResult.status` | `BuildReadinessToken.status` |
|---|---|---|
| `completed` after `finalizing_readiness` | `succeeded` | `ready` |
| `completed` after `finalizing_reuse` | `reused` | `ready` |
| `failed_terminal` | `failed` | unchanged or absent |
| `cancelled` | `cancelled` | unchanged or absent |
| `timed_out` | `timed_out` | unchanged or absent |

### Normative example
A build can end with:
- `BuildExecution.state = completed`
- `BuildResult.status = reused`
- `BuildReadinessToken.status = ready`

This means the lifecycle completed successfully **without** executing a new material build, while still yielding reusable outputs.

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
- A reused build still emits a distinct `BuildExecution` and `BuildResult` record, but `BuildResult.status=reused` is separate from lifecycle completion.
- Readiness invalidation is separate from build execution failure.
- Quarantine actions MUST remain auditable and reversible.
