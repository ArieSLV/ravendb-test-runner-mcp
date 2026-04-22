# State Machines Contract

## Purpose

Define authoritative lifecycle states and valid transitions.

## Run lifecycle

### States

- `created`
- `analyzing_workspace`
- `preflighting`
- `queued`
- `restoring`
- `building`
- `discovering`
- `executing`
- `harvesting`
- `normalizing`
- `completed`
- `completed_with_failures`
- `failed_terminal`
- `cancelling`
- `cancelled`
- `timed_out`

### Transitions

```text
created
  -> analyzing_workspace
  -> preflighting
  -> queued
  -> restoring (optional)
  -> building (optional)
  -> discovering (optional)
  -> executing
  -> harvesting
  -> normalizing
  -> completed | completed_with_failures

Any active state
  -> cancelling
  -> cancelled

Any active state
  -> timed_out

restoring/building/discovering/executing/harvesting/normalizing
  -> failed_terminal
```

### Invariants

- `completed`, `completed_with_failures`, `failed_terminal`, `cancelled`, and `timed_out` are terminal.
- A run cannot transition from one terminal state to another.
- `cancelling` is non-terminal.

## Attempt lifecycle

### States

- `attempt_created`
- `attempt_preparing`
- `attempt_running`
- `attempt_harvesting`
- `attempt_analyzing`
- `attempt_completed`
- `attempt_failed`
- `attempt_cancelled`
- `attempt_timed_out`

### Transitions

```text
attempt_created
  -> attempt_preparing
  -> attempt_running
  -> attempt_harvesting
  -> attempt_analyzing
  -> attempt_completed | attempt_failed

attempt_running
  -> attempt_cancelled
  -> attempt_timed_out
```

## Workspace setup lifecycle

### States

- `setup_pending`
- `license_probe_running`
- `license_missing`
- `license_invalid`
- `storage_starting`
- `ready`
- `startup_failed`

### Rules

- The application MUST NOT advertise itself as fully ready before reaching `ready`.
- `license_missing` and `license_invalid` are recoverable via first-run setup flow.

## Flaky classification lifecycle

### States

- `not_evaluated`
- `analysis_running`
- `suspected_flaky`
- `likely_flaky`
- `confirmed_flaky`
- `likely_infra_issue`
- `likely_environment_issue`
- `inconclusive`
- `quarantine_proposed`
- `quarantine_accepted`
- `quarantine_revoked`

### Rules

- Deterministic skip states must not directly transition to a flaky class without explicit override.
- Quarantine states are orthogonal workflow states layered on top of analysis outputs.

## Cancellation semantics

- Run cancellation requests target the entire run.
- Iterative run cancellation stops future attempts and terminates the current active attempt/run process tree.
- Optional future enhancement: "stop after current attempt".

## Validation requirements

- run state transition tests
- attempt state transition tests
- startup setup-state tests
- invalid transition rejection tests
- cancellation path tests
- timeout path tests
