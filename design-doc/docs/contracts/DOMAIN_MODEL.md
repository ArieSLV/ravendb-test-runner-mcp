# Domain Model Contract

## Purpose

Define the authoritative entity model used by:
- the shared orchestration core,
- RavenDB Embedded persistence,
- MCP responses,
- browser APIs,
- event payloads,
- flaky analytics.

## Scope

This contract defines:
- entities,
- identities,
- required fields,
- optional fields,
- invariants,
- authoritative ownership.

This file is authoritative for entity names and field semantics.
Persistence-specific details belong to `STORAGE_MODEL.md`.
Transport-specific field selection belongs to `MCP_TOOLS.md` and `WEB_API.md`.

## Shared identity rules

### Workspace identity

Canonical key:
- `workspaceId`

Derived from:
- normalized root path
- repository line
- current repository fingerprint scope

Invariant:
- one logical workspace root maps to one active workspace identity at a time.

### Test identity

Canonical stable identity:
- `testId`
- `projectId`
- `assemblyName`
- `targetFramework`
- `fullyQualifiedName`

Optional enrichment:
- `xunitUniqueId`
- `displayName`
- `sourceFilePath`
- `sourceLineNumber`

Invariant:
- `fullyQualifiedName` alone is not sufficient as the only stable identity key.

### Run identity

Canonical key:
- `runId`

Invariant:
- a run may contain multiple project steps
- a run may contain zero or more attempts
- run IDs are immutable

### Attempt identity

Canonical key:
- `runId` + `attemptIndex`

Invariant:
- attempt indexes are 1-based for iterative runs
- non-iterative runs MAY use implicit attempt index `0` in result models

## Entities

### WorkspaceSnapshot

Purpose:
- describe the currently analyzed workspace and its detected repository line

Required fields:
- `workspaceId`
- `rootPath`
- `repoLine`
- `gitSha`
- `branchName`
- `sdkVersion`
- `runnerFamily`
- `frameworkFamily`
- `semanticPluginId`
- `supportsAiEmbeddingsSemantics`
- `supportsAiConnectionStrings`
- `supportsAiAgentsSemantics`
- `supportsAiTestAttributes`
- `supportsSlowTestsIssues`
- `analyzedAtUtc`

Optional fields:
- `solutionPath`
- `globalJsonPath`
- `notes[]`

Invariants:
- `repoLine` is one of `v6.2`, `v7.1`, `v7.2`, `unsupported`
- capability fields are explicit booleans, not inferred ad hoc at call sites

### SemanticSnapshot

Purpose:
- store the analyzed semantic model derived from repository code and configs

Required fields:
- `semanticSnapshotId`
- `workspaceId`
- `pluginId`
- `snapshotVersion`
- `categoryCatalogVersion`
- `attributeRegistryVersion`
- `projectTopologyVersion`
- `createdAtUtc`

Optional fields:
- `compileSymbolHooks[]`
- `warnings[]`
- `versionSensitiveNotes[]`

Invariant:
- a semantic snapshot is immutable once published

### CompatibilityMatrix

Purpose:
- record capability decisions for the current workspace line and toolchain

Required fields:
- `compatibilityMatrixId`
- `workspaceId`
- `repoLine`
- `sdkVersion`
- `runnerFamily`
- `frameworkFamily`
- `adapterPackageVersion`
- `knownCapabilities`
- `versionSensitivePoints[]`
- `generatedAtUtc`

### TestProject

Required fields:
- `projectId`
- `name`
- `path`
- `role`
- `targetFrameworks[]`
- `assemblyName`
- `isRunnable`
- `references[]`

Role enum:
- `test`
- `infrastructure`
- `support`

### TestAssembly

Required fields:
- `assemblyId`
- `projectId`
- `assemblyName`
- `targetFramework`
- `outputStyle`
- `runnerConfigFiles[]`

OutputStyle enum:
- `library-style`
- `exe-style`

### TestCategory

Required fields:
- `categoryKey`
- `traitKey`
- `traitValue`
- `aliases[]`

Optional fields:
- `implies[]`
- `introducedInRepoLine`
- `deprecatedInRepoLine`

Invariant:
- canonical matching uses `traitKey` + `traitValue`

### TestRequirement

Required fields:
- `kind`
- `declaredBy`
- `runtimeSensitive`
- `confidenceClass`

Kind enum:
- `license`
- `nightly`
- `service`
- `cloud`
- `ai`
- `platform`
- `architecture`
- `intrinsics`
- `integration_toggle`
- `retry`
- `other`

### TestIdentity

Required fields:
- `testId`
- `projectId`
- `assemblyName`
- `targetFramework`
- `fullyQualifiedName`
- `classFqn`
- `methodName`
- `selectorStabilityLevel`

Optional fields:
- `displayName`
- `xunitUniqueId`
- `sourceFilePath`
- `sourceLineNumber`

SelectorStabilityLevel enum:
- `method-stable`
- `class-stable`
- `set-stable`
- `runtime-row`
- `unknown`

### EnvironmentProfile

Required fields:
- `environmentProfileId`
- `name`
- `inheritMode`
- `set`
- `unset[]`
- `repoSpecificFlags`
- `redactionRulesVersion`

InheritMode enum:
- `filtered`
- `empty`

### RunRequest

Required fields:
- `selector`
- `executionProfile`
- `requestOrigin`
- `requestedAtUtc`

Optional fields:
- `clientRequestId`
- `rawExpertFilter`
- `notes[]`

### RunPlan

