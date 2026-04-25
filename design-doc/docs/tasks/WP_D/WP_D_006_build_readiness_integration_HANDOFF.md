# WP_D_006 Handoff

## Completed task
`WP_D_006_build_readiness_integration`

## What changed
- Added build-domain readiness integration:
  - successful material builds issue a `BuildReadinessToken` only when an explicit `BuildFingerprint` is supplied;
  - failed, timed out, and cancelled builds do not issue readiness;
  - accepted reuse reports the existing readiness token without creating a new material readiness token;
  - readiness invalidation updates existing tokens to `invalidated`, `superseded`, or `missing_outputs` status through explicit inputs.
- Added RavenDB Embedded readiness token persistence:
  - `BuildReadinessTokens` collection;
  - `build-readiness/<workspace-hash>/<fingerprint>` ID validation;
  - idempotent equivalent writes;
  - deterministic payload drift rejection for reused immutable token IDs;
  - status-transition writes for explicit invalidation records.
- Integrated readiness handling into `RavenBuildResultStore.Save(...)` before artifact capture and result persistence.
- Preserved `WP_D_005` artifact/result behavior; no readiness integration writes occur unless a readiness request is supplied.

## Corrective passes
- Review finding fixed: accepted reuse can no longer link an arbitrary ready token.
- `reused_existing` now requires `BuildReuseDecision.ExistingBuildId` and validates that it matches the linked readiness token build.
- Reuse readiness validation now requires the token workspace to match the build execution workspace.
- A pre-populated `BuildExecution.BuildFingerprintId` must match the linked readiness token fingerprint; it is not overwritten silently.
- Readiness invalidation target status must be one of the terminal validity states: `invalidated`, `superseded`, or `missing_outputs`; `ready` is rejected as an invalidation target.
- Storage integration coverage verifies rejected readiness validation happens before `BuildExecution`, `BuildResult`, artifact metadata, or attachments are persisted.

## Touched contracts
- Aligned with `BUILD_SUBSYSTEM.md`:
  - readiness tokens remain the build-to-test handshake;
  - build reuse and readiness decisions are explicit and persisted.
- Aligned with `DOMAIN_MODEL.md` and `STATE_MACHINES.md`:
  - `BuildExecution.state`, `BuildResult.status`, and `BuildReadinessToken.status` remain separate vocabularies.
- Aligned with `STORAGE_MODEL.md`:
  - readiness token documents use the frozen collection and ID family;
  - lowerCamel RavenDB document conventions and existing index compatibility are preserved.
- No MCP host, Web API, SignalR/SSE, UI, or test execution subsystem surfaces were implemented.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Build/BuildReadinessIntegration.cs`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/BuildReadinessTokenPersistence.cs`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/BuildResultPersistence.cs`
- `tests/RavenDB.TestRunner.McpServer.Build.Tests/BuildReadinessIntegrationTests.cs`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/EmbeddedDatabaseBootstrapperTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Result: 0 warnings, 0 errors.
- Targeted build tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-006-build-readiness.trx"`
- Result: 65 build tests discovered, executed, and passed.
- Targeted storage tests passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-006-storage-readiness.trx"`
- Result: 10 storage tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.
- Corrective boundary validation passed:
  - `git diff --check`
  - `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
  - `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-006-readiness-boundary-corrective-build.trx"` with 69 tests discovered, executed, and passed
  - `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-d-006-readiness-boundary-corrective-storage.trx"` with 10 tests discovered, executed, and passed
  - `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal` with 8 workspace detection and capability checks passed

## Progress ledger update
- Mark `WP_D_006_build_readiness_integration` as `Done`.
- Record implementation commit `4f6c588`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- WP_E test planning must consume readiness tokens explicitly and must not perform hidden builds.
- MCP/Web/UI projections for build readiness remain for later work packages.
- Cleanup/removal-triggered readiness invalidation remains a future integration point once deletion execution exists.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
