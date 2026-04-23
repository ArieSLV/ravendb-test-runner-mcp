# ADR 0005 VERSION PLUGIN MODEL

## Context
Supporting v6.2, v7.1, v7.2, and future lines needs a scalable extension strategy.

## Decision
Use shared orchestration core plus per-line semantic plugins and capability matrices.

## Alternatives considered
- branch if/else chains
- one giant semantics class

## Consequences
- Future repo lines can be added without destabilizing existing lines.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would create long-term extensibility debt.
