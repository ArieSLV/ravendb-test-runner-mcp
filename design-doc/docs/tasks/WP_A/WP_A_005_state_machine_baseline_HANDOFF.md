# WP_A_005 Handoff

## Completed task
`WP_A_005_state_machine_baseline`

## What changed
- Added implementation-facing lifecycle vocabularies for builds, runs, attempts, and quarantine actions under `Shared.Contracts/StateMachineContracts`.
- Added explicit field-name constants that keep `BuildExecution.state`, `BuildResult.status`, and `BuildReadinessToken.status` separate.
- Added catalog metadata for lifecycle machines, allowed transitions, terminal semantics, and build lifecycle-to-result/readiness mappings.
- Reused `DocumentConventionCatalog`, `ImplementationProjectNames`, and `EventTypeNames` to record primary ownership, optimistic concurrency expectations, and event alignment without introducing runtime behavior.
- Kept the surface deterministic and transport-neutral; no storage, build execution, test execution, MCP, Web API, SignalR/SSE, or UI implementation was added.

## Touched contracts
- No design contract documents were changed.
- New implementation-facing contract surface mirrors `STATE_MACHINES.md` and stays aligned with `DOMAIN_MODEL.md`, `BUILD_SUBSYSTEM.md`, and `EVENT_MODEL.md`.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Shared.Contracts/StateMachineContracts/`

## Validation performed
- Build validation: `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to the installed .NET 10 SDK path.
- Dependency scope review: no RavenDB Embedded, MCP SDK, storage runtime, build execution, test execution, SignalR/SSE/Web API runtime, UI, package reference, or project reference changes were introduced.
- Naming review: the new contract surface remains under `RavenDB.TestRunner.McpServer.Shared.Contracts`.
- Lifecycle vocabulary review: build lifecycle states 15/15, build result statuses 6/6, build readiness statuses 4/4, run lifecycle states 13/13, attempt lifecycle states 8/8, and quarantine lifecycle states 5/5 are represented.
- Vocabulary separation review: `BuildExecution.state`, `BuildResult.status`, and `BuildReadinessToken.status` remain explicitly distinct and are not collapsed into one vocabulary.
- Terminal mapping review: all 5 build terminal mappings from `STATE_MACHINES.md` are represented without conflating lifecycle progression, final outcome, and readiness validity.
- Diff hygiene: `git diff --check` passed.

## Progress ledger update
- Mark `WP_A_005_state_machine_baseline` as `Done`.
- Keep `ENV-001` open.
- Keep WP_B/WP_C gated until full Phase 0 completion.

## Risks / follow-ups
- `ENV-001` remains open: the current shell still pins `MSBuildSDKsPath` to SDK 8.0.403, so .NET 10 validation requires a per-command override.
- Carry forward to `WP_A_006`: `WP_A_003` introduced exhaustive implementation-facing ID patterns for all 20 persisted document families, while `STORAGE_MODEL.md` currently lists example patterns for 12 families.

## ADR or design delta notes
- No ADR or design delta required.
