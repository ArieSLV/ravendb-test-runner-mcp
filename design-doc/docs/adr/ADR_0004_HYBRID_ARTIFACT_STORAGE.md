# ADR 0004 HYBRID ARTIFACT STORAGE

## Context
Raw build/test artifacts vary from tiny summaries to bulky logs and dumps.

## Decision
Use RavenDB for metadata and compact attachments; use the filesystem for large raw artifacts.

## Alternatives considered
- store everything in RavenDB
- store everything on filesystem

## Consequences
- Artifact access becomes policy-driven and size-aware.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would require storage migration and retention-policy rewrite.
