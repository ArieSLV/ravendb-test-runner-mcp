# Minimal Fix Set After Review

## Purpose

This document captures the **smallest coherent follow-up patch set** needed after the large execution-pack rewrite.

The intent is:

- keep the new product naming
- keep the new first-class build subsystem
- avoid another whole-pack redesign
- fix the specific contract-level gaps found during design review

This is **not** a rollback plan.
It is a targeted stabilization plan.

## Review Findings Being Addressed

1. Storage/artifact policy still reflects the old hybrid/filesystem-first direction instead of the clarified v1 artifact policy.
2. `MCP_TOOLS.md` is too high-level for a frozen parallel-implementation contract.
3. Build lifecycle terminology is inconsistent across state machine, result model, and readiness model.
4. Browser/UI contracts became weaker for the run/test side even though the build side became stronger.

## Fix Set

### Fix 1: Reconcile artifact policy with clarified v1 direction

**Goal**

Make the pack explicitly reflect the clarified v1 policy:

- test artifacts are attachment-first in RavenDB
- bulky/unbounded artifacts such as dumps and blame bundles are deferred or explicitly exceptional
- filesystem is no longer described as the normal canonical v1 artifact store

**Must update**

- `design-doc/docs/architecture/DECISION_FREEZE.md`
- `design-doc/docs/architecture/IMPLEMENTATION_SPEC.md`
- `design-doc/docs/contracts/STORAGE_MODEL.md`
- `design-doc/docs/contracts/ARTIFACTS_AND_RETENTION.md`

**Likely update**

- `design-doc/docs/contracts/BUILD_SUBSYSTEM.md`
- `design-doc/docs/tasks/TASK_INDEX.md`
- `design-doc/docs/work-packages/WP_B_STORAGE_AND_REGISTRY.md`
- `design-doc/docs/tasks/WP_B/*`

**Required outcome**

- one unambiguous v1 artifact story across the pack
- no split-brain wording like:
  - "filesystem is canonical"
  - while also expecting attachment-first v1 behavior

**Recommended rule**

For v1:

- RavenDB attachments are the default store for build/test artifacts
- filesystem-backed bulky artifacts are:
  - deferred,
  - exceptional,
  - or explicitly gated by policy / future milestone

### Fix 2: Re-freeze the MCP contract with concrete tool shapes

**Goal**

Restore `MCP_TOOLS.md` as a true implementation contract, not only a tool inventory.

**Must update**

- `design-doc/docs/contracts/MCP_TOOLS.md`

**Likely update**

- `design-doc/docs/contracts/BUILD_SUBSYSTEM.md`
- `design-doc/docs/contracts/DOMAIN_MODEL.md`

**Required outcome**

For each `build.*` and `tests.*` tool, define at minimum:

- purpose
- required request fields
- optional request fields
- required response fields
- idempotency expectation
- whether it is long-running
- whether it is cancelable
- progress behavior

**Most important tools to make concrete first**

- `build.plan`
- `build.run`
- `build.status`
- `build.results`
- `build.readiness`
- `tests.plan`
- `tests.run`
- `tests.run_status`
- `tests.run_results`
- `tests.iterative_run`

**Why this is minimal but critical**

Without this, parallel implementation can drift even if the architecture is good.

### Fix 3: Normalize build lifecycle vocabulary

**Goal**

Separate three different concepts cleanly:

1. lifecycle state
2. terminal result status
3. readiness validity status

**Must update**

- `design-doc/docs/contracts/STATE_MACHINES.md`
- `design-doc/docs/contracts/DOMAIN_MODEL.md`

**Likely update**

- `design-doc/docs/contracts/EVENT_MODEL.md`
- `design-doc/docs/contracts/BUILD_SUBSYSTEM.md`

**Required outcome**

Make the contract explicitly say:

- `BuildExecution.state` is lifecycle state
- `BuildResult.status` is terminal outcome
- `BuildReadinessToken.status` is readiness validity

**Recommended correction**

Do not use readiness-like or reuse-like values as lifecycle states.

In practice:

- remove or redefine ambiguous lifecycle terms like `ready`
- remove or redefine `reused_existing_ready_build` as a lifecycle state
- express reuse in `BuildReuseDecision` and/or `BuildResult.status`
- express readiness in `BuildReadinessToken.status`

**Preferred shape**

- lifecycle:
  - `created`
  - `queued`
  - `analyzing_graph`
  - `resolving_reuse`
  - `restoring`
  - `building`
  - `harvesting`
  - `completed`
  - `failed_terminal`
  - `cancelled`
  - `timed_out`

- terminal result:
  - `succeeded`
  - `failed`
  - `cancelled`
  - `timed_out`
  - `reused`
  - `invalid`

- readiness token:
  - `ready`
  - `superseded`
  - `invalidated`
  - `missing_outputs`

### Fix 4: Re-freeze run/test browser contracts

**Goal**

Keep the new build-first UI surfaces, but restore precise run/test UI contract definitions.

**Must update**

- `design-doc/docs/contracts/FRONTEND_VIEW_MODELS.md`

**Likely update**

- `design-doc/docs/contracts/WEB_API.md`
- `design-doc/docs/architecture/IMPLEMENTATION_SPEC.md`

**Required outcome**

`FRONTEND_VIEW_MODELS.md` should again define concrete run-side models, not only placeholders.

At minimum restore explicit field-level contracts for:

- `RunListItem`
- `RunDetailsView`
- `StepProgressView`
- `TestResultRow`
- `FailureDetailsView`
- `SkipExplanationView`
- `ReproCommandView`
- `AttemptComparisonView`
- `FlakyHistoryView`

Build linkage fields should remain additive:

- `linkedBuildId`
- `linkedReadinessTokenId`
- `buildReuseDecision`

But they should not replace the run/test-specific fields.

`WEB_API.md` should also state response compatibility with these models for the major run endpoints.

## Recommended Execution Order

1. Fix artifact/storage policy
2. Fix build lifecycle vocabulary
3. Re-freeze `MCP_TOOLS.md`
4. Re-freeze `FRONTEND_VIEW_MODELS.md`
5. Sync any task/work-package text that became misleading because of steps 1-4

## Important Boundary

This fix set is intentionally narrow.

It does **not** ask for:

- removal of the build subsystem
- rollback of phase/work-package/task restructuring
- rollback of naming consolidation
- rollback of separate build transports and status surfaces

It assumes the large rewrite stands, and only stabilizes the places where the new pack is still internally inconsistent or under-specified.
