# Phase 8 — Operator UI

## Purpose
Build a RavenDB Studio-aligned dashboard with live build/run visibility, plan inspectors, artifacts, flaky analysis, and build policy views.

## Prerequisites
Phases 0-7

## In scope
- operator UI shell
- build/test views
- diagnostics views
- policy views

## Out of scope
- production UI polish beyond what this phase requires
- unrelated contract rewrites outside explicit deltas
- ad hoc architectural renaming not covered by the frozen naming policy

## Touched modules
- RavenDB.TestRunner.McpServer.Web.Ui

## Required contracts
- FRONTEND_VIEW_MODELS.md
- WEB_API.md
- EVENT_MODEL.md

## Deliverables
- operator UI shell
- build/test views
- diagnostics views
- policy views

## Acceptance criteria
- phase outputs are stored in the expected modules and registries
- phase-specific contracts remain satisfied
- no unresolved critical TODOs remain inside this phase’s declared scope
- human integrator can approve handoff to dependent phases

## Validation gates
- unit and contract tests for touched contracts
- integration smoke for touched subsystem(s)
- update docs/tasks if new constraints are discovered

## Main risks
- contract drift
- insufficient validation
- parallel work misalignment

## Handoff conditions
- all required deliverables complete
- no contract-breaking change left undocumented
- ADRs added for any meaningful deviation

## May start in parallel with
- WP_H
