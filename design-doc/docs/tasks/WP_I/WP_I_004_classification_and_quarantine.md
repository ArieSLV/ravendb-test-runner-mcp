# WP_I_004

## Title

Implement stability signals, scoring, and quarantine proposal/accept/revoke workflow.

## Scope

Deliver the focused implementation unit described by this task and only the directly required supporting updates.

## Out of scope

- unrelated refactors
- contract redesign beyond explicit required deltas
- neighboring work package responsibilities
- speculative optimizations

## Prerequisites

- docs/contracts/ERROR_TAXONOMY.md; docs/contracts/SECURITY_AND_REDACTION.md
- `AGENTS.md`
- relevant phase brief and work package brief

## Touched modules/files

Primary modules:
- src/RavenMcp.Flaky; src/RavenMcp.Web.Api

Primary documents:
- `docs/architecture/DECISION_FREEZE.md`
- task-relevant contract files
- this task card
- `docs/tasks/TASK_INDEX.md` on completion

Expected new or changed implementation files:
- create or update the minimal file set required inside the listed modules
- do not expand the touched surface without justification in handoff

## Implementation notes

- Preserve frozen naming, ID, and capability rules.
- If branch-specific behavior is required, route it through the semantic plugin/capability model.
- Keep raw artifact handling aligned with the hybrid storage policy.
- If the task affects external surfaces, update tests and schemas first or in the same change.
- If contract impact is discovered, stop and document it explicitly.

## Validation steps

- run focused unit/contract/integration tests relevant to this task
- verify no contract mismatch with the referenced documents
- update task status only after validation evidence exists
- capture any generated artifacts or logs needed for handoff

## Definition of done

- the intended implementation output exists
- referenced contracts are satisfied
- validation has been executed or explicitly documented as blocked
- handoff note is written using `docs/tasks/HANDOFF_TEMPLATE.md`
- task status in `docs/tasks/TASK_INDEX.md` is updated by the integrator

## Handoff expectations

Include:
- scope completed
- touched modules/files
- touched contracts or `No contract delta`
- validation executed
- open risks
- suggested follow-up task if needed

## Acceptance criteria

- classification tests and audit tests pass
- no unexplained architectural drift
- work is merge-ready for the integrator
