# WP_C_006 Handoff

## Completed task
`WP_C_006_catalog_persistence_and_capability_matrix`

## What changed
- Added implementation-facing `TestCategoryCatalogEntry` under semantics abstractions.
- Added RavenDB Embedded semantic catalog persistence:
  - `RavenSemanticCatalogStore`
  - `SemanticCatalogPersistenceRequest`
  - `SemanticCatalogPersistenceResult`
  - `SemanticCatalogDocumentIds`
  - `SemanticSnapshotDocument`
  - `CapabilityMatrixDocument`
  - `TestCategoryCatalogEntryDocument`
- Semantic snapshots persist the `DOMAIN_MODEL.md` AI capability fields, plugin ID, category catalog version, custom attribute registry version, topology hash, workspace ID, and created timestamp.
- Capability matrices persist repo line, framework/runner/adapter families, capability dictionary, version-sensitive points, plugin ID, workspace ID, and created timestamp.
- Category catalog entries persist into the frozen `TestCatalogEntries` collection using the frozen `test-catalog/<workspace-hash>/<catalog-version>/<test-id-hash>` ID family with a deterministic hash of the category key as the final entry segment.
- Store writes are idempotent for already-created immutable snapshot/matrix/category documents.
- Storage identity validation rejects path separators, traversal segments, unsupported repo lines, and mismatched request/capability matrix repo lines.

## Touched contracts
- Aligned with `DOMAIN_MODEL.md`:
  - `SemanticSnapshot`
  - `CapabilityMatrix`
  - `TestCategory`
- Aligned with `STORAGE_MODEL.md`:
  - `SemanticSnapshots`
  - `CapabilityMatrices`
  - `TestCatalogEntries`
  - `semantic-snapshots/<workspace-hash>/<sem-hash>`
  - `capability-matrices/<workspace-hash>/<line>/<hash>`
  - `test-catalog/<workspace-hash>/<catalog-version>/<test-id-hash>`
- Aligned with `VERSIONING_AND_CAPABILITIES.md` for v6.2, v7.1, and v7.2 capability baselines.
- No design contract documents were changed.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Semantics.Abstractions/`
- `src/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded/`
- `tests/RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests/EmbeddedDatabaseBootstrapperTests.cs`

## Validation performed
- Diff hygiene passed:
  `git diff --check`
- Build passed with the `AGENTS.md` isolated .NET SDK 10.0.203 environment:
  `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal`
- Targeted storage validation passed:
  `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj --no-build --results-directory .\.tmp-review-results --logger "trx;LogFileName=wp-c-006-review.trx"`
- Result: 10 storage tests discovered, executed, and passed.
- Semantics harness passed:
  `dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Result: 8 workspace detection and capability checks passed.

## Progress ledger update
- Mark `WP_C_006_catalog_persistence_and_capability_matrix` as `Done`.
- Record implementation commit `d13736d`.
- Keep `ENV-001` open.
- Keep `TASK_INDEX.md` unchanged.

## Risks / follow-ups
- Category catalog entries use the frozen `TestCatalogEntries` collection because the storage contract has no separate category-catalog collection.
- Full test identity catalog persistence remains for later selector/test-planning work.
- No filesystem-backed semantic persistence, build/test execution, MCP host/tools, Web API, UI, or cleanup deletion executor was introduced.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
