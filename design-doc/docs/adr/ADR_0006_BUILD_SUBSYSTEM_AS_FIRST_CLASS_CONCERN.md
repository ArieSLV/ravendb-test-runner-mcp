# ADR 0006 BUILD SUBSYSTEM AS FIRST CLASS CONCERN

## Context
RavenDB-scale builds are expensive, and hidden rebuild behavior is architecturally unacceptable.

## Decision
Create a dedicated build subsystem with its own plans, executions, readiness tokens, tools, APIs, events, and UI surfaces.

## Alternatives considered
- hide build inside tests.run
- add one helper build tool only

## Consequences
- Build determinism, reuse, and visibility become explicit server responsibilities.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would undermine the key architectural constraint around rebuild prevention.
