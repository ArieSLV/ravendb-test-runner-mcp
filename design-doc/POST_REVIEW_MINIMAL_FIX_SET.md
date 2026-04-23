# POST_REVIEW_MINIMAL_FIX_SET.md

## Purpose
Capture the smallest coherent follow-up change set after the large execution-pack rewrite.

This document is intentionally **not** a new redesign. It exists to close the 4 substantive review findings without triggering another whole-pack reorganization.

## Scope
This fix set is limited to:
- storage policy consistency,
- MCP contract precision,
- build lifecycle vocabulary consistency,
- browser/UI contract precision for run/test surfaces.

This fix set does **not** reopen the following accepted decisions:
- first-class build subsystem,
- separate build transports and status APIs,
- revised product naming,
- phase/work-package/task reorganization,
- build-first architecture direction.

## Operating rule
Prefer **targeted normative edits** to existing top-level contract files over another large structural rewrite.

If a rule can be fixed by tightening an existing normative file, do that instead of creating new concept documents.

---

## Fix Set 1: Align artifact storage policy with the clarified v1 direction

### Problem
The rewritten pack freezes a hybrid artifact model where large raw artifacts default to the filesystem and only compact artifacts may live in RavenDB attachments.

That conflicts with the clarified v1 product direction:
- test artifacts in v1 should live in RavenDB as attachments,
- large bulky diagnostics such as dumps should be deferred to later milestones,
- v1 should not force implementers to build a hybrid filesystem ownership model as the default artifact path.

### Minimal correction
Adopt an explicit **v1 attachments-first policy**:
- RavenDB Embedded remains the authoritative metadata store,
- RavenDB attachments are the authoritative v1 artifact store for build/test artifacts that are in scope for v1,
- bulky binary diagnostics outside the practical attachment policy are out of v1 scope unless explicitly added in a later ADR or milestone note,
- filesystem artifact ownership is not the default v1 artifact strategy.

### Required edits
1. Update `docs/architecture/DECISION_FREEZE.md`
   - replace "Hybrid artifact storage is mandatory"
   - replace "Large raw artifacts default to filesystem"
   - freeze v1 as attachments-first
   - explicitly defer bulky diagnostic artifacts to later milestones

2. Update `docs/contracts/STORAGE_MODEL.md`
   - rewrite the "Authoritative storage rule"
   - remove filesystem-first language from the attachments policy
   - remove or mark as deferred the normative filesystem root layout
   - keep artifact references and retention metadata, but bind them to attachment-backed storage for v1

3. Update `docs/contracts/ARTIFACTS_AND_RETENTION.md`
   - replace threshold-routing language that defaults large artifacts to filesystem
   - state that v1 artifact classes are expected to be stored as attachments
   - move dumps / bulky blame bundles / oversized diagnostics into deferred scope language

### Acceptable simplification
It is fine if the pack still reserves a future extension point for filesystem-backed bulky diagnostics.

It is **not** fine for the current normative language to require hybrid storage in v1.

### Acceptance criteria
- a coding agent reading only the normative docs would implement attachment-backed v1 artifacts,
- no normative file tells implementers that large artifacts default to disk in v1,
- bulky diagnostic storage is clearly marked as deferred, not mandatory.

---

## Fix Set 2: Re-freeze MCP tool contracts at request/response level

### Problem
The new pack successfully introduces build as a first-class tool family, but `MCP_TOOLS.md` is now too high-level to act as a frozen parallel-implementation contract.

The main risk is not conceptual confusion. The risk is implementation drift:
- different agents can invent different payload shapes,
- idempotency and long-running behavior can diverge,
- build/test parity can become accidental instead of specified.

### Minimal correction
Keep the new build/test tool split, but restore a **compact schema-level freeze** inside `MCP_TOOLS.md`.

For every long-running or externally important tool, specify at minimum:
- request envelope,
- response envelope,
- stable IDs returned,
- polling/follow-up tools,
- cancellation behavior,
- idempotency / dedupe behavior where applicable,
- relationship to artifacts, events, and repro commands.

### Priority tools that must be fully frozen
#### Build family
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

#### Test/run family
- `tests.preflight`
- `tests.plan`
- `tests.run`
- `tests.run_status`
- `tests.run_output_tail`
- `tests.run_results`
- `tests.cancel`
- `tests.repro_command`
- `tests.iterative_run`

### Required edits
1. Update `docs/contracts/MCP_TOOLS.md`
   - keep the current structure
   - add compact normative request/response sections per priority tool
   - define canonical field names for linking build and run entities

2. Cross-check `docs/contracts/DOMAIN_MODEL.md`
   - ensure every referenced MCP payload maps to a named domain object or stable envelope

3. Cross-check `docs/contracts/ERROR_TAXONOMY.md`
   - ensure build and run tools use the same authoritative failure vocabulary

### Acceptable simplification
Not every low-frequency analytical tool needs a giant schema block.

But all core planning/execution/status/result tools must be precise enough that two agents would produce wire-compatible implementations.

