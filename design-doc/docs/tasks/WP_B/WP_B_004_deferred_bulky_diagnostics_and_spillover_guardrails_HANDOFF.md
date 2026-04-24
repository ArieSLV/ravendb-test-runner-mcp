# WP_B_004 Handoff

## Completed task
`WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails`

## What changed
- Added an explicit v1 artifact guardrail policy surface:
  - `V1ArtifactGuardrailDecision`
  - `V1ArtifactGuardrailPolicy`
- Added deferred reason vocabulary for:
  - deferred artifact class,
  - exceeded practical attachment guardrail,
  - no v1 spillover backend configured,
  - future extension required.
- Updated `RavenArtifactAttachmentStore` to consume the guardrail policy instead of embedding ad hoc storage/deferred decisions.
- Preserved the v1 storage rule:
  - in-scope artifacts under the practical guardrail remain RavenDB attachment-backed,
  - deferred bulky diagnostics are metadata-only deferred records,
  - oversized in-scope artifacts are explicitly deferred,
  - no filesystem-backed or external spillover store is configured or implemented by default.
- Added regression coverage proving:
  - normal in-scope artifacts remain attachment-backed under the guardrail,
  - every deferred bulky diagnostic class is non-attachment-backed and non-filesystem-backed,
  - oversized in-scope artifacts are deferred without a filesystem backend,
  - all deferred bulky diagnostic classes persist as `ArtifactRef` metadata with no RavenDB attachment payload.

## Touched contracts
- No design contract documents were changed.
- Implementation remains aligned with `STORAGE_MODEL.md`, `ARTIFACTS_AND_RETENTION.md`, `DOMAIN_MODEL.md`, and `SECURITY_AND_REDACTION.md`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Artifacts/`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build validation passed after shutting down a stale Roslyn/MSBuild build server left by an earlier timed-out validation attempt:
  `$env:DOTNET_CLI_HOME=(Join-Path (Resolve-Path '.').Path '.tmp-dotnet-home'); $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBuildSDKsPath='C:\Program Files\dotnet\sdk\10.0.203\Sdks'; dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Targeted storage validation passed:
  `$env:DOTNET_CLI_HOME=(Join-Path (Resolve-Path '.').Path '.tmp-dotnet-home'); $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBuildSDKsPath='C:\Program Files\dotnet\sdk\10.0.203\Sdks'; dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj -m:1 -v minimal --logger "trx;LogFileName=wp-b-004-deferred-guardrails.trx"`
- Result: 10 storage tests discovered, executed, and passed.
- Requested semantics command failed before harness execution during the implicit build path with no compiler errors emitted:
  `dotnet run --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Semantics harness passed after the clean build:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails` as `Done`.
- Keep `ENV-001` open.
- Keep later WP_B work packages not started.

## Risks / follow-ups
- No default filesystem or external artifact store exists in v1.
- A future ADR or milestone must define ownership, cleanup, browser/MCP retrieval, and migration semantics before enabling any non-attachment spillover backend.
- Retention cleanup, recovery, retrieval APIs, MCP tools, Web API, UI, build execution, and test execution remain out of scope.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used for this task.
