# WP_A_002 Handoff

## Completed task
`WP_A_002_shared_contracts_project_layout`

## What changed
- Added assembly marker types for the three Phase 0 foundational projects.
- Added a shared contract-layout registry in `Shared.Contracts`.
- Added approved implementation project-name constants based on the frozen namespace root.
- Mapped every contract document under `design-doc/docs/contracts/` to a primary target project/module and supporting project/module set.

## Touched contracts
- No design contract documents were changed.
- The source mapping covers all current files under `design-doc/docs/contracts/`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Core.Abstractions/CoreAbstractionsAssemblyMarker.cs`
- `src/RavenDB.TestRunner.McpServer.Domain/DomainAssemblyMarker.cs`
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/SharedContractsAssemblyMarker.cs`
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/ContractLayout/`

## Validation performed
- Build validation: `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to the installed .NET 10 SDK path.
- Contract-document mapping review: 13 contract documents found and 13 mappings present.
- Scope review: no RavenDB Embedded, MCP SDK, storage runtime, build execution, test execution, UI, or package dependencies were introduced.
- Naming review: no retired implementation names found in the scaffold.
- Diff hygiene: `git diff --check` passed.

## Progress ledger update
- Mark `WP_A_002_shared_contracts_project_layout` as `Done`.
- Keep `ENV-001` open.
- Keep WP_B/WP_C gated until full Phase 0 completion.

## Risks / follow-ups
- `ENV-001` remains open: the current shell still pins `MSBuildSDKsPath` to SDK 8.0.403, so .NET 10 validation requires a per-command override.
- `WP_A_003` remains responsible for document ID patterns, collection names, and module ownership tables.

## ADR or design delta notes
- No ADR or design delta required.
