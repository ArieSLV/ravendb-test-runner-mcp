# WP_A_003 Handoff

## Completed task
`WP_A_003_document_id_and_collection_conventions`

## What changed
- Added implementation-facing collection name constants for all mandatory collections in `STORAGE_MODEL.md`.
- Added deterministic document ID pattern constants for all persisted document families.
- Added a document convention catalog that ties each entity family to:
  - collection name
  - document ID pattern
  - primary module owner
  - persistence owner
  - supporting projects
  - related contract documents
  - optimistic concurrency expectation

## Touched contracts
- No design contract documents were changed.
- New contract surface is under `Shared.Contracts` and mirrors the frozen storage/domain/build/event/state artifact conventions.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentCollectionNames.cs`
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentConvention.cs`
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentConventionCatalog.cs`
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentIdPatterns.cs`

## Validation performed
- Build validation: `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to the installed .NET 10 SDK path.
- Dependency scope review: no RavenDB Embedded, MCP SDK, storage runtime, build/test execution, web API, UI, project reference, or package dependencies were introduced.
- Naming review: no retired implementation names found in the scaffold.
- Collection coverage review: 20/20 mandatory collections have constants and catalog mappings.
- Document ID pattern review: 20/20 persisted document families have deterministic ID pattern constants.
- Diff hygiene: `git diff --check` passed.

## Progress ledger update
- Mark `WP_A_003_document_id_and_collection_conventions` as `Done`.
- Keep `ENV-001` open.
- Keep WP_B/WP_C gated until full Phase 0 completion.

## Risks / follow-ups
- `ENV-001` remains open: the current shell still pins `MSBuildSDKsPath` to SDK 8.0.403, so .NET 10 validation requires a per-command override.
- Future WP_B storage implementation must treat these conventions as contract inputs, not as a completed RavenDB Embedded runtime.

## ADR or design delta notes
- No ADR or design delta required.