Required fields:
- `planId`
- `workspaceId`
- `repoLine`
- `semanticPluginId`
- `runnerFamily`
- `frameworkFamily`
- `configuration`
- `steps[]`
- `predictedSelection`
- `predictedSkips[]`
- `generatedAtUtc`

### RunStep

Required fields:
- `stepIndex`
- `projectId`
- `cwd`
- `argv[]`
- `resultsDirectory`
- `environmentProfileId`

Optional fields:
- `dotnetFilter`
- `runSettingsPath`
- `artifactPolicy`
- `diagnosticsMode`

### RunExecution

Required fields:
- `runId`
- `planId`
- `state`
- `phase`
- `currentStepIndex`
- `stepCount`
- `startedAtUtc`

Optional fields:
- `completedAtUtc`
- `cancellationRequestedAtUtc`
- `failureClassification`
- `activePid`

### RunArtifact

Required fields:
- `artifactId`
- `runId`
- `artifactKind`
- `storageKind`
- `contentType`
- `sizeBytes`
- `sha256`
- `retentionClass`

Optional fields:
- `attemptIndex`
- `stepIndex`
- `filesystemPath`
- `attachmentDocumentId`
- `attachmentName`
- `previewExcerpt`
- `sensitive`

ArtifactKind enum:
- `plan`
- `command`
- `env`
- `console`
- `stdout`
- `stderr`
- `trx`
- `junit`
- `diag`
- `blame`
- `normalized`
- `repro`
- `compare`
- `other`

StorageKind enum:
- `filesystem`
- `ravendb-attachment`
- `ravendb-document`

### SkipPrediction

Required fields:
- `testId`
- `reasonCode`
- `confidence`
- `requiresRuntimeValidation`
- `message`

### FailureClassification

Required fields:
- `kind`
- `scope`
- `phase`
- `retriable`
- `userFacingSummary`

Kind enum:
- `workspace_invalid`
- `unsupported_repo_shape`
- `toolchain_unavailable`
- `restore_error`
- `build_error`
- `discovery_error`
- `adapter_error`
- `no_tests_matched`
- `all_selected_tests_skipped`
- `test_failures`
- `host_crashed`
- `hung`
- `cancelled`
- `timed_out`
- `artifact_parse_failed`
- `inconsistent_result_set`

### NormalizedTestResult

Required fields:
- `testId`
- `runId`
- `status`
- `durationMs`
- `declaredRequirements[]`
- `attemptIndex`
- `evidenceSources[]`

Optional fields:
- `skipReason`
- `failureMessage`
- `stackTrace`
- `displayName`
- `failureSignatureHash`

### RunResult

Required fields:
- `runId`
- `status`
- `summary`
- `failureClassification`
- `normalizationPrecision`
- `generatedAtUtc`

Optional fields:
- `predictedVsActual`
- `attemptSummary`
- `compatNotes[]`

### FlakyPolicy

Required fields:
- `mode`
- `maxAttempts`
- `overallBudgetMs`
- `perAttemptTimeoutMs`
- `freezeEnvironment`

Optional fields:
- `passThreshold`
- `failureThreshold`
- `escalateDiagnosticsAfterFailures`
- `fallbackToSequentialAfterInconsistency`
- `quarantineOnConfidenceAtOrAbove`

### IterativeRunRequest

Required fields:
- `selector`
- `baseExecutionProfile`
- `flakyPolicy`

Optional fields:
- `comparisonMode`
- `notes[]`

### AttemptPlan

Required fields:
- `runId`
- `attemptIndex`
- `effectiveExecutionProfile`
- `derivedFrom`
- `commandSteps[]`

### AttemptResult

Required fields:
- `runId`
- `attemptIndex`
- `status`
- `summary`
- `durationMs`
- `generatedAtUtc`

Optional fields:
- `failureClassification`
- `signatureHashes[]`

### StabilitySignal

Required fields:
- `signalKind`
- `confidence`
- `evidenceRefs[]`

SignalKind enum:
- `outcome_oscillation`
- `duration_spike`
- `failure_signature_drift`
- `parallelism_sensitivity`
- `environment_sensitivity`
- `selector_instability`
- `host_instability`
- `deterministic_skip`
- `deterministic_failure`
- `other`

### FlakyClassification

Required fields:
- `classification`
- `score`
- `reasonCodes[]`

Classification enum:
- `suspected_flaky`
- `likely_flaky`
- `confirmed_flaky`
- `likely_infra_issue`
- `likely_environment_issue`
- `inconclusive`
- `not_flaky`

### HistoricalOutcomeRollup

Required fields:
- `testId`
- `window`
- `passRate`
- `failureRate`
- `skipRate`
- `distinctFailureSignatures`
- `profilesSeen[]`
- `updatedAtUtc`

### QuarantineDecision

Required fields:
- `testId`
- `decision`
- `confidence`
- `reasonCodes[]`
- `createdAtUtc`
- `reversible`

Decision enum:
- `proposed`
- `accepted`
- `rejected`
- `revoked`

## Cross-entity invariants

- Every `RunExecution` references a valid `RunPlan`.
- Every `RunArtifact` references a valid `runId`.
- Every `AttemptResult` belongs to exactly one `runId`.
- Every `NormalizedTestResult` must map to a `TestIdentity`.
- `FailureClassification.kind = no_tests_matched` and `all_selected_tests_skipped` are mutually exclusive.
- Deterministic skip reasons must not be promoted to flaky classification without explicit override and evidence.

## Validation requirements

- DTO/schema contract tests
- serialization compatibility tests
- identity stability tests
- run/attempt linkage tests
- deterministic-skip-not-flaky tests
