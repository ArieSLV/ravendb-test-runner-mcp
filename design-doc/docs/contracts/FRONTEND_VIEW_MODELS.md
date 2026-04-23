# FRONTEND_VIEW_MODELS.md

## Purpose
Define browser-facing view models for builds, runs, artifacts, diagnostics, policies, and flaky analysis.

## Scope
This file is normative for UI read-model shapes. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Build views
### BuildListItem
Fields:
- `buildId`
- `workspaceId`
- `scopeSummary`
- `state`
- `reuseDecision`
- `startedAtUtc`
- `durationMs`
- `linkedRunsCount`

### BuildDetailsView
Fields:
- `buildId`
- `phase`
- `steps`
- `reuseDecision`
- `readinessToken`
- `artifactSummary`
- `failureClassification`
- `reproCommand`

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
- `thresholdBytes`
- `binlogEnabled`
- `invalidations`

## Run views
### RunListItem
### RunDetailsView
### TestResultRow
### SkipExplanationView
### ReproCommandView

These MUST carry build linkage fields where relevant, such as:
- `linkedBuildId`
- `linkedReadinessTokenId`
- `buildReuseDecision`

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
### AttemptTimelineView
### QuarantineActionView

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
