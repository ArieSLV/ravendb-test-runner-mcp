# Event Model Contract

## Purpose

Define the typed event stream used by:
- browser live updates,
- MCP progress reporting,
- replay/reconnect behavior,
- audit trail persistence,
- flaky attempt analysis.

## Event design principles

- Events are append-only.
- Events are versioned by schema contract, not by silent field drift.
- Ordering is guaranteed per run stream, not globally across all runs.
- Events are emitted from authoritative state transitions, not speculative UI assumptions.

## Core event envelope

Every event MUST include:

- `eventId`
- `eventType`
- `occurredAtUtc`
- `workspaceId`
- `runId` when applicable
- `sequenceNumber` within the run stream when applicable
- `schemaVersion`
- `payload`

Optional:
- `attemptIndex`
- `stepIndex`
- `correlationId`
- `causationId`

## Event families

### Run lifecycle
- `run.created`
- `run.queued`
- `run.started`
- `run.phase_changed`
- `run.progress`
- `run.cancellation_requested`
- `run.cancelled`
- `run.completed`
- `run.failed`
- `run.timed_out`

### Step lifecycle
- `step.started`
- `step.output`
- `step.summary_updated`
- `step.completed`
- `step.failed`

### Test observation
- `test.result_observed`
- `test.skip_predicted`
- `test.skip_prediction_changed`

### Artifact lifecycle
- `artifact.available`
- `artifact.missing`
- `artifact.orphan_detected`

### Attempt lifecycle
- `attempt.started`
- `attempt.completed`
- `attempt.failed`
- `attempt.analysis_completed`

### Flaky lifecycle
- `flaky.analysis_completed`
- `flaky.quarantine_proposed`
- `flaky.quarantine_changed`

## Event schemas

### `run.created`
Payload:
- `planId`
- `repoLine`
- `frameworkFamily`
- `selectorSummary`

### `run.queued`
Payload:
- `queuePosition`

### `run.started`
Payload:
- `startedAtUtc`

### `run.phase_changed`
Payload:
- `previousPhase`
- `newPhase`

### `run.progress`
Payload:
- `current`
- `total`
- `message`

### `step.started`
Payload:
- `projectId`
- `commandPreview`
- `resultsDirectory`

### `step.output`
Payload:
- `stream`
- `cursor`
- `textChunk`
- `chunkIndex`

### `step.summary_updated`
Payload:
- `testsPassed`
- `testsFailed`
- `testsSkipped`

### `test.result_observed`
Payload:
- `testId`
- `status`
- `durationMs`
- `failureSignatureHash?`
- `skipReason?`

### `artifact.available`
Payload:
- `artifactId`
- `artifactKind`
- `storageKind`

### `attempt.started`
Payload:
- `attemptIndex`
- `effectiveProfile`

### `attempt.completed`
Payload:
- `attemptIndex`
- `summary`
- `classificationHint?`

### `flaky.analysis_completed`
Payload:
- `classification`
- `score`
- `reasonCodes[]`

## Ordering rules

- `sequenceNumber` MUST be monotonic within a run stream.
- `step.output` ordering is guaranteed within a stream for a given step.
- `test.result_observed` events may arrive before run completion.
- Attempt events are ordered relative to the attempt index for a given run.

## Replay rules

The system MUST support:
- replay from a stored checkpoint
- reconnect from the latest acknowledged event
- snapshot + replay recovery

The browser must be able to:
1. fetch the current run snapshot,
2. subscribe for new events,
3. catch up from the last seen sequence number.

## SignalR mapping

At minimum, the browser live layer MUST expose:
- workspace channel
- run channel
- optional artifact/diagnostics specific streams

## SSE mapping

SSE MAY be used for:
- read-only log or event streams
- diagnostics-only views
- fallback for simpler observers

## MCP progress relationship

MCP progress notifications are not the same as the browser event stream.
However, MCP progress must be derived from the same authoritative run state and SHOULD use the same internal event publication source where practical.

## Event retention

- Event metadata is retained in RavenDB
- Large text chunks MAY be summarized or referenced by cursor-backed log storage
- The authoritative audit trail for run state transitions must remain recoverable

## Validation requirements

- event schema tests
- event ordering tests
- replay tests
- reconnect tests
- browser snapshot + replay tests
- MCP progress consistency tests
