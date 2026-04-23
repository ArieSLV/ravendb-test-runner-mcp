# WEB_API.md

## Purpose
Define browser-facing HTTP APIs, SignalR hubs, and SSE/read-only stream endpoints for RavenDB Test Runner MCP Server.

## Scope
This file is normative for browser-facing surfaces. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Surface split
Browser-facing APIs are distinct from MCP APIs.

- MCP is for AI-agent integration.
- Browser-facing APIs are for the operator UI.
- Both surfaces project the same underlying authoritative registry and event model.

## Query endpoints
### Workspace and catalog
- `GET /api/workspaces`
- `GET /api/workspaces/{workspaceId}/capabilities`
- `GET /api/workspaces/{workspaceId}/projects`
- `GET /api/workspaces/{workspaceId}/categories`

### Build queries
- `GET /api/builds`
- `GET /api/builds/{buildId}`
- `GET /api/builds/{buildId}/results`
- `GET /api/builds/{buildId}/artifacts`
- `GET /api/builds/{buildId}/logs/{stream}?cursor=...`
- `GET /api/builds/{buildId}/readiness`
- `GET /api/builds/{buildId}/plan`

### Run queries
- `GET /api/runs`
- `GET /api/runs/{runId}`
- `GET /api/runs/{runId}/results`
- `GET /api/runs/{runId}/attempts`
- `GET /api/runs/{runId}/artifacts`
- `GET /api/runs/{runId}/logs/{stream}?cursor=...`
- `GET /api/runs/{runId}/plan`

### Flaky and settings
- `GET /api/flaky/{testId}/history`
- `GET /api/quarantine/actions/{actionId}`
- `GET /api/settings`

## Command endpoints
### Build commands
- `POST /api/builds/plan`
- `POST /api/builds`
- `POST /api/builds/{buildId}/cancel`
- `POST /api/builds/{buildId}/clean`
- `POST /api/builds/{buildId}/invalidate-readiness`

### Run commands
- `POST /api/runs/plan`
- `POST /api/runs`
- `POST /api/runs/{runId}/cancel`
- `POST /api/runs/{runId}/rerun-failed`
- `POST /api/runs/iterative`

### Flaky and settings commands
- `POST /api/flaky/analyze`
- `POST /api/quarantine/actions`
- `POST /api/settings/profiles`

## Compact response-shape notes
### `GET /api/runs`
Response shape:
- `items: RunListItem[]`
- `nextCursor` (optional)
- `totalCount` (optional)

Each `RunListItem` MUST include:
- `runId`
- `runPlanId`
- `state`
- `phase`
- `selectionSummary`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `startedAtUtc` (optional)
- `endedAtUtc` (optional)
- `canCancel`

### `GET /api/runs/{runId}`
Response shape:
- `view: RunDetailsView`

`RunDetailsView` MUST include:
- `runId`
- `runPlanId`
- `workspaceId`
- `state`
- `phase`
- `status` (optional until terminal)
- `selectionSummary`
- `executionProfileName`
- `linkedBuildId` (optional)
- `linkedBuildPlanId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `progress`
- `predictedSkips`
- `artifactSummary`
- `resultSummary`
- `failureClassification` (optional)
- `reproCommandSummary`

### `GET /api/runs/{runId}/results`
Response shape:
- `runId`
- `summary`
- `failureClassification` (optional)
- `buildLinkage`
- `resultRows: TestResultRow[]`
- `artifactSummary`

Each `TestResultRow` MUST include:
- `testId`
- `displayName`
- `fullyQualifiedName`
- `projectName`
- `status`
- `normalizedOutcome`
- `durationMs`
- `skipExplanation` (optional)
- `failureSignatureHash` (optional)
- `attemptIndexes` (optional)

### `GET /api/runs/{runId}/attempts`
Response shape:
- `runId`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)
- `attempts: AttemptSummaryView[]`

Each `AttemptSummaryView` MUST include:
- `attemptIndex`
- `status`
- `durationMs`
- `diagnosticEscalationLevel`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)
- `failureSignatureHash` (optional)

### `GET /api/runs/{runId}/artifacts`
Response shape:
- `runId`
- `items: ArtifactSummaryView[]`

Each `ArtifactSummaryView` MUST include:
- `artifactId`
- `artifactKind`
- `storageKind`
- `sizeBytes`
- `previewAvailable`
- `retentionClass`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

### `GET /api/runs/{runId}/plan`
Response shape:
- `runId`
- `plan: RunPlanInspectorView`

`RunPlanInspectorView` MUST include:
- `runPlanId`
- `selectionSummary`
- `executionProfileName`
- `buildPolicyMode`
- `linkedBuildId` (optional)
- `linkedBuildPlanId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `steps`
- `predictedSkips`

## Build response alignment rule
Build endpoints MAY return build-specific view models, but build and run responses MUST refer to the same underlying entity concepts:
- `linkedBuildId`
- `linkedReadinessTokenId`
- `buildReuseDecision`
- `failureClassification`
- `artifactSummary`

## Live transports
### SignalR hubs
- `/hubs/builds`
- `/hubs/runs`
- `/hubs/flaky`

SignalR is the primary browser live transport.

### SSE endpoints
- `GET /api/builds/{buildId}/events`
- `GET /api/runs/{runId}/events`
- `GET /api/builds/{buildId}/logs/{stream}/events`
- `GET /api/runs/{runId}/logs/{stream}/events`

SSE is supplementary and useful for read-only streams and cursor replay.

## Localhost security
- bind to localhost by default
- validate browser origin for local UI host
- distinguish browser session/auth from MCP session/auth behavior

## Validation requirements
- endpoint contract tests
- SignalR/SSE parity tests for event shapes
- log cursor tests
- localhost/origin validation tests
- build and run surface consistency tests
- UI contract tests proving build-linked run payloads are field-complete
