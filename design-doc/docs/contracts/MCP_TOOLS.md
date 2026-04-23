# MCP_TOOLS.md

## Purpose
Define authoritative MCP tool families, request/response envelopes, stable IDs, idempotency rules, and status/result linkages for RavenDB Test Runner MCP Server.

## Scope
This file is normative for the MCP surface. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Host topology
### Streamable HTTP host
- primary MCP host
- long-lived, independent process model
- local-first localhost posture
- SHOULD support resumability-friendly behavior for event streams where practical

### stdio bridge host
- compatibility host only
- thin bridge into the shared orchestration core
- MUST keep `stdout` protocol-clean
- MAY log to `stderr`

## Shared request/response rules
### Shared request context
All core tools MAY accept:
- `requestId` (optional)
- `clientRequestId` (optional)
- `expertMode` (optional)
- `requestedBy` (optional)

These map to `ToolRequestContext`.

### Shared response envelope
All core tools MUST return `ToolResponseEnvelope` with:
- `ok`
- `requestId` (optional)
- `warnings`
- `versionSensitiveNotes`
- `result`

### Stable linkage fields
Where build and test entities link, the canonical field names are:
- `linkedBuildId`
- `linkedBuildPlanId`
- `linkedReadinessTokenId`
- `buildReuseDecision`
- `buildPolicyMode`

### Long-running operation handle rule
Tools that create a long-running lifecycle MUST return an `OperationHandle` with:
- `operationKind`
- `operationId`
- `statusTool`
- `outputTool`
- `resultTool`
- `cancelTool`
- `progressToken`
- `canCancel`

## build.* tools
### `build.graph.analyze`
Purpose: analyze solution/project graph and normalized build scope.

#### Request
- `workspacePath` or `workspaceId`
- `scope: BuildScope`
- `context: ToolRequestContext` (optional)

#### Response result
- `workspaceId`
- `normalizedScope: BuildScope`
- `scopeHash`
- `graphSummary`
- `selectedRoots`
- `capabilityNotes`
- `warnings`

#### Stable IDs returned
- `workspaceId`
- `scopeHash`

#### Idempotency
Idempotent for the same workspace snapshot and same normalized scope.

### `build.plan`
Purpose: generate a deterministic build plan and explicit reuse decision.

#### Request
- `workspacePath` or `workspaceId`
- `scope: BuildScope`
- `policy: BuildPolicy`
- `context: ToolRequestContext` (optional)

#### Response result
- `buildPlan: BuildPlan`
- `reuseDecision: BuildReuseDecision`
- `predictedArtifacts`
- `linkedReadinessTokenId` (optional)
- `reproCommandPreview`

#### Stable IDs returned
- `buildPlanId`
- `linkedReadinessTokenId` (optional)

#### Follow-up tools
- `build.run`
- `build.repro_command`
- `build.readiness`

#### Idempotency
Deterministic for the same workspace snapshot, normalized scope, and policy.

### `build.run`
Purpose: execute a build under explicit server-owned policy.

#### Request
One of:
- `buildPlanId`
- or inline planning payload containing `workspacePath|workspaceId`, `scope`, and `policy`

Optional:
- `context: ToolRequestContext`

#### Response result
- `buildId`
- `buildPlanId`
- `buildFingerprintId` (optional when known immediately)
- `linkedReadinessTokenId` (optional, not guaranteed immediately)
- `handle: OperationHandle`

#### Stable IDs returned
- `buildId`
- `buildPlanId`

#### Follow-up tools
- `build.status`
- `build.output_tail`
- `build.results`
- `build.cancel`
- `build.repro_command`

#### Cancellation behavior
Cancelable while lifecycle state is active.

#### Idempotency / dedupe
If `clientRequestId` is supplied, identical request + identical workspace snapshot SHOULD dedupe to the same `buildId` until terminal completion or explicit invalidation.

