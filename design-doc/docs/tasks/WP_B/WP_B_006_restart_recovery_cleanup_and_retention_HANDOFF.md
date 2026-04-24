# WP_B_006 Handoff

## Completed task
`WP_B_006_restart_recovery_cleanup_and_retention`

## What changed
- Added an attachment-aware retention cleanup planning baseline.
- Added cleanup action/reason vocabulary:
  - `ArtifactCleanupActionKinds`
  - `ArtifactCleanupReasonCodes`
- Added RavenDB-backed cleanup journal support using the frozen `CleanupJournal` collection and `cleanup-journal/<date>/<guid>` ID shape.
- Added cleanup planning and journal types:
  - `ArtifactRetentionCleanupPlanRequest`
  - `ArtifactRetentionCleanupPlan`
  - `ArtifactRetentionCleanupPlanItem`
  - `CleanupJournalDocument`
  - `CleanupJournalArtifactDecisionDocument`
  - `CleanupJournalPersistenceResult`
  - `CleanupJournalDocumentIds`
  - `RavenArtifactRetentionCleanupStore`
- The cleanup planner reads existing `ArtifactRef` documents, evaluates retention policy, and produces deterministic retain/candidate decisions.
- Active references are modeled as explicit owner IDs passed into the planning request because build/run/attempt execution persistence is not implemented yet.
- Cleanup journal persistence records the plan and explicitly marks `DeletionExecuted = false`.

## Touched contracts
- Used `DocumentCollectionNames.CleanupJournal` and `DocumentIdPatterns.CleanupJournal`.
- Kept the attachments-first v1 artifact invariant from `STORAGE_MODEL.md` and `ARTIFACTS_AND_RETENTION.md`.
- No design contract documents were changed.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Artifacts/`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/EmbeddedDatabaseBootstrapperTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Targeted storage validation passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-b-006-retention-cleanup.trx"`
- Result: 10 storage tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_B_006_restart_recovery_cleanup_and_retention` as `Done`.
- Record implementation commit `e1b76ab`.
- Keep `ENV-001` open.
- Keep WP_C_006, WP_D_004, WP_E, WP_F, MCP host/tools, Web API, UI, scheduler, build execution, and test execution not started.

## Risks / follow-ups
- WP_B_006 does not delete documents or attachments. It plans and journals cleanup candidates only.
- A later explicit work package must implement deletion execution, retention scheduling, and any event/audit emission if required.
- Deferred bulky diagnostics remain metadata-only deferred records and are retained by the v1 planner with `no_filesystem_cleanup`.
- Cleanup planning uses caller-provided active owner IDs until build/run/attempt execution persistence exists.

## ADR or design delta notes
- No ADR or design delta required.
