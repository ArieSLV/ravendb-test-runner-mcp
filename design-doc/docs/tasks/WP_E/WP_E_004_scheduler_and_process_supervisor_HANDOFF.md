# WP_E_004 Handoff

## Completed task
`WP_E_004_scheduler_and_process_supervisor`

## What changed
- Added an in-memory test run scheduler/process-supervision boundary in `RavenDB.TestRunner.McpServer.TestExecution`.
- The scheduler consumes accepted `TestRunPlan` instances from WP_E_003 and preserves:
  - run plan ID;
  - workspace ID;
  - selector identities;
  - execution profile;
  - build linkage;
  - artifact descriptors.
- Added fakeable process runner contracts:
  - `ITestRunProcessRunner`;
  - `TestRunProcessRequest`;
  - `TestRunProcessResult`.
- Added deterministic schedule/cancel/timeout surfaces:
  - `Schedule(...)`;
  - `CancelBeforeStart(...)`;
  - `Cancel(...)`;
  - `Timeout(...)`.
- Enforced single-workspace active-run discipline with one active run per workspace.
- Blocked plans are rejected before runner invocation.
- Cancellation and timeout produce explicit lifecycle snapshots and terminal results.
- No real process execution implementation was added; tests use a fake runner and no product code invokes `dotnet test`.
- Corrective active-state pass:
  - active run registration now validates both workspace and run ID before mutation and rejects duplicate active run IDs with `run_already_active`;
  - duplicate run ID rejection does not leave the workspace active-run registry partially mutated;
  - process-runner exceptions now return a deterministic terminal `failed_terminal` result with `host_crashed` failure classification and release active run state.

## Touched contracts
- Aligned with `STATE_MACHINES.md`:
  - run snapshots use the frozen `RunExecution.state` vocabulary: `created`, `queued`, `resolving_build_dependency`, `preflighting`, `executing`, `harvesting`, `normalizing`, `completed`, `cancelling`, `cancelled`, `timeout_kill_pending`, `timed_out`, and `failed_terminal`.
- Aligned with `DOMAIN_MODEL.md`:
  - `RunExecution` and `RunStatusSnapshot`-style fields are represented in implementation-facing scheduler snapshots.
- Aligned with `BUILD_SUBSYSTEM.md`:
  - test execution consumes explicit build linkage/readiness/expert-skip decisions from the run plan;
  - unresolved build dependencies do not become executable work;
  - no hidden build path was introduced.
- Aligned with `ERROR_TAXONOMY.md`:
  - cancellation maps to `run_cancelled`;
  - timeout maps to `run_timed_out`;
  - non-zero test process results map to `test_failures`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.TestExecution/RunScheduling.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/TestRunSchedulerTests.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/SelectorNormalizationEngineTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- TestExecution tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.TestExecution.Tests\RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-004-active-state-corrective.trx"`
- Result: 49 tests discovered, executed, and passed.
- Existing Build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-004-build-boundary.trx"`
- Result: 70 tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_E_004_scheduler_and_process_supervisor` as `Done`.
- Record implementation commit `fd985c6`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- No default real process runner was added in this task. A later execution task can bind the process-runner abstraction to actual `dotnet test` invocation with controlled environment handling.
- Run persistence remains out of scope and should be added only by an explicitly scoped storage/test-execution task.
- WP_E_005 must formalize the build-to-test handoff without changing the scheduler’s no-hidden-build boundary.
- WP_F result harvesting and normalization remain out of scope.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
