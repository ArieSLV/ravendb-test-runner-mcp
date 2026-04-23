# FRONTEND_VIEW_MODELS.md

## Purpose
Define browser-facing view models for builds, runs, artifacts, diagnostics, policies, and flaky analysis.

## Scope
This file is normative for UI read-model shapes. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Shared supporting views
### ProgressView
Fields:
- `current`
- `total`
- `unit`
- `message`

### ArtifactSummaryView
Fields:
- `artifactId`
- `artifactKind`
- `storageKind`
- `sizeBytes`
- `previewAvailable`
- `retentionClass`
- `sensitive`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

### SelectionSummaryView
Fields:
- `projectCount`
- `assemblyCount`
- `exactMethodCount`
- `classSelectorCount`
- `categoryCount`
- `rawFilterUsed`
- `description`

## Build views
### BuildListItem
Fields:
- `buildId`
- `buildPlanId`
- `workspaceId`
- `scopeSummary`
- `state`
- `phase`
- `reuseDecision`
- `startedAtUtc`
- `endedAtUtc` (optional)
- `durationMs` (optional)
- `linkedRunsCount`
- `canCancel`

### BuildDetailsView
Fields:
- `buildId`
- `buildPlanId`
- `workspaceId`
- `state`
- `phase`
- `resultStatus` (optional)
- `failureClassification` (optional)
- `scopeSummary`
- `policyMode`
- `reuseDecision`
- `readinessTokenId` (optional)
- `progress: ProgressView`
- `artifactSummary: ArtifactSummaryView[]`
- `reproCommandSummary`
- `warnings`

### BuildGraphInspectorView
Fields:
- `buildPlanId`
- `graphSummary`
- `projectTargets`
- `configuration`
- `reasonForBuild`
- `policyExplanation`

### BuildPolicyView
Fields:
- `effectivePolicy`
- `reuseExplanation`
- `practicalAttachmentGuardrailBytes`
- `binlogEnabled`
- `invalidations`

## Run views
### RunListItem
Fields:
- `runId`
- `runPlanId`
- `workspaceId`
- `state`
- `phase`
- `status` (optional)
- `selectionSummary: SelectionSummaryView`
- `linkedBuildId` (optional)
- `linkedBuildPlanId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `startedAtUtc` (optional)
- `endedAtUtc` (optional)
- `durationMs` (optional)
- `resultCountsSummary` (optional)
- `canCancel`

### RunDetailsView
Fields:
- `runId`
- `runPlanId`
- `workspaceId`
- `state`
- `phase`
- `status` (optional)
- `selectionSummary: SelectionSummaryView`
- `executionProfileName`
- `linkedBuildId` (optional)
- `linkedBuildPlanId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `buildPolicyMode`
- `progress: ProgressView`
- `predictedSkips`
- `resultSummary`
- `artifactSummary: ArtifactSummaryView[]`
- `failureClassification` (optional)
- `startedAtUtc` (optional)
- `endedAtUtc` (optional)
- `attemptCount`
- `canCancel`

### RunPlanInspectorView
Fields:
- `runPlanId`
- `selectionSummary: SelectionSummaryView`
- `executionProfileName`
- `buildPolicyMode`
- `linkedBuildId` (optional)
- `linkedBuildPlanId` (optional)
- `linkedReadinessTokenId` (optional)
- `buildReuseDecision` (optional)
- `steps`
- `predictedSkips`
- `warnings`

### TestResultRow
Fields:
- `testId`
- `displayName`
- `fullyQualifiedName`
- `classFqn`
- `projectName`
- `categoryValues`
- `status`
- `normalizedOutcome`
- `durationMs`
- `linkedBuildId` (optional)
- `skipExplanation` (optional)
- `failureClassification` (optional)
- `failureSignatureHash` (optional)
- `attemptIndexes` (optional)
- `artifactSummary: ArtifactSummaryView[]`

### SkipExplanationView
Fields:
- `testId` (optional)
- `selectorSummary` (optional)
- `predictedReasonCodes`
- `predictedMessage`
- `actualReason` (optional)
- `deterministic`
- `requiresRuntimeValidation`
- `linkedRunId` (optional)
- `linkedBuildId` (optional)
- `evidence`

### ReproCommandView
Fields:
- `ownerKind` (`build`, `run`)
- `ownerId`
- `shell`
- `workingDirectory`
- `commands`
- `redactedEnvironmentDiff`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

### AttemptSummaryView
Fields:
- `attemptIndex`
- `status`
- `durationMs`
- `diagnosticEscalationLevel`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)
- `failureSignatureHash` (optional)

## Shared live views
### LiveConsoleLine
Fields:
- `cursor`
- `stream`
- `tsUtc`
- `text`
- `ownerKind` (`build`, `run`)
- `ownerId`

## Flaky views
### FlakyHistoryView
Fields:
- `testId`
- `classification`
- `score`
- `reasonCodes`
- `recentAttempts`
- `quarantineState` (optional)

### AttemptTimelineView
Fields:
- `runId`
- `attempts: AttemptSummaryView[]`
- `linkedBuildId` (optional)
- `linkedReadinessTokenId` (optional)

### QuarantineActionView
Fields:
- `quarantineActionId`
- `testId`
- `classification`
- `state`
- `policySource`
- `auditRefs`

## Required UI pages
- Builds list page
- Build details page
- Build graph / plan inspector
- Runs list page
- Run details page
- Live console/output viewer
- Results explorer
- Artifact explorer
- Diagnostics page
- Flaky analysis page
- Settings / policy page

## Validation requirements
- snapshot tests for view-model serialization
- UI contract tests against API payloads
- live update rendering tests for build and run pages
- regression tests ensuring run/test UI models remain as precise as build UI models
