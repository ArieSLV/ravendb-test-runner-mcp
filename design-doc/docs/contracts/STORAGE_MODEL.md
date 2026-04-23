# STORAGE_MODEL.md

## Purpose
Define RavenDB Embedded collections, document IDs, attachment-backed artifact rules, indexes, and deferred extension points for oversized diagnostics.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Authoritative storage rule
RavenDB Embedded is the authoritative metadata store for **RavenDB Test Runner MCP Server**.

For v1, RavenDB attachments are the authoritative artifact store for build and test artifacts that are in scope for v1.

Filesystem-backed artifact ownership is not the default v1 storage model. It is reserved as a deferred extension point for bulky diagnostics that are explicitly introduced by a later ADR or milestone decision.

## Authoritative document ownership rule
Artifact payloads MUST be represented by `ArtifactRef` documents. The authoritative binary/text payload for an in-scope v1 artifact MUST be stored as a RavenDB attachment on the owning `ArtifactRef` document.

Result and execution documents MAY reference artifacts, but they MUST NOT be treated as the canonical binary payload owner.

## Collections
Mandatory collections:
- `WorkspaceSnapshots`
- `SemanticSnapshots`
- `CapabilityMatrices`
- `BuildGraphSnapshots`
- `BuildPlans`
- `BuildExecutions`
- `BuildResults`
- `BuildReadinessTokens`
- `TestCatalogEntries`
- `RunPlans`
- `RunExecutions`
- `RunResults`
- `AttemptPlans`
- `AttemptResults`
- `ArtifactRefs`
- `FlakyFindings`
- `QuarantineActions`
- `Settings`
- `EventCheckpoints`
- `CleanupJournal`

## ID conventions
The following document ID patterns are authoritative for the mandatory collections:
- `WorkspaceSnapshots` -> `workspaces/<workspace-hash>`
- `SemanticSnapshots` -> `semantic-snapshots/<workspace-hash>/<sem-hash>`
- `CapabilityMatrices` -> `capability-matrices/<workspace-hash>/<line>/<hash>`
- `BuildGraphSnapshots` -> `build-graphs/<workspace-hash>/<scope-hash>/<hash>`
- `BuildPlans` -> `build-plans/<workspace-hash>/<date>/<guid>`
- `BuildExecutions` -> `builds/<workspace-hash>/<date>/<guid>`
- `BuildResults` -> `build-results/<build-id>`
- `BuildReadinessTokens` -> `build-readiness/<workspace-hash>/<fingerprint>`
- `TestCatalogEntries` -> `test-catalog/<workspace-hash>/<catalog-version>/<test-id-hash>`
- `RunPlans` -> `run-plans/<workspace-hash>/<date>/<guid>`
- `RunExecutions` -> `runs/<workspace-hash>/<date>/<guid>`
- `RunResults` -> `run-results/<run-id>`
- `AttemptPlans` -> `attempt-plans/<run-id>/<attempt-index>`
- `AttemptResults` -> `attempts/<run-id>/<attempt-index>`
- `ArtifactRefs` -> `artifacts/<owner-kind>/<owner-id>/<kind>/<guid>`
- `FlakyFindings` -> `flaky-findings/<test-id>/<window>/<guid>`
- `QuarantineActions` -> `quarantine-actions/<test-id>/<guid>`
- `Settings` -> `settings/<scope>/<key>`
- `EventCheckpoints` -> `event-checkpoints/<stream-kind>/<owner-id>`
- `CleanupJournal` -> `cleanup-journal/<date>/<guid>`

## Indexes
Required indexes at minimum:
- builds by workspace + state + createdAt
- builds by readiness token + fingerprint
- runs by workspace + state + createdAt
- artifacts by owner (build/run/attempt)
- artifacts by kind + createdAt + retentionClass
- semantic snapshots by workspace + plugin
- flaky findings by test + classification + updatedAt
- quarantine actions by state + test

## Optimistic concurrency
Optimistic concurrency MUST be enabled for mutable lifecycle documents:
- `BuildExecutions`
- `RunExecutions`
- `AttemptResults`
- `QuarantineActions`
- `EventCheckpoints`
- `ArtifactRefs` when attachment metadata is updated after first creation

Append-only result documents MAY avoid mutation where practical.

## Attachment-backed v1 artifact policy
### In-scope v1 artifact classes
The following artifact families are expected to be stored as RavenDB attachments by default in v1:
- build commands, summaries, output manifests
- build stdout / stderr / merged output
- build `binlog` when enabled for v1 diagnostics
- run commands, summaries, normalized result payloads
- run stdout / stderr / merged output
- run `trx`
- run `junit` when present
- compact diagnostic logs that remain within the configured v1 attachment policy
- attempt summaries and attempt diffs
- flaky analysis outputs and quarantine audit exports that remain within the configured v1 attachment policy

### Practical attachment guardrails
The implementation MAY enforce configurable attachment size guardrails to prevent pathological payloads from being materialized in v1.

If an artifact exceeds the v1 practical attachment policy and belongs to a deferred artifact class, the system MUST classify it as deferred / out-of-scope rather than silently routing it to the filesystem as a default v1 behavior.

### Deferred extension point
The following are explicitly deferred from the mandatory v1 storage policy unless later introduced by ADR:
- crash dumps
- bulky blame bundles
- oversized diagnostic export bundles
- pathological transcripts that exceed the practical attachment policy

If a later milestone introduces these classes, it MUST also define:
- external storage ownership rules
- retention and cleanup rules
- browser/MCP retrieval behavior
- migration behavior from the attachments-first baseline

## Attachment metadata
Each `ArtifactRef` MUST store:
- `artifactId`
- `ownerKind`
- `ownerId`
- `artifactKind`
- `storageKind`
- `attachmentName`
- `sizeBytes`
- `sha256`
- `contentType`
- `previewAvailable`
- `retentionClass`
- `createdAtUtc`
- `expiresAtUtc` (optional)
- `sensitive`
- `deferredReason` (optional)

## Retention metadata
Retention and cleanup metadata MUST remain authoritative even when the payload itself is an attachment. Cleanup decisions MUST be attachment-aware and MUST NOT assume path-based deletion.

## Non-authoritative future note
A future ADR MAY add filesystem-backed or object-backed bulky diagnostics, but such a capability is not normative for v1 and MUST NOT be assumed by current coding agents.

## Validation requirements
- Restart recovery MUST not orphan active build/run records.
- Cleanup MUST respect retention classes and active references.
- Attachment-backed artifact retrieval MUST be deterministic and test-covered.
- Oversized deferred artifacts MUST be rejected or deferred explicitly, not silently rerouted to a default filesystem path.
