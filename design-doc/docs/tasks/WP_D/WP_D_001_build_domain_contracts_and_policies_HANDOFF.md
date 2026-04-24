# WP_D_001 Handoff

## Completed task
`WP_D_001_build_domain_contracts_and_policies`

## What changed
- Added the `RavenDB.TestRunner.McpServer.Build` project to the solution.
- Added build domain contract records for scope, policy, fingerprint, readiness tokens, requests, plans, executions, results, artifacts, and reuse decisions.
- Added build policy vocabulary for the five frozen policy modes.
- Added explicit build ownership resolution rules:
  - test execution may consume an explicit readiness token
  - test execution may consume an explicit linked build
  - test execution must route build-creating policies through the build subsystem
  - expert skip-build requires expert mode and emits warnings
- Added lifecycle vocabulary accessors that preserve the separation of:
  - `BuildExecution.state`
  - `BuildResult.status`
  - `BuildReadinessToken.status`
- Added build artifact capture policy for v1 attachment-backed build artifacts and explicit binlog capture vocabulary.
- Added focused build contract tests.
- Corrective coverage added for deferred bulky build artifacts:
  - `build.dump`
  - `build.diagnostics.oversized`
  - both route to `deferred_external`, are policy-deferred, and are not in the v1 attachment-backed build artifact set.

## Touched contracts
- Implementation remains aligned with:
  - `docs/contracts/BUILD_SUBSYSTEM.md`
  - `docs/contracts/DOMAIN_MODEL.md`
  - `docs/contracts/STORAGE_MODEL.md`
  - `docs/contracts/MCP_TOOLS.md`
  - `docs/contracts/WEB_API.md`
  - `docs/contracts/STATE_MACHINES.md`
  - `docs/architecture/DECISION_FREEZE.md`
- No design contract documents were changed.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Build/`
- `tests/RavenDB.TestRunner.McpServer.Build.Tests/`
- `RavenDB.TestRunner.McpServer.sln`

## Validation performed
- `git diff --check` passed.
- `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203.
- `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj -m:1 -v minimal --logger "trx;LogFileName=wp-d-001-deferred-artifacts.trx"` succeeded.
- Corrective test result: 8 tests discovered/executed, 8 passed.
- Runtime reuse/fingerprint integration tests and build artifact/binlog smoke execution were not run because WP_D_001 intentionally does not implement graph analysis, reuse engine, process spawning, or actual build execution.
- Generated TRX/TestResults artifacts were removed after test-count verification.

## Progress ledger update
- Mark `WP_D_001_build_domain_contracts_and_policies` as `Done`.
- Record corrective implementation commit `31b4336`.
- Keep `ENV-001` open.
- Keep `WP_D_002+`, `WP_E_*`, MCP host/tools, Web API, UI, and test execution subsystem work not started.

## Risks / follow-ups
- WP_D_002+ still own graph analysis, fingerprint/reuse integration, scheduler/execution, binlog capture, and readiness integration.
- The current build contracts are pure policy/domain vocabulary and do not persist or execute build workflows yet.

## ADR or design delta notes
- No ADR or design delta required.
