# EVENT_MODEL.md

## Purpose
Define typed event streams for build, run, attempt, artifact, and quarantine lifecycles.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Event envelope
Every event MUST include:
- `eventId`
- `streamKind`
- `ownerId`
- `type`
- `sequence`
- `tsUtc`
- `payload`

`sequence` is authoritative only within a stream.

## Stream families
- `build/<build-id>`
- `run/<run-id>`
- `attempt/<run-id>/<attempt-index>`
- `workspace/<workspace-id>/catalog`
- `quarantine/<test-id>`

## Build events
- `build.created`
- `build.queued`
- `build.started`
- `build.phase_changed`
- `build.progress`
- `build.target_started`
- `build.output`
- `build.cache_hit`
- `build.cache_miss`
- `build.artifact_available`
- `build.completed`
- `build.failed`
- `build.cancelled`
- `build.timed_out`
- `build.readiness_issued`
- `build.readiness_invalidated`

## Run events
- `run.created`
- `run.queued`
- `run.started`
- `run.phase_changed`
- `run.progress`
- `run.output`
- `run.summary_updated`
- `test.result_observed`
- `run.artifact_available`
- `run.completed`
- `run.failed`
- `run.cancelled`
- `run.timed_out`

## Attempt events
- `attempt.started`
- `attempt.completed`
- `attempt.failed`
- `attempt.diff_available`
- `flaky.analysis_completed`

## Quarantine events
- `quarantine.proposed`
- `quarantine.approved`
- `quarantine.applied`
- `quarantine.reverted`
- `quarantine.rejected`

## Delivery rules
- SignalR is the primary browser event transport.
- SSE MAY be used for read-only streams and cursor replay.
- MCP progress notifications are a projection of the same underlying lifecycle state, not a separate source of truth.

## Replay rules
- Replay is cursor-based.
- Event checkpoints MUST be persisted.
- Reconnect MUST use checkpoint or `Last-Event-ID`-style cursor semantics where applicable.

## Validation requirements
- Ordering tests per stream family.
- Reconnect tests for build and run streams.
- Cursor replay tests for logs and event streams.
