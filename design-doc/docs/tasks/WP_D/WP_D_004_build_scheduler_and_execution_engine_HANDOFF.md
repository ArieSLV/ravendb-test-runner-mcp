# WP_D_004 Handoff

## Completed task
`WP_D_004_build_scheduler_and_execution_engine`

## What changed
- Added build-subsystem-owned command planning for restore, build, clean, and rebuild orchestration.
- Added deterministic `dotnet` command construction from existing `BuildPlan`, `BuildPolicy`, and `BuildGraphAnalysisResult` inputs.
- Added explicit binlog command intent when `BuildPolicy.captureBinlog` is enabled; full artifact/status/binlog harvesting remains for `WP_D_005`.
- Added child-process environment construction that uses an allowlist, applies explicit overrides, removes dangerous ambient `MSBuildSDKsPath` unless intentionally overridden, and sets deterministic .NET CLI environment values without mutating global/user environment variables.
- Added a process runner abstraction plus a default process runner using `ProcessStartInfo` with:
  - `UseShellExecute = false`
  - redirected stdout/stderr
  - explicit environment map
  - cancellation handling
  - timeout handling
  - process-tree kill on cancellation/timeout
- Added build execution engine lifecycle/result mapping for:
  - successful material build
  - non-zero exit code
  - cancellation
  - timeout
  - accepted reuse/no-material-build path
- Added an explicit build-subsystem ownership guard so non-build owners cannot execute hidden builds.

## Touched contracts
- Aligned with `BUILD_SUBSYSTEM.md`:
  - build subsystem owns restore/build/clean/rebuild orchestration
  - test execution cannot own hidden build work
  - build commands are centrally constructed
  - child-process environment is explicitly controlled
  - build reuse remains explicit input through `BuildReuseDecision`
- Aligned with `DOMAIN_MODEL.md`:
  - `BuildExecution.state` remains lifecycle state
  - `BuildResult.status` remains final outcome
  - readiness token status remains untouched by scheduler execution
- Aligned with `MCP_TOOLS.md` and `WEB_API.md` only at the domain level; no MCP host, Web API, UI, or test execution surface was implemented.
- No storage contract documents were changed.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Build/BuildSchedulerExecutionEngine.cs`
- `tests/RavenDB.TestRunner.McpServer.Build.Tests/BuildSchedulerExecutionEngineTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- Targeted build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-004-build-scheduler.trx"`
- Result: 42 build tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.
- Storage tests were not run because `Storage.RavenEmbedded` was not changed.

## Progress ledger update
- Mark `WP_D_004_build_scheduler_and_execution_engine` as `Done`.
- Record implementation commit `d6ed82a`.
- Keep `ENV-001` open; WP_D_004 adds deterministic child-process environment handling but does not mutate global/user environment variables.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- Full build artifact/status/binlog capture and persistence remains for `WP_D_005`.
- Build readiness integration remains for `WP_D_006`.
- Storage persistence of build executions/results is not introduced in this task.
- The default process runner is present, but tests use fakes and do not build the RavenDB repository.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
