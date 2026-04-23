# WP_A_004 Handoff

## Completed task
`WP_A_004_event_contract_baseline`

## What changed
- Added implementation-facing event envelope field constants.
- Added event stream family and stream pattern constants.
- Added all build, run, attempt, and quarantine event type constants from `EVENT_MODEL.md`.
- Added event stream/type catalog metadata that records primary owners and supporting projection consumers.
- Added ordering, cursor, checkpoint, and replay convention constants.
- Reused the existing document convention constants for `EventCheckpoints`.

## Touched contracts
- No design contract documents were changed.
- New contract surface is under `Shared.Contracts` and mirrors `EVENT_MODEL.md`, with ownership context from `DOMAIN_MODEL.md`, `STATE_MACHINES.md`, `BUILD_SUBSYSTEM.md`, `MCP_TOOLS.md`, and `WEB_API.md`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/EventContracts/`

## Validation performed
- Build validation: `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to the installed .NET 10 SDK path.
- Dependency scope review: no RavenDB Embedded, MCP SDK, storage runtime, build/test execution, web API, UI, project reference, or package dependencies were introduced.
- Naming review: no retired implementation names found in the scaffold.
- Event envelope review: all 7 required envelope fields are represented.
- Event stream review: all 5 stream patterns from `EVENT_MODEL.md` are represented.
- Event type review: 39/39 event type constants are represented and referenced by the catalog.
- Replay/ordering review: sequence scope, cursor, checkpoint, `Last-Event-ID`, MCP projection, SignalR/SSE projection, and replay semantics are represented without transport implementation.
- Diff hygiene: `git diff --check` passed.

## Progress ledger update
- Mark `WP_A_004_event_contract_baseline` as `Done`.
- Keep `ENV-001` open.
- Keep WP_B/WP_C gated until full Phase 0 completion.

## Risks / follow-ups
- `ENV-001` remains open: the current shell still pins `MSBuildSDKsPath` to SDK 8.0.403, so .NET 10 validation requires a per-command override.
- Carry forward to `WP_A_006`: `WP_A_003` introduced exhaustive implementation-facing ID patterns for all 20 persisted document families, while `STORAGE_MODEL.md` currently lists example ID patterns for 12 families. This was later resolved by synchronizing `STORAGE_MODEL.md` to the full 20-pattern baseline.

## ADR or design delta notes
- No ADR or design delta required.
