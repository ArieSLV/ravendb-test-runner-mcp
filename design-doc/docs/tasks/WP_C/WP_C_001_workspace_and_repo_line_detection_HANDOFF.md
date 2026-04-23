# WP_C_001 workspace and repo line detection handoff

## Task
- `WP_C_001_workspace_and_repo_line_detection`

## Summary
- Added a new `RavenDB.TestRunner.McpServer.Semantics.Abstractions` project with bounded workspace scanning, repo-line normalization, line detection, branch routing, and capability-matrix projection contracts.
- Added bounded Raven line plugins for `v6.2`, `v7.1`, and `v7.2` that score branch/package/path evidence and project capability matrices without relying on branch name alone.
- Added a dedicated self-contained semantics validation harness with three workspace fixtures and capability-matrix snapshot checks.

## Scope and assumptions
- Detection is intentionally bounded to:
  - branch names from explicit input or `.git/HEAD`,
  - project/package markers from `.csproj` / `.props` / `.targets`,
  - path/token markers for `SlowTests/Issues`, AI embeddings, AI connection strings, AI agents, and AI test attributes.
- No external RavenDB documentation was needed for this task.
- No RavenDB API shape was inferred from mixed-version source snippets.

## Files changed
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/RavenDB.TestRunner.McpServer.Semantics.Abstractions.csproj`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/SemanticsAbstractionsAssemblyMarker.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/RepoLines.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/CapabilityNames.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/CapabilityMatrix.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/FrameworkFamilyHint.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspacePackageReference.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspaceScanOptions.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspaceInspection.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspaceInspector.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspaceLineCandidate.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspaceLineDetectionResult.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/ICapabilityProvider.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/ISemanticPlugin.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/IWorkspaceLineDetector.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/IBranchLineRouter.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/RavenSemanticsPluginBase.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/WorkspaceLineDetector.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/BranchLineRouter.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V62/RavenDB.TestRunner.McpServer.Semantics.Raven.V62.csproj`
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V62/RavenV62Semantics.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V71/RavenDB.TestRunner.McpServer.Semantics.Raven.V71.csproj`
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V71/RavenV71Semantics.cs`
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V72/RavenDB.TestRunner.McpServer.Semantics.Raven.V72.csproj`
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V72/RavenV72Semantics.cs`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/RavenDB.TestRunner.McpServer.Semantics.Tests.csproj`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/Program.cs`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/WorkspaceFixture.cs`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/Snapshots/v62.capability-matrix.json`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/Snapshots/v71.capability-matrix.json`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/Snapshots/v72.capability-matrix.json`
- `design-doc/docs/tasks/WP_C/WP_C_001_workspace_and_repo_line_detection_HANDOFF.md`

## Validation performed
- Command:
  - `$env:MSBuildSDKsPath='C:\Program Files\dotnet\sdk\10.0.203\Sdks'; $env:DOTNET_CLI_HOME=(Resolve-Path '.').Path; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; & 'C:\Program Files\dotnet\dotnet.exe' run --project '.\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj'`
- Validation coverage:
  - `v6.2` workspace fixture detection
  - `v7.1` workspace fixture detection
  - `v7.2` workspace fixture detection
  - capability-matrix snapshot checks for all three fixtures
  - one conflicting-branch fixture proving richer evidence can override a mismatched branch line
- Environment note:
  - local validation only succeeded after pinning `MSBuildSDKsPath` to `C:\Program Files\dotnet\sdk\10.0.203\Sdks` because the machine defaulted to .NET SDK `8.0.403`

## Risks and follow-ups
- The detector is intentionally heuristic and path/package based. It does not parse RavenDB-specific source symbols yet.
- New semantics projects were not added to `RavenDB.TestRunner.McpServer.sln` to avoid solution-global edits in this bounded task and because the worktree already had unrelated solution changes.
- Integrator follow-up:
  - add the new semantics/test projects to the solution once solution-level ownership is clear,
  - wire plugin construction into the future composition root when the owning WP lands.

## Contract/doc impact
- No contract document text was edited.
- Implementation aligns with:
  - `DECISION_FREEZE.md`
  - `VERSIONING_AND_CAPABILITIES.md`
  - `DOMAIN_MODEL.md`
  - `NAMING_AND_MODULE_POLICY.md`

## ADR impact
- none

## Recommended status
- `Done`

## Suggested progress-ledger update
- `WP_C_001 Done - added bounded workspace detection, branch line routing, v6.2/v7.1/v7.2 capability discovery, and fixture/snapshot validation harness; solution integration deferred to integrator.`

## Integrator acceptance
- The integrator added the semantics projects and dedicated validation harness to `RavenDB.TestRunner.McpServer.sln`.
- Central validation was rerun with the SDK 10 override:
  - solution build succeeded
  - `--no-build` execution of the semantics validation harness succeeded
