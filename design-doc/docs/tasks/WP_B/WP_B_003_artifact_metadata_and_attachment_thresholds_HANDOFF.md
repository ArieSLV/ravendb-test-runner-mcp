# WP_B_003 Handoff

## Completed task
`WP_B_003_artifact_metadata_and_attachment_thresholds`

## What changed
- Added implementation-facing artifact metadata documents for RavenDB `ArtifactRefs`.
- Added artifact owner, retention, deferred-reason, write-request, and persistence-result vocabularies under `RavenDB.TestRunner.McpServer.Artifacts`.
- Added `RavenArtifactAttachmentStore` for v1 artifact persistence:
  - in-scope v1 artifact kinds store payloads as RavenDB attachments on their owning `ArtifactRef`,
  - deferred bulky diagnostics store explicit `deferred_external` metadata and no attachment,
  - artifacts exceeding the configured practical attachment guardrail are explicitly deferred and never silently routed to filesystem storage.
- Preserved the existing storage invariants:
  - lowerCamel RavenDB JSON/index field naming through `RavenStorageDocumentConventions`,
  - optimistic concurrency,
  - static artifact indexes,
  - process-wide Embedded RavenDB lifecycle behavior.
- Added embedded storage regression coverage for:
  - persisted `ArtifactRef` metadata fields,
  - RavenDB attachment payload retrieval,
  - owner and kind/retention static index queries,
  - deferred bulky diagnostics,
  - oversized in-scope artifacts crossing the practical attachment guardrail.

## Touched contracts
- No design contract documents were changed.
- Implementation remains aligned with `STORAGE_MODEL.md`, `ARTIFACTS_AND_RETENTION.md`, `DOMAIN_MODEL.md`, and `SECURITY_AND_REDACTION.md`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Artifacts/`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/`

## Validation performed
- Early build validation passed:
  `$env:DOTNET_CLI_HOME=(Join-Path (Resolve-Path '.').Path '.tmp-dotnet-home'); $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBuildSDKsPath='C:\Program Files\dotnet\sdk\10.0.203\Sdks'; dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Targeted storage validation passed:
  `$env:DOTNET_CLI_HOME=(Join-Path (Resolve-Path '.').Path '.tmp-dotnet-home'); $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBuildSDKsPath='C:\Program Files\dotnet\sdk\10.0.203\Sdks'; dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj -m:1 -v minimal --logger "trx;LogFileName=wp-b-003-artifacts.trx"`
- Result: 7 storage tests discovered, executed, and passed.
- Final solution-level validation is recorded in `IMPLEMENTATION_PROGRESS.md`.

## Progress ledger update
- Mark `WP_B_003_artifact_metadata_and_attachment_thresholds` as `Done`.
- Keep `ENV-001` open.
- Keep later WP_B work packages not started.

## Risks / follow-ups
- `WP_B_003` stores metadata and attachments but does not implement cleanup, retention sweeps, artifact retrieval APIs, MCP tools, Web API, UI, or test/build execution integration.
- Deferred bulky diagnostics remain explicit metadata records only until a later ADR or milestone introduces a non-attachment extension path.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used for this task.
- RavenDB attachment API behavior was verified against the installed `RavenDB.Client` / `RavenDB.Embedded` `7.2.1` packages through the integration test.
