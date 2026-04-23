# DOMAIN_MODEL.md

## Purpose
Define the stable domain entities for builds, tests, runs, attempts, artifacts, capabilities, and shared transport envelopes.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Scope boundaries
This model covers:
- workspace and semantic snapshots,
- build subsystem entities,
- test catalog entities,
- run/attempt entities,
- flaky-analysis entities,
- shared envelopes used by MCP and browser-facing surfaces.

## Naming invariant
All examples and type names are expressed for **RavenDB Test Runner MCP Server**. Internal implementation namespaces SHOULD follow `RavenDB.TestRunner.McpServer`.

## Workspace entities
### WorkspaceSnapshot
Fields:
- `workspaceId`
- `rootPath`
- `branchName`
- `gitSha`
- `repoLine`
- `sdkVersion`
- `dirtyFingerprint`
- `semanticPluginId`
- `capabilityMatrixId`
- `createdAtUtc`

### SemanticSnapshot
Fields:
- `semanticSnapshotId`
- `workspaceId`
- `pluginId`
- `categoryCatalogVersion`
- `customAttributeRegistryVersion`
- `topologyHash`
- `supportsAiEmbeddingsSemantics`
- `supportsAiConnectionStrings`
- `supportsAiAgentsSemantics`
- `supportsAiTestAttributes`

### CapabilityMatrix
Fields:
- `capabilityMatrixId`
- `workspaceId`
- `repoLine`
- `frameworkFamily`
- `runnerFamily`
- `adapterFamily`
- `capabilities` (dictionary)
- `versionSensitivePoints`

## Build subsystem entities
### BuildScope
Describes what is being built.
Fields:
- `kind` (`solution`, `project`, `projects`, `directory`)
- `paths`
- `configuration`
- `targetFrameworks`
- `runtimeIdentifiers`
- `buildProperties`

### BuildPolicy
Fields:
- `mode` (`require_existing_ready_build`, `build_if_missing_or_stale`, `force_incremental_build`, `force_rebuild`, `expert_skip_build`)
- `allowImplicitRestore`
- `captureBinlog`
- `captureArtifactsAsAttachments`
- `practicalAttachmentGuardrailBytes`
- `cleanBeforeBuild`
- `reuseExistingReadiness`

### BuildFingerprint
Fields:
- `fingerprintId`
- `workspaceId`
- `repoLine`
- `gitSha`
- `dirtyFingerprint`
- `sdkVersion`
- `scopeHash`
- `configuration`
- `propertyHash`
- `relevantEnvHash`
- `dependencyInputsHash`
- `outputManifestHash`

### BuildReadinessToken
Semantic definition:
- `BuildReadinessToken.status` expresses whether outputs remain reusable for future work. It is not the same thing as execution progress and not the same thing as final build result.

Fields:
- `readinessTokenId`
- `buildId`
- `workspaceId`
- `fingerprintId`
- `scopeHash`
- `configuration`
- `createdAtUtc`
- `expiresAtUtc` (optional)
- `status` (`ready`, `superseded`, `invalidated`, `missing_outputs`)

### BuildRequest
Fields:
- `buildRequestId`
- `workspaceId`
- `scope`
- `policy`
- `requestedBy`
- `reason`
- `clientRequestId`

### BuildPlan
Fields:
- `buildPlanId`
- `workspaceId`
- `scope`
- `policy`
- `reuseDecision`
- `steps`
- `expectedArtifacts`
- `createdAtUtc`

### BuildExecution
Semantic definition:
- `BuildExecution.state` expresses lifecycle progression only. It answers the question “where is the build in its execution flow?”.

Fields:
- `buildId`
- `buildPlanId`
- `workspaceId`
- `state` (`created`, `queued`, `analyzing_graph`, `resolving_reuse`, `restoring`, `building`, `harvesting`, `finalizing_readiness`, `finalizing_reuse`, `completed`, `cancelling`, `cancelled`, `timeout_kill_pending`, `timed_out`, `failed_terminal`)
- `phase`
- `currentStepIndex`
- `startedAtUtc`
- `endedAtUtc`
- `buildFingerprintId`
- `readinessTokenId`
- `canCancel`

