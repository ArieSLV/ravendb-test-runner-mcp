# WP_B_005 Handoff

## Completed task
`WP_B_005_event_checkpoint_and_resume_persistence`

## What changed
- Added RavenDB-backed event checkpoint persistence for stream resume state.
- Added checkpoint document/result/request types:
  - `EventCheckpointDocument`
  - `EventCheckpointWriteRequest`
  - `EventCheckpointPersistenceResult`
- Added `EventCheckpointDocumentIds` to create and validate IDs using the frozen `event-checkpoints/<stream-kind>/<owner-id>` pattern.
- Added `RavenEventCheckpointStore` for create, load, idempotent save, and monotonic update behavior.
- Build and run stream checkpoints are covered by integration assertions against the embedded RavenDB store.
- Checkpoint documents persist:
  - `checkpointId`
  - `streamKind`
  - `ownerId`
  - `cursor`
  - `sequence`
  - `updatedAtUtc`
- Invalid stream kinds, empty/traversal/backslash owner path segments, sequence regression, and cursor changes without sequence progress are rejected deterministically.

## Touched contracts
- Used the existing frozen event stream families from `Shared.Contracts/EventContracts`.
- Used the existing `DocumentIdPatterns.EventCheckpoint` and `DocumentCollectionNames.EventCheckpoints` constants.
- No design contract documents were changed.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/EmbeddedDatabaseBootstrapperTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Targeted storage validation passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-b-005-event-checkpoints.trx"`
- Result: 10 storage tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_B_005_event_checkpoint_and_resume_persistence` as `Done`.
- Record implementation commit `bcf7e24`.
- Keep `ENV-001` open.
- Keep later WP_B work packages not started.

## Risks / follow-ups
- Exact cursor string semantics remain transport-owned by later SignalR/SSE/MCP projection work.
- WP_B_005 uses the safest minimal monotonic policy: a checkpoint may advance to a higher sequence, and same-sequence saves are idempotent only when the cursor is unchanged.
- Attempt, workspace catalog, and quarantine stream families are accepted by frozen stream-family validation, but this task only integration-tested build and run stream checkpoints.
- Event publishing, replay APIs, browser transport, MCP progress notifications, scheduler integration, build execution, and test execution remain out of scope.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used for this task.