#### Artifact / repro relation
`build.results` is authoritative for artifacts and readiness. `build.repro_command` is authoritative for shell reproduction.

### `build.status`
Purpose: fetch build lifecycle status and partial summary.

#### Request
- `buildId`

#### Response result
- `status: BuildStatusSnapshot`
- `linkedReadinessTokenId` (optional)
- `latestArtifactIds`

#### Stable IDs returned
- `buildId`
- `linkedReadinessTokenId` (optional)

### `build.output_tail`
Purpose: fetch build stdout/stderr/merged output by cursor.

#### Request
- `buildId`
- `stream` (`stdout`, `stderr`, `merged`)
- `afterCursor` (optional)
- `maxLines`

#### Response result
- `page: OutputTailPage`

#### Stable IDs returned
- `buildId`
- `cursor`

### `build.results`
Purpose: fetch final or partial build result, artifacts, readiness token, and reuse decision.

#### Request
- `buildId`
- `includeArtifacts` (optional, default `true`)

#### Response result
- `buildExecution: BuildExecution`
- `buildResult: BuildResult` (optional until terminal or partial materialization)
- `artifacts: ArtifactRef[]`
- `linkedReadinessToken` (optional)
- `reuseDecision: BuildReuseDecision`

#### Stable IDs returned
- `buildId`
- `linkedReadinessTokenId` (optional)

### `build.cancel`
Purpose: cancel an active build.

#### Request
- `buildId`
- `reason` (optional)

#### Response result
- `buildId`
- `accepted`
- `state`

#### Idempotency
Idempotent after the first accepted cancellation request.

### `build.repro_command`
Purpose: return exact shell repro commands for the selected build.

#### Request
- `buildId`
- `shell` (`posix`, `powershell`, `cmd`)

#### Response result
- `reproCommands: ReproCommandSet`

### `build.clean`
Purpose: execute explicit clean / invalidation workflow.

#### Request
- `workspacePath` or `workspaceId`
- `scope: BuildScope`
- `mode` (`clean_outputs`, `invalidate_readiness`, `clean_and_rebuild`)
- `context: ToolRequestContext` (optional)

#### Response result
- `cleanOperationId`
- `affectedReadinessTokenIds`
- `handle: OperationHandle` (optional if long-running)

### `build.readiness`
Purpose: inspect readiness tokens and whether a future test run can reuse them.

#### Request
One of:
- `readinessTokenId`
- or `workspacePath|workspaceId + scope + configuration`

#### Response result
- `token` (optional)
- `reusable`
- `reuseDecision` (optional)
- `reasonCodes`

## tests.* tools
### `tests.preflight`
Purpose: evaluate selection, predicted skips, and build/test readiness without starting execution.

#### Request
- `workspacePath` or `workspaceId`
- `selector`
- `executionProfile`
- `buildPolicy`
- `linkedReadinessTokenId` (optional)
- `context: ToolRequestContext` (optional)

#### Response result
- `workspaceId`
- `selectionSummary: SelectionSummary`
- `predictedSkips`
- `runtimeUnknowns`
- `buildLinkage: BuildLinkage`
- `preflightWarnings`

### `tests.plan`
Purpose: create a deterministic run plan with explicit build linkage.

#### Request
- `workspacePath` or `workspaceId`
- `selector`
- `executionProfile`
- `buildPolicy`
- `linkedReadinessTokenId` (optional)
- `context: ToolRequestContext` (optional)

#### Response result
- `runPlan: RunPlan`
- `buildLinkage: BuildLinkage`
- `predictedArtifacts`
- `reproCommandPreview`

#### Stable IDs returned
- `runPlanId`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

### `tests.run`
Purpose: execute a test run with explicit build/test linkage.

#### Request
One of:
- `runPlanId`
- or inline planning payload containing `workspacePath|workspaceId`, `selector`, `executionProfile`, and build linkage inputs

Optional:
- `context: ToolRequestContext`

