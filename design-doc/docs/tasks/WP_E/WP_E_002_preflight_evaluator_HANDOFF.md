# WP_E_002 Handoff

## Completed task
`WP_E_002_preflight_evaluator`

## What changed
- Added in-memory test preflight evaluation in `RavenDB.TestRunner.McpServer.TestExecution`.
- Preflight accepts:
  - `workspaceId`;
  - normalized selector from WP_E_001;
  - execution profile input;
  - build policy;
  - optional linked build/readiness inputs;
  - expert mode;
  - caller-supplied deterministic catalog/runtime facts.
- Preflight returns:
  - `workspaceId`;
  - `SelectionSummary`;
  - deterministic `predictedSkips`;
  - deterministic `runtimeUnknowns`;
  - `BuildLinkage`;
  - `BuildDependencyResolution`;
  - `preflightWarnings`.
- Build-to-test dependency handling delegates to `BuildOwnershipModel.ResolveBuildDependency(...)`.
- Raw expert filters remain isolated from structured selector identity and are reported as runtime-expansion unknowns/warnings.
- Corrective pass: preflight now rejects a normalized selector carrying `ExpertRawFilter` unless `TestPreflightRequest.ExpertMode` is explicitly true, using `raw_filter_requires_expert_mode`.
- Deterministic skip predictions are explicitly marked deterministic and non-flaky.

## Touched contracts
- Aligned with `DOMAIN_MODEL.md`:
  - `SelectionSummary`, `BuildLinkage`, and `predictedSkips` are represented for later run planning.
- Aligned with `BUILD_SUBSYSTEM.md`:
  - test execution does not perform hidden builds;
  - build readiness/linkage is resolved through the build ownership model;
  - `build_if_missing_or_stale` requires build subsystem action instead of pretending tests can run.
- Aligned with `ERROR_TAXONOMY.md`:
  - deterministic skips remain deterministic and are not classified as flaky.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.TestExecution/PreflightEvaluation.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/PreflightEvaluatorTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- TestExecution tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.TestExecution.Tests\RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-002-raw-filter-expert-boundary.trx"`
- Result: 23 tests discovered, executed, and passed.
- Existing Build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-002-raw-filter-build-boundary.trx"`
- Result: 70 tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_E_002_preflight_evaluator` as `Done`.
- Record corrective implementation commit `313d1e1`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- WP_E_003 must turn this preflight result into a persisted run plan without expanding raw expert filters into canonical selector identity.
- Later test execution work must continue using build linkage/readiness from the build subsystem and must not perform hidden builds.
- Runtime unknowns are in-memory only for this task; persistence/MCP/Web/UI projection remains out of scope.

## ADR or design delta notes
- No ADR or design delta required.
