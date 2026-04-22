# ADR 0004 VERSION PLUGIN MODEL

## Context

Multiple RavenDB repository lines must be supported now and extended later without ad-hoc branching chaos.

## Decision

Use shared orchestration core plus version-specific semantic plugins and explicit capability matrix.

## Alternatives considered

Large if/else blocks; separate apps per version line; string-based branching only.

## Consequences

Keeps shared logic clean; increases plugin interface design discipline.

## Contract impact

Affects workspace detection, semantics packages, capability contracts, and cross-branch tests.

## Migration / rollback note

None recommended; plugin model is foundational.
