# MCP_TOOLS.md

## Purpose
Define authoritative MCP tool families for build and test orchestration in RavenDB Test Runner MCP Server.

## Scope
This file is normative for the MCP surface. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Surface overview
The MCP surface is split into first-class build and test tool families.

## Host topology
### Streamable HTTP host
- primary MCP host
- long-lived, independent process model
- local-first localhost posture
- SHOULD support resumability-friendly behavior for event streams where practical

### stdio bridge host
- compatibility host only
- thin bridge into the shared orchestration core
- MUST keep `stdout` protocol-clean
- MAY log to `stderr`

## build.* tools
### `build.graph.analyze`
Purpose: analyze solution/project graph and normalized build scope.

Request shape includes:
- workspace path or workspace ID
- build scope
- configuration
- optional property overrides

Response includes:
- normalized scope
- graph summary
- capability notes
- build-scope warnings

### `build.plan`
Purpose: generate a deterministic build plan and explicit reuse decision.

Response includes:
- `BuildPlan`
- `BuildReuseDecision`
- predicted artifacts
- build repro command preview

### `build.run`
Purpose: execute a build under explicit `BuildPolicy`.

Response includes:
- `buildId`
- `buildPlanId`
- progress token / stream handle
- `canCancel`

### `build.status`
Purpose: fetch build lifecycle status and partial summary.

### `build.output_tail`
Purpose: fetch or stream build stdout/stderr/merged output by cursor.

### `build.results`
Purpose: fetch final or partial build result, artifacts, readiness token, and reuse decision.

### `build.cancel`
Purpose: cancel an active build.

### `build.repro_command`
Purpose: return exact shell repro commands for the selected build.

### `build.clean`
Purpose: execute explicit clean/invalidation workflow.

### `build.readiness`
Purpose: inspect readiness tokens and whether a future test run can reuse them.

## tests.* tools
### Discovery and planning
- `tests.projects.list`
- `tests.categories.list`
- `tests.discover`
- `tests.preflight`
- `tests.plan`
- `tests.capabilities`

### Execution and output
- `tests.run`
- `tests.run_status`
- `tests.run_output_tail`
- `tests.run_results`
- `tests.cancel`
- `tests.rerun_failed`
- `tests.repro_command`

### Explainability and flaky tools
- `tests.explain_filter`
- `tests.explain_skip`
- `tests.iterative_run`
- `tests.flaky_analyze`
- `tests.flaky_history`
- `tests.compare_attempts`
- `tests.stability_report`
- `tests.quarantine_candidates`

## Build/test relationship contract
`tests.run` MUST NOT silently rebuild ad hoc. It MUST either:
1. reference a supplied build readiness token,
2. invoke the build subsystem under explicit build policy,
3. fail because policy forbids implicit build creation.

The MCP payload for test planning and execution MUST surface:
- effective build policy
- linked build ID when applicable
- linked readiness token when applicable
- build reuse decision when applicable

## Progress and cancellation
Long-running build and test tools MUST expose:
- progress updates
- explicit cancellation support
- stable status polling shapes
- partial result visibility where practical

## Validation requirements
- contract tests for all request/response shapes
- parity tests between Streamable HTTP and stdio bridge behavior
- progress/cancellation tests for `build.run`, `tests.run`, and `tests.iterative_run`
- regression tests for build visibility in the tests surface
