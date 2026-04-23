# ADR 0003 RAVENDB EMBEDDED AS METADATA STORE

## Context
The product requires rich document persistence, history, and stateful orchestration without SQLite.

## Decision
Use RavenDB Embedded as the mandatory metadata/state store.

## Alternatives considered
- SQLite
- pure filesystem metadata
- external hosted RavenDB in v1

## Consequences
- Metadata, history, and registries become first-class persisted state.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would require reworking storage contracts and collection assumptions.
