# WP_B_002 Handoff

## Completed task
`WP_B_002_collections_indexes_and_optimistic_concurrency`

## What changed
- Added a deterministic storage schema baseline for the 20 mandatory collections from Phase 0 document conventions.
- Added required index definitions for build/run lookups, artifact ownership/retention, semantic snapshots, flaky findings, and quarantine actions.
- Added explicit revision-policy decisions per mandatory collection:
  - mutable/concurrency-owned collections receive bounded revisions
  - append-oriented baseline collections have revisions explicitly disabled until later tasks require mutation history
- Configured embedded document stores with optimistic concurrency before RavenDB initializes the store.
- Applied index and revision baseline during embedded database bootstrap.
- Exposed the applied schema baseline in `EmbeddedDatabaseBootstrapResult`.

## Touched contracts
- No design contract documents were changed.
- Implementation remains aligned with `DECISION_FREEZE.md`, `STORAGE_MODEL.md`, `DOMAIN_MODEL.md`, `ARTIFACTS_AND_RETENTION.md`, and `SECURITY_AND_REDACTION.md`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/`

## Validation performed
- Build validation: `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to the installed .NET 10 SDK path.
- Targeted storage validation: `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj -m:1 -v minimal` succeeded with the same SDK override.
- Test result: 7 tests discovered and executed.
- Validation scope covered:
  - embedded startup/database initialization
  - required index deployment
  - revisions-policy decisions for all mandatory collections
  - optimistic concurrency convention and actual concurrency conflict behavior
  - artifact metadata routing remains attachments-first
  - WP_B_001 process-wide embedded lifecycle behavior remains intact
- Diff hygiene: `git diff --check` passed.

## Progress ledger update
- Mark `WP_B_002_collections_indexes_and_optimistic_concurrency` as `Done`.
- Keep `ENV-001` open.
- Keep WP_D not started in this wave.

## Risks / follow-ups
- RavenDB creates physical collections when documents are stored. WP_B_002 records mandatory collection metadata and applies indexes/revision policy, but does not seed synthetic documents solely to force empty collection materialization.
- Later WP_B tasks still own artifact metadata persistence, deferred bulky diagnostics, event checkpoints, restart recovery, retention, and cleanup behavior.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- Official RavenDB docs used:
  - RavenDB `7.2` index creation/deployment docs: `https://docs.ravendb.net/7.2/indexes/creating-and-deploying/`
  - RavenDB `7.2` revisions configuration docs: `https://docs.ravendb.net/7.2/document-extensions/revisions/client-api/operations/configure-revisions/`
  - RavenDB `7.2` optimistic concurrency docs: `https://docs.ravendb.net/7.2/client-api/session/configuration/how-to-enable-optimistic-concurrency/`
- Package/source behavior verified against installed `RavenDB.Embedded` / `RavenDB.Client` `7.2.1`.
