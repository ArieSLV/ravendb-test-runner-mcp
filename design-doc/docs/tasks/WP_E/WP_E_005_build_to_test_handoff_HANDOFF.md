# WP_E_005 Handoff

## Completed task
`WP_E_005_build_to_test_handoff`

## What changed
- Added an explicit build-to-test handoff contract in `RavenDB.TestRunner.McpServer.TestExecution`.
- Added `BuildToTestHandoffEvaluator`, `BuildToTestHandoffRequest`, and `BuildToTestHandoff`.
- The handoff is now machine-readable and carries:
  - kind;
  - status;
  - accepted/rejected decision;
  - runnable flag;
  - build-subsystem-action flag;
  - linked build ID;
  - linked build plan ID;
  - linked readiness token ID;
  - build reuse decision;
  - build policy mode;
  - build dependency resolution kind;
  - reason codes;
  - warnings.
- `TestPreflightEvaluator` now produces the explicit handoff alongside the existing `BuildLinkage` and `BuildDependencyResolution`.
- `TestRunPlanner` consumes the explicit handoff before creating executable run steps.
- `TestRunPlan` now preserves the handoff for later scheduler/MCP/Web/UI projection work.
- `TestRunScheduler` validates the accepted handoff before runner invocation and preserves it in run status snapshots.
- Corrective provenance pass:
  - readiness-token handoffs with an available token payload now reject when `LinkedBuildId` conflicts with `BuildReadinessToken.BuildId`;
  - readiness-token handoffs now reject when `BuildReuseDecision.ExistingBuildId` conflicts with the token build ID;
  - rejected provenance mismatch handoffs produce blocked run plans with no executable steps or artifact descriptors and are rejected by the scheduler before runner invocation.

## Accepted handoff cases
- Ready build-readiness token handoff.
- Linked build handoff.
- Expert skip-build handoff only when expert-mode proof was already accepted.

## Blocked handoff cases
- Missing readiness token / linked build / accepted expert skip.
- Build subsystem action required.
- Rejected build reuse decision.
- Non-ready readiness token payloads when available:
  - `invalidated`;
  - `missing_outputs`;
  - `superseded`.
- Readiness token ID mismatch.
- Readiness token workspace mismatch.
- Linked build/readiness token build ID mismatch.
- Build reuse existing-build/readiness token build ID mismatch.

## Touched contracts
- Aligned with `BUILD_SUBSYSTEM.md`:
  - tests never perform hidden builds;
  - runnable test plans require readiness token, linked build, or explicit expert skip-build handoff.
- Aligned with `DOMAIN_MODEL.md`:
  - `BuildLinkage` remains preserved in preflight, run plans, and run snapshots;
  - handoff metadata is explicit and does not collapse build execution/result/readiness vocabularies.
- Aligned with `ERROR_TAXONOMY.md`:
  - blocked build handoff remains a deterministic policy/runtime-unknown condition, not a flaky test result.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.TestExecution/BuildToTestHandoff.cs`
- `src/RavenDB.TestRunner.McpServer.TestExecution/PreflightEvaluation.cs`
- `src/RavenDB.TestRunner.McpServer.TestExecution/RunPlanning.cs`
- `src/RavenDB.TestRunner.McpServer.TestExecution/RunScheduling.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/BuildToTestHandoffTests.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/TestRunSchedulerTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- TestExecution tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.TestExecution.Tests\RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-005-provenance-corrective.trx"`
- Result: 60 tests discovered, executed, and passed.
- Existing Build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-005-provenance-build-boundary.trx"`
- Result: 70 tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_E_005_build_to_test_handoff` as `Done`.
- Record implementation commit `6a67748`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- Handoff persistence remains out of scope and should be added only by an explicitly scoped storage/test-execution task.
- WP_E_006 can use the preserved handoff metadata for reproducible command and execution summary generation.
- MCP/Web/UI/API projection remains out of scope.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
