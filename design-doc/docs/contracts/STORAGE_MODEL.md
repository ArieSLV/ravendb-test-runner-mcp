# Storage Model Contract

## Purpose

Define how metadata, state, history, and compact artifacts are stored in RavenDB Embedded, and how that storage integrates with filesystem-backed raw artifacts.

## Authoritative storage principles

- RavenDB Embedded is mandatory.
- RavenDB is the authoritative store for metadata/state/history.
- The filesystem is the authoritative store for large raw artifacts.
- RavenDB MAY store selected compact artifacts as attachments.
- The storage layer MUST remain restart-safe and recoverable.

## Database identity

Database name:
- `RavenMcpControlPlane`

This name MAY be configured, but default tooling assumes it exists.

## Collections

The storage layer MUST define at least the following collections:

- `WorkspaceSnapshots`
- `SemanticSnapshots`
- `CompatibilityMatrices`
- `TestProjects`
- `TestAssemblies`
- `TestCategories`
- `TestCatalogEntries`
- `EnvironmentProfiles`
- `RunPlans`
- `Runs`
- `RunEvents`
- `RunArtifacts`
- `AttemptPlans`
- `AttemptResults`
- `FlakyHistories`
- `QuarantineDecisions`
- `Settings`
- `EventCheckpoints`

## Document ID conventions

### Workspace snapshots
`workspaces/<workspace-hash>`

### Semantic snapshots
`semantic-snapshots/<workspace-hash>/<snapshot-hash>`

### Compatibility matrices
`compatibility/<workspace-hash>/<matrix-hash>`

### Test catalog entries
`tests/<workspace-hash>/<project-id>/<test-hash>`

### Environment profiles
`environment-profiles/<profile-name>`

### Run plans
`run-plans/<run-id>`

### Runs
`runs/<run-id>`

### Run events
`run-events/<run-id>/<sequence-number>`

### Run artifacts
`run-artifacts/<run-id>/<artifact-id>`

### Attempt plans
`attempt-plans/<run-id>/<attempt-index>`

### Attempt results
`attempt-results/<run-id>/<attempt-index>`

### Flaky histories
`flaky-history/<test-id>`

### Quarantine decisions
`quarantine/<test-id>/<decision-id>`

### Settings
`settings/<settings-key>`

### Event checkpoints
`event-checkpoints/<consumer-name>`

## Optimistic concurrency strategy

- Optimistic concurrency MUST be enabled for mutable execution-state documents.
- Mutable collections include:
  - `Runs`
  - `RunArtifacts`
  - `AttemptResults`
  - `FlakyHistories`
  - `QuarantineDecisions`
  - `Settings`
  - `EventCheckpoints`
- Immutable collections SHOULD be treated append-only where possible.

## Mutable vs immutable documents

### Immutable / append-only preferred
- `WorkspaceSnapshots`
- `SemanticSnapshots`
- `CompatibilityMatrices`
- `RunPlans`
- `AttemptPlans`
- `RunEvents`

### Mutable
- `Runs`
- `RunArtifacts`
- `AttemptResults`
- `FlakyHistories`
- `QuarantineDecisions`
- `Settings`
- `EventCheckpoints`

## Indexes

The implementation MUST provide indexes for:

1. `Runs_ByState_StartedAt`
2. `Runs_ByWorkspace_StartedAt`
3. `RunArtifacts_ByRun_Kind`
4. `TestCatalog_ByCategory_Project`
5. `TestCatalog_ByFqn`
6. `FlakyHistory_ByScore`
7. `AttemptResults_ByRun_AttemptIndex`
8. `RunEvents_ByRun_Sequence`
9. `Quarantine_ByDecision_Confidence`
10. `SemanticSnapshots_ByWorkspace_CreatedAt`

## Attachment policy

### RavenDB attachments are allowed for:
- compact JSON exports
- compact `trx` / `junit`
- normalized compare artifacts
- compact diagnostics summaries
- compact console excerpts

### RavenDB attachments are not the default for:
- large merged logs
- `diag` files of significant size
- crash dumps
- blame bundles
- bulky raw stdout/stderr streams

## Size-threshold rule

Implement two configurable thresholds:

- `CompactAttachmentThresholdBytes`
- `InlinePreviewThresholdBytes`

Default guidance:
- artifacts at or below the compact threshold MAY be stored as attachments
- artifacts above the compact threshold MUST be filesystem-backed unless an explicit override exists

The exact numeric threshold is version-sensitive and must be validated on target developer machines.

## Filesystem artifact root

Canonical artifact root:
- application-configured local path
- default under developer-controlled application data directory

Every artifact metadata document MUST store:
- storage kind
- filesystem path if applicable
- checksum
- size
- retention class
- attempt/step/run linkage

## Event persistence strategy

Two storage layers exist:

1. authoritative current run state in `Runs`
2. append-only event log in `RunEvents`

This enables:
- replay
- browser reconnect
- audit trails
- flaky analysis inputs

## Cleanup and retention

The storage layer MUST support:
- soft expiration markers in RavenDB metadata
- physical filesystem deletion workflow
- attachment cleanup
- orphan detection between RavenDB and filesystem
- cleanup journal entries

## Startup/bootstrap responsibilities

The storage subsystem MUST:
- start RavenDB Embedded
- validate license presence
- ensure database existence
- ensure indexes
- validate artifact root
- detect and report startup blocking conditions

## What is authoritative

Authoritative:
- collection names
- ID patterns
- optimistic concurrency policy
- attachment policy
- artifact-root relationship

Not authoritative:
- DTO field definitions
- transport field subsets

## Validation requirements

- embedded bootstrap tests
- license-probe-order tests
- document ID format tests
- optimistic concurrency tests
- index existence tests
- restart recovery tests
- attachment-vs-filesystem routing tests
- orphan cleanup tests