### BuildResult
Semantic definition:
- `BuildResult.status` expresses the final execution outcome. It answers the question “what outcome did the completed build record produce?”.

Fields:
- `buildId`
- `status` (`succeeded`, `failed`, `cancelled`, `timed_out`, `reused`, `invalid`)
- `failureClassification`
- `outputsManifest`
- `artifacts`
- `reproCommand`
- `reuseDecision`
- `warnings`

### BuildReuseDecision
Fields:
- `decision` (`reused_existing`, `rebuilt_stale`, `rebuilt_missing`, `rebuilt_forced`, `rejected_existing`, `skipped_by_policy`)
- `reasonCodes`
- `existingBuildId`
- `newBuildRequired`

### Build lifecycle mapping examples
| Execution state path | Result status | Readiness status |
|---|---|---|
| `... -> finalizing_readiness -> completed` | `succeeded` | `ready` |
| `... -> finalizing_reuse -> completed` | `reused` | `ready` |
| `... -> failed_terminal` | `failed` | absent or unchanged |
| `... -> cancelled` | `cancelled` | absent or unchanged |
| `... -> timed_out` | `timed_out` | absent or unchanged |

## Test catalog entities
### TestProject
Fields:
- `projectId`
- `name`
- `path`
- `assemblyName`
- `targetFrameworks`
- `projectOutputStyle`

### TestAssembly
Fields:
- `assemblyId`
- `projectId`
- `assemblyName`
- `targetFramework`
- `outputPathPattern`

### TestCategory
Fields:
- `categoryKey`
- `traitKey`
- `traitValue`
- `aliases`
- `implies`
- `repoLineSupport`

### TestRequirement
Fields:
- `kind`
- `declaredBy`
- `environmentKeys`
- `runtimeOnly`
- `confidence`

### TestIdentity
Fields:
- `testId`
- `projectId`
- `assemblyId`
- `fullyQualifiedName`
- `classFqn`
- `methodName`
- `selectorStabilityLevel`
- `xunitUniqueId` (optional)
- `sourceFilePath` (optional)
- `sourceLineNumber` (optional)

