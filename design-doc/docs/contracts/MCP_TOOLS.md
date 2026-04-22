# MCP Tools Contract

## Purpose

Define the authoritative MCP tool surface.

## MCP host model

The system has two MCP entry points:
- primary local Streamable HTTP host
- optional stdio bridge host

Both hosts expose the same tool contracts over the shared orchestration core.

## Tool design principles

- tools are strongly typed
- progress is explicit for long-running operations
- cancellation is supported where applicable
- request/response payloads include version/capability context where needed
- raw filters are expert-mode only

## Common envelope fields

Responses SHOULD include:
- `workspaceId`
- `repoLine`
- `semanticPluginId`
- `runnerFamily`
- `frameworkFamily`
- `warnings[]`
- `versionSensitiveNotes[]`

## Tool list

### `tests.projects.list`

Purpose:
- list test topology and supported test projects

Request:
- `workspacePath`

Response:
- workspace summary
- projects
- runnable test assemblies
- repo line/capabilities

Idempotent:
- yes

### `tests.categories.list`

Purpose:
- list canonical categories, aliases, and trait values

Request:
- `workspacePath`

Response:
- category catalog

Idempotent:
- yes

### `tests.capabilities`

Purpose:
- expose version/capability envelope

Request:
- `workspacePath`

Response:
- repo line
- plugin
- capability matrix

Idempotent:
- yes

### `tests.discover`

Purpose:
- discover tests under a selector

Request:
- `workspacePath`
- `selector`
- `mode`: `static|runtime|hybrid`

Response:
- matching tests
- stability level
- requirements
- categories

Idempotent:
- yes

### `tests.preflight`

Purpose:
- perform non-executing readiness and skip prediction

Request:
- `workspacePath`
- `selector`
- `executionProfile`

Response:
- toolchain status
- compatibility warnings
- predicted skips
- runtime-unknowns

Idempotent:
- yes

### `tests.plan`

Purpose:
- produce deterministic run plan without executing it

Request:
- `workspacePath`
- `selector`
- `executionProfile`

Response:
- `RunPlan`
- explanation
- predicted artifacts
- repro command preview

Idempotent:
- yes

### `tests.run`

Purpose:
- start a run

Request:
- `workspacePath`
- `selector`
- `executionProfile`
- optional `clientRequestId`

Response:
- `runId`
- `planId`
- initial state
- progress token

Idempotent:
- conditionally, when `clientRequestId` is used

Long-running:
- yes

Cancelable:
- yes

### `tests.run_status`

Purpose:
- current state for a run

Request:
- `runId`

Response:
- run lifecycle state
- phase
- step position
- summary so far
- active attempt if any

Idempotent:
- yes

### `tests.run_output_tail`

Purpose:
- tail merged/stdout/stderr logs by cursor

Request:
- `runId`
- `stream`
- `afterCursor`
- `maxLines`

Response:
- lines
- next cursor
- truncation flag

Idempotent:
- yes

### `tests.run_results`

Purpose:
- read normalized run results and artifacts

Request:
- `runId`
- paging options
- include artifacts?
- include tests?
- include attempts?

Response:
- `RunResult`
- `NormalizedTestResult[]`
- `RunArtifact[]`

Idempotent:
- yes

### `tests.cancel`

Purpose:
- cancel an active run

Request:
- `runId`
- `reason`

Response:
- accepted?
- current state

Idempotent:
- yes

### `tests.rerun_failed`

Purpose:
- start a rerun for failed tests from a prior run

Request:
- `sourceRunId`
- mode: `plan|start`

Response:
- rerun plan or run id
- coarsening warnings if any

Idempotent:
- no, unless guarded by client request id

### `tests.iterative_run`

Purpose:
- execute repeated or policy-driven reruns for flaky analysis

Request:
- `workspacePath`
- `selector`
- `executionProfile`
- `flakyPolicy`

Response:
- `runId`
- policy summary
- progress token

Long-running:
- yes

Cancelable:
- yes

### `tests.flaky_analyze`

Purpose:
- analyze a completed run or historical record for flaky classification

Request:
- `runId` or selector + window

Response:
- `FlakyClassification`
- `StabilitySignal[]`
- mitigation suggestions

### `tests.flaky_history`

Purpose:
- retrieve historical outcome rollups for a selector

Request:
- `workspacePath`
- `selector`
- `window`

Response:
- `HistoricalOutcomeRollup[]`

### `tests.compare_attempts`

Purpose:
- compare attempt-level outcomes and signatures

Request:
- `runId`
- attempts[]

Response:
- attempt comparison view
- signal deltas
- classification hints

### `tests.stability_report`

Purpose:
- summarize stability metrics for a selector or category

### `tests.quarantine_candidates`

Purpose:
- list proposed or candidate quarantines

### `tests.explain_filter`

Purpose:
- show normalized selector / dotnet filter mapping

### `tests.explain_skip`

Purpose:
- show deterministic or runtime-sensitive skip reasoning

### `tests.repro_command`

Purpose:
- return reproducible command(s) and environment summary

## Expert mode filter rule

If `rawExpertFilter` is accepted:
- it must be explicit in the request
- it must be stored as non-canonical input
- the server must still emit normalized internal selector representation where possible

## Progress and cancellation

Long-running tools MUST:
- expose progress token or equivalent
- emit progress updates
- honor cancellation through the shared cancellation model

## Validation requirements

- schema tests for all tools
- progress behavior tests
- cancellation tests
- idempotency tests where applicable
- expert-mode filter validation tests
