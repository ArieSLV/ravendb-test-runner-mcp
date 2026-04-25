# WP_E_003 Handoff

## Completed task
`WP_E_003_test_run_planner`

## What changed
- Added in-memory run planning in `RavenDB.TestRunner.McpServer.TestExecution`.
- The planner consumes:
  - caller-supplied `runPlanId`;
  - caller-supplied UTC `createdAtUtc`;
  - normalized selector from WP_E_001;
  - preflight result from WP_E_002;
  - execution profile input;
  - logical artifact root.
- The resulting plan includes:
  - workspace ID;
  - selection summary;
  - execution profile;
  - build linkage;
  - build dependency resolution;
  - deterministic run steps;
  - deterministic predicted skips;
  - runtime unknowns;
  - warnings;
  - deterministic logical artifact descriptors;
  - structured and canonical selector identities.
- Planning creates executable test-selection steps only when `BuildDependencyResolution.AllowsTestExecutionToProceed` is true.
- Unresolved build dependencies produce a blocked plan with no executable steps and no artifact descriptors.
- Accepted readiness tokens, linked builds, and expert skip decisions remain explicit in the plan through the build linkage and dependency resolution.
- Raw expert filters remain isolated from structured selector identity and are represented only as expert raw-filter markers/reason codes, not expanded into canonical structured identity.
- Artifact descriptors are logical/canonical only. No files are written, no filesystem spillover behavior is introduced, and no RavenDB persistence was added.

## Corrective passes
- Review finding fixed: run planning is now bound to the exact selector and execution profile evaluated by preflight.
- `TestPreflightResult` now carries:
  - `StructuredSelectorIdentity`;
  - `CanonicalSelectorRequestIdentity`;
  - deterministic `ExecutionProfileIdentity`.
- `TestRunPlanner` rejects selector identity mismatches with `selector_identity_mismatch`, including same-summary selector drift and raw expert-filter marker mismatches.
- `TestRunPlanner` rejects execution profile mismatches with `execution_profile_mismatch`.
- Execution profile option comparison uses deterministic ordinal key/value identity, so equivalent option dictionaries in different enumeration order are accepted.

## Touched contracts
- Aligned with `DOMAIN_MODEL.md`:
  - `RunPlan`, `SelectionSummary`, `BuildLinkage`, `steps`, and `predictedSkips` are represented as implementation-facing domain contracts.
- Aligned with `BUILD_SUBSYSTEM.md`:
  - run planning does not perform hidden builds;
  - build-to-test handoff remains explicit through readiness token, linked build, or expert skip resolution;
  - `build_if_missing_or_stale` remains a build-subsystem action requirement rather than a test-execution build.
- Aligned with `ERROR_TAXONOMY.md`:
  - deterministic predicted skips are preserved as non-flaky.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.TestExecution/RunPlanning.cs`
- `tests/RavenDB.TestRunner.McpServer.TestExecution.Tests/TestRunPlannerTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- TestExecution tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.TestExecution.Tests\RavenDB.TestRunner.McpServer.TestExecution.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-003-identity-corrective.trx"`
- Result: 35 tests discovered, executed, and passed.
- Existing Build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-e-003-build-boundary.trx"`
- Result: 70 tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_E_003_test_run_planner` as `Done`.
- Record corrective implementation commit `fecf1a0`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- RunPlan persistence is called out by `BUILD_SUBSYSTEM.md`, but WP_E_003 did not define a storage write-set. Persistence should be implemented in a later explicitly scoped storage/test-execution task.
- WP_E_004 must execute planned steps through a bounded process supervisor without changing the run plan identity or introducing hidden builds.
- MCP/Web/UI/API projection remains out of scope.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
