# VERSIONING_AND_CAPABILITIES.md

## Purpose
Define version plugins, capability routing, and branch-aware compatibility rules.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Supported repo lines
- `v6.2`
- `v7.1`
- `v7.2`

## Plugin model
### Shared abstractions
The shared core depends on plugin contracts only:
- `IWorkspaceLineDetector`
- `ISemanticPlugin`
- `ICapabilityProvider`
- `IResultNormalizationHintsProvider`
- `IBranchBuildHintsProvider` (optional if needed)

### Required plugins
- `RavenV62Semantics`
- `RavenV71Semantics`
- `RavenV72Semantics`

### Future plugins
Future repo lines MUST be added as peer plugins rather than by mutating prior plugin assumptions.

## Capability matrix fields
Minimum required capabilities:
- `frameworkFamily`
- `runnerFamily`
- `adapterFamily`
- `supportsSlowTestsIssuesProject`
- `supportsAiEmbeddingsSemantics`
- `supportsAiConnectionStrings`
- `supportsAiAgentsSemantics`
- `supportsAiTestAttributes`
- `supportsXunitV3SourceInfo`
- `supportsBuildGraphSpecialCases` (optional)

## Baseline compatibility table
| Repo line | Framework family | AI baseline | Notes |
|---|---|---|---|
| `v6.2` | `xunit.v2` | absent | no AI-specific test semantics |
| `v7.1` | transitional | present in richer form | AI Agents may exist |
| `v7.2` | modern baseline | fully expected | modern AI-capable baseline |

## Detection rules
Detection SHOULD use a combination of:
- branch name / repo line,
- file existence,
- package versions,
- semantic markers,
- capability scan results.

Detection MUST NOT rely solely on a single branch name string if richer evidence is available.

## Version-sensitive notes
- Result normalization may differ across xUnit v2/v3-era metadata surfaces.
- AI capability detection for future branches MUST remain capability-based rather than hard-coded to a version threshold.

## Validation requirements
- Integration fixtures MUST exist for v6.2, v7.1, and v7.2.
- Capability matrix snapshots MUST be regression-tested.
