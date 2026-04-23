# DOMAIN_MODEL.md

## Purpose
Define the stable domain entities for builds, tests, runs, attempts, artifacts, and compatibility state.

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
All examples and type names are expressed for RavenDB Test Runner MCP Server. Internal implementation namespaces SHOULD follow `RavenDB.TestRunner.McpServer`.

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
- `captureCompactArtifactsAsAttachments`
- `thresholdBytes`
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
- `resultsDirectory`
- `createdAtUtc`

### BuildExecution
Fields:
- `buildId`
- `buildPlanId`
- `workspaceId`
- `state`
- `phase`
- `currentStepIndex`
- `startedAtUtc`
- `endedAtUtc`
- `buildFingerprintId`
- `readinessTokenId`
- `canCancel`

### BuildResult
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
- `executionProfile`
- `buildDecision`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)
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
- `linkedBuildId`
- `linkedReadinessTokenId`

### RunResult
Fields:
- `runId`
- `status`
- `summary`
- `failureClassification`
- `artifacts`
- `buildDecision`
- `buildReuseDecision`
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

## Shared envelopes
### ArtifactRef
Fields:
- `artifactId`
- `kind`
- `storageKind` (`filesystem`, `raven_attachment`)
- `pathOrAttachmentKey`
- `sizeBytes`
- `sha256`
- `sensitive`

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

## Validation requirements
- Domain contract tests MUST serialize/deserialize all major entities.
- Identity rules MUST be stable across process restarts.
- Build-to-run references MUST remain valid after restart and reconnect.
