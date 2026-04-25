# WP_D_005 Handoff

## Completed task
`WP_D_005_build_artifacts_status_and_binlog_capture`

## What changed
- Added build artifact harvesting for material build results:
  - build command
  - stdout
  - stderr
  - merged output
  - build summary
  - build output manifest
  - binlog when capture is enabled and the planned binlog file exists
- Added accepted reuse handling that persists build execution/result status without process-runner artifacts.
- Added RavenDB-backed build execution/result persistence:
  - `BuildExecutions` document at `builds/<workspace-hash>/<date>/<guid>`
  - `BuildResults` document at `build-results/<build-id>`
- Added Raven attachment persistence for captured build artifacts through the existing `RavenArtifactAttachmentStore`.
- Preserved the v1 artifact invariant: in-scope build artifacts are attachment-backed when under the guardrail, oversized artifacts become explicit `deferred_external` metadata, and no filesystem spillover backend is introduced.

## Touched contracts
- Aligned with `BUILD_SUBSYSTEM.md`:
  - build artifact capture is build-subsystem owned
  - binlog capture remains policy-controlled
  - build result/status documents are persisted explicitly
- Aligned with `DOMAIN_MODEL.md` and `STATE_MACHINES.md`:
  - `BuildExecution.state` remains lifecycle state
  - `BuildResult.status` remains final outcome
  - readiness token status remains untouched for `WP_D_006`
- Aligned with `STORAGE_MODEL.md` and `ARTIFACTS_AND_RETENTION.md`:
  - `ArtifactRef` documents own artifact payloads
  - build result documents reference artifacts but do not own binary/text payloads
  - oversized artifact payloads are deferred metadata only

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Build/BuildArtifactCapture.cs`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/BuildResultPersistence.cs`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.csproj`
- `tests/RavenDB.TestRunner.McpServer.Build.Tests/BuildArtifactCaptureTests.cs`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/EmbeddedDatabaseBootstrapperTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- Targeted build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-005-build-artifacts-status.trx"`
- Result: 57 build tests discovered, executed, and passed.
- Targeted storage tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-005-storage-build-results.trx"`
- Result: 10 storage tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_D_005_build_artifacts_status_and_binlog_capture` as `Done`.
- Record implementation commit `87ea024`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- Build readiness token issuance/integration remains for `WP_D_006`.
- Full MCP/Web/UI/status streaming surfaces remain for later work packages.
- Output manifest capture is a deterministic baseline over supplied output paths; discovering real build output trees remains later build execution/readiness work.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
