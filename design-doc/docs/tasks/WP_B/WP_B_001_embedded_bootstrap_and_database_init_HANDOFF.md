# WP_B_001 Handoff

## Completed task
`WP_B_001_embedded_bootstrap_and_database_init`

## What changed
- Added the first bounded storage runtime project: `RavenDB.TestRunner.McpServer.Storage.RavenEmbedded`.
- Added the first bounded artifacts module: `RavenDB.TestRunner.McpServer.Artifacts`.
- Implemented RavenDB Embedded bootstrap with:
  - explicit license probe order support for explicit configuration, `RAVEN_License`, `RAVEN_LicensePath`, and `RAVEN_License_Path`
  - mandatory startup validation when no approved license source is available
  - explicit process-wide embedded server lifecycle control
  - deterministic rejection of conflicting embedded server reconfiguration attempts after the process-wide server is started
  - database creation/initialization through `DatabaseOptions` and `DatabaseRecord`
  - propagation of mandatory Phase 0 collection names into the bootstrap result
- Added an attachments-first artifact routing baseline that keeps in-scope v1 artifact classes on `raven_attachment` and classifies deferred bulky diagnostics as `deferred_external` rather than silently routing them to a filesystem default.
- Added a dedicated validation project for storage bootstrap that exercises license resolution, artifact routing, real embedded startup, document persistence, and attachment round-trip behavior.

## Touched contracts
- No design contract documents were changed.
- Implementation remains aligned with `DECISION_FREEZE.md`, `STORAGE_MODEL.md`, `ARTIFACTS_AND_RETENTION.md`, `DOMAIN_MODEL.md`, and `SECURITY_AND_REDACTION.md`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `src/RavenDB.TestRunner.McpServer.Artifacts/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/`
- `RavenDB.TestRunner.McpServer.sln`

## Validation performed
- Build validation: `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to the installed .NET 10 SDK path.
- Targeted storage validation: `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj -m:1 -v minimal` succeeded with the same SDK override.
- Validation scope covered:
  - license probe order
  - legacy `RAVEN_License_Path` support
  - repeated bootstrap using the same process-wide server configuration
  - deterministic rejection of conflicting process-wide embedded server configuration
  - artifact routing for attachments-first vs deferred classes
  - real embedded startup using the approved license path environment probe
  - database initialization, document persistence, and attachment round-trip
- Scope review: no MCP host, build subsystem, test execution workflow, web API, or UI implementation was introduced.
- Storage-rule review: no filesystem-owned default v1 artifact path was introduced.
- Diff hygiene: `git diff --check` passed.

## Progress ledger update
- Mark `WP_B_001_embedded_bootstrap_and_database_init` as `Done`.
- Keep `ENV-001` open.
- Keep `WP_C_001` in progress until the worker handoff is reviewed.

## Risks / follow-ups
- `ENV-001` remains open: SDK 10 validation still requires the per-command `MSBuildSDKsPath` override.
- Interactive first-run license setup is still deferred; `WP_B_001` implements the approved probe order and mandatory startup checks, but not the later interactive flow.
- RavenDB Embedded remains process-wide in v1 bootstrap. Repeated initialization is accepted only when the server configuration fingerprint matches; changing data directory, URL, dotnet path, license source, or licensing flags requires a process restart.
- Collections/indexes/concurrency policy, artifact metadata persistence, deferred bulky-diagnostic extension points, event checkpoints, and restart recovery remain in later WP_B tasks.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- Official RavenDB docs used:
  - RavenDB `7.2` embedded server docs: `https://docs.ravendb.net/7.2/server/embedded`
  - RavenDB `7.2` attachments docs: `https://docs.ravendb.net/7.2/document-extensions/attachments/store-attachments/store-attachments-local/`
- Package/version assumption used for this bounded task:
  - `RavenDB.Embedded` `7.2.1`
- This version choice is an implementation-time inference from the current official `7.2` docs line and current stable NuGet package, not a new architecture decision.