## Shared build linkage entities
### BuildLinkage
Fields:
- `linkedBuildId` (optional)
- `linkedBuildPlanId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `buildPolicyMode`

### SelectionSummary
Fields:
- `projectCount`
- `assemblyCount`
- `exactMethodCount`
- `classSelectorCount`
- `categoryCount`
- `rawFilterUsed`
- `description`

## Run entities
### RunRequest
Fields:
- `runRequestId`
- `workspaceId`
- `selector`
- `executionProfile`
- `buildPolicy`
- `buildReadinessTokenId` (optional)
- `clientRequestId`

### RunPlan
Fields:
- `runPlanId`
- `workspaceId`
- `selector`
- `selectionSummary`
- `executionProfile`
- `buildLinkage`
- `steps`
- `predictedSkips`

### RunExecution
Fields:
- `runId`
- `runPlanId`
- `workspaceId`
- `state`
- `phase`
- `currentStepIndex`
- `startedAtUtc`
- `endedAtUtc`
- `buildLinkage`

### RunResult
Fields:
- `runId`
- `status`
- `summary`
- `failureClassification`
- `artifacts`
- `buildLinkage`
- `normalizedTests`

## Iterative/flaky entities
### FlakyPolicy
Fields:
- `mode`
- `maxAttempts`
- `overallBudgetMs`
- `perAttemptTimeoutMs`
- `escalateDiagnosticsAfterFailures`
- `fallbackToSequentialAfterInconsistency`
- `freezeEnvironment`
- `allowQuarantineAction`

### IterativeRunRequest
Fields:
- `iterativeRunId`
- `workspaceId`
- `selector`
- `executionProfile`
- `buildPolicy`
- `flakyPolicy`

### AttemptPlan
Fields:
- `attemptIndex`
- `linkedRunPlanId`
- `effectiveProfile`
- `linkedBuildId`
- `linkedReadinessTokenId`
- `diagnosticEscalationLevel`

### AttemptResult
Fields:
- `attemptIndex`
- `status`
- `durationMs`
- `failureSignatureHash`
- `artifacts`
- `buildContext`

### StabilitySignal
Fields:
- `kind`
- `confidence`
- `evidenceRefs`

### FlakyClassification
Fields:
- `kind`
- `score`
- `reasonCodes`
- `automatable`

### QuarantineAction
Fields:
- `quarantineActionId`
- `testId`
- `classification`
- `policySource`
- `state` (`proposed`, `approved`, `applied`, `reverted`, `rejected`)
- `auditRefs`

## Shared transport and contract envelopes
### ToolRequestContext
Fields:
- `requestId` (optional)
- `clientRequestId` (optional)
- `expertMode` (optional)
- `requestedBy` (optional)

### ToolResponseEnvelope
Fields:
- `ok`
- `requestId` (optional)
- `warnings`
- `versionSensitiveNotes`
- `result`

### OperationHandle
Fields:
- `operationKind` (`build`, `run`, `iterative_run`)
- `operationId`
- `statusTool`
- `outputTool`
- `resultTool`
- `cancelTool`
- `progressToken`
- `canCancel`

### ProgressSnapshot
Fields:
- `current`
- `total`
- `unit`
- `message`

### BuildStatusSnapshot
Fields:
- `buildId`
- `buildPlanId`
- `state`
- `phase`
- `progress`
- `buildLinkage`
- `resultStatus` (optional)
- `failureClassification` (optional)
- `canCancel`

### RunStatusSnapshot
Fields:
- `runId`
- `runPlanId`
- `state`
- `phase`
- `progress`
- `buildLinkage`
- `resultStatus` (optional)
- `failureClassification` (optional)
- `canCancel`

### OutputTailRequest
Fields:
- `ownerKind` (`build`, `run`)
- `ownerId`
- `stream` (`stdout`, `stderr`, `merged`)
- `afterCursor` (optional)
- `maxLines`

### OutputTailPage
Fields:
- `ownerKind`
- `ownerId`
- `stream`
- `cursor`
- `lines`
- `truncated`

### ReproCommandSet
Fields:
- `ownerKind`
- `ownerId`
- `shell`
- `workingDirectory`
- `commands`
- `redactedEnvironmentDiff`

### ArtifactRef
Fields:
- `artifactId`
- `kind`
- `storageKind` (`raven_attachment`, `deferred_external`)
- `locator`
- `attachmentName` (optional)
- `sizeBytes`
- `sha256`
- `sensitive`
- `previewAvailable`

### FailureClassification
Fields:
- `kind`
- `scope`
- `phase`
- `retriable`
- `suggestedAction`

## Invariants
1. Build IDs and run IDs are distinct lifecycles.
2. A run MAY reference a build, but a build MUST NOT be hidden inside a run without persistence.
3. Build reuse MUST always produce an explicit `BuildReuseDecision`.
4. Raw filters MUST NOT be canonical internal identity.
5. Deterministic skips MUST NOT be classified as flaky.
6. All MCP and browser payloads for core build/run operations MUST map to named domain objects or stable envelopes defined here.

## Validation requirements
- Domain contract tests MUST serialize/deserialize all major entities.
- Identity rules MUST be stable across process restarts.
- Build-to-run references MUST remain valid after restart and reconnect.
- Mapping between `BuildExecution.state`, `BuildResult.status`, and `BuildReadinessToken.status` MUST be test-covered.
