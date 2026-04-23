# BUILD_SUBSYSTEM.md

## Purpose
Define the first-class build subsystem for RavenDB Test Runner MCP Server, including ownership, policies, lifecycles, persistence, status surfaces, and integration boundaries with test execution.

## Scope
This file is normative for the build subsystem. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Why this subsystem exists
The RavenDB repository is large enough that build cost is architecturally significant. Hidden or repeated rebuilds can dominate total test latency and make the product unusable. Therefore the server itself owns build policy and build determinism.

## Non-negotiable invariants
1. Build orchestration is a first-class subsystem, not a hidden pre-step inside test execution.
2. The build subsystem owns restore/build/clean/rebuild workflows.
3. The test subsystem MUST NOT improvise build behavior.
4. Every build decision MUST be persisted and explainable.
5. Meaningless repeated rebuilds are an architectural failure.
6. A test run MUST reference either:
   - a valid build readiness token,
   - a linked build execution,
   - an explicit expert-mode `skip build` decision with warnings.

## Responsibilities
The build subsystem owns:
- build graph analysis
- build scope normalization
- build policy evaluation
- build fingerprinting
- build reuse decisions
- restore/build/clean/rebuild execution
- build status and progress
- build artifact capture, including `binlog`
- build readiness issuance and invalidation
- build history and summaries
- build MCP tools
- build browser APIs and live event streams
- build-focused UI surfaces

## Inputs
- `WorkspaceSnapshot`
- `CapabilityMatrix`
- build scope (`solution`, `project`, `projects`, `directory`)
- `BuildPolicy`
- configuration, target frameworks, runtime identifiers if applicable
- relevant MSBuild properties
- relevant environment inputs

## Outputs
- `BuildPlan`
- `BuildExecution`
- `BuildResult`
- `BuildReuseDecision`
- `BuildFingerprint`
- `BuildReadinessToken`
- build artifacts and summaries
- build event stream
- build repro command

## Build graph analysis
The subsystem MUST analyze and persist enough information to explain:
- what was requested
- which root(s) were selected
- how the effective build graph or scope summary was derived
- which inputs matter for reuse or invalidation
- why a new build was required or not required

The graph layer MUST be deterministic for the same workspace snapshot and scope.

## Build policy behaviors
### `require_existing_ready_build`
Fail if no acceptable readiness token exists.

### `build_if_missing_or_stale`
Reuse an existing build when the stored fingerprint and policy allow; otherwise produce a new build.

### `force_incremental_build`
Run build again under explicit server control even if reuse may have been possible.

### `force_rebuild`
Perform an explicit rebuild path. This MUST be represented as a first-class build mode, not inferred implicitly.

### `expert_skip_build`
Allowed only in explicit expert mode. The system MUST emit warnings and persist the decision.

## Build fingerprinting
The build fingerprint MUST include enough data to explain reuse decisions. At minimum it SHOULD include:
- workspace identity
- repo line / plugin family
- git SHA and dirty fingerprint
- selected solution/project paths
- configuration
- relevant MSBuild property hash
- relevant environment hash
- package/dependency input hash
- output manifest hash when available

A build reuse decision MUST cite the fingerprint comparison or policy rule that allowed or rejected reuse.

## Build readiness
`BuildReadinessToken` is the contract between build and test subsystems.

A readiness token MUST be issued only when:
- build execution completed successfully, or
- reuse was accepted and the referenced outputs are still considered valid by policy.

A readiness token MUST be invalidated when:
- outputs are missing,
- a newer incompatible build supersedes it,
- policy or fingerprint changes reject it,
- cleanup/removal affects required outputs.

## Lifecycle vocabulary rule
The build subsystem uses three distinct but coordinated concepts:
- `BuildExecution.state` for lifecycle progression,
- `BuildResult.status` for final execution outcome,
- `BuildReadinessToken.status` for future output reusability.

The subsystem narrative, MCP tools, browser APIs, and UI MUST use these terms consistently.

### Canonical interpretation
- execution ends in lifecycle completion,
- result records `succeeded`, `failed`, `cancelled`, `timed_out`, `reused`, or `invalid`,
- readiness records whether the outputs remain reusable later.

## Artifact policy
### v1 authoritative rule
Build artifacts that are in scope for v1 MUST default to RavenDB attachments through `ArtifactRef` ownership.

### In-scope v1 build artifacts
- command payloads
- summaries
- output manifests
- stdout/stderr/merged logs
- `binlog` when the selected build profile enables it
- compact diagnostics in scope for v1

### Deferred bulky diagnostics
Bulky build diagnostics outside the practical v1 attachment policy are not part of the mandatory v1 storage path. They MUST be treated as deferred / out-of-scope unless a later ADR adds a separate spillover model.

## Status and transport surfaces
Build is first-class at every surface layer.

### MCP surface
The following tools are authoritative for build operations:
- `build.graph.analyze`
- `build.plan`
- `build.run`
- `build.status`
- `build.output_tail`
- `build.results`
- `build.cancel`
- `build.repro_command`
- `build.clean`
- `build.readiness`

### Browser-facing API surface
The following endpoint families are authoritative:
- `/api/builds`
- `/api/builds/{buildId}`
- `/api/builds/{buildId}/results`
- `/api/builds/{buildId}/artifacts`
- `/api/builds/{buildId}/logs/{stream}`
- `/api/builds/{buildId}/events`
- `/api/builds/{buildId}/readiness`

### Live event surface
The build subsystem emits:
- `build.created`
- `build.queued`
- `build.started`
- `build.phase_changed`
- `build.progress`
- `build.target_started`
- `build.output`
- `build.cache_hit`
- `build.cache_miss`
- `build.artifact_available`
- `build.completed`
- `build.failed`
- `build.cancelled`
- `build.timed_out`
- `build.readiness_issued`
- `build.readiness_invalidated`

### UI surface
The operator UI MUST expose:
- Builds list
- Build details
- Build graph / plan inspector
- Build output viewer
- Build artifact explorer
- Build policy and reuse explanation view

## Build-to-test handshake
`tests.run` MUST NOT silently rebuild ad hoc. It MUST either:
1. reference a supplied readiness token,
2. ask the build subsystem to satisfy the declared `BuildPolicy`,
3. fail because policy forbids implicit build creation.

The resulting `RunPlan` MUST persist:
- linked build ID when present
- linked readiness token when present
- reuse decision when present
- the exact build policy used

## Diagnostics and observability
The build subsystem SHOULD capture `binlog` under the configured build profile and MUST be able to stream progressive output to MCP and browser consumers.

The build subsystem MUST emit structured telemetry for:
- graph analysis duration
- restore duration
- build duration
- readiness issuance/invalidation
- reuse hit/miss ratio
- build cache rejection reasons

## Validation requirements
- reuse correctness tests
- no-chaotic-rebuild tests
- readiness issuance/invalidation tests
- build artifact and binlog tests
- build status/API/event parity tests
- restart recovery tests for active builds
- lifecycle vocabulary mapping tests (`BuildExecution.state` vs `BuildResult.status` vs `BuildReadinessToken.status`)