### Acceptance criteria
- `MCP_TOOLS.md` is sufficient for parallel agent implementation without guessing,
- build tools and test tools expose symmetrical status/result behavior where intended,
- payload linking between build and run is explicitly named, not implied.

---

## Fix Set 3: Normalize build lifecycle vocabulary across state, result, and readiness

### Problem
The rewrite introduced a strong build subsystem, but its lifecycle language is currently split across three concepts:
- execution state machine,
- build result status,
- readiness token status.

Each concept is legitimate, but the current wording leaves room for ambiguous mappings such as:
- `ready` vs `completed`,
- `reused_existing_ready_build` vs `status=reused`,
- readiness state vs result state vs execution state.

That ambiguity is dangerous because it leaks into:
- persistence,
- MCP polling,
- UI state rendering,
- event emission,
- cleanup and invalidation logic.

### Minimal correction
Do **not** collapse these three concepts into one.

Instead, explicitly define the relationship between them:
- `BuildExecution.state` expresses lifecycle progression,
- `BuildResult.status` expresses final execution outcome,
- `BuildReadinessToken.status` expresses future reusability of outputs.

Then define the canonical mappings and examples in one place.

### Required edits
1. Update `docs/contracts/STATE_MACHINES.md`
   - keep the build state machine
   - add a short normative mapping note that distinguishes lifecycle state from result status and readiness status
   - replace awkward special-case names like `reused_existing_ready_build` if needed with a state vocabulary that maps more cleanly to `BuildResult.status=reused`

2. Update `docs/contracts/DOMAIN_MODEL.md`
   - add brief semantic definitions for:
     - `BuildExecution.state`
     - `BuildResult.status`
     - `BuildReadinessToken.status`
   - add a small mapping table or normative examples

3. Cross-check `docs/contracts/BUILD_SUBSYSTEM.md`
   - ensure the subsystem narrative uses the same terms consistently

### Recommended vocabulary rule
- execution ends in lifecycle completion,
- result records success/failure/reused/cancelled/timed_out,
- readiness records whether the outputs remain reusable.

### Acceptance criteria
- no reader has to infer how build execution state maps to build result status,
- reused-build behavior is described consistently in all core contracts,
- readiness invalidation is clearly separate from execution failure.

---

## Fix Set 4: Re-freeze browser/UI contracts for run/test surfaces

### Problem
The build side of the browser contract became stronger, but the run/test side regressed into placeholder view names and endpoint inventories.

That is risky because run/test UX is still a primary part of the product:
- runs list and run details,
- result rows,
- skip explanations,
- artifact and log inspection,
- live updates tied to build linkage.

If those view models stay underspecified, API and UI implementations will drift.

### Minimal correction
Do not redesign the web surface.

Simply restore the same degree of precision for run/test browser contracts that the pack already gives to build surfaces.

### Required edits
1. Update `docs/contracts/FRONTEND_VIEW_MODELS.md`
   - expand:
     - `RunListItem`
     - `RunDetailsView`
     - `TestResultRow`
     - `SkipExplanationView`
     - `ReproCommandView`
   - include the concrete fields required for build linkage, progress, selection summary, normalized outcome, and artifact summaries

2. Update `docs/contracts/WEB_API.md`
   - keep the endpoint inventory
   - add compact response-shape notes for the core run endpoints:
     - `GET /api/runs`
     - `GET /api/runs/{runId}`
     - `GET /api/runs/{runId}/results`
     - `GET /api/runs/{runId}/attempts`
     - `GET /api/runs/{runId}/artifacts`
     - `GET /api/runs/{runId}/plan`

3. Cross-check `docs/contracts/MCP_TOOLS.md`
   - ensure the browser-facing and MCP-facing result concepts refer to the same underlying entities

### Acceptance criteria
- frontend and backend agents can implement run/test pages without inventing fields,
- build linkage is explicit in run/test UI models,
- browser API payloads for runs are at least as well frozen as the build payloads.

---

## Sequence of work
Apply the fix set in this order:

1. `DECISION_FREEZE.md`
2. `STORAGE_MODEL.md`
3. `ARTIFACTS_AND_RETENTION.md`
4. `STATE_MACHINES.md`
5. `DOMAIN_MODEL.md`
6. `BUILD_SUBSYSTEM.md`
7. `MCP_TOOLS.md`
8. `WEB_API.md`
9. `FRONTEND_VIEW_MODELS.md`

Reason:
- fix storage and lifecycle policy first,
- then re-freeze wire contracts,
- then re-freeze browser/UI contracts on top of the corrected domain language.

## Explicit non-goals
This follow-up should **not**:
- rename phases again,
- renumber ADRs again,
- repartition work packages again,
- split the build subsystem back into a half-measure,
- reopen the first-class build decision.

## Definition of done
This minimal follow-up is complete when:
- the 4 review findings are closed,
- the existing rewrite remains structurally intact,
- no second whole-pack reorganization is needed,
- the resulting pack is again safe for parallel implementation.
