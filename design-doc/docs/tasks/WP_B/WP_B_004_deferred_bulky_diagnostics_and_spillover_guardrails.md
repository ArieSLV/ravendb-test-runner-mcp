# WP B 004 deferred bulky diagnostics and spillover guardrails

## Task ID
`WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails`

## Title
Define the deferred bulky-diagnostics extension point and explicit out-of-v1-scope spillover guardrails.

## Purpose
Deliver one bounded step of RavenDB Test Runner MCP Server without changing frozen architecture implicitly.

## Scope
- define how v1 handles artifacts that exceed the practical attachment guardrail
- ensure such artifacts are classified as deferred / out-of-scope rather than silently becoming default filesystem-owned artifacts
- document future extension hooks without implementing a mandatory hybrid v1 storage path
- update only the contracts/docs/modules required by this task
- preserve the naming and build-subsystem invariants

## Out of scope
- implementing a general-purpose filesystem artifact store as a required v1 subsystem
- unrelated refactors
- opportunistic architecture changes without ADR
- undocumented contract drift

## Prerequisites
- Phase 0 contract freeze approved

## Touched modules/files
- RavenDB.TestRunner.McpServer.Storage.RavenEmbedded
- RavenDB.TestRunner.McpServer.Artifacts

## Inputs
- docs/architecture/DECISION_FREEZE.md
- docs/contracts/DOMAIN_MODEL.md
- docs/contracts/STORAGE_MODEL.md
- docs/contracts/ARTIFACTS_AND_RETENTION.md
- docs/contracts/SECURITY_AND_REDACTION.md

## Expected outputs
- Define the deferred bulky-diagnostics extension point and explicit out-of-v1-scope spillover guardrails.
- updated handoff note
- updated task status in TASK_INDEX.md if completed

## Implementation notes
- Stay within the declared scope.
- This task is about v1 policy clarity and explicit deferred handling, not about making filesystem ownership the default artifact path.
- Escalate design changes through ADR / design delta if required.

## Validation steps
- embedded startup integration test
- document persistence test
- attachment-backed artifact routing test
- explicit deferred-classification test for oversized artifacts

## Definition of done
- the task output exists in the declared module(s)
- the relevant contract references remain accurate
- validation steps were executed or explicitly blocked with reasons
- handoff note completed using `HANDOFF_TEMPLATE.md`

## Handoff expectations
- summarize exactly what changed
- mention any contract/doc updates
- mention risks and follow-ups
- mention any ADR impact
