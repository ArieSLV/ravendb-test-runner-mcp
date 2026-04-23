# ADR 0008 SINGLE USER LOCAL FIRST V1

## Context
The first release is aimed at individual developers running a local standalone tool.

## Decision
Assume single-user local-first mode for v1 and defer team-shared semantics.

## Alternatives considered
- team-shared first
- cloud-first

## Consequences
- Auth and operations stay simpler while still permitting a later expansion path.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would require stronger multi-user authz and isolation earlier.
