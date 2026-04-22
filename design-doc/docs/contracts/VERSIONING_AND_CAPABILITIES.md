# Versioning and Capabilities Contract

## Purpose

Define repository line detection, semantic plugin routing, and capability-based behavior.

## Scope

This contract governs:
- supported RavenDB lines
- semantic plugin identities
- capability names
- plugin responsibilities
- forward-extension rules

## Authoritative rule

All version-specific behavior MUST flow through:
- repository line detection
- semantic plugin routing
- explicit capabilities

Direct ad-hoc branching on raw version strings inside unrelated subsystems is forbidden.

## Supported lines in v1

- `v6.2`
- `v7.1`
- `v7.2`

## Repository line detection

Detection inputs:
- repository branch name if authoritative
- `global.json`
- package matrix
- test topology
- semantic plugin signatures
- known project/config markers

Detection result:
- `repoLine`
- `detectionConfidence`
- `versionSensitiveNotes[]`

## Semantic plugin IDs

- `RavenV62Semantics`
- `RavenV71Semantics`
- `RavenV72Semantics`

## Shared semantic plugin interface

Every plugin MUST implement capabilities for:

- workspace applicability check
- category extraction
- custom attribute detection
- requirement extraction
- retry semantics extraction
- branch capability discovery
- topology enrichment
- compatibility notes
- plugin-specific validation warnings

## Capability vocabulary

### Core capabilities

- `supportsSlowTestsIssues`
- `supportsAiEmbeddingsSemantics`
- `supportsAiConnectionStrings`
- `supportsAiAgentsSemantics`
- `supportsAiTestAttributes`
- `supportsXunitV3SourceInfo`
- `supportsRunSettingsOverrides`
- `supportsDirectTestDriverCoupling`
- `supportsNightlyWindowControls`
- `supportsRavenRetryAttributes`
- `supportsCloudRequirementChecks`

### Storage and diagnostics capabilities

- `supportsCompactArtifactAttachments`
- `supportsRestartRecovery`
- `supportsAttemptComparisonArtifacts`

### UI and operator capabilities

- `supportsRunPlanExplainability`
- `supportsLiveLogStreaming`
- `supportsFlakyHeatmap`
- `supportsQuarantineSuggestions`

## Baseline capability matrix

| Capability | v6.2 | v7.1 | v7.2 |
|---|---|---|---|
| supportsSlowTestsIssues | false | true | true |
| supportsAiEmbeddingsSemantics | false | true | true |
| supportsAiConnectionStrings | false | true | true |
| supportsAiAgentsSemantics | false | true | true |
| supportsAiTestAttributes | false | true | true |
| supportsXunitV3SourceInfo | false | false | true |
| supportsRunSettingsOverrides | true | true | true |
| supportsDirectTestDriverCoupling | true | true | false |
| supportsNightlyWindowControls | true | true | true |
| supportsRavenRetryAttributes | true | true | true |
| supportsCloudRequirementChecks | true | true | true |

## Version-sensitive notes

The following are version-sensitive and MUST be validated by integration tests:

- theory-row identity precision
- exact xUnit v2 vs xUnit v3 metadata richness
- adapter-emitted source information
- AI test attribute coverage in `v7.1`
- exact TestDriver coupling behavior in `v7.1`

## Future version extension rule

To add a new line such as `v7.3` or `v8.0`:

1. create a new plugin package, e.g. `RavenV73Semantics`
2. define capability deltas
3. add repository detection rules
4. add cross-branch tests
5. update compatibility matrix
6. add ADR only if architecture changes, not for routine capability extension

## What is authoritative

Authoritative:
- plugin ID
- capability names
- repoLine value
- plugin responsibility boundary

Not authoritative here:
- persistence document layout
- transport field subsets

## Validation requirements

- branch-detection tests
- plugin-routing tests
- capability matrix tests
- unsupported-line graceful failure tests
- future-line placeholder tests