#### Response result
- `runId`
- `runPlanId`
- `buildLinkage: BuildLinkage`
- `handle: OperationHandle`

#### Stable IDs returned
- `runId`
- `runPlanId`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

#### Follow-up tools
- `tests.run_status`
- `tests.run_output_tail`
- `tests.run_results`
- `tests.cancel`
- `tests.repro_command`

#### Cancellation behavior
Cancelable while lifecycle state is active.

#### Idempotency / dedupe
If `clientRequestId` is supplied, identical request + identical workspace snapshot SHOULD dedupe to the same `runId` until terminal completion or explicit invalidation.

#### Build relation rule
`tests.run` MUST NOT silently rebuild ad hoc. It MUST either:
1. reference a supplied readiness token,
2. invoke the build subsystem under explicit build policy,
3. fail because policy forbids implicit build creation.

### `tests.run_status`
Purpose: fetch run lifecycle status and partial summary.

#### Request
- `runId`

#### Response result
- `status: RunStatusSnapshot`
- `selectionSummary: SelectionSummary`
- `latestArtifactIds`

### `tests.run_output_tail`
Purpose: fetch or stream run stdout/stderr/merged output by cursor.

#### Request
- `runId`
- `stream` (`stdout`, `stderr`, `merged`)
- `afterCursor` (optional)
- `maxLines`

#### Response result
- `page: OutputTailPage`

### `tests.run_results`
Purpose: fetch final or partial run results and artifacts.

#### Request
- `runId`
- `includeTests` (optional, default `true`)
- `includeArtifacts` (optional, default `true`)
- `includeAttempts` (optional, default `false`)

#### Response result
- `runExecution: RunExecution`
- `runResult: RunResult` (optional until terminal or partial materialization)
- `artifacts: ArtifactRef[]`
- `attemptResults` (optional)

### `tests.cancel`
Purpose: cancel an active test run.

#### Request
- `runId`
- `reason` (optional)

#### Response result
- `runId`
- `accepted`
- `state`

#### Idempotency
Idempotent after the first accepted cancellation request.

### `tests.repro_command`
Purpose: return exact shell repro commands for the selected run.

#### Request
- `runId`
- `shell` (`posix`, `powershell`, `cmd`)

#### Response result
- `reproCommands: ReproCommandSet`

### `tests.iterative_run`
Purpose: execute a flaky/iterative run policy while preserving build linkage and attempt history.

#### Request
- `workspacePath` or `workspaceId`
- `selector`
- `executionProfile`
- `buildPolicy`
- `linkedReadinessTokenId` (optional)
- `flakyPolicy`
- `context: ToolRequestContext` (optional)

#### Response result
- `runId`
- `runPlanId`
- `buildLinkage: BuildLinkage`
- `iterationPolicy`
- `handle: OperationHandle`

#### Stable IDs returned
- `runId`
- `runPlanId`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

#### Follow-up tools
- `tests.run_status`
- `tests.run_output_tail`
- `tests.run_results`
- `tests.compare_attempts`
- `tests.cancel`

#### Cancellation behavior
Cancellation applies to the iterative run lifecycle as a whole, not just the current attempt, unless future policy explicitly adds stop-after-current-attempt semantics.

## Symmetry and authoritative link rule
Build and test tool families MUST expose symmetrical status/result/follow-up behavior where intended:
- planning returns a plan object
- execution returns a stable lifecycle ID and `OperationHandle`
- status returns lifecycle progression
- output tail returns cursor-based output
- results returns terminal or partial outcome plus artifacts
- repro command returns authoritative shell reproduction data
- cancel returns cancellation acceptance state

## Validation requirements
- contract tests for all request/response shapes
- parity tests between Streamable HTTP and stdio bridge behavior
- progress/cancellation tests for `build.run`, `tests.run`, and `tests.iterative_run`
- regression tests for build linkage visibility in the tests surface
- wire compatibility tests proving two agents implementing from this file produce the same payload names and linkage fields
