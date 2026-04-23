# STORAGE_MODEL.md

## Purpose
Define RavenDB Embedded collections, IDs, indexes, attachments policy, and filesystem artifact ownership.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Authoritative storage rule
RavenDB Embedded is the authoritative metadata store for RavenDB Test Runner MCP Server. The filesystem is the authoritative store for large raw artifacts.

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
Examples:
- `workspaces/<workspace-hash>`
- `semantic-snapshots/<workspace-hash>/<sem-hash>`
- `capability-matrices/<workspace-hash>/<line>/<hash>`
- `build-graphs/<workspace-hash>/<scope-hash>/<hash>`
- `build-plans/<workspace-hash>/<date>/<guid>`
- `builds/<workspace-hash>/<date>/<guid>`
- `build-readiness/<workspace-hash>/<fingerprint>`
- `run-plans/<workspace-hash>/<date>/<guid>`
- `runs/<workspace-hash>/<date>/<guid>`
- `attempts/<run-id>/<attempt-index>`
- `artifacts/<kind>/<guid>`
- `flaky-findings/<test-id>/<window>/<guid>`

## Indexes
Required indexes at minimum:
- builds by workspace + state + createdAt
- builds by readiness token + fingerprint
- runs by workspace + state + createdAt
- artifacts by owner (build/run/attempt)
- semantic snapshots by workspace + plugin
- flaky findings by test + classification + updatedAt
- quarantine actions by state + test

## Optimistic concurrency
Optimistic concurrency MUST be enabled for mutable lifecycle documents:
- BuildExecutions
- RunExecutions
- AttemptResults
- QuarantineActions
- EventCheckpoints

Append-only result documents MAY avoid mutation where practical.

## Attachments policy
### Use RavenDB attachments when
- artifact size is at or below the configured threshold
- artifact type is compact and useful to query or render directly
- retaining the artifact with the document improves debugging ergonomics

### Use filesystem when
- artifact is large or unbounded
- artifact is append-heavy
- artifact is binary and potentially bulky
- artifact is a dump, blame bundle, or large transcript

## Filesystem root layout
```text
<artifact-root>/
  builds/<build-id>/
    command.json
    stdout.log
    stderr.log
    merged.log
    build.binlog
    output-manifest.json
  runs/<run-id>/
    plan.json
    summary.json
    step-001/
      stdout.log
      stderr.log
      merged.log
      results.trx
    attempts/
      attempt-001/
      attempt-002/
```

## Retention metadata
Each artifact ref MUST store:
- owner kind and ID
- storage kind
- retention class
- createdAtUtc
- expiresAtUtc (optional)
- sensitive flag
- preview availability

## Validation requirements
- Restart recovery MUST not orphan active build/run records.
- Cleanup MUST respect retention classes and active references.
- Attachment threshold routing MUST be deterministic and test-covered.
