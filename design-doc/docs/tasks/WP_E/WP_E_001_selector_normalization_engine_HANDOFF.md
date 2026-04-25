# WP_E_001 Handoff

## Completed task
`WP_E_001_selector_normalization_engine`

## What changed
- Added the foundational `RavenDB.TestRunner.McpServer.TestExecution` project.
- Added the focused `RavenDB.TestRunner.McpServer.TestExecution.Tests` project.
- Implemented deterministic structured selector normalization:
  - trims selector values;
  - rejects empty selector values after trimming;
  - deduplicates with ordinal comparison;
  - sorts unordered selector sets with ordinal ordering;
  - emits stable structured and request identities.
- Implemented expert-mode raw filter isolation:
  - raw filters are rejected without explicit expert mode;
  - raw filters are preserved as expert-only payload when expert mode is enabled;
  - raw filter values are not included in canonical structured selector identity.
- Added a selector summary surface with project, assembly, class, method, category, and raw-filter counts plus stable description text.
- Added a small build-boundary contract that keeps hidden build execution forbidden and records that build orchestration remains owned by the build subsystem.

## Corrective passes
- Review finding fixed: `expert_skip_build` at the test-execution build boundary now requires explicit expert mode.
- `TestExecutionBuildBoundary.Validate(...)` rejects `expert_skip_build` with `ExpertMode=false` using `expert_mode_required`.
- `expert_skip_build_accepted` is emitted only when `ExpertMode=true`.
- Hidden-build rejection and non-expert build policy modes remain unchanged.

## Touched contracts
- Aligned with `DOMAIN_MODEL.md`:
  - raw filters are not canonical internal identity;
  - selector summaries expose stable counts and descriptions for later run planning.
- Aligned with `BUILD_SUBSYSTEM.md`:
  - test execution does not perform hidden builds;
  - build policy ownership remains with the build subsystem.
- Aligned with `ERROR_TAXONOMY.md` by preserving deterministic validation reason codes for selector and boundary failures.

## Touched modules/files
- `RavenDB.TestRunner.McpServer.sln`
- `src/RavenDB.TestRunner.McpServer.TestExecution/RavenDB.TestRunner.McpServer.TestExecution.csproj`
- `src/RavenDB.TestRunner.McpServer.TestExecution/SelectorNormalization.cs`
- `src/RavenDB.TestRunner.McpServer.TestExecution/TestExecutionAssemblyMarker.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/GlobalUsings.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/SelectorNormalizationEngineTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Initial sandboxed build failed during NuGet restore with `NU1301` TLS/auth errors before project compilation.
- Build passed after rerunning the same command with approved restore access and the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- New TestExecution tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.TestExecution.Tests\RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-001-selector-normalization.trx"`
- Result: 12 tests discovered, executed, and passed.
- Existing Build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-001-build-boundary.trx"`
- Result: 70 tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.
- Corrective expert-skip boundary validation passed:
  - `git diff --check`
  - `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
  - `dotnet test .\tests\RavenDB.TestRunner.McpServer.TestExecution.Tests\RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-001-expert-skip-corrective.trx"` with 14 tests discovered, executed, and passed
  - `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-001-expert-skip-build-boundary.trx"` with 70 tests discovered, executed, and passed
  - `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal` with 8 workspace detection and capability checks passed

## Progress ledger update
- Mark `WP_E_001_selector_normalization_engine` as `Done`.
- Record implementation commit `5dedf2d`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- WP_E_002 must consume normalized selectors for preflight evaluation without expanding raw-filter identity.
- WP_E_003 must map selector summaries into run plans without allowing test execution to perform hidden builds.
- MCP/Web/UI/test execution scheduler surfaces remain out of scope for this task.

## ADR or design delta notes
- No ADR or design delta required.
