# ADR 0004 ATTACHMENTS-FIRST V1 ARTIFACTS (legacy filename retained)

## Context
Earlier revisions assumed a default hybrid artifact model with RavenDB for metadata and compact attachments, and the filesystem for large raw artifacts. Post-review, the v1 product direction was clarified:
- v1 build/test artifacts should live in RavenDB as attachments,
- bulky binary diagnostics should be deferred unless later introduced,
- coding agents should not be forced to implement filesystem-owned artifact storage as the default v1 path.

## Decision
Freeze **v1 as attachments-first**.

Use RavenDB Embedded for metadata and use RavenDB attachments as the authoritative v1 artifact store for in-scope build/test artifacts.

Reserve a future extension point for bulky diagnostics that may later require a non-attachment storage path, but do not make that hybrid path normative in v1.

## Alternatives considered
- keep the previous hybrid-by-default model
- store everything in RavenDB without any extension point
- store everything on the filesystem

## Consequences
- v1 coding agents implement attachment-backed artifacts by default.
- Browser and MCP retrieval logic can assume attachment-backed artifacts for the current in-scope classes.
- Bulky diagnostics remain explicitly deferred and do not silently leak into filesystem ownership.
- A later ADR may still introduce a hybrid spillover model if needed.

## Contract impact
- `DECISION_FREEZE.md`
- `STORAGE_MODEL.md`
- `ARTIFACTS_AND_RETENTION.md`
- derivative browser/MCP contracts that expose artifact summaries

## Migration / rollback note
A future move to hybrid spillover is still possible, but it must be introduced explicitly and coherently across contracts, storage, browser APIs, and MCP payloads.
