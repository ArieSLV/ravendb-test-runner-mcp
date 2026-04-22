# Frontend View Models Contract

## Purpose

Define the browser-facing view models used by the operator UI.

## Design principles

- frontend state is derived from authoritative backend state
- browser view models may aggregate data from multiple domain entities
- view models are stable contracts for UI feature work

## View models

### `RunListItem`

Fields:
- `runId`
- `workspaceId`
- `repoLine`
- `selectorSummary`
- `state`
- `phase`
- `startedAtUtc`
- `durationMs`
- `summaryCounts`
- `activeAttemptIndex?`

### `RunDetailsView`

Fields:
- `runId`
- `planId`
- `repoLine`
- `frameworkFamily`
- `phase`
- `state`
- `currentStepIndex`
- `stepCount`
- `compatWarnings[]`
- `summary`
- `failureClassification?`
- `predictedVsActual?`

### `StepProgressView`

Fields:
- `stepIndex`
- `projectName`
- `state`
- `commandPreview`
- `resultsDirectory`
- `artifactsReady[]`
- `durationMs?`

### `LiveConsoleLine`

Fields:
- `cursor`
- `stream`
- `ts`
- `text`
- `sensitiveRedactionApplied`

### `TestResultRow`

Fields:
- `testId`
- `displayName`
- `status`
- `durationMs`
- `attemptIndexes[]`
- `categoryKeys[]`
- `projectName`
- `skipReason?`
- `failureSignatureHash?`

### `FailureDetailsView`

Fields:
- `testId`
- `failureMessage`
- `stackTrace`
- `failureClassification`
- `attemptComparisons[]`

### `ArtifactLinkView`

Fields:
- `artifactId`
- `artifactKind`
- `storageKind`
- `sizeBytes`
- `retentionClass`
- `downloadUrl`
- `previewAvailable`
- `sensitive`

### `ReproCommandView`

Fields:
- `shell`
- `commands[]`
- `argv[][]`
- `environmentSummary`
- `warnings[]`

### `SkipExplanationView`

Fields:
- `testId`
- `reasonCode`
- `confidence`
- `requiresRuntimeValidation`
- `message`
- `inputsUsed[]`

### `FlakyHistoryView`

Fields:
- `testId`
- `window`
- `classification`
- `score`
- `passRate`
- `failureRate`
- `skipRate`
- `distinctFailureSignatures`
- `attemptTimeline[]`
- `mitigationRecommendations[]`

### `AttemptComparisonView`

Fields:
- `runId`
- `attempts[]`
- `signals[]`
- `environmentDiffs[]`
- `durationComparison`
- `signatureComparison`

### `RunPlanInspectorView`

Fields:
- `planId`
- `selectorExplanation`
- `normalizedFilters[]`
- `steps[]`
- `predictedSkips[]`
- `environmentProfileSummary`

## Validation requirements

- frontend model snapshot tests
- browser contract tests
- partial-state rendering tests
- nullability/optional-field tests
